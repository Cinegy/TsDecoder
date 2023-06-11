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
    /// A Satelite Delivery Descriptor <see cref="Descriptor"/>.
    /// </summary>
    /// <remarks>
    /// For details please refer to the original documentation,
    /// e.g. <i>ETSI EN 300 468 V1.15.1 (2016-03)</i> or alternate versions.
    /// </remarks>
    public class SatelliteDeliverySystemDescriptor : Descriptor
    {
        public static string[] PolarizationDescription = new string[]
        {"Linear-horizontal", "linear-vertical", "circular-left", "circular-right"};

        public static string[] RoleOffDescription = new string[] { "α = 0,35", "α = 0,25", "α = 0,20", "reserved" };

        public static string ModulationDescription(byte modulation)
        {
            switch (modulation)
            {
                case 0x0:
                    return "Auto";
                case 0x01:
                    return "QPSK";
                case 0x02:
                    return "8PSK";
                case 0x03:
                    return "16-QAM (n/a for DVB-S2)";
                default:
                    return "Impossible in a 2 bit number";
            }
        }

        public static string FECInnerDescription(byte fecInner)
        {
            switch (fecInner)
            {
                case 0x0:
                    return "not defined";
                case 0x1:
                    return "1/2 conv. code rate";
                case 0x2:
                    return "2/3 conv. code rate";
                case 0x3:
                    return "3/4 conv. code rate";
                case 0x4:
                    return "5/6 conv. code rate";
                case 0x5:
                    return "7/8 conv. code rate";
                case 0x6:
                    return "8/9 conv. code rate";
                case 0x7:
                    return "3/5 conv. code rate ";
                case 0x8:
                    return "4/5 conv. code rate";
                case 0x9:
                    return "9/10 conv. code rate";
                case 0xf:
                    return "no conv. coding";
                default:
                    return "reserved";
            }
        }

        public SatelliteDeliverySystemDescriptor(byte[] stream, int start) : base(stream, start)
        {
            Frequency =
                $"{stream[start + 2] >> 4 & 0x0F}{stream[start + 2] & 0x0F}{stream[start + 3] >> 4 & 0x0F}{stream[start + 3] & 0x0F}{stream[start + 4] >> 4 & 0x0F}{stream[start + 4] & 0x0F}{stream[start + 5] >> 4 & 0x0F}{stream[start + 5] & 0x0F}";
            OrbitalPosition =
                $"{stream[start + 6] >> 4 & 0x0F}{stream[start + 6] & 0x0F}{stream[start + 7] >> 4 & 0x0F}{stream[start + 7] & 0x0F}";
            WestEastFlag = (stream[start + 8] >> 7 & 0x01) == 0x01;
            Polarization = (byte)(stream[start + 8] >> 5 & 0x03);
            RollOff = (byte)(stream[start + 8] >> 3 & 0x03);
            ModulationSystem = (stream[start + 8] >> 2 & 0x01) == 0x01;
            Modulation = (byte)(stream[start + 8] & 0x03);
            SymbolRate =
                $"{stream[start + 9] >> 4 & 0x0F}{stream[start + 9] & 0x0F}{stream[start + 10] >> 4 & 0x0F}{stream[start + 10] & 0x0F}{stream[start + 11] >> 4 & 0x0F}{stream[start + 11] & 0x0F}{stream[start + 12] >> 4 & 0x0F}";
            FECInner = (byte)(stream[start + 12] & 0x0F);
        }

        public string Frequency { get; }
        public string FrequencyString => string.Format("{0} GHz", Frequency.Insert(3, ","));
        public string OrbitalPosition { get; }
        public string OrbitalPositionString => string.Format("{0} deg", OrbitalPosition.Insert(3, ","));
        public bool WestEastFlag { get; }
        public byte Polarization { get; }
        public byte RollOff { get; }
        public string RollOffString => RoleOffDescription[RollOff];
        public string PolarizationString => PolarizationDescription[Polarization];
        public bool ModulationSystem { get; }
        public string ModulationSystemString => ModulationSystem ? "S2" : "S";
        public byte Modulation { get; }
        public string ModulationString => ModulationDescription(Modulation);
        public string SymbolRate { get; }
        public byte FECInner { get; }
        public string FECInnerString => FECInnerDescription(FECInner);
    }
}
