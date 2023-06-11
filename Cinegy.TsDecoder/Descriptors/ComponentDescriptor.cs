﻿/* Copyright 2017-2023 Cinegy GmbH.

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

using System.Text;
using Cinegy.TsDecoder.TransportStream;

namespace Cinegy.TsDecoder.Descriptors
{
    /// <summary>
    /// A Component Descriptor <see cref="Descriptor"/>.
    /// </summary>
    /// <remarks>
    /// For details please refer to the original documentation,
    /// e.g. <i>ETSI EN 300 468 V1.15.1 (2016-03)</i> or alternate versions.
    /// </remarks>
    public class ComponentDescriptor : Descriptor
    {
        public static string GetComponentDescription(byte streamContent, byte streamContentExt, byte componentType)
        {
            switch (streamContent)
            {
                case 0x00:
                    return "reserved for future us";
                case 0x01:
                    switch (componentType)
                    {
                        case 0x00:
                            return "reserved for future use";
                        case 0x01:
                            return "MPEG-2 video, 4:3 aspect ratio, 25 Hz (see note 2)";
                        case 0x02:
                            return "MPEG-2 video, 16:9 aspect ratio with pan vectors, 25 Hz (see note 2)";
                        case 0x03:
                            return "MPEG-2 video, 16:9 aspect ratio without pan vectors, 25 Hz (see note 2)";
                        case 0x04:
                            return "MPEG-2 video, > 16:9 aspect ratio, 25 Hz (see note 2)";
                        case 0x05:
                            return "MPEG-2 video, 4:3 aspect ratio, 30 Hz (see note 2)";
                        case 0x06:
                            return "MPEG-2 video, 16:9 aspect ratio with pan vectors, 30 Hz (see note 2)";
                        case 0x07:
                            return "MPEG-2 video, 16:9 aspect ratio without pan vectors, 30 Hz (see note 2)";
                        case 0x08:
                            return "MPEG-2 video, > 16:9 aspect ratio, 30 Hz(see note 2)";
                        case 0x09:
                            return "MPEG-2 high definition video, 4:3 aspect ratio, 25 Hz (see note 2)";
                        case 0x0A:
                            return
                                "MPEG-2 high definition video, 16:9 aspect ratio with pan vectors, 25 Hz (see note 2)";
                        case 0x0B:
                            return
                                "MPEG-2 high definition video, 16:9 aspect ratio without pan vectors, 25 Hz (see note 2)";
                        case 0x0C:
                            return "MPEG-2 high definition video, > 16:9 aspect ratio, 25 Hz (see note 2)";
                        case 0x0D:
                            return "MPEG-2 high definition video, 4:3 aspect ratio, 30 Hz (see note 2)";
                        case 0x0E:
                            return
                                "MPEG-2 high definition video, 16:9 aspect ratio with pan vectors, 30 Hz (see note 2)";
                        case 0x0F:
                            return
                                "MPEG-2 high definition video, 16:9 aspect ratio without pan vectors, 30 Hz (see note 2)";
                        case 0x10:
                            return
                                "MPEG-2 high definition video, > 16:9 aspect ratio, 30 Hz (see note 2)0x11 to 0xAFreserved for future use";
                        case 0xFF:
                            return "reserved for future use";
                        default:
                            return "user defined";
                    }
                case 0x02:
                    switch (componentType)
                    {
                        case 0x00:
                            return "reserved for future use";
                        case 0x01:
                            return "MPEG-1 Layer 2 audio, single mono channel";
                        case 0x02:
                            return "MPEG-1 Layer 2 audio, dual mono channel";
                        case 0x03:
                            return "MPEG-1 Layer 2 audio, stereo (2 channel)";
                        case 0x04:
                            return "MPEG-1 Layer 2 audio, multi-lingual, multi-channel";
                        case 0x05:
                            return "MPEG-1 Layer 2 audio, surround sound";
                        case 0x40:
                            return "MPEG-1 Layer 2 audio description for the visually impaired (see note 5)";
                        case 0x41:
                            return "MPEG-1 Layer 2 audio for the hard of hearing";
                        case 0x42:
                            return "receiver-mix supplementary audio as per annex E of TS 101 154 [9]";
                        case 0x47:
                            return "MPEG-1 Layer 2 audio, receiver-mix audio description";
                        case 0x48:
                            return "MPEG-1 Layer 2 audio, broadcast-mix audio description";
                        case 0xFF:
                            return "reserved for future use";
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
                        case 0x00:
                            return "reserved for future use";
                        case 0x01:
                            return "EBU Teletext subtitles";
                        case 0x02:
                            return "associated EBU Teletext";
                        case 0x03:
                            return "VBI data";
                        case 0x10:
                            return "DVBsubtitles (normal) with no monitor aspect ratio criticality";
                        case 0x11:
                            return "DVB subtitles (normal) for display on 4:3 aspect ratio monitor";
                        case 0x12:
                            return "DVB subtitles (normal) for display on 16:9 aspect ratio monitor";
                        case 0x13:
                            return "DVB subtitles (normal) for display on 2.21:1 aspect ratio monitor";
                        case 0x14:
                            return "DVB subtitles (normal) for display on a high definition monitor";
                        case 0x15:
                            return
                                "DVB subtitles (normal) with plano-stereoscopic disparity for display on a high definition monitor";
                        case 0x20:
                            return "DVB subtitles (for the hard of hearing) with no monitor aspect ratio criticality";
                        case 0x21:
                            return "DVB subtitles (for the hard of hearing) for display on 4:3   aspect ratio monito";
                        case 0x22:
                            return "DVB subtitles (for the hard of hearing) for display on 16:9 aspect ratio monitor";
                        case 0x23:
                            return "DVB subtitles (for the hard of hearing) for display on 2.21:1 aspect ratio monitor";
                        case 0x24:
                            return "DVB subtitles (for the hard of hearing) for display on a high definition monitor";
                        case 0x25:
                            return
                                "DVB subtitles (for the hard of hearing) with plano-stereoscopic disparity for display on a high definition monitor";
                        case 0x30:
                            return "open (in-vision) sign language interpretation for the deaf(see note 7)";
                        case 0x31:
                            return "closed sign language interpretation for the deaf(see note 7)";
                        case 0x40:
                            return "video up-sampled from standard definition source material(see note 7)";
                        case 0x80:
                            return "dependent SAOC-DE data stream";
                        case 0xFF:
                            return "reserved for future use";
                    }
                    if (componentType >= 0xB0 || componentType <= 0xFE)
                    {
                        return "user defined";
                    }
                    return "reserved for future use";
                case 0x04:
                    return componentType <= 0x7F
                        ? "reserved for AC-3 audio modes (refer to table D.1)"
                        : "reserved for enhanced AC-3 audio modes (refer to table D.1)";

                case 0x05:
                    switch (componentType)
                    {
                        case 0x00:
                            return "reserved for future use";
                        case 0x01:
                            return "H.264/AVC standard definition video, 4:3 aspect ratio, 25 Hz (see note2)";
                        case 0x02:
                            return "reserved for future use";
                        case 0x03:
                            return "H.264/AVC standard definition video, 16:9 aspect ratio, 25 Hz (see note2)";
                        case 0x04:
                            return "H.264/AVC standard definition video, > 16:9 aspect ratio, 25 Hz (see note 2)";
                        case 0x05:
                            return "H.264/AVC standard definition video, 4:3 aspect ratio, 30 Hz (see note 2)";
                        case 0x06:
                            return "reserved for future use";
                        case 0x07:
                            return "H.264/AVC standard definition video, 16:9 aspect ratio, 30 Hz (see note 2)";
                        case 0x08:
                            return "H.264/AVC standard definition video, > 16:9 aspect ratio, 30 Hz (see note 2)";
                        case 0x0B:
                            return "H.264/AVC high definition video, 16:9 aspect ratio, 25 Hz (see note 2)";
                        case 0x0C:
                            return "H.264/AVC high definition video, > 16:9 aspect ratio, 25 Hz (see note 2)";
                        case 0x0F:
                            return "H.264/AVC high definition video, 16:9 aspect ratio, 30 Hz (see note 2)";
                        case 0x10:
                            return "H.264/AVC high definition video, > 16:9 aspect ratio, 30 Hz (see note 2)";
                        case 0x80:
                            return
                                "H.264/AVC plano-stereoscopic frame compatible high definition video, 16:9 aspect ratio, 25 Hz, Side-by-Side (see notes 2 and 3)";
                        case 0x81:
                            return
                                "H.264/AVC plano-stereoscopic frame compatible high definition video, 16:9 aspect ratio, 25 Hz, Top-and-Bottom (see notes 2 and 3)";
                        case 0x82:
                            return
                                "H.264/AVC plano-stereoscopic frame compatible high definition video, 16:9 aspect ratio, 30 Hz, Side-by-Side (see notes 2, 3 and 4)";
                        case 0x83:
                            return
                                "H.264/AVC stereoscopic frame compatible high definition video, 16:9 aspect ratio, 30 Hz, Top-and-Bottom (see notes 2, 3 and 4)";
                        case 0x84:
                            return
                                "H.264/MVC dependent view, plano-stereoscopic service compatible video (see notes 2, 3 and 4)";
                    }
                    if (componentType >= 0xB0 || componentType <= 0xFE)
                    {
                        return "user defined";
                    }
                    return "reserved for future use";

                case 0x06:
                    switch (componentType)
                    {

                        case 0x00:
                            return "reserved for future use";
                        case 0x01:
                            return "HE AAC audio, single mono channel(see note 6)";
                        case 0x02:
                            return "reserved for future use";
                        case 0x03:
                            return "HE AAC audio, stereo (see note 6)";
                        case 0x04:
                            return "reserved for future use";
                        case 0x05:
                            return "HE AAC audio, surround sound (see note 6)";
                        case 0x40:
                            return "HE AAC audio description for the visually impaired (see notes 5 and 6)";
                        case 0x41:
                            return "HE AAC audio for the hard of hearing (see note 6)";
                        case 0x42:
                            return
                                "HE AAC receiver-mix supplementary audio as per annex E of TS 101 154 [9] (see note 6)";
                        case 0x43:
                            return "HE AACv2 audio, stereo";
                        case 0x44:
                            return "HE AACv2 audio description for the visually impaired (see note 5)";
                        case 0x45:
                            return "HE AACv2 audio for the hard of hearing";
                        case 0x46:
                            return "HE AACv2 receiver-mix supplementary audio as per annex E of TS 101 154[9]";
                        case 0x47:
                            return "HE AAC receiver-mix audio description for the visually impaired";
                        case 0x48:
                            return "HE AAC broadcast-mix audio description for the visually impaired";
                        case 0x49:
                            return "HE AACv2 receiver-mix audio description for the visually impaired";
                        case 0x4A:
                            return "HE AACv2 broadcast-mix audio description for the visually impaired";
                        case 0xA0:
                            return "HE AAC, or HE AAC v2 with SAOC-DE ancillary data (see notes 6 and 7)";
                        case 0xFF:
                            return "reserved for future use";
                    }
                    if (componentType >= 0xB0 || componentType <= 0xFE)
                    {
                        return "user defined";
                    }
                    return "reserved for future use";
                case 0x07:
                    if (componentType <= 0x7F)
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
                                case 0x00:
                                    return "HEVC Main Profile high definition video, 50 Hz(note 2)";
                                case 0x01:
                                    return "HEVC Main 10 Profile high definition video, 50 Hz(note 2)";
                                case 0x02:
                                    return "HEVC Main Profile high definition video, 60 Hz (note 2)";
                                case 0x03:
                                    return "HEVC Main 10 Profile high definition video, 60 Hz (note 2)";
                                case 0x04:
                                    return "HEVC ultra high definition video (note 2)";
                                default:
                                    return "reserved for future use";
                            }
                        default:
                            return "reserved for future use";
                    }
                case 0x0A:
                    return "reserved for future use";
                case 0x0B:
                    switch (streamContentExt)
                    {
                        case 0x0F:
                            switch (componentType)
                            {
                                case 0x00:
                                    return "less than 16:9 aspect ratio";
                                case 0x01:
                                    return "16:9 aspect ratio";
                                case 0x02:
                                    return "greater than 16:9 aspect ratio";
                                default:
                                    return "reserved for future use";
                            }
                        default:
                            return "reserved for future use";
                    }
                default:
                    return "user defined";
            }
            return "Unknown";
        }

        public ComponentDescriptor(byte[] stream, int start) : base(stream, start)
        {
            StreamContentExt = (byte)(stream[start + 2] >> 4 & 0x0F);
            StreamContent = (byte)(stream[start + 2] & 0x0F);
            ComponentType = stream[start + 3];
            ComponentTag = stream[start + 4];

            Iso639LanguageCode = Encoding.UTF8.GetString(stream, start + 5, 3);
            TextChar = new Text(stream, start + 5 + 3, DescriptorLength + 2 - 5 - 3);
        }


        public byte StreamContentExt { get; }
        public byte StreamContent { get; }
        public byte ComponentType { get; }
        public byte ComponentTag { get; }
        public string Iso639LanguageCode { get; }
        public Text TextChar { get; }
        public string ComponentDescription => GetComponentDescription(StreamContent, StreamContentExt, ComponentType);
    }
}
