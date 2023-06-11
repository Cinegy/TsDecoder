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

public class H264BitReader : BitReader
{
    public H264BitReader(byte[] buf) : base(buf)
    {
    }
    
    public uint Get_UE()
    {
        var leadingZeroBits = 0;

        while (Get_Bits(1) == 0)
            leadingZeroBits++;

        return leadingZeroBits == 0 ? 0 : (1u << leadingZeroBits) - 1 + Get_Bits(leadingZeroBits);
    }

    public int Get_SE()
    {
        var val = Get_UE();
        //return val == 0 ? 0 : (int)((val + 1) >> 1) * ((val & 1) != 0 ? 1 : -1);
        var sign = (val & 1) - 1;
        return val == 0 ? 0 : (int)((((val + 1) >> 1) ^ sign) - sign);
    }
    
    public uint FindStartCode()
    {
        Align();

        for (;;)
        {
            if (BitsLeft < 32)
                return 0;

            var code = Show_Bits32_Aligned();

            if (((code ^ ~(code + 0x7efefeff)) & 0x81010100) != 0)
            {
                if ((code & 0xFFFFFF00) == 0x00000100)
                    return code;

                if (code == 0x00000001)
                {
                    Flush_Bits(8);
                    code = Show_Bits32_Aligned();
                    Unget_Bits(8);
                    return code;
                }

                Flush_Bits(8); // going slow
                continue;
            }

            Flush_Bits(32);
        }
    }
    
    public int SkipStartCode()
    {
        if (!IsAligned())
            return 0;

        var code = Show_Bits32_Aligned();

        if ((code & 0xFFFFFF00) == 0x00000100)
        {
            Flush_Bits(32);
            return 32;
        }

        if (code == 0x00000001)
        {
            Flush_Bits(40);
            return 40;
        }

        return 0;
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