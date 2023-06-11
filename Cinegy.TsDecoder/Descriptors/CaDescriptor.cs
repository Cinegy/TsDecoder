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
    /// <summary>
    /// A CA Descriptor <see cref="Descriptor"/>.
    /// </summary>
    /// <remarks>
    /// For details please refer to the original documentation,
    /// e.g. <i>ETSI EN 300 468 V1.15.1 (2016-03)</i> or alternate versions.
    /// </remarks>
    public class CaDescriptor : Descriptor
    {
        public CaDescriptor(byte[] stream, int start) : base(stream, start)
        {
            SystemIdentifier = stream[start + 2] << 8 + stream[start + 3];
            CaPid = (stream[start + 4] & 0x1f) << 8 + stream[start + 4];
        }

        public int CaPid { get; }
        public int SystemIdentifier { get; }

    }
}
