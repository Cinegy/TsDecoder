/*
   Copyright 2016-2023 Cinegy GmbH

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

namespace Cinegy.TsDecoder.DataAccess
{
    public class BitWriter
    {
        internal byte[] Buffer;
        private int _bitPos;
        
        public BitWriter(byte[] buf)
        {
            Buffer = buf;
        }
        
        public void Put_Bits(uint val, int n)
        {
            if (n == 0)
                return;

            var bitsLeft = 8 - _bitPos;

            if (n < bitsLeft)
            {
                val <<= bitsLeft - n;
                Buffer[BytePos] |= (byte)val;
                _bitPos += n;
                return;
            }

            var bigVal = (long)Buffer[BytePos] << n - bitsLeft | val;

            var nn = n + _bitPos;

            while (nn >= 8)
            {
                Write_Byte((byte)(bigVal >> nn - 8));
                nn -= 8;
            }

            _bitPos += n;
            _bitPos &= 7;

            if (nn != 0)
            {
                Buffer[BytePos] = (byte)(bigVal << 8 - _bitPos);
            }
        }
        
        protected virtual void Write_Byte(byte val)
        {
            Buffer[BytePos++] = val;
        }
        
        public void Put_Bits32_Aligned(uint val)
        {
            Align();
            Write_Byte((byte)(val >> 24));
            Write_Byte((byte)(val >> 16));
            Write_Byte((byte)(val >> 8));
            Write_Byte((byte)(val >> 0));
        }
        
        public bool Put_Bool(bool val)
        {
            Put_Bits(val ? 1u : 0u, 1);
            return val;
        }
        
        private int BitsToAlign()
        {
            return _bitPos - 1 & 7 ^ 7;
        }
        
        public bool IsAligned()
        {
            return (_bitPos & 7) == 0;
        }
        
        public void Align()
        {
            //if(m_pos&7) m_pos = (m_pos+7)&~7;
            //m_pos += BitsToAlign();
            Put_Bits(0, BitsToAlign());
        }

        public int BytePos { get; set; }
        
        public int BitPos
        {
            get => (BytePos << 3) + _bitPos;
            set
            {
                if (value > Buffer.Length * 8)
                    throw new IndexOutOfRangeException("BitPos is outside the bounds");

                BytePos = value >> 3;
                _bitPos = value & 7;
            }
        }
        
        public int BytesInBuffer => BitPos + 7 >> 3; 
    }
    
}
