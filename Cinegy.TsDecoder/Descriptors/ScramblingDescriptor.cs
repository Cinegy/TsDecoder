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
    /// A Scrambling Descriptor <see cref="Descriptor"/>.
    /// </summary>
    /// <remarks>
    /// For details please refer to the original documentation,
    /// e.g. <i>ETSI EN 300 468 V1.15.1 (2016-03) Table 84</i> or alternate versions.
    /// </remarks>
    public class ScramblingDescriptor : Descriptor
    {
        public ScramblingDescriptor(byte[] stream, int start) : base(stream, start)
        {

            var lastIndex = start + 2;

            try
            {
                if (DescriptorLength != 0)
                {
                    ScramblingMode = (stream[lastIndex + 0] << 8) + stream[lastIndex + 1];
                    lastIndex += 4;
                }
            }
            catch (IndexOutOfRangeException)
            {
                throw new ArgumentOutOfRangeException("The Scrambling Descriptor message is short");
            }
        }

        public int ScramblingMode { get; private set; }

    }
}
