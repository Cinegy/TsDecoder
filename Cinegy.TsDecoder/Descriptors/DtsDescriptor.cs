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

namespace Cinegy.TsDecoder.Descriptors
{
    /// <summary>
    /// A DTS Descriptor <see cref="Descriptor"/>.
    /// </summary>
    /// <remarks>
    /// For details please refer to the original documentation,
    /// e.g. <i>ETSI EN 300 468 V1.15.1 (2016-03) table G.1: DTS Audio Descriptor </i> or alternate versions.
    /// </remarks>
    public class DtsDescriptor : Descriptor
    {
        /*DTSAudioStreamDescriptor(){
         * descriptor_tag               8   uimsbf
         * descriptor_lengh             8   uimsbf
         * sample_rate_code             4   bslbf
         * bit_rate_code                6   bslbf
         * nblks                        7   bslbf
         * fsize                        14  uimsbf
         * surround_mode                6   bslbf
         * lfeflag                      1   uimsbf
         * extended_surround_flag       2   uimsbf
         * for(i=0;i<N;i++){
         *      additional_info_byte    8   bslbf
         */

        public DtsDescriptor(byte[] stream, int start)
            : base(stream, start)
        {
            var idx = start + 2;
            try
            {
                SamplerateCode = stream[2] >> 4 & 0x0f;
                Bitrate = (stream[2] & 0x0f) << 2 | stream[3] >> 6 & 0x02;
                NumberOfBlocks = (stream[3] & 0x3f) << 2 | stream[4] >> 7 & 0x01;
                FrameSize = (stream[4] & 0x7f) << 7 | stream[5] >> 1;
                SuroundMode = (stream[5] & 0x01) << 6 | stream[6] >> 3 & 0x1f;
                LfeFlag = stream[6] >> 2 & 0x01;
                ExtendedSurroundFlag = stream[6] & 0x03;
                AdditionalInfoBytes = new byte[DescriptorLength - 5];
                Buffer.BlockCopy(stream, stream[7], AdditionalInfoBytes, 0, DescriptorLength - 5);
            }
            catch (IndexOutOfRangeException)
            {
                throw new ArgumentOutOfRangeException("The DTS Descriptor Message is short!");
            }
        }

        public int SamplerateCode { get; }
        public int Bitrate { get; }
        public int NumberOfBlocks { get; }
        public int FrameSize { get; }
        public int SuroundMode { get; }
        public int LfeFlag { get; }
        public int ExtendedSurroundFlag { get; }
        public byte[] AdditionalInfoBytes { get; }
    }
}
