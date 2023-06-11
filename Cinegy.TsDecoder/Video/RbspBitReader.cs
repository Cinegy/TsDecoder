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

using System;
using Cinegy.TsDecoder.DataAccess;

namespace Cinegy.TsDecoder.Video;

public class RbspBitReader : BitReader
{
    //public byte[] RbspData { get; private set; }

    public RbspBitReader(byte[] buf) : base(buf)
    {
        //RbspData = new byte[buf.Length];
        //var bw = new BitWriter(RbspData);
        //while (More_RBSP_Data())
        //{
        //    bw.Put_Bool(Get_Bool());
        //}
    }
    
    public bool RBSP_Trailing_Bits()
    {
        if (BitsLeft <= 0)
            return false;

        var rbsp_stop_bit = Get_Bits(1);
        //int rbsp_alignment_zero_bits = Show_Bits(BitsToAlign());
        var rbsp_zero_bits = Show_Bits(Math.Min(23, BitsLeft)); // aligning_bits + next_start_code_bits

        Unget_Bits(1);

        return rbsp_stop_bit == 1 && rbsp_zero_bits == 0;
    }
    
    public bool More_RBSP_Data()
    {
        if (BitsLeft <= 0)
            return false;

        return !RBSP_Trailing_Bits();
    }
}