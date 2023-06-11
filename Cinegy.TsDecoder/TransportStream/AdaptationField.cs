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
    public struct AdaptationField
    {
        public byte FieldSize;
        public bool DiscontinuityIndicator;
        public bool RandomAccessIndicator;
        public bool ElementaryStreamPriorityIndicator;
        public bool PcrFlag;
        public bool OpcrFlag;
        public bool SplicingPointFlag;
        public bool TransportPrivateDataFlag;
        public bool AdaptationFieldExtensionFlag;
        public ulong Pcr;
        public ulong Opcr;
    }


}
