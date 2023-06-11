/* Copyright 2016-2023 Cinegy GmbH.

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

using System.IO;

namespace Cinegy.TsDecoder.Video
{
    public class H265NalUnit : NalUnit
    {
        public uint RefIdc;
        public bool SvcExtensionFlag;
        public bool AvcVideoDescriptor3dExtensionFlag;
        public int UnitType;
        public byte[] RbspData;
        
        public H265NalUnit()
        {
        }

        public H265NalUnit(byte[] data, int offset, int length)
        {
            Init(data, offset, length);
        }

        public sealed override void Init(byte[] data, int offset, int length)
        {
            var nalUnitStart = FindNalUnit(data[..length], offset, out var nalUnitLength);
            var dataPos = nalUnitStart;
            UnitType = (int)(data[dataPos] & 0b01111110)>>1;
            dataPos+=2;
            var numBytesInRbsp = 0;
            
            RbspData = new byte[nalUnitLength - 2];
            for (var i = 2; i < nalUnitLength; i++)
            {
                if (i + 2 < nalUnitLength && data[dataPos] == 0 && data[dataPos + 1] == 0 && data[dataPos + 2] == 0x3)
                {
                    RbspData[numBytesInRbsp++] = data[dataPos++];
                    RbspData[numBytesInRbsp++] = data[dataPos++];
                    dataPos++;
                    i += 2;
                }
                else
                {
                    RbspData[numBytesInRbsp++] = data[dataPos++];
                }
            }

            NalUnitSize = nalUnitLength;
            ReadBytes = dataPos - offset;
        }

        private int FindNalUnit(byte[] data, int offset, out int length)
        {
            var dataPos = offset;
            length = 0;

            //check we have entered the function at a NAL unit start
            if (data[dataPos] == 0 && data[dataPos+1] == 0 && data[dataPos+2] == 1)
            {
                dataPos += 3;
                //start code is 001
            }
            else if (data[dataPos] == 0 && data[dataPos+1] == 0 && data[dataPos+2] == 0 && data[dataPos+3] == 1)
            {
                dataPos += 4;
                //start code is 0001
            }
            else
            {
                //corrupt data
                //TODO: Don't except here, since corruption in stream will trigger exception storm and kill things...
                throw new InvalidDataException("Could not find start code in NAL unit data");
            }

            if ((data[dataPos] & 0b10000000) != 0)
            {
                //TODO: Don't except here, since corruption in stream will trigger exception storm and kill things...
                throw new InvalidDataException("NAL unit forbidden zero bit is not zero");
            }
            
            //set the start position to after the 3-byte start code (Table 7.3.1 shows the NAL unit defined exclusive of a start code)
            var startPosition = dataPos;

            while (dataPos + 3 < data.Length)
            {
                if (data[dataPos] == 0 && data[dataPos + 1] == 0 && data[dataPos + 2] == 1)
                {
                    break;
                    //encountered next start code - trailing start codes are always only 001 as prior zeros are considered 'trailing zero 8 bits' at end of last NAL
                }
                else
                {
                    dataPos++;
                }
            }

            if (dataPos + 3 == data.Length)
            {
                dataPos += 3;
            }

            length = dataPos - startPosition;
            return startPosition;
        }
    }
}