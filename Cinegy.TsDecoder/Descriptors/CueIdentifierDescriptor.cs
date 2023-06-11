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
    /// A Cue Identifier Descriptor <see cref="Descriptor"/>.
    /// </summary>
    /// <remarks>
    /// For details please refer to the original documentation,
    /// e.g. <i>ETSI EN 300 468 V1.15.1 (2016-03)</i> or alternate versions.
    /// </remarks>
    public class CueIdentifierDescriptor : Descriptor
    {
        public string CueStreamTypeDescription(byte cueType)
        {
            switch (cueType)
            {
                case 0x00:
                    return "splice_insert, splice_null, splice_schedule";
                case 0x01:
                    return "All Commands";
                case 0x02:
                    return "Segmentation";
                case 0x03:
                    return "Tiered Splicing";
                case 0x04:
                    return "Tiered Segmentation";
                default:
                    if (cueType >= 0x05 && cueType <= 0x7f) return "Reserved";
                    break;
            }
            return "User Defined";
        }


        public CueIdentifierDescriptor(byte[] stream, int start)
            : base(stream, start)
        {
            CueStreamType = stream[start + 2];
        }

        public byte CueStreamType { get; }
        public string CueStreamTypeString => CueStreamTypeDescription(CueStreamType);
    }
}
