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
using System.Text;

namespace Cinegy.TsDecoder.Descriptors
{
    /// <summary>
    /// A Registration Descriptor <see cref="Descriptor"/>.
    /// </summary>
    /// <remarks>
    /// For details please refer to the original documentation,
    /// e.g. <i>ETSI EN 300 468 V1.15.1 (2016-03)</i> or alternate versions.
    /// </remarks>
    public class RegistrationDescriptor : Descriptor
    {
        //ISO/IEC 13818-1:2007 Table 2-51
        public RegistrationDescriptor() {
            DescriptorTag = 0x5;
            DescriptorLength = 0x4;
        }

        public RegistrationDescriptor(byte[] stream, int start) : base(stream, start)
        {
            var idx = start + 2; //start + desc tag byte + desc len byte 

            Buffer.BlockCopy(stream, idx, FormatIdentifier, 0, 4);
            idx += 4;

            if (DescriptorLength <= 4) return;
            AdditionalIdentificationInfo = new byte[DescriptorLength - 4];
            Buffer.BlockCopy(stream, idx, AdditionalIdentificationInfo, 0, AdditionalIdentificationInfo.Length);
        }

        public byte[] FormatIdentifier { get; set; } = new byte[4];

        public string Organization { 
            get 
            {
                var termination = 4;
                for (var i = 0; i < 4; i++)
                {
                    if (FormatIdentifier[i] == 0)
                    {
                        termination = i;
                        break;
                    }
                    else if (FormatIdentifier[i] < 32 || FormatIdentifier[i] > 126 )
                    {
                        return string.Empty;
                    }
                }

                return Encoding.ASCII.GetString(FormatIdentifier, 0, termination);
            }
        }

        public byte[] AdditionalIdentificationInfo { get; }

        public byte[] GetData()
        {
            if(DescriptorLength > 6)
            {
                throw new NotImplementedException("Registration Descriptors with Addition ID Info set are not yet supported for serialization");
            }

            var data = new byte[2 + DescriptorLength];
            data[0] = DescriptorTag;
            data[1] = DescriptorLength;
            Buffer.BlockCopy(FormatIdentifier, 0, data, 2, 4);
            return data;
        }
    }
}
