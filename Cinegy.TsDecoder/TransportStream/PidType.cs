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
    public enum PidType
    {
        PatPid = 0x0,
        CatPid = 0x1,
        TsDescPid = 0x2,
        NitPid = 0x10,
        SdtBatPid = 0x11,
        EitPid = 0x12,
        NullPid = 0x1FFF
    }


}
