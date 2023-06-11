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
    public class Descriptor
    {
        public Descriptor() { }

        public Descriptor(byte[] stream, int start)
        {
            if (start + 1 > stream.Length)
            {
                //corrupt packet
                return;
            }

            DescriptorTag = stream[start];
            DescriptorLength = stream[start + 1];

            if (stream.Length - start - 2 >= DescriptorLength)
            {
                Data = new byte[DescriptorLength];
                Buffer.BlockCopy(stream, start + 2, Data, 0, DescriptorLength);
            }
            else
            {
                //corrupt packet
                //Debug.WriteLine($"Descriptor has length beyond packet {Name} - {DescriptorTag}.");
            }
        }

        public byte DescriptorTag { get; internal set; }

        public byte DescriptorLength { get; internal set; }

        public byte[] Data { get; }

        public virtual string LongName => DescriptorDictionaries.DescriptorTypeDescriptions[DescriptorTag];

        public virtual string Name => DescriptorDictionaries.DescriptorTypeShortDescriptions[DescriptorTag];

        public override string ToString()
        {
            return $"(0x{DescriptorTag:x2}): {Name}, Length: {DescriptorLength}";
        }
    }
}
