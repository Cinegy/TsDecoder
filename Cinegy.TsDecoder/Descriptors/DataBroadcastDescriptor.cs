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
    /// A Data Broadcast Descriptor <see cref="Descriptor"/>.
    /// </summary>
    /// <remarks>
    /// For details please refer to the original documentation,
    /// e.g. <i>ETSI EN 300 468 V1.15.1 (2016-03)</i> or alternate versions.
    /// </remarks>
    public class DataBroadcastDescriptor : Descriptor
    {
        public DataBroadcastDescriptor(byte[] stream, int start) : base(stream, start)
        {
            var lastIndex = start + 2;

            try
            {
                if (DescriptorLength != 0)
                {
                    DataBroadcastId = (stream[lastIndex] << 8) + stream[lastIndex + 1];
                    lastIndex += 2;

                    ComponentTag = stream[lastIndex];
                    lastIndex++;

                    int selectorLength = stream[lastIndex];
                    lastIndex++;

                    if (selectorLength != 0)
                    {
                        Buffer.BlockCopy(stream, lastIndex, SelectorBytes, 0, selectorLength);
                        lastIndex += selectorLength;
                    }

                    LanguageCode = Encoding.UTF8.GetString(stream, lastIndex, 3);
                    lastIndex += 3;
                }

                int textLength = stream[lastIndex];
                lastIndex++;

                if (textLength != 0)
                {
                    TextDescription = Encoding.UTF8.GetString(stream, lastIndex, textLength);
                    lastIndex += textLength;
                }
            }
            catch (IndexOutOfRangeException)
            {
                throw new ArgumentOutOfRangeException("The Data Broadcast Descriptor message is short");
            }
        }

        public int DataBroadcastId { get; private set; }
        public int ComponentTag { get; private set; }
        public string LanguageCode { get; private set; }
        public string TextDescription { get; private set; }
        public byte[] SelectorBytes { get; private set; }
    }
}
