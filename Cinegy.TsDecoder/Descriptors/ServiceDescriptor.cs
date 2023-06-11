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

using Cinegy.TsDecoder.TransportStream;

namespace Cinegy.TsDecoder.Descriptors
{
    /// <summary>
    /// A Service Descriptor <see cref="Descriptor"/>.
    /// </summary>
    /// <remarks>
    /// For details please refer to the original documentation,
    /// e.g. <i>ETSI EN 300 468 V1.15.1 (2016-03)</i> or alternate versions.
    /// </remarks>
    public class ServiceDescriptor : Descriptor
    {
        public static string GetServiceTypeDescription(byte serviceType)
        {
            switch (serviceType)
            {
                case 0x00:
                    return "reserved for future use";
                case 0x01:
                    return "digital television service (see note 1)";
                case 0x02:
                    return "digital radio sound service (see note 2)";
                case 0x03:
                    return "Teletext service";
                case 0x04:
                    return "NVOD reference service (see note 1)";
                case 0x05:
                    return "NVOD time-shifted service (see note 1)";
                case 0x06:
                    return "mosaic service";
                case 0x07:
                    return "FM radio service";
                case 0x08:
                    return "DVB SRM service [49]";
                case 0x09:
                    return "reserved for future use";
                case 0x0A:
                    return "advanced codec digital radio sound service";
                case 0x0B:
                    return "H.264/AVC mosaic service";
                case 0x0C:
                    return "data broadcast service";
                case 0x0D:
                    return "reserved for Common Interface Usage (EN 50221[37])";
                case 0x0E:
                    return "RCS Map (see EN301790[7])";
                case 0x0F:
                    return "RCS FLS (see EN301790[7])";
                case 0x10:
                    return "DVB MHP service 0x11 MPEG-2 HD digital television service";
                case 0x16:
                    return "H.264/AVC SD digital television service";
                case 0x17:
                    return "H.264/AVC SD NVOD time-shifted service";
                case 0x18:
                    return "H.264/AVC SD NVOD reference service";
                case 0x19:
                    return "H.264/AVC HD digital television service";
                case 0x1A:
                    return "H.264/AVC HD NVOD time-shifted service";
                case 0x1B:
                    return "H.264/AVC HD NVOD reference service";
                case 0x1C:
                    return "H.264/AVC frame compatible plano-stereoscopic HD digital television service (see note 3)";
                case 0x1D:
                    return "H.264/AVC frame compatible plano-stereoscopic HD NVOD time-shifted service (see note 3)";
                case 0x1E:
                    return "H.264/AVC frame compatible plano-stereoscopic HD NVOD reference service (see note 3)";
                case 0x1F:
                    return "HEVC digital television service";
                case 0xFF:
                    return "reserved for future use";
            }
            if (serviceType >= 0x20 || serviceType <= 0x7F)
            {
                return "reserved for future use";
            }
            if (serviceType >= 0x80 || serviceType <= 0xFE)
            {
                return "user defined";
            }
            if (serviceType >= 0x12 || serviceType <= 0x15)
            {
                return "reserved for future use";
            }
            return "unknown";
        }

        public ServiceDescriptor(byte[] stream, int start) : base(stream, start)
        {
            ServiceType = stream[start + 2];
            ServiceProviderNameLength = stream[start + 3];
            ServiceProviderName = new Text(stream, start + 4, ServiceProviderNameLength);
            ServiceNameLength = stream[start + 4 + ServiceProviderNameLength];
            ServiceName = new Text(stream, start + 4 + ServiceProviderNameLength + 1, ServiceNameLength);
        }

        public byte ServiceType { get; } //8 uimsbf
        public string ServiceTypeDescription => GetServiceTypeDescription(ServiceType);
        public byte ServiceProviderNameLength { get; } // 8 uimsbf
        public Text ServiceProviderName { get; } // 
        public byte ServiceNameLength { get; } // 8 uimsbf 
        public Text ServiceName { get; }
    }
}
