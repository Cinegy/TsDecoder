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

using System.Collections.Generic;
using System.Text;

namespace Cinegy.TsDecoder.Descriptors
{
    /// <summary>
    /// A Subtitling Descriptor <see cref="Descriptor"/>.
    /// </summary>
    /// <remarks>
    /// For details please refer to the original documentation,
    /// e.g. <i>ETSI EN 300 468 V1.15.1 (2016-03)</i> or alternate versions.
    /// </remarks>
    public class SubtitlingDescriptor : Descriptor
    {

        public SubtitlingDescriptor(byte[] stream, int start) : base(stream, start)
        {
            var languages = new List<Language>();
            var currentPos = start + 2;
            do
            {
                var lang = new Language
                {
                    Iso639LanguageCode = Encoding.UTF8.GetString(stream, currentPos, 3),
                    SubtitlingType = stream[currentPos + 3],
                    CompositionPageId = (ushort)((stream[currentPos + 4] << 8) + stream[currentPos + 5]),
                    AncillaryPageId = (ushort)((stream[currentPos + 6] << 8) + stream[currentPos + 7])
                };

                languages.Add(lang);

                currentPos += 8;

            } while (currentPos < start + 2 + DescriptorLength);
            Languages = languages;
        }

        public class Language
        {
            public string Iso639LanguageCode { get; internal set; }
            public byte SubtitlingType { get; internal set; }
            public ushort CompositionPageId { get; internal set; }
            public ushort AncillaryPageId { get; internal set; }
        }

        public IEnumerable<Language> Languages { get; }
    }
}
