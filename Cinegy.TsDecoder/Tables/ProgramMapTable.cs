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

using System.Collections.Generic;
using Cinegy.TsDecoder.TransportStream;
using System;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Cinegy.TsDecoder.Tables
{
    public class ProgramMapTable : Table
    {
        public ushort ProgramNumber { get; set; }
        public byte VersionNumber { get; set; }
        public bool CurrentNextIndicator { get; set; }
        public byte SectionNumber { get; set; }
        public byte LastSectionNumber { get; set; }
        public ushort PcrPid { get; set; }
        public byte ProgramInfoLength { get; set; }
        public List<EsInfo> EsStreams { get; set; }
        public uint Crc { get; set; }

        public byte[] GetData()
        {
            if(ProgramInfoLength == 0)
            {
                if (Descriptors != null)
                {
                    foreach(var desc in Descriptors)
                    {
                        ProgramInfoLength += (byte)(2 + desc.DescriptorLength);
                    }
                }                
            }
            
            if (SectionLength == 0)
            {
                SectionLength = (ushort)(9 + ProgramInfoLength + 4);
                if (EsStreams != null)
                {
                    foreach (var esInfo in EsStreams)
                    {
                        SectionLength += 5;
                        SectionLength += esInfo.DescriptorsLength;                 
                    }
                }
            }

            var data = new byte[3 + SectionLength];
            data[0] = TableId;
            data[1] = 0b10000000; //section_syntax_indicator shall be '1', followed by '0', followed by 2 reserved bytes, and a leading 2 '0's from section length
            data[1] = (byte)((SectionLength >> 8) & 0b00000011);
            data[2] = (byte)(SectionLength & 0xFF);
            data[3] = (byte)(ProgramNumber >> 8);
            data[4] = (byte)(ProgramNumber & 0xFF);
            data[5] = (byte)(VersionNumber & 0b00111110);
            if (CurrentNextIndicator) data[5] += 0b00000001;
            data[6] = SectionNumber;
            data[7] = LastSectionNumber;
            data[8] = (byte)((PcrPid >> 8) & 0b00011111);
            data[9] = (byte)(PcrPid & 0xFF); 
            data[10] = (byte)((ProgramInfoLength >> 8) & 0b00000011);
            data[11] = (byte)(ProgramInfoLength & 0xFF);
            
            //TODO: This is a crude 'hop' over space for program descriptors - chaos will ensue if there is actually supposed to be any
            var idx = 12 + data[11];

            //if we have Elementary Streams, add them to this table
            if (EsStreams != null)
            {
                foreach (var esInfo in EsStreams)
                {
                    var esData = esInfo.GetData();
                    Buffer.BlockCopy(esData, 0, data, idx, esData.Length);
                    idx += esData.Length;
                }
            }
            //var idx = 8;
            //for (var i = 0; i < ProgramNumbers.Length; i++)
            //{
            //    data[idx] = (byte)(ProgramNumbers[i] >> 8);
            //    data[idx + 1] = (byte)(ProgramNumbers[i] & 0xFF);
            //    data[idx + 2] = (byte)((Pids[i] >> 8) & 0b000111111);
            //    data[idx + 3] = (byte)(Pids[i] & 0xFF);
            //    i++;
            //    idx += 4;
            //}

            //TODO: CRC needs calc'ing here

            return data;
        }
    }

}
