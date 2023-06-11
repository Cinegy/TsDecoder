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

namespace Cinegy.TsDecoder.Descriptors;

public class HevcDescriptor : Descriptor
{
    public byte ProfileSpace { get; set; }
    
    public bool TierFlag { get; set; }
    
    public byte ProfileIdc { get; set; }

    public HevcDescriptor(byte[] stream, int start) : base(stream, start)
    {
        var idx = start + 2;

        ProfileSpace = (byte)((stream[idx] & 0b11000000) >> 6);
        TierFlag = (stream[idx] & 0b00100000) == 0b00100000;
        ProfileIdc = (byte)(stream[idx] & 0b00011111);
        //TODO: unpack the reset of the descriptor
    }
}