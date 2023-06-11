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

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Cinegy.TsDecoder.TransportStream
{

    public class Pes
    {
        public const uint DefaultPacketStartCodePrefix = 0x000001;
        
        public uint PacketStartCodePrefix { get; set; } //3 bytes 	0x000001
        
        public byte StreamId { get; set; } //	1 byte 	Examples: Audio streams (0xC0-0xDF), Video streams (0xE0-0xEF) [4][5][6][7]
                                           //Note: The above 4 bytes is called the 32 bit start code.
        public ushort PesPacketLength { get; set; } //	2 bytes 	Specifies the number of bytes remaining in the packet after this field. Can be zero. If the PES packet length is set to zero, the PES packet can be of any length. A value of zero for the PES packet length can be used only when the PES packet payload is a video elementary stream.[8]
        
        public OptionalPes OptionalPesHeader { get; set; } //	variable length (length >= 9) 	not present in case of Padding stream & Private stream 2 (navigation data)
        
        public byte[] Data { get; set; } //		See elementary stream. In the case of private streams the first byte of the payload is the sub-stream number.

        public static readonly IList<PesStreamTypes> SimplePesTypes = new[] {
                PesStreamTypes.ProgramStreamMap,
                PesStreamTypes.PaddingStream,
                PesStreamTypes.PrivateStream2,
                PesStreamTypes.ECMStream,
                PesStreamTypes.EMMStream,
                PesStreamTypes.ProgramStreamDirectory,
                PesStreamTypes.DSMCCStream,
                PesStreamTypes.H2221TypeEStream
            };

        private ushort _pesBytes;
        private readonly byte[] _data;

        public Pes(PesStreamTypes type, byte[] payload, OptionalPes optionalPesHeader = null)
        {
            switch (type){
                case PesStreamTypes.ProgramStreamMap:
                case PesStreamTypes.PrivateStream2:
                case PesStreamTypes.ECMStream:
                case PesStreamTypes.EMMStream:
                case PesStreamTypes.ProgramStreamDirectory:
                case PesStreamTypes.DSMCCStream:
                case PesStreamTypes.H2221TypeEStream:
                    PacketStartCodePrefix = DefaultPacketStartCodePrefix;
                    StreamId = (byte)type;
                    PesPacketLength = (ushort)payload.Length;
                    _data = ArrayPool<byte>.Shared.Rent(PesPacketLength);
                    Buffer.BlockCopy(payload, 0, _data, 0, PesPacketLength);
                    break;
                case PesStreamTypes.PaddingStream:
                    PacketStartCodePrefix = DefaultPacketStartCodePrefix;
                    StreamId = (byte)type;
                    PesPacketLength = (ushort)payload.Length;
                    _data = ArrayPool<byte>.Shared.Rent(PesPacketLength);
                    Array.Fill<byte>(_data,0xFF);
                    break;
                default:
                    if (optionalPesHeader is null)
                    {
                        throw new ArgumentException(
                            $"PES streams of type {type} must provide the optional PES header object");
                    }
                    PacketStartCodePrefix = DefaultPacketStartCodePrefix;
                    StreamId = (byte)type;
                    PesPacketLength = (ushort)(payload.Length + optionalPesHeader.PesHeaderLength + 3);
                    OptionalPesHeader = optionalPesHeader;
                    _data = ArrayPool<byte>.Shared.Rent(payload.Length);
                    Buffer.BlockCopy(payload, 0, _data, 0, payload.Length);
                    break;
            }
        }

        public Pes(TsPacket packet)
        {
            PacketStartCodePrefix = (uint)((packet.Payload[0] << 16) + (packet.Payload[1] << 8) + packet.Payload[2]);
            StreamId = packet.Payload[3];
            PesPacketLength = (ushort)((packet.Payload[4] << 8) + packet.Payload[5]);
            
            if (packet.PayloadLen > 0 && PesPacketLength > 0)
            {
                //_data = new byte[PesPacketLength + 6];
                _data = ArrayPool<byte>.Shared.Rent(PesPacketLength + 6);
                Buffer.BlockCopy(packet.Payload, 0, _data, _pesBytes, packet.PayloadLen);
                _pesBytes += (ushort)packet.PayloadLen;
            }
            else //hopefully this is therefore video - so we'll stick a massive buffer in to catch
            {
                //allocate a rather comically large buffer for now, but arraypool should save us being too evil...
                //_data = new byte[1024 * 1024 * 10];
                _data = ArrayPool<byte>.Shared.Rent(1024 * 1024 * 10);
                Buffer.BlockCopy(packet.Payload, 0, _data, _pesBytes, packet.PayloadLen);
                _pesBytes += (ushort)packet.PayloadLen;
            }
        }

        public bool HasAllBytes()
        {
            if (PesPacketLength == 0)
            {
                //video = no idea really how to tell it's all over!
                return true;
            }
            return _pesBytes >= PesPacketLength + 6 && PesPacketLength > 0;
        }

        public bool Add(TsPacket packet)
        {
            if (packet.PayloadUnitStartIndicator) return false;

            if (PesPacketLength == 0)
            {
                if (packet is not { Payload: { }, PayloadLen: > 0 }) return false;

                //video = no idea really how to tell it's all over - you just need faith it will ;)
                Buffer.BlockCopy(packet.Payload, 0, _data, _pesBytes, packet.PayloadLen);
                _pesBytes += (ushort)(packet.PayloadLen);
                return true;
            }

            if ((PesPacketLength + 6 - _pesBytes) > packet.PayloadLen)
            {
                Buffer.BlockCopy(packet.Payload, 0, _data, _pesBytes, packet.PayloadLen);
                _pesBytes += (ushort)(packet.PayloadLen);
            }
            else
            {
                Buffer.BlockCopy(packet.Payload, 0, _data, _pesBytes, (PesPacketLength + 6 - _pesBytes));
                _pesBytes += (ushort)(PesPacketLength + 6 - _pesBytes);
            }

            return true;
        }

        //see table 2-21 in ISO 13818-1:2019
        public byte[] GetDataFromPes()
        {
            var data = new byte[6 + PesPacketLength];

            //set start code
            data[0] = 0x0;
            data[1] = 0x0;
            data[2] = 0x1;

            //set streamid
            data[3] = StreamId;

            //set length
            data[4] = (byte)(PesPacketLength >> 8);
            data[5] = (byte)(PesPacketLength & 0xFF);

            if (SimplePesTypes.Contains((PesStreamTypes)StreamId) || StreamId == (byte)PesStreamTypes.PaddingStream)
            {
                //TODO: this needs  review - it might insert incorrect data!
                Buffer.BlockCopy(_data, 0, data, 6, PesPacketLength);

                return data;
            }

            //other types have the optional pes header structure, so we must now pack these fields
            data[6] = 0b10000000; //leading fixed '10' bits
            data[6] += (byte)(OptionalPesHeader.ScramblingControl << 4);
            data[6] += (byte)(OptionalPesHeader.Priority ? 1 << 3 : 0);
            data[6] += (byte)(OptionalPesHeader.DataAlignmentIndicator ? 1 << 2 : 0);
            data[6] += (byte)(OptionalPesHeader.Copyright ? 1 << 1 : 0);
            data[6] += (byte)(OptionalPesHeader.OriginalOrCopy ? 1 : 0);
            //TODO: Fill in other flags

            data[8] += OptionalPesHeader.PesHeaderLength;

            var payloadPosition = 9;
            if (OptionalPesHeader.OptionalFields is { Length: > 0 })
            {
                Buffer.BlockCopy(OptionalPesHeader.OptionalFields,0,data,payloadPosition,OptionalPesHeader.OptionalFields.Length);
                payloadPosition += OptionalPesHeader.OptionalFields.Length;
            }
            
            Buffer.BlockCopy(Data,0,data,payloadPosition,PesPacketLength - 3);
            
            return data;
        }

        public bool Decode()
        {
            if (!HasAllBytes()) return false;

            if (!SimplePesTypes.Contains((PesStreamTypes)StreamId))
            {
                OptionalPesHeader = new OptionalPes
                {
                    MarkerBits = (byte)((_data[6] >> 6) & 0x03),
                    ScramblingControl = (byte)((_data[6] >> 4) & 0x03),
                    Priority = (_data[6] & 0x08) == 0x08,
                    DataAlignmentIndicator = (_data[6] & 0x04) == 0x04,
                    Copyright = (_data[6] & 0x02) == 0x02,
                    OriginalOrCopy = (_data[6] & 0x01) == 0x01,
                    PtsdtsIndicator = (byte)((_data[7] >> 6) & 0x03),
                    EscrFlag = (_data[7] & 0x20) == 0x20,
                    EsRateFlag = (_data[7] & 0x10) == 0x10,
                    DsmTrickModeFlag = (_data[7] & 0x08) == 0x08,
                    AdditionalCopyInfoFlag = (_data[7] & 0x04) == 0x04,
                    CrcFlag = (_data[7] & 0x02) == 0x02,
                    ExtensionFlag = (_data[7] & 0x01) == 0x01,
                    PesHeaderLength = _data[8]
                };
            }

            Data = new byte[_pesBytes];
            Buffer.BlockCopy(_data,0,Data,0,_pesBytes);
            ArrayPool<byte>.Shared.Return(_data);

            return true;
        }
    }
}
