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
    public class MetadataDescriptor : Descriptor
    {
        public ushort ApplicationFormat { get; set; }

        public uint ApplicationFormatId { get; set; }

        public byte MetadataFormat { get; set; }

        public uint MetadataFormatId { get; set; }

        public byte MetadataServiceId { get; set; }

        public byte DecoderConfigFlag { get; set; }

        public bool DsmCcFlag { get; set; }

        public byte[] ServiceIdRecord { get; set; }

        public MetadataDescriptor(byte[] stream, int start) : base(stream, start)
        {
            var idx = start + 2;
            ApplicationFormat = (ushort)(stream[idx++] << 8);
            ApplicationFormat += (ushort)(stream[idx++] & 0xFF);

            if (ApplicationFormat == 0xFFFF)
            {
                ApplicationFormatId = (uint)(stream[idx++] << 24);
                ApplicationFormatId += (uint)(stream[idx++] << 16);
                ApplicationFormatId += (uint)(stream[idx++] << 8);
                ApplicationFormatId += (uint)(stream[idx++] & 0xFF);
            }

            MetadataFormat = stream[idx++];

            if (MetadataFormat == 0xFF)
            {
                MetadataFormatId = (uint)(stream[idx++] << 24);
                MetadataFormatId += (uint)(stream[idx++] << 16);
                MetadataFormatId += (uint)(stream[idx++] << 8);
                MetadataFormatId += (uint)(stream[idx++] & 0xFF);
            }

            MetadataServiceId = stream[idx++];

            DecoderConfigFlag = (byte)((stream[idx] & 0b11100000) >> 5);
            DsmCcFlag = (stream[idx++] & 0b00010000) > 0;

            if (DsmCcFlag)
            {
                var serviceIdLen = stream[idx++];
                ServiceIdRecord = new byte[serviceIdLen];
                for (int i = 0; i < serviceIdLen; i++)
                {
                    ServiceIdRecord[i] = stream[idx++];
                }
            }
        }

    }
}
