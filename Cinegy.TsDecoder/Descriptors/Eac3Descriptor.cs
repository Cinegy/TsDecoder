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

namespace Cinegy.TsDecoder.Descriptors
{
    /// <summary>
    /// A Enhanced AC3 Descriptor <see cref="Descriptor"/>.
    /// </summary>
    /// <remarks>
    /// For details please refer to the original documentation,
    /// e.g. <i>ETSI EN 300 468 V1.15.1 (2016-03)</i> or alternate versions.
    /// </remarks>
    public class Eac3Descriptor : Descriptor
    {
        /*
           * EAC-3_descriptor()
           * {    
           * descriptor_tag 8 uimsbf  
           * descriptor_length 8 uimsbf  
           * component_type_flag 1 bslbf  
           * bsid_flag 1 bslbf  
           * mainid_flag 1 bslbf  
           * asvc_flag 1 bslbf  
           * mixinfoexists 1 bslbf  
           * substream1_flag 1 bslbf  
           * substream2_flag 1 bslbf  
           * substream3_flag 1 bslbf 
           * if (component_type_flag == 1)
           *      { 8 uimsbf   component_type    }    
           *  if (bsid_flag == 1)
           *      { 8 uimsbf   bsid    }    
           *  if (mainid_flag == 1)
           *      { 8 uimsbf   mainid    }    
           *  if (asvc_flag == 1)
           *      { 8 uimsbf   asvc    }    
           *  if (substream1_flag == 1)
           *      { 8 uimsbf   substream1    }    
           *  if (substream2_flag == 1)
           *      { 8 uimsbf   substream2    }    
           *  if (substream3_flag == 1)
           *      { 8 uimsbf   substream3    }  
           *  for(i=0;i<N;i++)
           *      { 8 uimsbf   additional_info_byte    }   }
        */

        public bool ComponentTypeFlag { get; }
        public bool BsIdFlag { get; }
        public bool MainIdFlag { get; }
        public bool AsvcFlag { get; }
        public bool MixInfoExists { get; }
        public bool Substream1Flag { get; }
        public bool Substream2Flag { get; }
        public bool Substream3Flag { get; }
        public byte ComponentType { get; }
        public byte BsId { get; }
        public byte MainId { get; }
        public byte Asvc { get; }
        public byte Substream1 { get; }
        public byte Substream2 { get; }
        public byte Substream3 { get; }

        public Eac3Descriptor(byte[] stream, int start)
            : base(stream, start)
        {

            var idx = start + 2;
            ComponentTypeFlag = (stream[idx] & 0x80) == 0x80;
            BsIdFlag = (stream[idx] & 0x40) == 0x40;
            MainIdFlag = (stream[idx] & 0x20) == 0x20;
            AsvcFlag = (stream[idx] & 0x10) == 0x10;
            MixInfoExists = (stream[idx] & 0x08) == 0x08;
            Substream1Flag = (stream[idx] & 0x04) == 0x04;
            Substream2Flag = (stream[idx] & 0x02) == 0x02;
            Substream3Flag = (stream[idx++] & 0x01) == 0x01;

            if (ComponentTypeFlag) ComponentType = stream[idx++];
            if (BsIdFlag) BsId = stream[idx++];
            if (MainIdFlag) MainId = stream[idx++];
            if (AsvcFlag) Asvc = stream[idx++];

            if (Substream1Flag) Substream1 = stream[idx++];
            if (Substream2Flag) Substream2 = stream[idx++];
            if (Substream3Flag) Substream3 = stream[idx];

        }


    }
}
