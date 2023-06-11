/* Copyright 2017-2023 Cinegy GmbH.

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
*/

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Cinegy.TsDecoder.TransportStream
{
    public class TsPacketFactory
    {
        private readonly Meter _tsPktMeter = new("Cinegy.TsDecoder");
        private readonly ObservableCounter<long> _tsDataProcessedCounter;
        private readonly ObservableCounter<long> _tsCorruptedPacketsCounter;
        private readonly ArrayPool<byte> sharedBytePool  = ArrayPool<byte>.Shared;

        private const byte SyncByte = 0x47;

        private ulong _lastPcr;
        private ulong _lastOpcr;

        private byte[] _residualData;
        private int _residualDataSz;

        public const int TsPacketFixedSize = 188;
        public const int MaxAdaptationFieldSize = 183;
        
        public long TotalDataProcessed { get; private set; }

        public long TotalCorruptedTsPackets { get; private set; }

        public event TsPacketReadyEventHandler TsPacketReady;

        public delegate void TsPacketReadyEventHandler(object sender, TsPacketReadyEventArgs args);

        public TsPacketFactory()
        {
            _tsDataProcessedCounter = _tsPktMeter.CreateObservableCounter("dataProcessed", () => TotalDataProcessed, "Bytes",
                "Total volume of data pushed into TS Packet Factory");

            _tsCorruptedPacketsCounter = _tsPktMeter.CreateObservableCounter("corruptedPackets", () => TotalCorruptedTsPackets,
                "TS Packets",
                "Total number of discarded TS packets determined during framing as being damaged, or marked with TEI fields");
        }
        
        /// <summary>
        /// Accepts a data array, and loads this data into the factory. When data is pushed, it will raise a TsPacketReady event for each TS packet that is generated.
        /// </summary>
        /// <param name="data">Byte array containing TS stream data</param>
        /// <param name="dataSize">Optional length parameter to limit amount of data read from referenced array.</param>
        public void PushData(byte[] data, int dataSize = 0, bool retainPayload = true, bool preserveSourceData = false)
        {
            if (dataSize == 0)
            {
                dataSize = data.Length;
            }

            TotalDataProcessed += dataSize;

            var packets = GetTsPacketsFromData(data, dataSize, retainPayload, preserveSourceData);

            foreach (var tsPacket in packets)
            {
                OnTsPacketReadyDetected(tsPacket);
            }
        }

        public static byte[] GetDataFromTsPacket(TsPacket tsPacket)
        {
            var data = new byte[TsPacketFixedSize];
            data[0] = SyncByte;
            //set PID into bytes
            data[1] = (byte)(tsPacket.Pid >> 8);
            data[2] = (byte)(tsPacket.Pid & 0xFF);
            //apply flags (packed into top of PID bytes
            if (tsPacket.TransportErrorIndicator) data[1] += 0b10000000;
            if (tsPacket.PayloadUnitStartIndicator) data[1] += 0b01000000;
            if (tsPacket.TransportPriority) data[1] += 0b00100000;
            //pack scrambling and adaptation field control into byte with CC
            data[3] = (byte)(tsPacket.ScramblingControl << 6);
            if (tsPacket.AdaptationFieldExists) { data[3] += 0b00100000; }
            if (tsPacket.ContainsPayload) { data[3] += 0b00010000; }
            data[3] += (byte)(tsPacket.ContinuityCounter & 0b00001111);

            var idx = 4;
            if (tsPacket.AdaptationFieldExists)
            {
                var afData = GetDataFromAdaptationField(tsPacket.AdaptationField);
                Buffer.BlockCopy(afData, 0, data, idx, afData.Length);
                idx += afData.Length;
            }

            if (tsPacket.ContainsPayload)
            {
                Buffer.BlockCopy(tsPacket.Payload, 0, data, idx, tsPacket.PayloadLen);
            }

            return data;
        }

        public TsPacket[] GetRentedTsPacketsFromData(byte[] data, out int packetCount, int dataSize = 0, bool retainPayload = true, bool preserveSourceData = false)
        {
            return GetTsPacketsFromData(data, dataSize, retainPayload, preserveSourceData, true, out packetCount);
        }

        /// <summary>
        /// Returns TsPackets for any input data. If data ends with incomplete packet, this is stored and prepended to next call. 
        /// If data stream is restarted, prior buffer will be skipped as sync will not be acknowledged - but any restarts should being with first byte as sync to avoid possible merging with prior data if lengths coincide.
        /// </summary>
        /// <param name="data">Aligned or unaligned data buffer containing TS packets. Aligned is more efficient if possible.</param>
        /// <param name="dataSize">Optional length parameter to limit amount of data read from referenced array.</param>
        /// <param name="retainPayload">Optional parameter to trigger any resulting TS payload to be copied into the returned structure</param>
        /// <param name="preserveSourceData">Optional parameter to trigger complete copy of source data for TS packet to be held in array for quick access</param>
        /// <returns>Complete TS packets from this data and any prior partial data rolled over.</returns>
        public TsPacket[] GetTsPacketsFromData(byte[] data, int dataSize = 0, bool retainPayload = true, bool preserveSourceData = false)
        {
            return GetTsPacketsFromData(data, dataSize, retainPayload, preserveSourceData, false, out _);
        }

        public void ReturnTsPackets(TsPacket[] rentedPackets, int pktCount)
        {
            for(var i = 0;i< pktCount;i++)
            {
                if(rentedPackets[i].SourceData != null) ArrayPool<byte>.Shared.Return(rentedPackets[i].SourceData);
                if (rentedPackets[i].Payload == null)
                {
                   // Console.WriteLine("Null payload!");
                }
                else
                {
                    sharedBytePool.Return(rentedPackets[i].Payload);
                    //rentedPackets[i].Payload = null;
                }
               // if(rentedPacket.Payload != null) ArrayPool<byte>.Shared.Return(rentedPacket.Payload);
            }
            ArrayPool<TsPacket>.Shared.Return(rentedPackets);
        }
        
        protected virtual void OnTsPacketReadyDetected(TsPacket tsPacket)
        {
            var handler = TsPacketReady;
            if (handler == null) return;
            var args = new TsPacketReadyEventArgs { TsPacket = tsPacket };
            handler(this, args);
        }

        private TsPacket[] GetTsPacketsFromData(byte[] data, int dataSize, bool retainPayload, bool preserveSourceData, bool rentPackets, out int packetCounter)
        {
            try
            {
                var rentedDataArray = false;

                if (dataSize == 0) { dataSize = data.Length; }
                
                TotalDataProcessed += dataSize;
                
                if (_residualData != null)
                {
                    rentedDataArray = true;
                    var resizedData = sharedBytePool.Rent(dataSize + _residualDataSz);
                    Buffer.BlockCopy(_residualData, 0, resizedData, 0, _residualDataSz);
                    Buffer.BlockCopy(data, 0, resizedData, _residualDataSz, dataSize);
                    dataSize += _residualDataSz;
                    data = resizedData;
                }

                var maxPackets = dataSize / TsPacketFixedSize;

                var tsPackets = rentPackets ? ArrayPool<TsPacket>.Shared.Rent(maxPackets) : new TsPacket[maxPackets];

                packetCounter = 0;

                var start = FindSync(data, 0, ref dataSize);
                
                while (start >= 0 && dataSize - start >= TsPacketFixedSize)
                {
                    var tsPacket = new TsPacket
                    {
                        SyncByte = data[start],
                        Pid = (ushort)(((data[start + 1] & 0x1F) << 8) + data[start + 2]),
                        TransportErrorIndicator = (data[start + 1] & 0x80) != 0,
                        PayloadUnitStartIndicator = (data[start + 1] & 0x40) != 0,
                        TransportPriority = (data[start + 1] & 0x20) != 0,
                        ScramblingControl = (byte)(data[start + 3] >> 6),
                        AdaptationFieldExists = (data[start + 3] & 0x20) != 0,
                        ContainsPayload = (data[start + 3] & 0x10) != 0,
                        ContinuityCounter = (byte)(data[start + 3] & 0xF),
                        SourceBufferIndex = start
                    };

                    if (preserveSourceData)
                    {
                        tsPacket.SourceDataLen = TsPacketFixedSize;
                        tsPacket.SourceData = rentPackets ? sharedBytePool.Rent(TsPacketFixedSize) : new byte[TsPacketFixedSize];
                        Buffer.BlockCopy(data, start, tsPacket.SourceData, 0, TsPacketFixedSize);
                    }

                    //collect any adaptation field parameters into a struct for quick / easy access
                    //skip packets with error indicators or on the null PID
                    if (tsPacket.TransportErrorIndicator) TotalCorruptedTsPackets++;

                    if (!tsPacket.TransportErrorIndicator && tsPacket.Pid != (short)PidType.NullPid)
                    {
                        var payloadOffs = start + 4;
                        var payloadSize = TsPacketFixedSize - 4;//max possible payload size

                        if (tsPacket.AdaptationFieldExists)
                        {
                            tsPacket.AdaptationField = new AdaptationField()
                            {
                                FieldSize = data[payloadOffs++],
                                DiscontinuityIndicator = (data[payloadOffs] & 0x80) != 0,
                                RandomAccessIndicator = (data[payloadOffs] & 0x40) != 0,
                                ElementaryStreamPriorityIndicator = (data[payloadOffs] & 0x20) != 0,
                                PcrFlag = (data[payloadOffs] & 0x10) != 0,
                                OpcrFlag = (data[payloadOffs] & 0x8) != 0,
                                SplicingPointFlag = (data[payloadOffs] & 0x4) != 0,
                                TransportPrivateDataFlag = (data[payloadOffs] & 0x2) != 0,
                                AdaptationFieldExtensionFlag = (data[payloadOffs++] & 0x1) != 0
                            };

                            if (tsPacket.AdaptationField.FieldSize >= payloadSize)
                            {
                                //this has gone very wrong, and we should not trust any of the data sample we have...
                                return Flush(data, dataSize, rentedDataArray);
                            }
                            
                            if (tsPacket.AdaptationField is { PcrFlag: true, FieldSize: > 0 })
                            {
                                //Packet has PCR
                                tsPacket.AdaptationField.Pcr = ((uint)data[payloadOffs++] << 24) +
                                                               (uint)(data[payloadOffs++] << 16) +
                                                               (uint)(data[payloadOffs++] << 8) + data[payloadOffs++];

                                tsPacket.AdaptationField.Pcr <<= 1;

                                //TODO: this is hinky, and needs careful researching = bit it touches slightly the lowest bit (so does almost nothing) - not sure why...
                                //if ((data[start + 4] & 0x80) == 1)
                                //{
                                //    tsPacket.AdaptationField.Pcr |= 1;
                                //}

                                tsPacket.AdaptationField.Pcr *= 300;
                                var iLow = (uint)((data[payloadOffs++] & 1) << 8) + data[payloadOffs++];
                                tsPacket.AdaptationField.Pcr += iLow;
                                
                                if (_lastPcr == 0) _lastPcr = tsPacket.AdaptationField.Pcr;
                            }

                            if (tsPacket.AdaptationField is { OpcrFlag: true, FieldSize: > 0 })
                            {
                                //Packet has OPCR
                                tsPacket.AdaptationField.Opcr = ((uint)data[payloadOffs++] << 24) +
                                                                (uint)(data[payloadOffs++] << 16) +
                                                                (uint)(data[payloadOffs++] << 8) + data[payloadOffs++];

                                tsPacket.AdaptationField.Opcr <<= 1;
                                
                                //TODO: see above - hinky
                                //if ((data[start + 4] & 0x80) == 1)
                                //{
                                //    tsPacket.AdaptationField.Opcr |= 1;
                                //}

                                tsPacket.AdaptationField.Opcr *= 300;
                                var iLow = (uint)((data[payloadOffs++] & 1) << 8) + data[payloadOffs++];
                                tsPacket.AdaptationField.Opcr += iLow;

                                if (_lastOpcr == 0) _lastOpcr = tsPacket.AdaptationField.Opcr;
                            }

                            payloadSize = TsPacketFixedSize - 5 - tsPacket.AdaptationField.FieldSize;
                            payloadOffs = start + 5 + tsPacket.AdaptationField.FieldSize;
                        }
                        

                        //if a packet has a payload start, check for a PES start-code and then map some key fields into a PesHdr struct for quick / easy access
                        if (tsPacket is { ContainsPayload: true, PayloadUnitStartIndicator: true })
                        {
                            if (payloadOffs < dataSize - 1 && data[payloadOffs] == 0 && data[payloadOffs + 1] == 0 && data[payloadOffs + 2] == 1)
                            {
                                tsPacket.PesHeader = new PesHdr
                                {
                                    StartCode = (uint)((data[payloadOffs] << 16) + (data[payloadOffs + 1] << 8) +
                                                       data[payloadOffs + 2]),
                                    StreamId = data[payloadOffs + 3],
                                    PacketLength = (ushort)((data[payloadOffs + 4] << 8) + data[payloadOffs + 5]),
                                    Pts = -1,
                                    Dts = -1
                                };

                                //check to see if this is the kind of PES that has the more advanced flags and fields (like PTS / DTS)
                                if (!Pes.SimplePesTypes.Contains((PesStreamTypes)tsPacket.PesHeader.StreamId))
                                {
                                    var ptsDtsFlag = data[payloadOffs + 7] >> 6;

                                    tsPacket.PesHeader.HeaderLength = (byte)(3 + data[payloadOffs + 8]);

                                    switch (ptsDtsFlag)
                                    {
                                        case 2:
                                            tsPacket.PesHeader.Pts = Get_TimeStamp(2, data, payloadOffs + 9);
                                            break;
                                        case 3:
                                            tsPacket.PesHeader.Pts = Get_TimeStamp(3, data, payloadOffs + 9);
                                            tsPacket.PesHeader.Dts = Get_TimeStamp(1, data, payloadOffs + 14);
                                            break;
                                        case 1:
                                            //forbidden value - this has gone very wrong, and we should not trust any of the data sample we have...
                                            return Flush(data, dataSize, rentedDataArray);
                                    }
                                }
                            }
                        }

                        //copy the TS payload (exclusive of any adaptation field, but inclusive of any PES header) into the payload array of the TS packet
                        if (payloadSize >= 1 && retainPayload)
                        {
                            tsPacket.PayloadLen = payloadSize;
                            if (payloadSize > 184)
                            {
                                //this has gone very wrong, and we should not trust any of the data sample we have...
                                return Flush(data, dataSize, rentedDataArray);
                            }
                            tsPacket.Payload = rentPackets ? sharedBytePool.Rent(TsPacketFixedSize) : new byte[payloadSize];
                            Buffer.BlockCopy(data, payloadOffs, tsPacket.Payload, 0, payloadSize);
                        }
                    }

                    tsPackets[packetCounter++] = tsPacket;

                    start += TsPacketFixedSize;

                    if (start >= dataSize)
                        break;
                    if (data[start] == SyncByte) continue;
                    
                    //this has gone very wrong, and we should not trust any of the data sample we have...
                    return Flush(data, dataSize, rentedDataArray);
                }

                if (start + TsPacketFixedSize == dataSize)
                {
                    if (rentedDataArray)
                    {
                        sharedBytePool.Return(data);
                    }
                    return tsPackets;
                }

                //we have 'residual' data to carry over to next call
                _residualDataSz = dataSize - start;
                if (_residualDataSz > 0)
                {
                    if(_residualData != null) sharedBytePool.Return(_residualData);
                    _residualData =sharedBytePool.Rent(_residualDataSz);
                    Buffer.BlockCopy(data, start, _residualData, 0, _residualDataSz);
                }
                else
                {
                    if (_residualData != null)
                    {
                        sharedBytePool.Return(_residualData);
                        _residualData = null;
                    }
                }

                if (rentedDataArray)
                {
                    sharedBytePool.Return(data);
                }

                return tsPackets;
            }

            catch (Exception ex)
            {
                Debug.WriteLine("Exception within GetTsPacketsFromData method: " + ex.Message);
            }

            packetCounter = 0;
            return null;
        }

        private static byte[] GetDataFromAdaptationField(AdaptationField af)
        {
            var data = new byte[af.FieldSize + 1];

            data[0] = af.FieldSize;
            if (af.DiscontinuityIndicator) data[1] += 0b10000000;
            if (af.RandomAccessIndicator) data[1] += 0b01000000;
            if (af.ElementaryStreamPriorityIndicator) data[1] += 0b00100000;
            if (af.PcrFlag) data[1] += 0b00010000;
            if (af.OpcrFlag) data[1] += 0b00001000;
            //TODO: skip unsupported flags - should make this except or implement it...
            //if (af.SplicingPointFlag) data[5] += 0b00000100;
            //if (af.TransportPrivateDataFlag) data[5] += 0b00000010;
            //if (af.AdaptationFieldExtensionFlag) data[5] += 0b00000001;
            var idx = 2;
            if (af.PcrFlag)
            {
                //TODO: Read PCR data, format into correct bytes, push into data
                idx += 6;
            }
            if (af.OpcrFlag)
            {
                //TODO: Read OPCR data, format into correct bytes, push into data
                idx += 6;
            }
            //TODO: if SPF, TPDF or ADEF are set, advance index and add this data - when implemented

            while (idx < data.Length)
            {
                data[idx++] = 0xFF;
            }

            return data;
        }

        private static long Get_TimeStamp(int code, IList<byte> data, int offs)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            if (code == 0)
            {
                Debug.WriteLine("Method has been called with incorrect code to match against - check for fault in calling method.");
                throw new Exception("PES Syntax error: 0 value timestamp code check passed in");
            }

            if (data[offs + 0] >> 4 != code)
                throw new Exception("PES Syntax error: Wrong timestamp code");

            if ((data[offs + 0] & 1) != 1)
                throw new Exception("PES Syntax error: Invalid timestamp marker bit");

            if ((data[offs + 2] & 1) != 1)
                throw new Exception("PES Syntax error: Invalid timestamp marker bit");

            if ((data[offs + 4] & 1) != 1)
                throw new Exception("PES Syntax error: Invalid timestamp marker bit");

            long a = (data[offs + 0] >> 1) & 7;
            long b = (data[offs + 1] << 7) | (data[offs + 2] >> 1);
            long c = (data[offs + 3] << 7) | (data[offs + 4] >> 1);

            return (a << 30) | (b << 15) | c;
        }

        private static int FindSync(IList<byte> tsData, int offset,ref int dataLength)
        {
            if (tsData == null) throw new ArgumentNullException(nameof(tsData));

            //not big enough to be any kind of single TS packet
            if (dataLength < TsPacketFixedSize)
            {
                return -1;
            }

            try
            {
                var endOfSyncPos = 0;
                for (var i = offset; i < dataLength; i++)
                {
                    //check to see if we found a sync byte
                    if (tsData[i] != SyncByte) continue;
                    while (endOfSyncPos < dataLength)
                    {
                        endOfSyncPos += TsPacketFixedSize;
                        if (endOfSyncPos < dataLength && tsData[endOfSyncPos] != SyncByte) break;
                    }

                    if (endOfSyncPos < dataLength)
                    {
                        var zeroSyncCheckPos = endOfSyncPos;
                        //we did not sync to end, try to evaluate for special case of zero-padded data
                        while (zeroSyncCheckPos < dataLength)
                        {
                            zeroSyncCheckPos += TsPacketFixedSize;
                            if (zeroSyncCheckPos < dataLength && tsData[zeroSyncCheckPos] != 0) break;
                        }
                        dataLength = endOfSyncPos;
                    }

                    if (endOfSyncPos < dataLength)
                    {
                        //corrupted tail of data, loss of sync - for now, we'll throw the whole block
                        return -1;
                    }

                    return i;
                }
                return -1;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Problem in FindSync algorithm... : ", ex.Message);
                throw;
            }
        }

        private  TsPacket[] Flush(byte[] data, int dataSize, bool rentedDataArray)
        {
            var maxPackets = dataSize / TsPacketFixedSize;
            TotalCorruptedTsPackets += maxPackets;
            if (rentedDataArray)
            {
                sharedBytePool.Return(data);
            }
            _residualData = null;
            _residualDataSz = 0;

            return null;
        }
    }

    public class TsPacketReadyEventArgs : EventArgs
    {
        public TsPacket TsPacket { get; set; }
    }
}
