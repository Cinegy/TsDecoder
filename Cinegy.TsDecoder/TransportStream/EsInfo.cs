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
using System.Collections.Generic;
using System.Linq;
using Cinegy.TsDecoder.Descriptors;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Cinegy.TsDecoder.TransportStream
{
    public class EsInfo
    {
        public byte StreamType { get; set; }

        public ushort ElementaryPid { get; set; }

        public ushort EsInfoLength { get; set; }

        public ushort DescriptorsLength { 
            get
            {
                var descsLength = 0;
                //only serialize RegistrationDescriptors for now
                var regDescs = Descriptors.Where(i => i.GetType() == typeof(RegistrationDescriptor));

                foreach (var esDesc in regDescs)
                {
                    descsLength += (ushort)(2 + esDesc.DescriptorLength);
                }
                return (ushort)descsLength;
            }
        }

        public ICollection<Descriptor> Descriptors { get; set; }

        public byte[] SourceData { get; set; }

        public byte[] GetData()
        {
            var dataLength = 5;
            var descsLength = DescriptorsLength;
            dataLength += descsLength;

            var data = new byte[dataLength];
            var idx = 0;

            data [idx++] = StreamType;
            data [idx++] = (byte) ((ElementaryPid >> 8) & 0b00011111);
            data[idx++] = (byte) (ElementaryPid & 0xFF);
            data[idx++] = (byte)((descsLength >> 8) & 0b00000011);
            data[idx++] = (byte)(descsLength & 0xFF);
            
            var regDescs = Descriptors.Where(i => i.GetType() == typeof(RegistrationDescriptor));
            foreach (var esDesc in regDescs)
            {
                var registrationDesc = (RegistrationDescriptor)esDesc;
                if (registrationDesc != null)
                {
                    var descLength = (ushort)(2 + esDesc.DescriptorLength);
                    descsLength += descLength;
                    var descData = registrationDesc.GetData();
                    Buffer.BlockCopy(descData, 0, data, idx, descLength);
                    idx += descLength;
                }
            }

            return data;
        }
    }
}
