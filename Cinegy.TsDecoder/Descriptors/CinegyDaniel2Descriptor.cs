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

using System.Text;
using Cinegy.TsDecoder.TransportStream;

namespace Cinegy.TsDecoder.Descriptors
{
    public class CinegyDaniel2Descriptor : RegistrationDescriptor //if format-identifier / organization tag == 2LND
    {
        public CinegyDaniel2Descriptor(byte[] stream, int start) : base(stream, start)
        {
            //TODO:Set correct length check
            if (!(AdditionalIdentificationInfo?.Length < 30)) return;

            FourCc = Encoding.ASCII.GetString(stream, start + 2, 4);
            Version = stream[start + 6];
            HdrSize = stream[start + 7];
            Flags = Utils.Convert2BytesToUshort(stream, start + 8);
            Width = Utils.Convert4BytesToUint(stream, start + 10);
            Height = Utils.Convert4BytesToUint(stream, start + 14);
            FrameRate = Utils.Convert4BytesToUint(stream, start + 18);
            AspectRatio = Utils.Convert4BytesToUint(stream, start + 22);
            MainQuantizer = Utils.Convert4BytesToUint(stream, start + 26); //this is very wrong, i cannot just jam a float in here like this :-)

            //todo: fill in the rest of all this
            ChromaQuantizerAdd = 0;// 1 percents. ChromaQuantizer = MainQuantizer * (100 + ChromaQuantizerAdd) / 100
            AlphaQuantizerAdd = 0;// 1 percents. AlphaQuantizer = MainQuantizer * (100 + AlphaQuantizerAdd ) / 100
            Orientation = 0; // 1 see ORIENTATION
            InterlaceType = 0; // 1 see INTERLACE_TYPE
            ChromaFormat = 0; // 1 see CHROMA_FORMAT
            BitDepth = 0;       // 1 Main Bitdepth                   
            VideoFormat = 0;     // 1
            ColorPrimaries = 0;    // 1
            TransferCharacteristics = 0;// 1
            MatrixCoefficients = 0;  // 1
            MaxFrameSize = 0; // 4 if 0 - unspecified (VBR), if != 0 - specifies max public byte rate and CBR
            EncodeMethod = 0; // 1
            FrameType = 0; // 1 0 - total Intra, 1 - can have Delta-blocks, 2 - total Delta
            TempRef = 0;       // 1
            TimeCode = 0;       // 4 
            FrameSize = 0; // 4 12 Coded frame size (in bytes), including header
            NumExtraRecords = 0;// 1
            ExtraRecordsSize = 0;// 2
        }

        public override string LongName => "Cinegy Daniel2 Coded Video";

        public override string Name => "Daniel2 Video";

        public string FourCc { get; }       // 4 'D''N''L''2'                                      

        public byte Version { get; }        // 1 Coded codec version                               
        public byte HdrSize { get; }        // 1 size of header, not including extra records
        public ushort Flags { get; }        // 2 see D2_FLAGS

        public uint Width { get; }          // 4 Frame width                                       
        public uint Height { get; }         // 4 Frame height                                      

        public uint FrameRate { get; }      // 4 bits 0..19 are numerator, bits 20..31 are denominator (assuming 0 == 1 for denom).
        public uint AspectRatio { get; }    // 4 bits 0..15 are numerator, bits 16..31 are denominator

        public float MainQuantizer { get; } // 4 Main Quantizer
        public byte ChromaQuantizerAdd { get; } // 1 percents. ChromaQuantizer = MainQuantizer * (100 + ChromaQuantizerAdd) / 100
        public byte AlphaQuantizerAdd { get; } // 1 percents. AlphaQuantizer  = MainQuantizer * (100 + AlphaQuantizerAdd ) / 100

        public byte Orientation { get; } // 1 see ORIENTATION
        public byte InterlaceType { get; } // 1 see INTERLACE_TYPE

        public byte ChromaFormat { get; } // 1 see CHROMA_FORMAT
        public byte BitDepth { get; } // 1 Main Bitdepth                                     

        public byte VideoFormat { get; }
        public byte ColorPrimaries { get; }
        public byte TransferCharacteristics { get; }
        public byte MatrixCoefficients { get; }

        public uint MaxFrameSize { get; } // 4 if 0 - unspecified (VBR), if != 0 - specifies max public byte rate and CBR

        // frame parameters
        public byte EncodeMethod { get; }   // 1
        public byte FrameType { get; }      // 1 0 - total Intra, 1 - can have Delta-blocks, 2 - total Delta
        public byte TempRef { get; }        // 1

        public uint TimeCode { get; }       // 4 
        public uint FrameSize { get; }      // 4 12 Coded frame size (in bytes), including header

        public byte NumExtraRecords { get; } // 1
        public ushort ExtraRecordsSize { get; } // 2

        //TODO: work out what to do with this - probably just delete...
        // public byte __dummy[8]{ get; };     // 

    }

}
