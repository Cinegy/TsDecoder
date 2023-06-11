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

namespace Cinegy.TsDecoder.Descriptors
{
    public class MetadataStdDescriptor : Descriptor
    {
        //STD model input leak rate in bits per second
        public uint InputLeakRateBits { get; set; }

        public uint BufferSizeKB { get; set; }

        //STD model output leak rate in bits per second
        public uint OutputLeakRateBits { get; set; }

        public MetadataStdDescriptor(byte[] stream, int start) : base(stream, start)
        {
            var idx = start + 2;

            InputLeakRateBits = (uint)((stream[idx++] << 16) & 0b00111111);
            InputLeakRateBits += (uint)(stream[idx++] << 8);
            InputLeakRateBits += (uint)(stream[idx++] & 0xFF);
            InputLeakRateBits *= 400;

            BufferSizeKB = (uint)((stream[idx++] << 16) & 0b00111111);
            BufferSizeKB += (uint)(stream[idx++] << 8);
            BufferSizeKB += (uint)(stream[idx++] & 0xFF);

            OutputLeakRateBits = (uint)((stream[idx++] << 16) & 0b00111111);
            OutputLeakRateBits += (uint)(stream[idx++] << 8);
            OutputLeakRateBits += (uint)(stream[idx++] & 0xFF);
            OutputLeakRateBits *= 400;

        }

    }
}
