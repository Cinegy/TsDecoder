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

using System;
using System.Collections.Generic;
using Cinegy.TsDecoder.Descriptors;
using Cinegy.TsDecoder.TransportStream;

namespace Cinegy.TsDecoder.Tables
{
    public class TableFactory
    {
        private ushort _tableBytes;
        internal byte[] Data;

        public int CorruptedPackets { get; internal set; } = 0;

        public int TablePid { get; private set; } = -1;

        protected ITable InProgressTable { get; set; }
        
        public delegate void TableChangeEventHandler(object sender, TransportStreamEventArgs args);
        
        public void ResetCounters()
        {
            CorruptedPackets = 0;
        }

        protected int GetDescriptors(byte descriptorsLength, int startOfNextField)
        {
            var descriptors = new List<Descriptor>();
            var startPos = startOfNextField;
            while (startOfNextField < startPos + descriptorsLength)
            {
                if (startOfNextField > Data.Length)
                {
                    return -1;
                }
                var des = DescriptorFactory.DescriptorFromData(Data, startOfNextField);
                descriptors.Add(des);

                startOfNextField += des.DescriptorLength + 2;
            }

            InProgressTable.Descriptors = descriptors;

            return startOfNextField;
        }
        
        internal bool HasAllBytes()
        {
            return _tableBytes >= InProgressTable.SectionLength + 3 && InProgressTable.SectionLength > 0;
        }

        internal void CheckPid(int newPid)
        {
            if (TablePid == newPid) return;

            if (TablePid == -1)
            {
                TablePid = newPid;
            }
            else
            {
                CorruptedPackets++;
                #if DEBUG
                Console.WriteLine("TableFactory cannot have mixed PIDs added after startup");
                #endif
            }
        }

        protected void AddData(TsPacket packet)
        {
            CheckPid(packet.Pid);

            if (packet.PayloadUnitStartIndicator)
            {
                Data = new byte[InProgressTable.SectionLength + 3];
                _tableBytes = 0;
            }

            if (packet.PayloadLen == 0)
            {
                CorruptedPackets++;
                return;
            }

            if ((InProgressTable.SectionLength + 3 - _tableBytes) > packet.PayloadLen)
            {
                Buffer.BlockCopy(packet.Payload, 0, Data, _tableBytes, packet.PayloadLen);
                _tableBytes += (ushort)(packet.PayloadLen);
            }
            else
            {
                Buffer.BlockCopy(packet.Payload, 0, Data, _tableBytes, (InProgressTable.SectionLength + 3 - _tableBytes));
                _tableBytes += (ushort)(InProgressTable.SectionLength + 3 - _tableBytes);
            }
        }
        
        // The associated table has changed / been updated
        public event TableChangeEventHandler TableChangeDetected;


        protected void OnTableChangeDetected()
        {
            var handler = TableChangeDetected;
            if (handler == null) return;

            var generatingPid = -1;

            if (InProgressTable != null)
            {
                generatingPid = InProgressTable.Pid;
            }
            
            var args = new TransportStreamEventArgs { TsPid = generatingPid };
            handler(this, args);
        }
    }
}
