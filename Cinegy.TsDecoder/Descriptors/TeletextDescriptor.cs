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
    /// A TeleText Descriptor <see cref="Descriptor"/>.
    /// </summary>
    /// <remarks>
    /// For details please refer to the original documentation,
    /// e.g. <i>ETSI EN 300 468 V1.15.1 (2016-03)</i> or alternate versions.
    /// </remarks>
    public class TeletextDescriptor : Descriptor
    {
        public static Dictionary<byte, string> TeletextTypes = new Dictionary<byte, string>()
        {
            {0x00, "reserved for future use"},
            {0x01, "initial Teletext page"},
            {0x02, "Teletext subtitle page"},
            {0x03, "additional information page"},
            {0x04, "programme schedule page"},
            {0x05, "Teletext subtitle page for hearing impaired people"},
            {0x07, "reserved for future use"},
            {0x08, "reserved for future use"},
            {0x09, "reserved for future use"},
            {0x0A, "reserved for future use"},
            {0x0B, "reserved for future use"},
            {0x0C, "reserved for future use"},
            {0x0D, "reserved for future use"},
            {0x0E, "reserved for future use"},
            {0x0F, "reserved for future use"},
            {0x10, "reserved for future use"},
            {0x11, "reserved for future use"},
            {0x12, "reserved for future use"},
            {0x13, "reserved for future use"},
            {0x14, "reserved for future use"},
            {0x15, "reserved for future use"},
            {0x16, "reserved for future use"},
            {0x17, "reserved for future use"},
            {0x18, "reserved for future use"},
            {0x19, "reserved for future use"},
            {0x1A, "reserved for future use"},
            {0x1B, "reserved for future use"},
            {0x1C, "reserved for future use"},
            {0x1D, "reserved for future use"},
            {0x1E, "reserved for future use"},
            {0x1F, "reserved for future use"}
        };


        public TeletextDescriptor(byte[] stream, int start)
            : base(stream, start)
        {
            var languages = new List<Language>();
            var currentPos = start + 2;
            do
            {
                var lang = new Language
                {
                    Iso639LanguageCode = Encoding.UTF8.GetString(stream, currentPos, 3),
                    TeletextType = (byte)(stream[currentPos + 3] >> 3 & 0x01f),
                    TeletextMagazineNumber = (byte)(stream[currentPos + 3] & 0x7),
                    TeletextPageNumber = stream[currentPos + 4]
                };

                languages.Add(lang);

                currentPos += 5;

            } while (currentPos < start + 2 + DescriptorLength);
            Languages = languages;
        }

        public class Language
        {
            public Language()
            {
            }

            public Language(Language lang)
            {
                Iso639LanguageCode = lang.Iso639LanguageCode;
                TeletextType = lang.TeletextType;
                TeletextMagazineNumber = lang.TeletextMagazineNumber;
                TeletextPageNumber = lang.TeletextPageNumber;
            }

            public string Iso639LanguageCode { get; internal set; }
            public byte TeletextType { get; internal set; }
            public byte TeletextMagazineNumber { get; internal set; }
            public byte TeletextPageNumber { get; internal set; }
        }

        public IEnumerable<Language> Languages { get; }
    }
}
