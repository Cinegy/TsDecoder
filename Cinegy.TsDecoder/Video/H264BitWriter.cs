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

using Cinegy.TsDecoder.DataAccess;

namespace Cinegy.TsDecoder.Video;

public class H264BitWriter : BitWriter
{
    public H264BitWriter(byte[] buf) : base(buf)
    {
    }

    protected override void Write_Byte(byte val)
    {
        if (val <= 1 && BytePos >= 2 && Buffer[BytePos - 1] == 0 && Buffer[BytePos - 2] == 0)
            Buffer[BytePos++] = 03;

        Buffer[BytePos++] = val;
    }

    public void Put_StartCode(int len)
    {
        Align();

        for (var i = 0; i < len - 1; i++)
            base.Write_Byte(0);

        base.Write_Byte(1);
    }

    public void Put_UE(uint val)
    {
        if (val == 0)
        {
            Put_Bits(1, 1);
            return;
        }

        val += 1;
        var lsbPos = __bsr(val);

        Put_Bits(val, lsbPos * 2 + 1);
    }

    public void Put_SE(int val)
    {
        if (val > 0)
            Put_UE((uint)(val * 2 - 1));
        else
            Put_UE((uint)(-val * 2));
    }
    
#if true
    private static int __bsr(uint v)
    {
        var r = 0;
        while ((v >>= 1) != 0)
        {
            r++;
        }
        return r;
    }
#else
        private uint LeadingZeros(uint x)
        {
            x |= (x >> 1);
            x |= (x >> 2);
            x |= (x >> 4);
            x |= (x >> 8);
            x |= (x >> 16);
            return (sizeof(int) * 8 - Ones(x));
        }
        private uint Ones(uint x)
        {
            x -= ((x >> 1) & 0x55555555);
            x = (((x >> 2) & 0x33333333) + (x & 0x33333333));
            x = (((x >> 4) + x) & 0x0f0f0f0f);
            x += (x >> 8);
            x += (x >> 16);
            return (x & 0x0000003f);
        }
#endif
}