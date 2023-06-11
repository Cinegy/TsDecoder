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

using System.Runtime.InteropServices;

namespace Cinegy.TsDecoder.Video
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct H264HrdParams
    {
        public bool present_flag;
        public byte cpb_cnt;
        public byte bit_rate_scale;
        public byte cpb_size_scale;
        public fixed uint bit_rate_value[32];
        public fixed uint cpb_size_value[32];
        public fixed bool cbr_flag[32];
        public byte initial_cpb_removal_delay_length;
        public byte cpb_removal_delay_length;
        public byte dpb_output_delay_length;
        public byte time_offset_length;

        public void Read(ref H264BitReader br)
        {
            present_flag = br.Get_Bool();

            if (!present_flag)
                return;

            cpb_cnt        = (byte)(br.Get_UE() + 1);
            bit_rate_scale = (byte)br.Get_Bits(4);
            cpb_size_scale = (byte)br.Get_Bits(4);

            for (var idx = 0; idx < cpb_cnt; idx++)
            {
                bit_rate_value[idx] = br.Get_UE() + 1;
                cpb_size_value[idx] = br.Get_UE() + 1;
                cbr_flag[idx] = br.Get_Bool();
            }

            initial_cpb_removal_delay_length = (byte)(br.Get_Bits(5) + 1);
            cpb_removal_delay_length         = (byte)(br.Get_Bits(5) + 1);
            dpb_output_delay_length          = (byte)(br.Get_Bits(5) + 1);
            time_offset_length               = (byte)br.Get_Bits(5);
        }

        public void Write(ref H264BitWriter bw)
        {
            bw.Put_Bool(present_flag);

            if (!present_flag)
                return;

            bw.Put_UE(cpb_cnt - 1u);
            bw.Put_Bits(bit_rate_scale, 4);
            bw.Put_Bits(cpb_size_scale, 4);

            for (var idx = 0; idx < cpb_cnt; idx++)
            {
                bw.Put_UE(bit_rate_value[idx] - 1);
                bw.Put_UE(cpb_size_value[idx] - 1);
                bw.Put_Bool(cbr_flag[idx]);
            }

            bw.Put_Bits(initial_cpb_removal_delay_length - 1u, 5);
            bw.Put_Bits(cpb_removal_delay_length - 1u, 5);
            bw.Put_Bits(dpb_output_delay_length - 1u, 5);
            bw.Put_Bits(time_offset_length, 5);
        }
    }
}

