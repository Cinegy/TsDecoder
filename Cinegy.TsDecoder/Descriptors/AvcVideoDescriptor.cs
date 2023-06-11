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

public class AvcVideoDescriptor : Descriptor
{
    public byte ProfileIdc { get; set; }

    public bool ConstraintSet0Flag { get; set; }

    public bool ConstraintSet1Flag { get; set; }
    
    public bool ConstraintSet2Flag { get; set; }
    
    public bool ConstraintSet3Flag { get; set; }
    
    public bool ConstraintSet4Flag { get; set; }

    public bool ConstraintSet5Flag { get; set; }
    
    public byte AvcCompatibleFlags { get; set; }

    public byte LevelIdc { get; set; }

    public bool AvcStillPresentFlag { get; set; }

    public bool Avc24HourPictureFlag { get; set; }

    public bool FramePackingSeiNotPresentFlag { get; set; }


    public AvcVideoDescriptor(byte[] stream, int start) : base(stream, start)
    {  
        var idx = start + 2;
        ProfileIdc = stream[idx++];
        
        ConstraintSet0Flag = (stream[idx] & 0b10000000) == 0b10000000;
        ConstraintSet1Flag = (stream[idx] & 0b01000000) == 0b01000000;
        ConstraintSet2Flag = (stream[idx] & 0b00100000) == 0b00100000;
        ConstraintSet3Flag = (stream[idx] & 0b00010000) == 0b00010000;
        ConstraintSet4Flag = (stream[idx] & 0b00001000) == 0b00001000;
        ConstraintSet5Flag = (stream[idx] & 0b00000100) == 0b00000100;
        AvcCompatibleFlags = (byte)(stream[idx++] & 0b00000011);

        LevelIdc = stream[idx++];
        
        AvcStillPresentFlag = (stream[idx] & 0b10000000) == 0b10000000;
        Avc24HourPictureFlag = (stream[idx] & 0b01000000) == 0b01000000;
        FramePackingSeiNotPresentFlag = (stream[idx] & 0b00100000) == 0b00100000;
    }
}