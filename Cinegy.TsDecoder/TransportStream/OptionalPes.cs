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

// ReSharper disable UnusedAutoPropertyAccessor.Global

using System;

namespace Cinegy.TsDecoder.TransportStream
{
    public class OptionalPes
    {
        private int _length = -1;

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

        public byte PesHeaderLength
        {
            get
            {
                //check if this field has been set, and return if it has
                if (_length != -1)
                {
                    return (byte)_length;
                }

                //the value is unset, so we shall attempt to calculate it from flag presence
                var length = 0;

                switch (PtsdtsIndicator)
                {
                    case 0b10:
                        //5 bytes for just a PTS value
                        length += 5;
                        break;
                    case 0b11:
                        //10 bytes for both a PTS and DTS
                        length += 10;
                        break;
                }

                //ESCR is described in 6 bytes
                if (EscrFlag) length += 6;

                //ES rate is described in 3 bytes
                if (EsRateFlag) length += 3;

                //Dsm trick mode is described in a bye
                if(DsmTrickModeFlag)
                {
                    length += 8;
                }

                //additional copy info is just a byte
                if (AdditionalCopyInfoFlag) length += 1;

                //crc flag as 2 byte description
                if (CrcFlag) length += 2;

                //extension flag is not supported - throw an exception if detected
                if (ExtensionFlag)
                {
                    throw new NotSupportedException(
                        "PES Extension Flag set in optional PES header, and this is unsupported for decoding implicit PES lengths");
                }

                if(OptionalFields!=null) length += OptionalFields.Length;

                return (byte)length;
            }
            set => _length = value;
        }


        public byte[] OptionalFields { get; set; } // 	variable length 	presence is determined by flag bits above
    }
}
