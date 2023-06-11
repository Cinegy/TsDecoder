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
    public enum PesStreamTypes
    {
        ProgramStreamMap = 0xBC,
        PrivateStream1 = 0xBD,
        PaddingStream = 0xBE,
        PrivateStream2 = 0xBF,
        ECMStream = 0xF0,
        EMMStream = 0xF1,
        DSMCCStream = 0xF2,
        IEC13522Stream = 0xF3,
        H2221TypeAStream = 0xF4,
        H2221TypeBStream = 0xF5,
        H2221TypeCStream = 0xF6,
        H2221TypeDStream = 0xF7,
        H2221TypeEStream = 0xF8,
        AncillaryStream = 0xF9,
        IEC144961SLPacketizedStream = 0xFA,
        IEC144961FlexMuxStream = 0xFB,
        ProgramStreamDirectory = 0xFF
    }


}
