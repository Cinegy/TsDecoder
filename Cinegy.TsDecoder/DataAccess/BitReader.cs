/*   Copyright 2017-2023 Cinegy GmbH

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
    public class BitReader
    {
        private readonly byte[] _buffer;
        private int _pos;
        
        public BitReader(byte[] buf)
        {
            _buffer = buf;
        }
        
        private int get_03_code_bits(int n)
        {
            var currentOffs = _pos >> 3;
            var nextOffs = _pos + n >> 3;

            if (nextOffs >= _buffer.Length)
            {
                throw new IndexOutOfRangeException("Reading the bits beyond the buffer");
            }

            if (currentOffs < 2)
                return 0;
             
            var cnt = 0;

            for (var i = currentOffs; i <= nextOffs; i++)
                if (_buffer[i] == 0x03 && _buffer[i - 1] == 0 && _buffer[i - 2] == 0)
                    cnt++;

            return cnt * 8;
        }
        
        public uint Show_Bits(int n)
        {
            switch (n)
            {
                case < 0:
                    throw new Exception("ShowBits(): The value on N is negative");
                case 0:
                    return 0;
                case > 32:
                    throw new Exception("ShowBits(): The value on N is too big");
            }

            if (_pos + n > _buffer.Length * 8)
            {
                if (_pos >= _buffer.Length * 8)
                    throw new Exception("ShowBits(): Reading the bits beyond the buffer");

                var safeBits = _buffer.Length * 8 - _pos;

                if (safeBits <= 0)
                {
                    throw new IndexOutOfRangeException("Reading the bits beyond the buffer");
                }

                return Show_Bits(safeBits) << n - safeBits;
            }

            var offs = _pos >> 3;
            var pPos = _pos & 7;

            long val = 0;
            var bitsRead = 0;

            for (; ; )
            {
                var x = _buffer[offs++];

                if (x == 0x03 && offs >= 3 && _buffer[offs - 2] == 0 && _buffer[offs - 3] == 0)
                    continue;

                val = val << 8 | x;

                if ((bitsRead += 8) >= n + pPos)
                    break;
            }

            val >>= bitsRead - (n + pPos);

            return (uint)(val & ~0U >> 32 - n);
        }
        
        public void Flush_Bits(int n)
        {
            _pos += n + get_03_code_bits(n);
        }
        
        public uint Get_Bits(int n)
        {
            var val = Show_Bits(n);
            Flush_Bits(n);
            return val;
        }
        
        public uint Show_Bits32_Aligned()
        {
            var offs = _pos + 7 >> 3;

            if (offs + 4 > _buffer.Length)
                throw new Exception("ShowBits32(): Reading the bits beyond the buffer");

            return (uint)(_buffer[offs + 0] << 24 |
                   _buffer[offs + 1] << 16 |
                   _buffer[offs + 2] << 8 |
                   _buffer[offs + 3] << 0);
        }
        
        public bool Get_Bool()
        {
            return Get_Bits(1) != 0;
        }
        
        public void Unget_Bits(int n)
        {
            if (_pos < n)
                throw new Exception("Too many bits to rewind");

            _pos -= n;
        }
        
        private int BitsToAlign()
        {
            return _pos - 1 & 7 ^ 7;
        }
        
        public bool IsAligned()
        {
            return (_pos & 7) == 0;
        }
        
        public void Align()
        {
            //if(_pos&7) _pos = (_pos+7)&~7;
            _pos += BitsToAlign();
        }
        
        public int BitPos
        {
            get => _pos;
            set
            {
                if (value > _buffer.Length * 8)
                    throw new Exception("BitPos is outside the bounds");

                _pos = value;
            }
        }
        
        public int BitsLeft => _buffer.Length * 8 - _pos;
    }
    
}
