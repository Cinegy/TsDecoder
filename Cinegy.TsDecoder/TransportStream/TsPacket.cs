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

namespace Cinegy.TsDecoder.TransportStream
{
    public struct PesHdr
    {
        public long Pts;
        public long Dts;
        public int StartCode;
        public byte[] Payload;
    }

    public struct TsPacket
    {
        public byte SyncByte; //should always be 0x47 - indicates start of a TS packet
        public bool TransportErrorIndicator; //Set when a demodulator can't correct errors from FEC data - this would inform a stream processor to ignore the packet
        public bool PayloadUnitStartIndicator; //true = the start of PES data or PSI otherwise zero only. 
        public bool TransportPriority; //true = the current packet has a higher priority than other packets with the same PID.
        public short Pid; //Packet identifier flag, used to associate one packet with a set
        public short ScramblingControl; // '00' = Not scrambled, For DVB-CSA only:'01' = Reserved for future use, '10' = Scrambled with even key, '11' = Scrambled with odd key
        public bool AdaptationFieldExists;
        public bool ContainsPayload;
        public short ContinuityCounter;
        public PesHdr PesHeader;
        public byte[] Payload;
        public AdaptationField AdaptationField;
    }

    public struct AdaptationField
    {
        public int FieldSize;
        public bool DiscontinuityIndicator;
        public bool RandomAccessIndicator;
        public bool ElementaryStreamPriorityIndicator;
        public bool PcrFlag;
        public bool OpcrFlag;
        public bool SplicingPointFlag;
        public bool TransportPrivateDataFlag;
        public bool AdaptationFieldExtensionFlag;
        public ulong Pcr;
    }

    public enum PidType
    {
        PatPid = 0x0,
        CatPid = 0x1,
        TsDescPid = 0x2,
        NitPid = 0x10,
        SdtPid = 0x11,
        EitPid = 0x12,
        NullPid = 0x1FFF
    }


}
