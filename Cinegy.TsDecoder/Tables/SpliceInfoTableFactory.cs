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
    public class SpliceInfoTableFactory : TableFactory
    {
        /// <summary>
        /// The last decoded EventInformation table, with the EventInformationItems associated with that table section
        /// </summary>
        public SpliceInfoTable SpliceInfoTable { get; private set; }
     
        private new SpliceInfoTable InProgressTable
        {
            get { return base.InProgressTable as SpliceInfoTable; }
            set { base.InProgressTable = value; }
        }

        private HashSet<int> _sectionsCompleted = new HashSet<int>();

        public void AddPacket(TsPacket packet)
        {
            CheckPid(packet.Pid);

            if (packet.PayloadUnitStartIndicator)
            {
                InProgressTable = new SpliceInfoTable { Pid = packet.Pid, PointerField = packet.Payload[0] };

                if (InProgressTable.PointerField > packet.Payload.Length)
                {
                    Debug.Assert(true, "Splice Info Table has packet pointer outside the packet.");
                }

                var pos = 1 + InProgressTable.PointerField;

               // InProgressTable.VersionNumber = (byte)(packet.Payload[pos + 5] & 0x3E);

                InProgressTable.TableId = packet.Payload[pos];

                //TODO: Refactor with enum for well-known table IDs, and add option below as filter
                if (InProgressTable.TableId != 0xFC)
                {
                    InProgressTable = null;
                    return;
                }

                /* if (SpliceInfoTable?.VersionNumber != InProgressTable.VersionNumber)
                 {
                     //if the version number of any section jumps, we need to refresh
                     _sectionsCompleted = new HashSet<int>();
                     NetworkInformationItems = new List<NetworkInformationItem>();
                 }*/

                InProgressTable.SectionLength =
                    (short)(((packet.Payload[pos + 1] & 0x3) << 8) + packet.Payload[pos + 2]);

               
            }

            if (InProgressTable == null) return;

           /* if (_sectionsCompleted.Contains(InProgressTable.SectionNumber))
            {
                InProgressTable = null;
                return;
            }*/

            AddData(packet);

            if (!HasAllBytes()) return;

            InProgressTable.ProtocolVersion = packet.Payload[InProgressTable.PointerField + 4];
            InProgressTable.EncryptedPacket = (packet.Payload[InProgressTable.PointerField + 5] & 0x80) == 0x80;
            InProgressTable.EncryptionAlgorithm = (byte)((packet.Payload[InProgressTable.PointerField + 5] >> 1) & 0x3F);
            InProgressTable.PTSAdjustment = (ulong)(((((ulong)packet.Payload[InProgressTable.PointerField + 5] >> 7) & 0x1) << 32) + ((ulong)packet.Payload[InProgressTable.PointerField + 6] << 24) + ((ulong)packet.Payload[InProgressTable.PointerField + 7] << 16) + ((ulong)packet.Payload[InProgressTable.PointerField + 8] << 8) + ((ulong)packet.Payload[InProgressTable.PointerField + 9]));
            InProgressTable.CWIndex = packet.Payload[InProgressTable.PointerField + 10];
            InProgressTable.Tier = (ushort)((packet.Payload[InProgressTable.PointerField + 11] << 4) + ((packet.Payload[InProgressTable.PointerField + 12] >> 4) & 0xF));
            InProgressTable.SpliceCommandLength = (ushort)(((packet.Payload[InProgressTable.PointerField + 12] & 0xF) << 8) + packet.Payload[InProgressTable.PointerField + 13]);
            InProgressTable.SpliceCommandType = packet.Payload[InProgressTable.PointerField + 14];

            var startOfNextField = (ushort)(InProgressTable.PointerField + 15);   
            
            if(InProgressTable.SpliceCommandType == 5)
            {
                var si = new SpliceInsert();
                si.SpliceEventId = (ulong)((packet.Payload[startOfNextField] << 24) + (packet.Payload[startOfNextField + 1] << 16) + (packet.Payload[startOfNextField + 2] << 8) + (packet.Payload[startOfNextField + 3]));
                si.SpliceEventCancelIndicator = (packet.Payload[startOfNextField + 4] & 0x80) == 0x80;

                if (!si.SpliceEventCancelIndicator)
                {
                    si.OutOfNetworkIndicator = si.SpliceEventCancelIndicator = (packet.Payload[startOfNextField + 5] & 0x80) == 0x80;
                    si.ProgramSpliceFlag = si.SpliceEventCancelIndicator = (packet.Payload[startOfNextField + 5] & 0x40) == 0x40;
                    si.DurationFlag = si.SpliceEventCancelIndicator = (packet.Payload[startOfNextField + 5] & 0x20) == 0x20;
                    si.SpliceImmediateFlag = si.SpliceEventCancelIndicator = (packet.Payload[startOfNextField + 5] & 0x10) == 0x10;
                    if(si.ProgramSpliceFlag && !si.SpliceImmediateFlag)
                    {
                        var st = new SpliceTime()
                        {
                            TimeSpecifiedFlag = (packet.Payload[startOfNextField + 6] & 0x80) == 0x80
                        };
                        if (st.TimeSpecifiedFlag)
                        {
                            st.PTSTime = (ulong)((((((ulong)packet.Payload[startOfNextField + 6] >> 7) & 0x1) << 32)) + ((ulong)packet.Payload[startOfNextField + 7] << 24) + ((ulong)packet.Payload[startOfNextField + 8] << 16) + ((ulong)packet.Payload[startOfNextField + 9] << 8) + ((ulong)packet.Payload[startOfNextField + 10]));                            
                        }

                        si.SpliceTime = st;
                    }
                    else if (!si.ProgramSpliceFlag)
                    {
                        si.ComponentCount = packet.Payload[startOfNextField + 6];
                        startOfNextField += 7;
                        List<SpliceInsert.Component> components = new List<SpliceInsert.Component>();
                        for (int i = 0; i < si.ComponentCount; i++)
                        {
                            SpliceInsert.Component component = new SpliceInsert.Component()
                            {
                                ComponentTag = packet.Payload[startOfNextField]
                            };

                            if (!si.SpliceImmediateFlag)
                            {
                                var st = new SpliceTime()
                                {
                                    TimeSpecifiedFlag = (packet.Payload[startOfNextField + 1] & 0x80) == 0x80
                                };
                                if (st.TimeSpecifiedFlag)
                                {
                                    st.PTSTime = (ulong)((((((ulong)packet.Payload[startOfNextField + 1] >> 7) & 0x1) << 32)) + ((ulong)packet.Payload[startOfNextField + 2] << 24) + ((ulong)packet.Payload[startOfNextField + 3] << 16) + ((ulong)packet.Payload[startOfNextField + 4] << 8) + ((ulong)packet.Payload[startOfNextField + 5]));
                                    startOfNextField += 5;
                                }
                                component.SpliceTime = st;
                                startOfNextField++;                               
                            }
                            components.Add(component);
                        }
                        si.Components = components;
                    }
                    if (si.DurationFlag)
                    {
                        var br = new BreakDuration()
                        {
                            AutoReturn = (packet.Payload[startOfNextField] & 0x80) == 0x80
                        };
                        br.Duration = (ulong)((((((ulong)packet.Payload[startOfNextField] >> 7) & 0x1) << 32)) + ((ulong)packet.Payload[startOfNextField + 1] << 24) + ((ulong)packet.Payload[startOfNextField + 2] << 16) + ((ulong)packet.Payload[startOfNextField + 3] << 8) + ((ulong)packet.Payload[startOfNextField + 4]));
                        startOfNextField += 5;
                    }

                    si.UniqueProgramId = (ushort)((packet.Payload[startOfNextField] << 8) + packet.Payload[startOfNextField + 1]);
                    si.AvailNum = packet.Payload[startOfNextField + 2];
                    si.AvailsExpected = packet.Payload[startOfNextField + 3];
                }

                InProgressTable.Splice = si;
            }     

            SpliceInfoTable = InProgressTable;          

            OnTableChangeDetected();
        }
    }
}
