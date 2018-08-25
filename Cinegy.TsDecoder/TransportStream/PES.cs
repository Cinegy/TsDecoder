/* Copyright 2017 Cinegy GmbH.

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
using System.Collections.Generic;
using System.Diagnostics;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Cinegy.TsDecoder.TransportStream
{
    public class OptionalPes
    {
        public byte MarkerBits { get; set; } //	2 	10 binary or 0x2 hex
        public byte ScramblingControl { get; set; } //	2 	00 implies not scrambled
        public bool Priority { get; set; } //	1 	
        public bool DataAlignmentIndicator { get; set; } // 	1 	1 indicates that the PES packet header is immediately followed by the video start code or audio syncword
        public bool Copyright { get; set; } //	1 	1 implies copyrighted
        public bool OriginalOrCopy { get; set; } //	1 	1 implies original
        public byte PtsdtsIndicator { get; set; } //	2 	11 = both present, 01 is forbidden, 10 = only PTS, 00 = no PTS or DTS
        public bool EscrFlag { get; set; } //	1 	
        public bool EsRateFlag { get; set; } //	1 	
        public bool DsmTrickModeFlag { get; set; } // 	1 	
        public bool AdditionalCopyInfoFlag { get; set; } //	1 	
        public bool CrcFlag { get; set; } //	1 	
        public bool ExtensionFlag { get; set; } // 	1 	
        public byte PesHeaderLength { get; set; } //	8 	gives the length of the remainder of the PES header
        public byte[] OptionalFields { get; set; } // 	variable length 	presence is determined by flag bits above
    }

    public class Pes
    {
        private PesHeader _header;
        public const uint DefaultPacketStartCodePrefix = 0x000001;
        
        public PesHeader Header => _header;

        public OptionalPes OptionalPesHeader { get; set; } //	variable length (length >= 9) 	not present in case of Padding stream & Private stream 2 (navigation data)

        public byte[] Data
        {
            get
            {
                if (_data.Length == _pesBytes) return _data;

                var buff = new byte[_pesBytes];
                Buffer.BlockCopy(_data,0,buff,0,_pesBytes);
                return buff;
            }
        } //		See elementary stream. In the case of private streams the first byte of the payload is the sub-stream number.

        private ushort _pesBytes;
        private readonly byte[] _data;

        public Pes(TsPacket packet)
        {
            PopulateHeader(packet);

            var bufferSize = _header.PacketLength - _header.HeaderLength - 3;
            if (bufferSize < 1)
            {
                //unknown buffer size - allocate max anticipated encoded frame size
                bufferSize = 144 * 8 * 1024; //approx big enough for 8 DV25 frames - will do for now
            }
        
            _data = new byte[bufferSize];
            
            Buffer.BlockCopy(packet.Payload, Header.HeaderLength + 9, _data, _pesBytes, packet.Payload.Length - Header.HeaderLength - 9);
            _pesBytes += (ushort)(packet.Payload.Length - Header.HeaderLength - 9);
        }

        public bool HasAllBytes()
        {
            return _pesBytes >= _header.PacketLength && _header.PacketLength > 0;
        }

        public bool Add(TsPacket packet)
        {
            if (packet.PayloadUnitStartIndicator) return false;

            if (_data.Length - _pesBytes >= packet.Payload.Length)
            {
                Buffer.BlockCopy(packet.Payload, 0, _data, _pesBytes, packet.Payload.Length);
                _pesBytes += (ushort)packet.Payload.Length;
            }
            else
            {
                Buffer.BlockCopy(packet.Payload, 0, _data, _pesBytes, _data.Length - _pesBytes);
                _pesBytes += (ushort)(_data.Length - _pesBytes);
            }

            return true;
        }

        private void PopulateHeader(TsPacket tsPacket)
        {
            if (!tsPacket.ContainsPayload || !tsPacket.PayloadUnitStartIndicator) return;
            
            var data = tsPacket.Payload;

            var startCode = (uint)((data[0] << 16) + (data[1] << 8) +
                                   data[2]);

            if (startCode != 1) return;

            _header = new PesHeader
            {
                StartCode = 1,
                StreamId = data[3],
                PacketLength = (ushort)((data[4] << 8) + data[5]),
                Pts = -1,
                Dts = -1
            };
            
            if (_header.StreamId != (uint)PesStreamTypes.ProgramStreamMap &&
                _header.StreamId != (uint)PesStreamTypes.PaddingStream &&
                _header.StreamId != (uint)PesStreamTypes.PrivateStream2 &&
                _header.StreamId != (uint)PesStreamTypes.ECMStream &&
                _header.StreamId != (uint)PesStreamTypes.EMMStream &&
                _header.StreamId != (uint)PesStreamTypes.ProgramStreamDirectory &&
                _header.StreamId != (uint)PesStreamTypes.DSMCCStream &&
                _header.StreamId != (uint)PesStreamTypes.H2221TypeEStream)
            {
                var ptsDtsFlag = data[7] >> 6;

                _header.HeaderLength = data[8];

                switch (ptsDtsFlag)
                {
                    case 2:
                        _header.Pts = Get_TimeStamp(2, data, 9);
                        break;
                    case 3:
                        _header.Pts = Get_TimeStamp(3, data, 9);
                        _header.Dts = Get_TimeStamp(1, data, 14);
                        break;
                    case 1:
                        throw new Exception("PES Syntax error: pts_dts_flag = 1");
                }
            }

            _header.Payload = new byte[_header.HeaderLength];
            Buffer.BlockCopy(data, 0, _header.Payload, 0, _header.HeaderLength);
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

        //public bool Decode()
        //{
        //    if (!HasAllBytes()) return false;

        //    //OptionalPesHeader = new OptionalPes
        //    //{
        //    //    MarkerBits = (byte) ((_data[6] >> 6) & 0x03),
        //    //    ScramblingControl = (byte) ((_data[6] >> 4) & 0x03),
        //    //    Priority = (_data[6] & 0x08) == 0x08,
        //    //    DataAlignmentIndicator = (_data[6] & 0x04) == 0x04,
        //    //    Copyright = (_data[6] & 0x02) == 0x02,
        //    //    OriginalOrCopy = (_data[6] & 0x01) == 0x01,
        //    //    PtsdtsIndicator = (byte) ((_data[7] >> 6) & 0x03),
        //    //    EscrFlag = (_data[7] & 0x20) == 0x20,
        //    //    EsRateFlag = (_data[7] & 0x10) == 0x10,
        //    //    DsmTrickModeFlag = (_data[7] & 0x08) == 0x08,
        //    //    AdditionalCopyInfoFlag = (_data[7] & 0x04) == 0x04,
        //    //    CrcFlag = (_data[7] & 0x02) == 0x02,
        //    //    ExtensionFlag = (_data[7] & 0x01) == 0x01,
        //    //    PesHeaderLength = _data[8]
        //    //};

        //    Data = _data;

        //    return true;
        //}
    }
}
