/* Copyright 2017 Cinegy GmbH.

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

namespace Cinegy.TsDecoder.TransportStream
{
    public class Descriptor
    {
        public Descriptor(byte[] stream, int start)
        {
            DescriptorTag = stream[start];
            DescriptorLength = stream[start + 1];
        }
        public byte DescriptorTag { get; }
        public byte DescriptorLength { get; }
    }

    public class TeletextDescriptor : Descriptor
    {
        public static Dictionary<byte, string> TeletextTypes = new Dictionary<byte, string>(){
            {0x00 , "reserved for future use"},
            {0x01 ,  "initial Teletext page"},
            {0x02 ,  "Teletext subtitle page"},
            {0x03 ,  "additional information page"},
            {0x04 ,  "programme schedule page"},
            {0x05 ,  "Teletext subtitle page for hearing impaired people"},
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
                var lang = new Language();
                lang.Iso639LanguageCode = Encoding.UTF8.GetString(stream, currentPos, 3);
                // lang.TeletextType = (byte)(stream[current_pos + 3] & 0x1f);
                lang.TeletextType = (byte)((stream[currentPos + 3] >> 3) & 0x01f);
                lang.TeletextMagazineNumber = (byte)((stream[currentPos + 3]) & 0x7);
                //lang.TeletextMagazineNumber = (byte)((stream[current_pos + 3] >> 5) & 0x7);
                lang.TeletextPageNumber = stream[currentPos + 4];

                languages.Add(lang);

                currentPos += 5;

            } while (currentPos < start + 2 + DescriptorLength);
            Languages = languages;
        }
        public class Language
        {
            public Language() { }
            public Language(Language lang)
            {
                Iso639LanguageCode = lang.Iso639LanguageCode;
                TeletextType = lang.TeletextType;
                TeletextMagazineNumber = lang.TeletextMagazineNumber;
                TeletextPageNumber = lang.TeletextPageNumber;
            }

            public string Iso639LanguageCode { get; set; }
            public byte TeletextType { get; set; }
            public byte TeletextMagazineNumber { get; set; }
            public byte TeletextPageNumber { get; set; }
        }
        public IEnumerable<Language> Languages { get; set; }        
    }

    public class RegistrationDescriptor : Descriptor
    {
        public RegistrationDescriptor(byte[] stream, int start) : base(stream, start)
        {
            if ((stream.Length - start - 2) > DescriptorLength)
            {
                Organization = Encoding.UTF8.GetString(stream, start + 2, DescriptorLength);
            }
        }
        public string Organization { get; set; }
    }

    public class StreamIdentifierDescriptor : Descriptor
    {
        public StreamIdentifierDescriptor(byte[] stream, int start) : base(stream, start)
        {
            ComponentTag = stream[start + 2];
        }

        public byte ComponentTag { get; set; }
    }

    public class Iso639LanguageDescriptor : Descriptor
    {
        public Iso639LanguageDescriptor(byte[] stream, int start) : base(stream, start)
        {
            Language = Encoding.UTF8.GetString(stream, start + 2, 3);
            AudioType = stream[start + 5];
        }

        public string Language { get; set; }
        public byte AudioType { get; set; }
    }

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
                    So639LanguageCode = Encoding.UTF8.GetString(stream, currentPos, 3),
                    SubtitlingType = stream[currentPos + 3],
                    CompositionPageId = (ushort) ((stream[currentPos + 4] << 8) + stream[currentPos + 5]),
                    AncillaryPageId = (ushort) ((stream[currentPos + 6] << 8) + stream[currentPos + 7])
                };

                languages.Add(lang);

                currentPos += 8;

            } while (currentPos < start + 2 + DescriptorLength);
            Languages = languages;
        }
        public class Language
        {
            public string So639LanguageCode { get; set; }
            public byte SubtitlingType { get; set; }
            public ushort CompositionPageId { get; set; }
            public ushort AncillaryPageId { get; set; }
        }
        public IEnumerable<Language> Languages { get; set; }
    }

    public class DataBroadcastIdDescriptor : Descriptor
    {
        public DataBroadcastIdDescriptor(byte[] stream, int start) : base(stream, start)
        {
            DataBroadcastId = (ushort)((stream[start + 2] << 8) + stream[start + 3]);
        }

        public ushort DataBroadcastId { get; set; }
    }

    public class ServiceListDescriptor : Descriptor
    {
        public static string ServiceTypeDescription(byte serviceType)
        {
            if (serviceType <= 0x0C)
            {
                switch (serviceType)
                {
                    case 0x00: return "reserved for future use";
                    case 0x01: return "digital television service";
                    case 0x02: return "digital radio sound service";
                    case 0x03: return "teletext service";
                    case 0x04: return "NVOD reference service";
                    case 0x05: return "NVOD time-shifted service";
                    case 0x06: return "mosaic service";
                    case 0x07: return "PAL coded signal";
                    case 0x08: return "SECAM coded signal";
                    case 0x09: return "D/D2-MAC";
                    case 0x0A: return "FM Radio";
                    case 0x0B: return "NTSC coded signal";
                    case 0x0C: return "data broadcast service";
                }
            }
            else if (serviceType >= 0x0D && serviceType <= 0x7F)
            {
                return "reserved for future use";
            }
            else if (serviceType >= 0x80 && serviceType <= 0xFE)
            {
                return "user defined";
            }

            return "Forbidden";
        }

        public class Service
        {
            public Service() { }
            public Service(Service service)
            {
                ServiceId = service.ServiceId;
                ServiceType = service.ServiceType;
            }

            public ushort ServiceId { get; set; }
            public byte ServiceType { get; set; }
            public string ServiceTypeString => ServiceTypeDescription(ServiceType);
        }
        
        public ServiceListDescriptor(byte[] stream, int start) : base(stream, start)
        {
            var services = new List<Service>();
            var startOfNextBlock = (ushort)(start + 2);
            while (startOfNextBlock < (start + DescriptorLength + 2))
            {
                var service = new Service();

                service.ServiceId = (ushort)((stream[startOfNextBlock] << 8) + stream[startOfNextBlock + 1]);
                service.ServiceType = stream[startOfNextBlock + 2];

                startOfNextBlock += 3;

                services.Add(service);
            }
            Services = services;
        }
        public IEnumerable<Service> Services { get; set; }

    }

    public class ServiceDescriptor : Descriptor
    {        
        public static string GetServiceTypeDescription(byte serviceType)
        {
            switch (serviceType)
            {
                case 0x00: return "reserved for future use";
                case 0x01: return "digital television service (see note 1)";
                case 0x02: return "digital radio sound service (see note 2)";
                case 0x03: return "Teletext service";
                case 0x04: return "NVOD reference service (see note 1)";
                case 0x05: return "NVOD time-shifted service (see note 1)";
                case 0x06: return "mosaic service";
                case 0x07: return "FM radio service";
                case 0x08: return "DVB SRM service [49]";
                case 0x09: return "reserved for future use";
                case 0x0A: return "advanced codec digital radio sound service";
                case 0x0B: return "H.264/AVC mosaic service";
                case 0x0C: return "data broadcast service";
                case 0x0D: return "reserved for Common Interface Usage (EN 50221[37])";
                case 0x0E: return "RCS Map (see EN301790[7])";
                case 0x0F: return "RCS FLS (see EN301790[7])";
                case 0x10: return "DVB MHP service 0x11 MPEG-2 HD digital television service";
                case 0x16: return "H.264/AVC SD digital television service";
                case 0x17: return "H.264/AVC SD NVOD time-shifted service";
                case 0x18: return "H.264/AVC SD NVOD reference service";
                case 0x19: return "H.264/AVC HD digital television service";
                case 0x1A: return "H.264/AVC HD NVOD time-shifted service";
                case 0x1B: return "H.264/AVC HD NVOD reference service";
                case 0x1C: return "H.264/AVC frame compatible plano-stereoscopic HD digital television service (see note 3)";
                case 0x1D: return "H.264/AVC frame compatible plano-stereoscopic HD NVOD time-shifted service (see note 3)";
                case 0x1E: return "H.264/AVC frame compatible plano-stereoscopic HD NVOD reference service (see note 3)";
                case 0x1F: return "HEVC digital television service";
                case 0xFF: return "reserved for future use";
            }
            if (serviceType >= 0x20 || serviceType <= 0x7F)
            {
                return "reserved for future use";
            }
            else if (serviceType >= 0x80 || serviceType <= 0xFE)
            {
                return "user defined";
            }
            else if (serviceType >= 0x12 || serviceType <= 0x15)
            {
                return "reserved for future use";
            }
            return "unknown";
        }

        public ServiceDescriptor(byte[] stream, int start) : base(stream, start)
        {
            ServiceType = stream[start + 2];
            ServiceProviderNameLength = stream[start + 3];
            ServiceProviderName = new Text(stream, start + 4, ServiceProviderNameLength); // new Text(System.Text.Encoding.UTF8.GetString(stream, start + 4, ServiceProviderNameLength));
            ServiceNameLength = stream[start + 4 + ServiceProviderNameLength];
            ServiceName = new Text(stream, start + 4 + ServiceProviderNameLength + 1, ServiceNameLength);//new Text(System.Text.Encoding.UTF8.GetString(stream, start + 4 + ServiceProviderNameLength + 1, ServiceNameLength));
        }
        public byte ServiceType { get; set; }//8 uimsbf
        public string ServiceTypeDescription => GetServiceTypeDescription(ServiceType);
        public byte ServiceProviderNameLength { get; set; }// 8 uimsbf
        public Text ServiceProviderName { get; set; }// 
        public byte ServiceNameLength { get; set; }// 8 uimsbf 
        public Text ServiceName { get; set; }      
    }

    public class ExtendedEventDescriptor : Descriptor
    {
        public class Item
        {
            public Item() { }
            public Item(Item item)
            {
                this.ItemDescriptionLength = item.ItemDescriptionLength;
                this.ItemDescriptionChar = new Text(item.ItemDescriptionChar);
                this.ItemLength = item.ItemLength;
                this.ItemChar = new Text(item.ItemChar);
            }
            public byte ItemDescriptionLength { get; set; }
            public Text ItemDescriptionChar { get; set; }
            public byte ItemLength { get; set; }
            public Text ItemChar { get; set; }
        }
        
        public ExtendedEventDescriptor(byte[] stream, int start) : base(stream, start)
        {
            DescriptorNumber = (byte)((stream[start + 2] >> 4) & 0x0F);
            LastDescriptorNumber = (byte)((stream[start + 2]) & 0x0F);
            ISO639LanguageCode = System.Text.Encoding.UTF8.GetString(stream, start + 3, 3);
            LengthOfItems = stream[start + 6];
            ushort startOfItem = (ushort)(start + 7);
            List<Item> items = new List<Item>();
            while (startOfItem < (start + 7 + LengthOfItems))
            {
                Item item = new Item();

                item.ItemDescriptionLength = stream[startOfItem];
                item.ItemDescriptionChar = new Text(stream, startOfItem + 1, item.ItemDescriptionLength);
                item.ItemLength = stream[startOfItem + 1 + item.ItemDescriptionLength];
                item.ItemChar = new Text(stream, startOfItem + 1 + item.ItemDescriptionLength + 1, item.ItemLength);
                startOfItem = (ushort)(startOfItem + 1 + item.ItemDescriptionLength + 1 + item.ItemLength);
                items.Add(item);
            }
            Items = items;
            TextLength = stream[startOfItem];
            TextChar = new Text(stream, startOfItem + 1, TextLength);
        }

        public byte DescriptorNumber { get; set; }
        public byte LastDescriptorNumber { get; set; }
        public System.String ISO639LanguageCode { get; set; }
        public byte LengthOfItems { get; set; }
        public IEnumerable<Item> Items { get; set; }
        public byte TextLength { get; set; }
        public Text TextChar { get; set; }
    }

    public class ShortEventDescriptor : Descriptor
    {       
        public ShortEventDescriptor(byte[] stream, int start) : base(stream, start)
        {
            ISO639LanguageCode = System.Text.Encoding.UTF8.GetString(stream, start + 2, 3);
            EventNameLength = stream[start + 5];
            EventNameChar = new Text(stream, start + 6, EventNameLength);
            TextLength = stream[start + 6 + EventNameLength];
            TextChar = new Text(stream, start + 6 + EventNameLength + 1, TextLength); 
        }
        public System.String ISO639LanguageCode { get; set; }
        public byte EventNameLength { get; set; }     
        public Text EventNameChar { get; set; }     
        public byte TextLength { get; set; }     
        public Text TextChar { get; set; }   
    }

    public class ComponentDescriptor : Descriptor
    {
        public static System.String GetComponentDescription(byte streamContent, byte streamContentExt, byte componentType)
        {
            switch (streamContent)
            {
                case 0x00: return "reserved for future us";
                case 0x01:
                    switch (componentType)
                    {
                        case 0x00: return "reserved for future use";
                        case 0x01: return "MPEG-2 video, 4:3 aspect ratio, 25 Hz (see note 2)";
                        case 0x02: return "MPEG-2 video, 16:9 aspect ratio with pan vectors, 25 Hz (see note 2)";
                        case 0x03: return "MPEG-2 video, 16:9 aspect ratio without pan vectors, 25 Hz (see note 2)";
                        case 0x04: return "MPEG-2 video, > 16:9 aspect ratio, 25 Hz (see note 2)";
                        case 0x05: return "MPEG-2 video, 4:3 aspect ratio, 30 Hz (see note 2)";
                        case 0x06: return "MPEG-2 video, 16:9 aspect ratio with pan vectors, 30 Hz (see note 2)";
                        case 0x07: return "MPEG-2 video, 16:9 aspect ratio without pan vectors, 30 Hz (see note 2)";
                        case 0x08: return "MPEG-2 video, > 16:9 aspect ratio, 30 Hz(see note 2)";
                        case 0x09: return "MPEG-2 high definition video, 4:3 aspect ratio, 25 Hz (see note 2)";
                        case 0x0A: return "MPEG-2 high definition video, 16:9 aspect ratio with pan vectors, 25 Hz (see note 2)";
                        case 0x0B: return "MPEG-2 high definition video, 16:9 aspect ratio without pan vectors, 25 Hz (see note 2)";
                        case 0x0C: return "MPEG-2 high definition video, > 16:9 aspect ratio, 25 Hz (see note 2)";
                        case 0x0D: return "MPEG-2 high definition video, 4:3 aspect ratio, 30 Hz (see note 2)";
                        case 0x0E: return "MPEG-2 high definition video, 16:9 aspect ratio with pan vectors, 30 Hz (see note 2)";
                        case 0x0F: return "MPEG-2 high definition video, 16:9 aspect ratio without pan vectors, 30 Hz (see note 2)";
                        case 0x10: return "MPEG-2 high definition video, > 16:9 aspect ratio, 30 Hz (see note 2)0x11 to 0xAFreserved for future use";
                        case 0xFF: return "reserved for future use";
                        default: return "user defined";
                    }
                case 0x02:
                    switch (componentType)
                    {
                        case 0x00: return "reserved for future use";
                        case 0x01: return "MPEG-1 Layer 2 audio, single mono channel";
                        case 0x02: return "MPEG-1 Layer 2 audio, dual mono channel";
                        case 0x03: return "MPEG-1 Layer 2 audio, stereo (2 channel)";
                        case 0x04: return "MPEG-1 Layer 2 audio, multi-lingual, multi-channel";
                        case 0x05: return "MPEG-1 Layer 2 audio, surround sound";
                        case 0x40: return "MPEG-1 Layer 2 audio description for the visually impaired (see note 5)";
                        case 0x41: return "MPEG-1 Layer 2 audio for the hard of hearing";
                        case 0x42: return "receiver-mix supplementary audio as per annex E of TS 101 154 [9]";
                        case 0x47: return "MPEG-1 Layer 2 audio, receiver-mix audio description";
                        case 0x48: return "MPEG-1 Layer 2 audio, broadcast-mix audio description";
                        case 0xFF: return "reserved for future use";
                    }
                    if (componentType >= 0x43 || componentType <= 0x46)
                    {
                        return "reserved for future use";
                    }
                    else if (componentType >= 0x06 || componentType <= 0x3F)
                    {
                        return "reserved for future use";
                    }
                    else if (componentType >= 0x49 || componentType <= 0xAF)
                    {
                        return "reserved for future use";
                    }
                    else if (componentType >= 0xB0 || componentType <= 0xFE)
                    {
                        return "user-defined";
                    }
                    break;
                case 0x03:
                    switch (componentType)
                    {
                        case 0x00: return "reserved for future use";
                        case 0x01: return "EBU Teletext subtitles";
                        case 0x02: return "associated EBU Teletext";
                        case 0x03: return "VBI data";
                        case 0x10: return "DVBsubtitles (normal) with no monitor aspect ratio criticality";
                        case 0x11: return "DVB subtitles (normal) for display on 4:3 aspect ratio monitor";
                        case 0x12: return "DVB subtitles (normal) for display on 16:9 aspect ratio monitor";
                        case 0x13: return "DVB subtitles (normal) for display on 2.21:1 aspect ratio monitor";
                        case 0x14: return "DVB subtitles (normal) for display on a high definition monitor";
                        case 0x15: return "DVB subtitles (normal) with plano-stereoscopic disparity for display on a high definition monitor";
                        case 0x20: return "DVB subtitles (for the hard of hearing) with no monitor aspect ratio criticality";
                        case 0x21: return "DVB subtitles (for the hard of hearing) for display on 4:3   aspect ratio monito";
                        case 0x22: return "DVB subtitles (for the hard of hearing) for display on 16:9 aspect ratio monitor";
                        case 0x23: return "DVB subtitles (for the hard of hearing) for display on 2.21:1 aspect ratio monitor";
                        case 0x24: return "DVB subtitles (for the hard of hearing) for display on a high definition monitor";
                        case 0x25: return "DVB subtitles (for the hard of hearing) with plano-stereoscopic disparity for display on a high definition monitor";
                        case 0x30: return "open (in-vision) sign language interpretation for the deaf(see note 7)";
                        case 0x31: return "closed sign language interpretation for the deaf(see note 7)";
                        case 0x40: return "video up-sampled from standard definition source material(see note 7)";
                        case 0x80: return "dependent SAOC-DE data stream";
                        case 0xFF: return "reserved for future use";
                    }
                    if (componentType >= 0xB0 || componentType <= 0xFE)
                    {
                        return "user defined";
                    }
                    return "reserved for future use";
                case 0x04:
                    if (componentType >= 0x00 || componentType <= 0x7F)
                    {
                        return "reserved for AC-3 audio modes (refer to table D.1)";
                    }
                    return "reserved for enhanced AC-3 audio modes (refer to table D.1)";

                case 0x05:
                    switch (componentType)
                    {
                        case 0x00: return "reserved for future use";
                        case 0x01: return "H.264/AVC standard definition video, 4:3 aspect ratio, 25 Hz (see note2)";
                        case 0x02: return "reserved for future use";
                        case 0x03: return "H.264/AVC standard definition video, 16:9 aspect ratio, 25 Hz (see note2)";
                        case 0x04: return "H.264/AVC standard definition video, > 16:9 aspect ratio, 25 Hz (see note 2)";
                        case 0x05: return "H.264/AVC standard definition video, 4:3 aspect ratio, 30 Hz (see note 2)";
                        case 0x06: return "reserved for future use";
                        case 0x07: return "H.264/AVC standard definition video, 16:9 aspect ratio, 30 Hz (see note 2)";
                        case 0x08: return "H.264/AVC standard definition video, > 16:9 aspect ratio, 30 Hz (see note 2)";
                        case 0x0B: return "H.264/AVC high definition video, 16:9 aspect ratio, 25 Hz (see note 2)";
                        case 0x0C: return "H.264/AVC high definition video, > 16:9 aspect ratio, 25 Hz (see note 2)";
                        case 0x0F: return "H.264/AVC high definition video, 16:9 aspect ratio, 30 Hz (see note 2)";
                        case 0x10: return "H.264/AVC high definition video, > 16:9 aspect ratio, 30 Hz (see note 2)";
                        case 0x80: return "H.264/AVC plano-stereoscopic frame compatible high definition video, 16:9 aspect ratio, 25 Hz, Side-by-Side (see notes 2 and 3)";
                        case 0x81: return "H.264/AVC plano-stereoscopic frame compatible high definition video, 16:9 aspect ratio, 25 Hz, Top-and-Bottom (see notes 2 and 3)";
                        case 0x82: return "H.264/AVC plano-stereoscopic frame compatible high definition video, 16:9 aspect ratio, 30 Hz, Side-by-Side (see notes 2, 3 and 4)";
                        case 0x83: return "H.264/AVC stereoscopic frame compatible high definition video, 16:9 aspect ratio, 30 Hz, Top-and-Bottom (see notes 2, 3 and 4)";
                        case 0x84: return "H.264/MVC dependent view, plano-stereoscopic service compatible video (see notes 2, 3 and 4)";
                    }
                    if (componentType >= 0xB0 || componentType <= 0xFE)
                    {
                        return "user defined";
                    }
                    return "reserved for future use";

                case 0x06:
                    switch (componentType)
                    {

                        case 0x00: return "reserved for future use";
                        case 0x01: return "HE AAC audio, single mono channel(see note 6)";
                        case 0x02: return "reserved for future use";
                        case 0x03: return "HE AAC audio, stereo (see note 6)";
                        case 0x04: return "reserved for future use";
                        case 0x05: return "HE AAC audio, surround sound (see note 6)";
                        case 0x40: return "HE AAC audio description for the visually impaired (see notes 5 and 6)";
                        case 0x41: return "HE AAC audio for the hard of hearing (see note 6)";
                        case 0x42: return "HE AAC receiver-mix supplementary audio as per annex E of TS 101 154 [9] (see note 6)";
                        case 0x43: return "HE AACv2 audio, stereo";
                        case 0x44: return "HE AACv2 audio description for the visually impaired (see note 5)";
                        case 0x45: return "HE AACv2 audio for the hard of hearing";
                        case 0x46: return "HE AACv2 receiver-mix supplementary audio as per annex E of TS 101 154[9]";
                        case 0x47: return "HE AAC receiver-mix audio description for the visually impaired";
                        case 0x48: return "HE AAC broadcast-mix audio description for the visually impaired";
                        case 0x49: return "HE AACv2 receiver-mix audio description for the visually impaired";
                        case 0x4A: return "HE AACv2 broadcast-mix audio description for the visually impaired";
                        case 0xA0: return "HE AAC, or HE AAC v2 with SAOC-DE ancillary data (see notes 6 and 7)";
                        case 0xFF: return "reserved for future use";
                    }
                    if (componentType >= 0xB0 || componentType <= 0xFE)
                    {
                        return "user defined";
                    }
                    return "reserved for future use";
                case 0x07:
                    if (componentType >= 0x00 || componentType <= 0x7F)
                    {
                        return "reserved for DTS® and DTS-HD® audio modes (refer to annex G)";
                    }
                    return "reserved for future use";
                case 0x08:
                    if (componentType == 0x00)
                    {
                        return "reserved for future use";
                    }
                    else if (componentType == 0x01)
                    {
                        return "DVB SRM data [49]";
                    }
                    return "reserved for DVB CPCM modes [46], [i.4]";
                case 0x09:
                    switch (streamContentExt)
                    {
                        case 0x00:
                            switch (componentType)
                            {
                                case 0x00: return "HEVC Main Profile high definition video, 50 Hz(note 2)";
                                case 0x01: return "HEVC Main 10 Profile high definition video, 50 Hz(note 2)";
                                case 0x02: return "HEVC Main Profile high definition video, 60 Hz (note 2)";
                                case 0x03: return "HEVC Main 10 Profile high definition video, 60 Hz (note 2)";
                                case 0x04: return "HEVC ultra high definition video (note 2)";
                                default: return "reserved for future use";
                            }
                        default: return "reserved for future use";
                    }
                case 0x0A: return "reserved for future use";
                case 0x0B:
                    switch (streamContentExt)
                    {
                        case 0x0F:
                            switch (componentType)
                            {
                                case 0x00: return "less than 16:9 aspect ratio";
                                case 0x01: return "16:9 aspect ratio";
                                case 0x02: return "greater than 16:9 aspect ratio";
                                default: return "reserved for future use";
                            }
                        default: return "reserved for future use";
                    }
                default: return "user defined";
            }
            return "Unknown";
        }

        public ComponentDescriptor(byte[] stream, int start) : base(stream, start)
        {
            StreamContentExt = (byte)((stream[start + 2] >> 4) & 0x0F);
            StreamContent = (byte)(stream[start + 2] & 0x0F);
            ComponentType = stream[start + 3];
            ComponentTag = stream[start + 4];

            ISO639LanguageCode = System.Text.Encoding.UTF8.GetString(stream, start + 5, 3);
            TextChar = new Text(stream, start + 5 + 3, DescriptorLength + 2 - 5 - 3);
        }


        public byte StreamContentExt { get; set; } 
        public byte StreamContent { get; set; } 
        public byte ComponentType { get; set; } 
        public byte ComponentTag { get; set; } 
        public System.String ISO639LanguageCode { get; set; } 
        public Text TextChar { get; set; } 
        public System.String ComponentDescription { get { return GetComponentDescription(StreamContent, StreamContentExt, ComponentType); } }
    }

    public class SatelliteDeliverySystemDescriptor : Descriptor
    {
        public static System.String[] PolarizationDescription = new System.String[] { "Linear-horizontal", "linear-vertical", "circular-left", "circular-right" };
        public static System.String[] RoleOffDescription = new System.String[] { "α = 0,35", "α = 0,25", "α = 0,20", "reserved" };
        public static System.String ModulationDescription(byte modulation)
        {
            switch (modulation)
            {
                case 0x0: return "Auto";
                case 0x01: return "QPSK";
                case 0x02: return "8PSK";
                case 0x03: return "16-QAM (n/a for DVB-S2)";
                default: return "Impossible in a 2 bit number";
            }
        }

        public static System.String FECInnerDescription(byte fECInner)
        {
            switch (fECInner)
            {
                case 0x0: return "not defined";
                case 0x1: return "1/2 conv. code rate";
                case 0x2: return "2/3 conv. code rate";
                case 0x3: return "3/4 conv. code rate";
                case 0x4: return "5/6 conv. code rate";
                case 0x5: return "7/8 conv. code rate";
                case 0x6: return "8/9 conv. code rate";
                case 0x7: return "3/5 conv. code rate ";
                case 0x8: return "4/5 conv. code rate";
                case 0x9: return "9/10 conv. code rate";
                case 0xf: return "no conv. coding";
                default: return "reserved";
            }
        }
       
        public SatelliteDeliverySystemDescriptor(byte[] stream, int start) : base(stream, start)
        {
            Frequency = System.String.Format("{0}{1}{2}{3}{4}{5}{6}{7}", (stream[start + 2] >> 4) & 0x0F, stream[start + 2] & 0x0F, (stream[start + 3] >> 4) & 0x0F, stream[start + 3] & 0x0F, (stream[start + 4] >> 4) & 0x0F, stream[start + 4] & 0x0F, (stream[start + 5] >> 4) & 0x0F, stream[start + 5] & 0x0F);
            OrbitalPosition = System.String.Format("{0}{1}{2}{3}", (stream[start + 6] >> 4) & 0x0F, stream[start + 6] & 0x0F, (stream[start + 7] >> 4) & 0x0F, stream[start + 7] & 0x0F);
            WestEastFlag = ((stream[start + 8] >> 7) & 0x01) == 0x01;
            Polarization = (byte)((stream[start + 8] >> 5) & 0x03);
            RollOff = (byte)((stream[start + 8] >> 3) & 0x03);
            ModulationSystem = ((stream[start + 8] >> 2) & 0x01) == 0x01;
            Modulation = (byte)(stream[start + 8] & 0x03);
            SymbolRate = System.String.Format("{0}{1}{2}{3}{4}{5}{6}", (stream[start + 9] >> 4) & 0x0F, stream[start + 9] & 0x0F, (stream[start + 10] >> 4) & 0x0F, stream[start + 10] & 0x0F, (stream[start + 11] >> 4) & 0x0F, stream[start + 11] & 0x0F, (stream[start + 12] >> 4) & 0x0F);
            FECInner = (byte)(stream[start + 12] & 0x0F);
        }

        public System.String Frequency { get; set; }
        public System.String FrequencyString { get { return System.String.Format("{0} GHz", Frequency.Insert(3, ",")); } }
        public System.String OrbitalPosition { get; set; }
        public System.String OrbitalPositionString { get { return System.String.Format("{0} deg", OrbitalPosition.Insert(3, ",")); } }
        public bool WestEastFlag { get; set; }
        public byte Polarization { get; set; }
        public byte RollOff { get; set; }
        public System.String RollOffString { get { return RoleOffDescription[RollOff]; } }
        public System.String PolarizationString { get { return PolarizationDescription[Polarization]; } }
        public bool ModulationSystem { get; set; }
        public System.String ModulationSystemString { get { return ModulationSystem ? "S2" : "S"; } }
        public byte Modulation { get; set; }
        public System.String ModulationString { get { return ModulationDescription(Modulation); } }
        public System.String SymbolRate { get; set; }
        public byte FECInner { get; set; }
        public System.String FECInnerString { get { return FECInnerDescription(FECInner); } }
    }

    public class NetworkNameDescriptor : Descriptor
    {
        public NetworkNameDescriptor(byte[] stream, int start) : base(stream, start)
        {
            byte startOfName = (byte)(start + 2);
            switch (stream[start + 2])
            {
                case 0x1F: startOfName += 2; break;
                case 0x10: startOfName += 3; break;
            }
            NetworkName = System.Text.Encoding.UTF8.GetString(stream, startOfName, DescriptorLength - (startOfName - start) + 2);
        }
        public System.String NetworkName { get; set; }
    }

    public class TerrestrialDeliverySystemDescriptor : Descriptor
    {
        public static System.String[] BandwidthDescription = new System.String[] { "8 MHz", "7 MHz", "reserved for future us", "reserved for future us", "reserved for future us", "reserved for future us", "reserved for future us", "reserved for future us" };
        public static System.String[] ConstellationDescription = new System.String[] { "QPSK", "16-QAM", "64-QAM", "reserved for future use" };
        public static System.String[] HierarchyInformationDescription = new System.String[] { "non-hierarchical", "α = 1", "α = 2", "α = 4", "reserved for future us", "reserved for future us", "reserved for future us", "reserved for future us" };
        public static System.String[] CodeRateDescription = new System.String[] { "1/2", "2/3", "3/4", "5/6", "7/8", "reserved for future us", "reserved for future us", "reserved for future us" };
        public static System.String[] GuardIntervalDescription = new System.String[] { "1/32", "1/16", "1/8", "1/4" };
        public static System.String[] TransmissionModeDescription = new System.String[] { "2k mode", "8k mode", "reserved for future us", "reserved for future us4" };
       
        public TerrestrialDeliverySystemDescriptor(byte[] stream, int start) : base(stream, start)
        {
            CentreFrequency = (uint)((stream[start + 2] << 24) + (stream[start + 3] << 16) + (stream[start + 4] << 8) + (stream[start + 5]));
            Bandwidth = (byte)((stream[start + 6] >> 5) & 0x07);
            ReservedFutureUse = (byte)(stream[start + 6] & 0x1F);
            Constellation = (byte)((stream[start + 7] >> 6) & 0x03);
            HierarchyInformation = (byte)((stream[start + 7] >> 3) & 0x07);
            CodeRateHPStream = (byte)(stream[start + 7] & 0x07);
            CodeRateLPStream = (byte)((stream[start + 8] >> 5) & 0x07);
            GuardInterval = (byte)((stream[start + 8] >> 3) & 0x03);
            TransmissionMode = (byte)((stream[start + 8] >> 1) & 0x03);
            OtherFrequencyFlag = (bool)((stream[start + 8] & 0x01) == 0x01);
            ReservedFutureUse2 = (uint)((stream[start + 9] << 24) + (stream[start + 10] << 16) + (stream[start + 11] << 8) + (stream[start + 12]));
        }

        public uint CentreFrequency { get; set; }
        public byte Bandwidth { get; set; }
        public System.String BandwidthString { get { return BandwidthDescription[Bandwidth]; } }
        public byte ReservedFutureUse { get; set; }
        public byte Constellation { get; set; }
        public System.String ConstellationString { get { return ConstellationDescription[Constellation]; } }
        public byte HierarchyInformation { get; set; }
        public System.String HierarchyInformationString { get { return HierarchyInformationDescription[HierarchyInformation]; } }
        public byte CodeRateHPStream { get; set; }
        public System.String CodeRateHPStreamString { get { return CodeRateDescription[CodeRateHPStream]; } }
        public byte CodeRateLPStream { get; set; }
        public System.String CodeRateLPStreamString { get { return CodeRateDescription[CodeRateLPStream]; } }
        public byte GuardInterval { get; set; }
        public System.String GuardIntervalString { get { return GuardIntervalDescription[GuardInterval]; } }
        public byte TransmissionMode { get; set; }
        public bool OtherFrequencyFlag { get; set; }
        public uint ReservedFutureUse2 { get; set; }
    }

    public static class DescriptorFactory
    {
        public static Descriptor DescriptorFromData(byte[] stream, int start)
        {
            switch (stream[start])
            {
                case 0x05: return new RegistrationDescriptor(stream, start);
                case 0x0a: return new Iso639LanguageDescriptor(stream, start);
                case 0x40: return new NetworkNameDescriptor(stream, start);
                case 0x41: return new ServiceListDescriptor(stream, start);
                case 0x43: return new SatelliteDeliverySystemDescriptor(stream, start);
                case 0x48: return new ServiceDescriptor(stream, start);
                case 0x4D: return new ShortEventDescriptor(stream, start);
                case 0x4E: return new ExtendedEventDescriptor(stream, start);
                case 0x50: return new ComponentDescriptor(stream, start);
                case 0x52: return new StreamIdentifierDescriptor(stream, start);
                case 0x56: return new TeletextDescriptor(stream, start);
                case 0x59: return new SubtitlingDescriptor(stream, start);
                case 0x5a: return new TerrestrialDeliverySystemDescriptor(stream, start);
                case 0x66: return new DataBroadcastIdDescriptor(stream, start);

                default: return new Descriptor(stream, start);
            }
        }
    }
}
