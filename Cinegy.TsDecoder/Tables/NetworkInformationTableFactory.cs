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
using System.Diagnostics;
using System.IO;
using Cinegy.TsDecoder.TransportStream;


namespace Cinegy.TsDecoder.Tables
{
    class NetworkInformationTableFactory : TableFactory
    {
        /// <summary>
        /// The last decoded EventInformation table, with the EventInformationItems associated with that table section
        /// </summary>
        public NetworkInformationTable NetworkInformationTable { get; private set; }

        /// <summary>
        /// An agregated list of all current EventInformationItems, as pulled from all EventInformationTables with the same ID and version.
        /// </summary>
        public List<NetworkInformationItem> NetworkInformationItems { get; set; } = new List<NetworkInformationItem>();

        private new NetworkInformationTable InProgressTable
        {
            get { return base.InProgressTable as NetworkInformationTable; }
            set { base.InProgressTable = value; }
        }

        private HashSet<int> _sectionsCompleted = new HashSet<int>();

        public void AddPacket(TsPacket packet)
        {
            CheckPid(packet.Pid);

            if (packet.PayloadUnitStartIndicator)
            {
                InProgressTable = new NetworkInformationTable { Pid = packet.Pid, PointerField = packet.Payload[0] };

                if (InProgressTable.PointerField > packet.Payload.Length)
                {
                    Debug.Assert(true, "Network Information Table has packet pointer outside the packet.");
                }

                var pos = 1 + InProgressTable.PointerField;

                InProgressTable.VersionNumber = (byte)(packet.Payload[pos + 5] & 0x3E);

                InProgressTable.TableId = packet.Payload[pos];

                //TODO: Refactor with enum for well-known table IDs, and add option below as filter
                if (InProgressTable.TableId != 0x40)
                {
                    InProgressTable = null;
                    return;
                }

                if (NetworkInformationTable?.VersionNumber != InProgressTable.VersionNumber)
                {
                    //if the version number of any section jumps, we need to refresh
                    _sectionsCompleted = new HashSet<int>();
                    NetworkInformationItems = new List<NetworkInformationItem>();
                }

                InProgressTable.SectionLength =
                    (short)(((packet.Payload[pos + 1] & 0x3) << 8) + packet.Payload[pos + 2]);

                InProgressTable.TransportStreamId = (ushort)((packet.Payload[pos + 3] << 8) + packet.Payload[pos + 4]);
                InProgressTable.CurrentNextIndicator = (packet.Payload[pos + 5] & 0x1) != 0;
                InProgressTable.SectionNumber = packet.Payload[pos + 6];
                InProgressTable.LastSectionNumber = packet.Payload[pos + 7];              
            }

            if (InProgressTable == null) return;

            if (_sectionsCompleted.Contains(InProgressTable.SectionNumber))
            {
                InProgressTable = null;
                return;
            }

            AddData(packet);

            if (!HasAllBytes()) return;

            InProgressTable.NetworkDescriptorsLength = (ushort)(((Data[InProgressTable.PointerField + 9] & 0x3) << 8) + Data[InProgressTable.PointerField + 10]);

            var startOfNextField = (ushort)(InProgressTable.PointerField + 11);

            List<Descriptor> descriptors = new List<Descriptor>();
            var endOfDescriptors = InProgressTable.PointerField + 11 + InProgressTable.NetworkDescriptorsLength;

            if (endOfDescriptors > Data.Length)
            {
                throw new InvalidDataException("Descriptor data in Network Information is marked beyond available data");
            }

            while (startOfNextField < endOfDescriptors)
            {
                Descriptor des = DescriptorFactory.DescriptorFromData(Data, startOfNextField);
                descriptors.Add(des);
                startOfNextField += (byte)(des.DescriptorLength + 2);
            }
            InProgressTable.Descriptors = descriptors;

            InProgressTable.TransportStreamLoopLength = (ushort)(((Data[startOfNextField] & 0x3) << 8) + Data[startOfNextField + 1]);
            
            startOfNextField += 2;
            var transportStreamLoopEnd = (byte)(startOfNextField + InProgressTable.TransportStreamLoopLength);

            var items = new List<NetworkInformationItem>();

            while (startOfNextField < transportStreamLoopEnd)
            {
                var item = new NetworkInformationItem
                {
                    TransportStreamId = (ushort)((Data[startOfNextField] << 8) + Data[startOfNextField + 1]),
                    OriginalNetworkId = (ushort)((Data[startOfNextField + 2] << 8) + Data[startOfNextField + 3]),
                    ReservedFutureUse = (byte)((Data[startOfNextField + 4] >> 4) & 0x0F),
                    TransportDescriptorsLength = (ushort)(((Data[startOfNextField + 4] & 0x3) << 8) + Data[startOfNextField + 5])
                };

                descriptors = new List<Descriptor>();

                startOfNextField = (byte)(startOfNextField + 6);
                endOfDescriptors = (byte)(startOfNextField + item.TransportDescriptorsLength);

                if (endOfDescriptors > Data.Length)
                {
                    throw new InvalidDataException("Descriptor data in Network Information Item is marked beyond available data");
                }

                while (startOfNextField < endOfDescriptors)
                {
                    Descriptor des = DescriptorFactory.DescriptorFromData(Data, startOfNextField);
                    descriptors.Add(des);
                    startOfNextField += (byte)(des.DescriptorLength + 2);
                }
                item.Descriptors = descriptors;
                items.Add(item);
            }

            InProgressTable.Items = items;

            NetworkInformationItems.AddRange(items);

            NetworkInformationTable = InProgressTable;
            _sectionsCompleted.Add(InProgressTable.SectionNumber);

            OnTableChangeDetected();
        }
    }
}
