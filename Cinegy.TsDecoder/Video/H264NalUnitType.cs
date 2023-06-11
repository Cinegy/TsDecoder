/* Copyright 2016-2023 Cinegy GmbH.

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

namespace Cinegy.TsDecoder.Video;

public enum H264NalUnitType
{
    Unspecified = 0,
    CodedSliceNonIdrPicture = 1,
    CodedSliceDataPartitionA = 2,
    CodedSliceDataPartitionB= 3,
    CodedSliceDataPartitionC = 4,
    CodedSliceIdrPictureSlice = 5,
    SupplementalEnhancementInfo = 6,
    SequenceParameterSet = 7,
    PictureParameterSet = 8,
    AccessUnit = 9,
    EndOfSequence = 10,
    EndOfStream = 11,
    Filler = 12,
    SpsExtension = 13,
    PrefixNalUnit = 14,
    SubsetSps = 15,
    DepthParameterSet = 16,
    Reserved17 = 17,
    Reserved18 = 18,
    CodedSliceAuxCodedPicture = 19,
    CodedSliceExtension = 20,
    CodedSliceExtensionDepthViewComponent = 21,
    Reserved22 = 22,
    Reserved23 = 23,
    Reserved24 = 24,
    Unspecified25 = 25,
    Unspecified26 = 26,
    Unspecified27 = 27,
    Unspecified28 = 28,
    Unspecified29 = 29,
    Unspecified30 = 30,
    Unspecified31 = 31
}