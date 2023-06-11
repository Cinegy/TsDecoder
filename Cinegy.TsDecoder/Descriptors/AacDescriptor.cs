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
    /// A AAC Descriptor <see cref="Descriptor"/>.
    /// </summary>
    /// <remarks>
    /// For details please refer to the original documentation,
    /// e.g. <i>ETSI EN 300 468 V1.15.1 (2016-03) Table H.1 </i> or alternate versions.
    /// </remarks>
    public class AacDescriptor : Descriptor
    {
        /*DTSAudioStreamDescriptor(){
         * descriptor_tag               8   uimsbf
         * descriptor_lengh             8   uimsbf
         * profile_and_level            8   uimsbf
         * aac_type_flag                1   bslbf
         * saoc_de_flag                 1   bslbf
         * reserve_for future_use       6   bslbf
         * if(aac_type_flag==1){          
         *    aac_type                  8   uimsbf
         * }         
         * for(i=0;i<N;i++){
         *      additional_info_byte    8   bslbf
         */

        public AacDescriptor(byte[] stream, int start)
            : base(stream, start)
        {
            var idx = start + 2;
            try
            {
                var headerLength = 2;
                ProfileAndLevel = stream[2];
                AacTypeFlag = stream[3] >> 7 & 0x01;

                var i = 4;
                if (AacTypeFlag == 0x01)
                {
                    headerLength++;
                    AacType = stream[i++];
                }

                AdditionalInfoBytes = new byte[DescriptorLength - headerLength];
                Buffer.BlockCopy(stream, stream[i], AdditionalInfoBytes, 0, DescriptorLength - headerLength);

            }
            catch (IndexOutOfRangeException)
            {
                throw new ArgumentOutOfRangeException("The AAC Descriptor Message is short!");
            }
        }
        public byte ProfileAndLevel { get; }
        public int AacTypeFlag { get; }
        public bool SaocDeTypeFlag { get; }
        public byte AacType { get; }
        public byte[] AdditionalInfoBytes { get; }

    }
}
