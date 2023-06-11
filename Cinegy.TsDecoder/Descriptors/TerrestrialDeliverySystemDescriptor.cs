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
    /// <summary>
    /// A Terrestrial Delivery System Descriptor <see cref="Descriptor"/>.
    /// </summary>
    /// <remarks>
    /// For details please refer to the original documentation,
    /// e.g. <i>ETSI EN 300 468 V1.15.1 (2016-03)</i> or alternate versions.
    /// </remarks>
    public class TerrestrialDeliverySystemDescriptor : Descriptor
    {
        public static string[] BandwidthDescription = new string[]
        {
            "8 MHz", "7 MHz", "reserved for future us", "reserved for future us", "reserved for future us",
            "reserved for future us", "reserved for future us", "reserved for future us"
        };

        public static string[] ConstellationDescription = new string[]
        {
            "QPSK", "16-QAM", "64-QAM", "reserved for future use"
        };

        public static string[] HierarchyInformationDescription = new string[]
        {
            "non-hierarchical", "α = 1", "α = 2", "α = 4", "reserved for future us", "reserved for future us",
            "reserved for future us", "reserved for future us"
        };

        public static string[] CodeRateDescription = new string[]
        {
            "1/2", "2/3", "3/4", "5/6", "7/8", "reserved for future us", "reserved for future us", "reserved for future us"
        };

        public static string[] GuardIntervalDescription = new string[] { "1/32", "1/16", "1/8", "1/4" };

        public static string[] TransmissionModeDescription = new string[]
        {
            "2k mode", "8k mode", "reserved for future us", "reserved for future us4"
        };

        public TerrestrialDeliverySystemDescriptor(byte[] stream, int start) : base(stream, start)
        {
            CentreFrequency =
                (uint)
                    ((stream[start + 2] << 24) + (stream[start + 3] << 16) + (stream[start + 4] << 8) +
                     stream[start + 5]);
            Bandwidth = (byte)(stream[start + 6] >> 5 & 0x07);
            ReservedFutureUse = (byte)(stream[start + 6] & 0x1F);
            Constellation = (byte)(stream[start + 7] >> 6 & 0x03);
            HierarchyInformation = (byte)(stream[start + 7] >> 3 & 0x07);
            CodeRateHPStream = (byte)(stream[start + 7] & 0x07);
            CodeRateLPStream = (byte)(stream[start + 8] >> 5 & 0x07);
            GuardInterval = (byte)(stream[start + 8] >> 3 & 0x03);
            TransmissionMode = (byte)(stream[start + 8] >> 1 & 0x03);
            OtherFrequencyFlag = (stream[start + 8] & 0x01) == 0x01;
            ReservedFutureUse2 =
                (uint)
                    ((stream[start + 9] << 24) + (stream[start + 10] << 16) + (stream[start + 11] << 8) +
                     stream[start + 12]);
        }

        public uint CentreFrequency { get; }
        public byte Bandwidth { get; }
        public string BandwidthString => BandwidthDescription[Bandwidth];
        public byte ReservedFutureUse { get; }
        public byte Constellation { get; }
        public string ConstellationString => ConstellationDescription[Constellation];
        public byte HierarchyInformation { get; }
        public string HierarchyInformationString => HierarchyInformationDescription[HierarchyInformation];
        public byte CodeRateHPStream { get; }
        public string CodeRateHPStreamString => CodeRateDescription[CodeRateHPStream];
        public byte CodeRateLPStream { get; }
        public string CodeRateLPStreamString => CodeRateDescription[CodeRateLPStream];
        public byte GuardInterval { get; }
        public string GuardIntervalString => GuardIntervalDescription[GuardInterval];
        public byte TransmissionMode { get; }
        public bool OtherFrequencyFlag { get; }
        public uint ReservedFutureUse2 { get; }
    }
}
