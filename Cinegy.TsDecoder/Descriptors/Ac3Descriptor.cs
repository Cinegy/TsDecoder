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
    /// A AC3 Descriptor <see cref="Descriptor"/>.
    /// </summary>
    /// <remarks>
    /// For details please refer to the original documentation,
    /// e.g. <i>ETSI EN 300 468 V1.15.1 (2016-03)</i> or alternate versions.
    /// </remarks>
    public class Ac3Descriptor : Descriptor
    {
        /*
           * AC-3_descriptor()
           * {    
           * descriptor_tag 8 uimsbf  
           * descriptor_length 8 uimsbf  
           * component_type_flag 1 bslbf  
           * bsid_flag 1 bslbf  
           * mainid_flag 1 bslbf  
           * asvc_flag 1 bslbf  
           * reserved_flags 4 bslbf  
           * if (component_type_flag == 1)
           *      * { 8 uimsbf   component_type    }    
           *  if (bsid_flag == 1)
           *      { 8 uimsbf   bsid    }    
           *  if (mainid_flag == 1)
           *      { 8 uimsbf   mainid    }    
           *  if (asvc_flag == 1)
           *      { 8 uimsbf   asvc    }    
           *  for(i=0;i<N;i++)
           *      { 8 uimsbf   additional_info_byte    }   }
        */

        public bool ComponentTypeFlag { get; }
        public bool BsIdFlag { get; }
        public bool MainIdFlag { get; }
        public bool AsvcFlag { get; }

        public byte ComponentType { get; }
        public byte BsId { get; }
        public byte MainId { get; }
        public byte Asvc { get; }

        public Ac3Descriptor(byte[] stream, int start)
            : base(stream, start)
        {
            var idx = start + 2;
            ComponentTypeFlag = (stream[idx] & 0x80) == 0x80;
            BsIdFlag = (stream[idx] & 0x40) == 0x40;
            MainIdFlag = (stream[idx] & 0x20) == 0x20;
            AsvcFlag = (stream[idx++] & 0x10) == 0x10;

            if (ComponentTypeFlag) ComponentType = stream[idx++];
            if (BsIdFlag) BsId = stream[idx++];
            if (MainIdFlag) MainId = stream[idx++];
            if (AsvcFlag) Asvc = stream[idx];
        }
    }
}
