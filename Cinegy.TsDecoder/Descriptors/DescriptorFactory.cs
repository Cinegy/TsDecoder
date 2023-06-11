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
    public static class DescriptorFactory
    {
        public static Descriptor DescriptorFromData(byte[] stream, int start)
        {
            switch (stream[start])
            {
                case 0x05:
                    var regDesc = new RegistrationDescriptor(stream, start);
                    switch (regDesc.Organization)
                    {
                        case "2LND":
                            return new CinegyDaniel2Descriptor(stream, start);
                        case "CNGY":
                            return new CinegyTechMetadataDescriptor(stream, start);
                        default:
                            return regDesc;
                    }
                case 0x09: return new CaDescriptor(stream, start);
                case 0x0a: return new Iso639LanguageDescriptor(stream, start);
                case 0x26: return new MetadataDescriptor(stream, start);
                case 0x27: return new MetadataStdDescriptor(stream, start);
                case 0x28: return new AvcVideoDescriptor(stream, start);
                case 0x38: return new HevcDescriptor(stream, start);
                case 0x3F: return new ExtensionDescriptor(stream, start);
                case 0x40: return new NetworkNameDescriptor(stream, start);
                case 0x41: return new ServiceListDescriptor(stream, start);
                case 0x43: return new SatelliteDeliverySystemDescriptor(stream, start);
                case 0x47: return new BouquetNameDescriptor(stream, start);
                case 0x48: return new ServiceDescriptor(stream, start);
                case 0x49: return new CountryAvailabilityDescriptor(stream, start);
                case 0x4A: return new LinkageDescriptor(stream, start);
                case 0x4D: return new ShortEventDescriptor(stream, start);
                case 0x4E: return new ExtendedEventDescriptor(stream, start);
                case 0x50: return new ComponentDescriptor(stream, start);
                case 0x52: return new StreamIdentifierDescriptor(stream, start);
                case 0x53: return new CaIdentifierDescriptor(stream, start);
                case 0x54: return new ContentDescriptor(stream, start);
                case 0x55: return new ParentalRatingDescriptor(stream, start);
                case 0x56: return new TeletextDescriptor(stream, start);
                case 0x59: return new SubtitlingDescriptor(stream, start);
                case 0x5a: return new TerrestrialDeliverySystemDescriptor(stream, start);
                case 0x5F: return new PrivateDataSpecifierDescriptor(stream, start);
                case 0x64: return new DataBroadcastDescriptor(stream, start);
                case 0x65: return new ScramblingDescriptor(stream, start);
                case 0x66: return new DataBroadcastIdDescriptor(stream, start);
                case 0x6a: return new Ac3Descriptor(stream, start);
                case 0x7a: return new Eac3Descriptor(stream, start);
                case 0x7b: return new DtsDescriptor(stream, start);
                case 0x7c: return new AacDescriptor(stream, start);
                case 0x83: return new LcnDescriptor(stream, start);
                case 0x8A: return new CueIdentifierDescriptor(stream, start);
                default: return new Descriptor(stream, start);
            }
        }
    }
}
