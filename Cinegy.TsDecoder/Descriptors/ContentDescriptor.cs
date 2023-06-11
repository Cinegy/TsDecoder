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
using System.Collections.ObjectModel;

namespace Cinegy.TsDecoder.Descriptors
{
    /// <summary>
    /// A Content Descriptor <see cref="Descriptor"/>.
    /// </summary>
    /// <remarks>
    /// For details please refer to the original documentation,
    /// e.g. <i>ETSI EN 300 468 V1.15.1 (2016-03)</i> or alternate versions.
    /// </remarks>
    public class ContentDescriptor : Descriptor
    {
        public static string GetConentNibble2Description(byte contentnibble1, byte contentnibble2)
        {
            switch (contentnibble1)
            {
                case 0x0:
                    if (contentnibble2 >= 0x0 || contentnibble2 <= 0xF)
                    {
                        return "undefined content";

                    }
                    break;
                case 0x1:
                    switch (contentnibble2)
                    {
                        case 0x00:
                            return "movie/drama (general)";
                        case 0x01:
                            return "detective/thriller";
                        case 0x02:
                            return "adventure/western/war";
                        case 0x03:
                            return "science fiction/fantasy/horror";
                        case 0x04:
                            return "comedy";
                        case 0x05:
                            return "soap/melodrama/folkloric";
                        case 0x06:
                            return "romance";
                        case 0x07:
                            return "serious/classical/religious/historical movie/drama";
                        case 0x08:
                            return "adult movie/drama";
                        case 0x09:
                            return "reserved for future use";
                        case 0x0A:
                            return "reserved for future use";
                        case 0x0B:
                            return "reserved for future use";
                        case 0x0C:
                            return "reserved for future use";
                        case 0x0D:
                            return "reserved for future use";
                        case 0x0E:
                            return "reserved for future use";
                        case 0x0F:
                            return "user defined";
                    }
                    break;
                case 0x2:
                    switch (contentnibble2)
                    {
                        case 0x00:
                            return "news/current affairs (general)";
                        case 0x01:
                            return "news/weather report";
                        case 0x02:
                            return "news magazine";
                        case 0x03:
                            return "documentary";
                        case 0x04:
                            return "discussion/interview/debate";
                        case 0x05:
                            return "reserved for future use";
                        case 0x06:
                            return "reserved for future use";
                        case 0x07:
                            return "reserved for future use";
                        case 0x08:
                            return "reserved for future use";
                        case 0x09:
                            return "reserved for future use";
                        case 0x0A:
                            return "reserved for future use";
                        case 0x0B:
                            return "reserved for future use";
                        case 0x0C:
                            return "reserved for future use";
                        case 0x0D:
                            return "reserved for future use";
                        case 0x0E:
                            return "reserved for future use";
                        case 0xF:
                            return "user defined";
                    }
                    break;
                case 0x3:
                    switch (contentnibble2)
                    {
                        case 0x00:
                            return "show/game show (general)";
                        case 0x01:
                            return "game show/quiz/contest";
                        case 0x02:
                            return "variety show";
                        case 0x03:
                            return "talk show";
                        case 0x04:
                            return "reserved for future use";
                        case 0x05:
                            return "reserved for future use";
                        case 0x06:
                            return "reserved for future use";
                        case 0x07:
                            return "reserved for future use";
                        case 0x08:
                            return "reserved for future use";
                        case 0x09:
                            return "reserved for future use";
                        case 0x0A:
                            return "reserved for future use";
                        case 0x0B:
                            return "reserved for future use";
                        case 0x0C:
                            return "reserved for future use";
                        case 0x0D:
                            return "reserved for future use";
                        case 0x0E:
                            return "reserved for future use";
                        case 0xF:
                            return "user defined";
                    }
                    break;
                case 0x4:
                    switch (contentnibble2)
                    {
                        case 0x00:
                            return "sports (general)";
                        case 0x01:
                            return "special events (Olympic Games, World Cup, etc.)";
                        case 0x02:
                            return "sports magazines";
                        case 0x03:
                            return "football/soccer";
                        case 0x04:
                            return "tennis/squash";
                        case 0x05:
                            return "team sports (excluding football)";
                        case 0x06:
                            return "athletics";
                        case 0x07:
                            return "motor sport";
                        case 0x08:
                            return "water sport";
                        case 0x09:
                            return "winter sports";
                        case 0x0A:
                            return "equestrian";
                        case 0x0B:
                            return "martial sports";
                        case 0x0C:
                            return "reserved for future use";
                        case 0x0D:
                            return "reserved for future use";
                        case 0x0E:
                            return "reserved for future use";
                        case 0xF:
                            return "user defined";
                    }
                    break;
                case 0x5:
                    switch (contentnibble2)
                    {
                        case 0x00:
                            return "children's/youth programmes (general)";
                        case 0x01:
                            return "pre-school children's programmes";
                        case 0x02:
                            return "entertainment programmes for 6 to14";
                        case 0x03:
                            return "entertainment programmes for 10 to 16";
                        case 0x04:
                            return "informational/educational/school programmes";
                        case 0x05:
                            return "cartoons/puppets";
                        case 0x06:
                            return "reserved for future use";
                        case 0x07:
                            return "reserved for future use";
                        case 0x08:
                            return "reserved for future use";
                        case 0x09:
                            return "reserved for future use";
                        case 0x0A:
                            return "reserved for future use";
                        case 0x0B:
                            return "reserved for future use";
                        case 0x0C:
                            return "reserved for future use";
                        case 0x0D:
                            return "reserved for future use";
                        case 0x0E:
                            return "reserved for future use";
                        case 0xF:
                            return "user defined";
                    }
                    break;
                case 0x6:
                    switch (contentnibble2)
                    {
                        case 0x00:
                            return "music/ballet/dance (general)";
                        case 0x01:
                            return "rock/pop";
                        case 0x02:
                            return "serious music/classical music";
                        case 0x03:
                            return "folk/traditional music";
                        case 0x04:
                            return "jazz";
                        case 0x05:
                            return "musical/opera";
                        case 0x06:
                            return "ballet";
                        case 0x07:
                            return "reserved for future use";
                        case 0x08:
                            return "reserved for future use";
                        case 0x09:
                            return "reserved for future use";
                        case 0x0A:
                            return "reserved for future use";
                        case 0x0B:
                            return "reserved for future use";
                        case 0x0C:
                            return "reserved for future use";
                        case 0x0D:
                            return "reserved for future use";
                        case 0x0E:
                            return "reserved for future use";
                        case 0xF:
                            return "user defined";
                    }
                    break;
                case 0x7:
                    switch (contentnibble2)
                    {
                        case 0x00:
                            return "arts/culture (without music, general)";
                        case 0x01:
                            return "performing arts";
                        case 0x02:
                            return "fine arts";
                        case 0x03:
                            return "religion";
                        case 0x04:
                            return "popular culture/traditional arts";
                        case 0x05:
                            return "literature";
                        case 0x06:
                            return "film/cinema";
                        case 0x07:
                            return "experimental film/video";
                        case 0x08:
                            return "broadcasting/press";
                        case 0x09:
                            return "new media";
                        case 0x0A:
                            return "arts/culture magazines";
                        case 0x0B:
                            return "fashion";
                        case 0x0C:
                            return "reserved for future use";
                        case 0x0D:
                            return "reserved for future use";
                        case 0x0E:
                            return "reserved for future use";
                        case 0xF:
                            return "user defined";
                    }
                    break;
                case 0x8:
                    switch (contentnibble2)
                    {
                        case 0x00:
                            return "social/political issues/economics (general)";
                        case 0x01:
                            return "magazines/reports/documentary";
                        case 0x02:
                            return "economics/social advisory";
                        case 0x03:
                            return "remarkable people";
                        case 0x04:
                            return "reserved for future use";
                        case 0x05:
                            return "reserved for future use";
                        case 0x06:
                            return "reserved for future use";
                        case 0x07:
                            return "reserved for future use";
                        case 0x08:
                            return "reserved for future use";
                        case 0x09:
                            return "reserved for future use";
                        case 0x0A:
                            return "reserved for future use";
                        case 0x0B:
                            return "reserved for future use";
                        case 0x0C:
                            return "reserved for future use";
                        case 0x0D:
                            return "reserved for future use";
                        case 0x0E:
                            return "reserved for future use";
                        case 0xF:
                            return "user defined";
                    }
                    break;
                case 0x9:
                    switch (contentnibble2)
                    {
                        case 0x00:
                            return "education/science/factual topics (general)";
                        case 0x01:
                            return "nature/animals/environment";
                        case 0x02:
                            return "technology/natural sciences";
                        case 0x03:
                            return "medicine/physiology/psychology";
                        case 0x04:
                            return "foreign countries/expeditions";
                        case 0x05:
                            return "social/spiritual sciences";
                        case 0x06:
                            return "further education";
                        case 0x07:
                            return "languages";
                        case 0x08:
                            return "reserved for future use";
                        case 0x09:
                            return "reserved for future use";
                        case 0x0A:
                            return "reserved for future use";
                        case 0x0B:
                            return "reserved for future use";
                        case 0x0C:
                            return "reserved for future use";
                        case 0x0D:
                            return "reserved for future use";
                        case 0x0E:
                            return "reserved for future use";
                        case 0xF:
                            return "user defined";
                    }
                    break;
                case 0xA:
                    switch (contentnibble2)
                    {
                        case 0x00:
                            return "leisure hobbies (general)";
                        case 0x01:
                            return "tourism/travel";
                        case 0x02:
                            return "handicraft";
                        case 0x03:
                            return "motoring";
                        case 0x04:
                            return "fitness and health";
                        case 0x05:
                            return "cooking";
                        case 0x06:
                            return "advertisement/shopping";
                        case 0x07:
                            return "gardening";
                        case 0x08:
                            return "reserved for future use";
                        case 0x09:
                            return "reserved for future use";
                        case 0x0A:
                            return "reserved for future use";
                        case 0x0B:
                            return "reserved for future use";
                        case 0x0C:
                            return "reserved for future use";
                        case 0x0D:
                            return "reserved for future use";
                        case 0x0E:
                            return "reserved for future use";
                        case 0xF:
                            return "user defined";
                    }
                    break;
                case 0xB:
                    switch (contentnibble2)
                    {
                        case 0x00:
                            return "original language";
                        case 0x01:
                            return "black and white";
                        case 0x02:
                            return "unpublished";
                        case 0x03:
                            return "live broadcast";
                        case 0x04:
                            return "plano-stereoscopic";
                        case 0x05:
                            return "local or regional";
                        case 0x06:
                            return "reserved for future use";
                        case 0x07:
                            return "reserved for future use";
                        case 0x08:
                            return "reserved for future use";
                        case 0x09:
                            return "reserved for future use";
                        case 0x0A:
                            return "reserved for future use";
                        case 0x0B:
                            return "reserved for future use";
                        case 0x0C:
                            return "reserved for future use";
                        case 0x0D:
                            return "reserved for future use";
                        case 0x0E:
                            return "reserved for future use";
                        case 0xF:
                            return "user defined";
                    }
                    break;
                case 0xF:
                    if (contentnibble2 >= 0x0 || contentnibble2 <= 0xF)
                    {
                        return "user defined";
                    }
                    break;
            }
            if ((contentnibble1 >= 0xC || contentnibble1 <= 0xE) && (contentnibble2 >= 0x0 || contentnibble2 <= 0xF))
            {
                return "reserved for future use";
            }
            return "";

        }
        public static string GetConentNibble1Description(byte contentnibble1)
        {
            switch (contentnibble1)
            {
                case 0x0: return "undefined content";
                case 0x1: return "Movie/Drama:";
                case 0x2: return "News/Current affairs:";
                case 0x3: return "Show/Game show:";
                case 0x4: return "Sports:";
                case 0x5: return "Children's/Youth programmes:";
                case 0x6: return "Music/Ballet/Dance:";
                case 0x7: return "Arts/Culture (without music)::";
                case 0x8: return "Social/Political issues/Economics:";
                case 0x9: return "Education/Science/Factual topics:";
                case 0xA: return "Leisure hobbies:";
                case 0xB: return "Special characteristics:";
                case 0xF: return "user defined";
                default: return "reserved for future use:";
            }

        }
        public ContentDescriptor(byte[] stream, int start) : base(stream, start)
        {
            var lastindex = start + 2;
            var length = DescriptorLength;
            while (length > 0)
            {
                if (lastindex + 2 < stream.Length)
                {
                    //corrupt descriptor
                    ContentTypes = new List<ContentType>();
                    return;
                }

                try
                {
                    var contentnibble1 = stream[lastindex] >> 4;
                    var contentnibble2 = stream[lastindex] & 0x0f;
                    lastindex++;
                    var userdefined = (int)stream[lastindex];
                    lastindex++;
                    if (ContentTypes == null)
                    {
                        ContentTypes = new Collection<ContentType>();
                    }
                    ContentTypes.Add(new ContentType(contentnibble1, contentnibble2, userdefined));
                    length -= 2;
                }
                catch (IndexOutOfRangeException)
                {
                    throw new ArgumentOutOfRangeException("The Content Descriptor message is to short!");
                }
            }

        }

        public ICollection<ContentType> ContentTypes { get; }
        
        public class ContentType
        {
            private int contentnibble1;
            private int contentnibble2;
            private int userdefined;

            public ContentType(int contentnibble1, int contentnibble2, int userdefined)
            {
                this.contentnibble1 = contentnibble1;
                this.contentnibble2 = contentnibble2;
                this.userdefined = userdefined;
            }
        }
    }
}
