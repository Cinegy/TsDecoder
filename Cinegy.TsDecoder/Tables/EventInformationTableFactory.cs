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
    public class EventInformationTableFactory : TableFactory
    {
        /// <summary>
        /// The last decoded EventInformation table, with the EventInformationItems associated with that table section
        /// </summary>
        public EventInformationTable EventInformationTable { get; private set; }

        /// <summary>
        /// An agregated list of all current EventInformationItems, as pulled from all EventInformationTables with the same ID and version.
        /// </summary>
        public List<EventInformationItem> EventInformationItems { get; set; } = new List<EventInformationItem>();

        private new EventInformationTable InProgressTable
        {
            get { return base.InProgressTable as EventInformationTable; }
            set { base.InProgressTable = value; }
        }

        private HashSet<int> _sectionsCompleted = new HashSet<int>();

        public void AddPacket(TsPacket packet)
        {
            CheckPid(packet.Pid);

            if (packet.PayloadUnitStartIndicator)
            {
                InProgressTable = new EventInformationTable { Pid = packet.Pid, PointerField = packet.Payload[0] };

                if (InProgressTable.PointerField > packet.Payload.Length)
                {
                    Debug.Assert(true, "Event Information Table has packet pointer outside the packet.");
                }

                var pos = 1 + InProgressTable.PointerField;

                InProgressTable.VersionNumber = (byte)(packet.Payload[pos + 5] & 0x3E);

                InProgressTable.TableId = packet.Payload[pos];

                //TODO: Refactor with enum for well-known table IDs, and add option below as filter
                if (InProgressTable.TableId != 0x4e)
                {
                    InProgressTable = null;
                    return;
                }

                if (EventInformationTable?.VersionNumber != InProgressTable.VersionNumber)
                {
                    //if the version number of any section jumps, we need to refresh
                    _sectionsCompleted = new HashSet<int>();
                    EventInformationItems = new List<EventInformationItem>();
                }

                InProgressTable.SectionLength =
                    (short)(((packet.Payload[pos + 1] & 0x3) << 8) + packet.Payload[pos + 2]);

                InProgressTable.SericeId = (ushort)((packet.Payload[pos + 3] << 8) + packet.Payload[pos + 4]);
                InProgressTable.CurrentNextIndicator = (packet.Payload[pos + 5] & 0x1) != 0;
                InProgressTable.SectionNumber = packet.Payload[pos + 6];
                InProgressTable.LastSectionNumber = packet.Payload[pos + 7];
                InProgressTable.TransportStreamId = (ushort)((packet.Payload[pos + 8] << 8) + packet.Payload[pos + 9]);
                InProgressTable.OriginalNetworkId = (ushort)((packet.Payload[pos + 10] << 8) + packet.Payload[pos + 11]);
            }

            if (InProgressTable == null) return;

            if (_sectionsCompleted.Contains(InProgressTable.SectionNumber))
            {
                InProgressTable = null;
                return;
            }

            AddData(packet);

            if (!HasAllBytes()) return;

            

            InProgressTable.SegmentLastSectionNumber = (Data[InProgressTable.PointerField + 13]);
            InProgressTable.LastTableId = (Data[InProgressTable.PointerField + 14]);

            var startOfNextField = (ushort)(InProgressTable.PointerField + 15);

            var transportStreamLoopEnd = (ushort)(InProgressTable.SectionLength - 4);

            var items = new List<EventInformationItem>();

            while (startOfNextField < transportStreamLoopEnd)
            {
                var item = new EventInformationItem
                {
                    EventId = (ushort)((Data[startOfNextField] << 8) + Data[startOfNextField + 1]),
                    StartTime = (ulong)(((ulong)(Data[startOfNextField + 2]) << 32) + ((ulong)(Data[startOfNextField + 3]) << 24) + ((ulong)(Data[startOfNextField + 4]) << 16) + ((ulong)(Data[startOfNextField + 5]) << 8) + ((ulong)(Data[startOfNextField + 6]))),
                    Duration = (uint)((Data[startOfNextField + 7] << 16) + (Data[startOfNextField + 8] << 8) + Data[startOfNextField + 9]),
                    RunningStatus = (byte)((Data[startOfNextField + 10] >> 5) & 0x07),
                    FreeCAMode = (bool)((Data[startOfNextField + 10] & 0x10) == 0x10),
                    DescriptorsLoopLength = (ushort)(((Data[startOfNextField + 10] & 0x3) << 8) + Data[startOfNextField + 11])
                };

                var descriptors = new List<Descriptor>();

                startOfNextField = (ushort)(startOfNextField + 12);
                var endOfDescriptors = (ushort)(startOfNextField + item.DescriptorsLoopLength);

                if (endOfDescriptors > Data.Length)
                {
                    throw new InvalidDataException("Descriptor data in Event Information is marked beyond available data");
                }

                while (startOfNextField < endOfDescriptors)
                {
                    var des = DescriptorFactory.DescriptorFromData(Data, startOfNextField);
                    descriptors.Add(des);
                    startOfNextField += (ushort)(des.DescriptorLength + 2);
                }
                item.Descriptors = descriptors;
                items.Add(item);
            }

            InProgressTable.Items = items;

            EventInformationItems.AddRange(items);
            
            if(InProgressTable.VersionNumber == EventInformationTable.VersionNumber) return;

            EventInformationTable = InProgressTable;
            _sectionsCompleted.Add(InProgressTable.SectionNumber);

            OnTableChangeDetected();
        }
    }
}
