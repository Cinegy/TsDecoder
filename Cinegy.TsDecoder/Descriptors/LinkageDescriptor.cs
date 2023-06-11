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
    /// A Linkage Descriptor <see cref="Descriptor"/>.
    /// </summary>
    /// <remarks>
    /// For details please refer to the original documentation,
    /// e.g. <i>ETSI EN 300 468 V1.15.1 (2016-03) Table 57 </i> or alternate versions.
    /// </remarks>
    public class LinkageDescriptor : Descriptor
    {
        public LinkageDescriptor(byte[] stream, int start) : base(stream, start)
        {
            try
            {
                TransportStreamId = (ushort)((Data[0] << 8) + Data[1]);

                OriginalNetworkId = (ushort)((Data[2] << 8) + Data[3]);

                ServiceId = (ushort)((Data[4] << 8) + Data[5]);

                LinkageType = Data[6];

                //TODO: rest of message
            }
            catch (IndexOutOfRangeException)
            {
                throw new ArgumentOutOfRangeException("The Linkage Descriptor Message is short!");
            }

        }

        public ushort TransportStreamId { get; }
        public ushort OriginalNetworkId { get; }
        public ushort ServiceId { get; }
        public byte LinkageType { get; }
    }
}
