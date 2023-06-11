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

namespace Cinegy.TsDecoder.TransportStream
{
    internal static class Utils
    {
        public static int ConvertBCDToInt(byte[] byteData, int index, int count)
        {
            var result = 0;
            var shift = 4;

            for (var nibbleIndex = 0; nibbleIndex < count; nibbleIndex++)
            {
                result = (result * 10) + ((byteData[index] >> shift) & 0x0f);

                if (shift == 4)
                    shift = 0;
                else
                {
                    shift = 4;
                    index++;
                }
            }

            return result;
        }

        public static string ToHexString(this byte[] hex)
        {
            if (hex == null) return null;
            if (hex.Length == 0) return string.Empty;

            var s = new StringBuilder();
            foreach (var b in hex)
            {
                s.Append(b.ToString("x2"));
            }
            return s.ToString();
        }

        public static ushort Convert2BytesToUshort(byte[] buffer, int offset)
        {
            return (ushort)((buffer[offset] << 8) + buffer[offset + 1]);
        }

        public static uint Convert3BytesToUint(byte[] buffer, int offset)
        {
            return (uint)((buffer[offset] << 16) + (buffer[offset + 1] << 8) + buffer[offset + 2]);
        }

        public static uint Convert4BytesToUint(byte[] buffer, int offset)
        {
            return (uint)((buffer[offset] << 24) + (buffer[offset+1] << 16) + (buffer[offset + 2] << 8) + buffer[offset + 3]);
        }
        
        public static long Convert8BytesToLong(byte[] buffer, int offset)
        {
            long temp = 0;

            for (var index = 0; index < 8; index++)
                temp = (temp << 8) + buffer[offset + index];

            return temp;
        }
    }
}
