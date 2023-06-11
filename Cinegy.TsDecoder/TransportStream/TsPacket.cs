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

namespace Cinegy.TsDecoder.TransportStream
{

    public struct TsPacket
    {
        public byte SyncByte; //should always be 0x47 - indicates start of a TS packet
        public bool TransportErrorIndicator; //Set when a demodulator can't correct errors from FEC data - this would inform a stream processor to ignore the packet
        public bool PayloadUnitStartIndicator; //true = the start of PES data or PSI otherwise zero only. 
        public bool TransportPriority; //true = the current packet has a higher priority than other packets with the same PID.
        public ushort Pid; //Packet identifier flag, used to associate one packet with a set
        public byte ScramblingControl; // '00' = Not scrambled, For DVB-CSA only:'01' = Reserved for future use, '10' = Scrambled with even key, '11' = Scrambled with odd key
        public bool AdaptationFieldExists; // Combined with Contains Payload flag - '00' = Reserved, '01' = Payload only, '10' = Adaptation field only, '11' = Adaptation field and payload
        public bool ContainsPayload;
        public byte ContinuityCounter;
        public PesHdr PesHeader;
        public byte[] Payload;
        public int PayloadLen; //length of valid data within Payload field (source data array may be oversized if rented)
        public AdaptationField AdaptationField;
        public byte[] SourceData; //original data used to construct packet (if chosen to retain)
        public int SourceDataLen; //length of valid data within SourceData field (source data array may be oversized if rented)
        public int SourceBufferIndex; //index into original data buffer used to construct packet
    }


}
