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

namespace Cinegy.TsDecoder.Tables
{
    public class SpliceTime
    {
       public bool TimeSpecifiedFlag { get; set; }
       public ulong PTSTime { get; set; }
       public System.String SpliceTimeString {
            get {
                long t_stamp = (long)PTSTime;
                int hour = (int)(t_stamp / 324000000);
                long r = t_stamp % 324000000;
                int minute = (int)(r / 5400000);
                r = r % 5400000;
                int second = (int)(r / 90000);
                r = r % 90000;
                int millis = (int)(r / 90);                

                return string.Format("{0:D2}:{1:D2}:{2:D2}.{3:D2}", hour, minute, second, millis);              
            }
        }
    }

    public class BreakDuration
    {
        public bool AutoReturn { get; set; }
        public ulong Duration { get; set; }
    }

    public interface ISplice
    {

    }

    public class SpliceInsert : ISplice
    {
        public class Component
        {
            public byte ComponentTag { get; set; }
            public SpliceTime SpliceTime { get; set; }
        }

        public ulong SpliceEventId { get; set; }
        public bool SpliceEventCancelIndicator { get; set; }
        public bool OutOfNetworkIndicator { get; set; }
        public bool ProgramSpliceFlag { get; set; }
        public bool DurationFlag { get; set; }
        public bool SpliceImmediateFlag { get; set; }
        public SpliceTime SpliceTime { get; set; }
        public byte ComponentCount { get; set; }
        public System.Collections.Generic.IEnumerable<Component> Components { get; set; }
        public BreakDuration BreakDuration { get; set; }
        public ushort UniqueProgramId { get; set; }
        public byte AvailNum { get; set; }
        public byte AvailsExpected { get; set; }
    }

    public class SpliceInfoTable : Table
    {
        public byte ProtocolVersion { get; set; }
        public bool EncryptedPacket { get; set; }
        public byte EncryptionAlgorithm { get; set; }
        public ulong PTSAdjustment { get; set; }
        public byte CWIndex { get; set; }
        public ushort Tier { get; set; }
        public ushort SpliceCommandLength { get; set; }
        public ushort SpliceCommandType { get; set; }
        public ISplice Splice { get; set; }
    }
}
