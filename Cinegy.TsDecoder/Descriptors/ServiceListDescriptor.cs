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

namespace Cinegy.TsDecoder.Descriptors
{
    /// <summary>
    /// A Service List Descriptor <see cref="Descriptor"/>.
    /// </summary>
    /// <remarks>
    /// For details please refer to the original documentation,
    /// e.g. <i>ETSI EN 300 468 V1.15.1 (2016-03)</i> or alternate versions.
    /// </remarks>
    public class ServiceListDescriptor : Descriptor
    {
        public static string ServiceTypeDescription(byte serviceType)
        {
            if (serviceType <= 0x0C)
            {
                switch (serviceType)
                {
                    case 0x00:
                        return "reserved for future use";
                    case 0x01:
                        return "digital television service";
                    case 0x02:
                        return "digital radio sound service";
                    case 0x03:
                        return "teletext service";
                    case 0x04:
                        return "NVOD reference service";
                    case 0x05:
                        return "NVOD time-shifted service";
                    case 0x06:
                        return "mosaic service";
                    case 0x07:
                        return "PAL coded signal";
                    case 0x08:
                        return "SECAM coded signal";
                    case 0x09:
                        return "D/D2-MAC";
                    case 0x0A:
                        return "FM Radio";
                    case 0x0B:
                        return "NTSC coded signal";
                    case 0x0C:
                        return "data broadcast service";
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
            public Service()
            {
            }

            public Service(Service service)
            {
                ServiceId = service.ServiceId;
                ServiceType = service.ServiceType;
            }

            public ushort ServiceId { get; internal set; }
            public byte ServiceType { get; internal set; }
            public string ServiceTypeString => ServiceTypeDescription(ServiceType);
        }

        public ServiceListDescriptor(byte[] stream, int start) : base(stream, start)
        {
            var services = new List<Service>();
            var startOfNextBlock = (ushort)(start + 2);
            while (startOfNextBlock < start + DescriptorLength + 2)
            {
                var service = new Service
                {
                    ServiceId = (ushort)((stream[startOfNextBlock] << 8) + stream[startOfNextBlock + 1]),
                    ServiceType = stream[startOfNextBlock + 2]
                };

                startOfNextBlock += 3;

                services.Add(service);
            }
            Services = services;
        }

        public IEnumerable<Service> Services { get; }

    }
}
