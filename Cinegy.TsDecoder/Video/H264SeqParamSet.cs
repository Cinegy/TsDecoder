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
using System.Runtime.InteropServices;

namespace Cinegy.TsDecoder.Video;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct H264SeqParamSet
{
    public byte nalu_byte;
    public byte profile_idc;                         // baseline, main, etc.
    public byte level_idc;
    public bool constrained_set0_flag;               // nonzero: bitstream obeys all set 0 constraints
    public bool constrained_set1_flag;               // nonzero: bitstream obeys all set 1 constraints
    public bool constrained_set2_flag;               // nonzero: bitstream obeys all set 2 constraints
    public bool constrained_set3_flag;               // nonzero: bitstream obeys all set 3 constraints
    public bool constrained_set4_flag;               // nonzero: bitstream obeys all set 4 constraints
    public bool constrained_set5_flag;               // nonzero: bitstream obeys all set 5 constraints
        
    public byte chroma_format_idc;

    public byte seq_parameter_set_id;               // id of this sequence parameter set
    public byte log2_max_frame_num;                 // Number of bits to hold the frame_num
    public byte pic_order_cnt_type;                 // Picture order counting method

    public bool delta_pic_order_always_zero_flag;   // If zero, delta_pic_order_cnt fields are
    // present in slice header.
    public bool frame_mbs_only_flag;                // Nonzero indicates all pictures in sequence
    // are coded as frames (not fields).
    public bool gaps_in_frame_num_value_allowed_flag;

    public bool mb_adaptive_frame_field_flag;       // Nonzero indicates frame/field switch
    // at macroblock level
    public bool direct_8x8_inference_flag;          // Direct motion vector derivation method

    public bool vui_parameters_present_flag;        // Zero indicates default VUI parameters

    public bool frame_cropping_flag;                // Nonzero indicates frame crop offsets are present.
    public uint frame_cropping_rect_left_offset;
    public uint frame_cropping_rect_right_offset;
    public uint frame_cropping_rect_top_offset;
    public uint frame_cropping_rect_bottom_offset;
    public uint log2_max_pic_order_cnt_lsb;         // Value of MaxPicOrderCntLsb.
    public int  offset_for_non_ref_pic;

    public int  offset_for_top_to_bottom_field;     // Expected pic order count difference from
    // top field to bottom field.

    public uint num_ref_frames_in_pic_order_cnt_cycle;
    public fixed int offset_for_ref_frame[64];           // pointer to array of stored frame offsets,
    // length num_stored_frames_in_pic_order_cnt_cycle,
    // for pic order cnt type 1
    public uint num_ref_frames;                     // total number of pics in decoded pic buffer
    public uint frame_width_in_mbs;
    public uint frame_height_in_map_units;
    public uint frame_height_in_mbs;

    public uint aux_format_idc;                     // See H.264 standard for details.
    public uint bit_depth_aux;
    public uint bit_depth_luma;
    public uint bit_depth_chroma;
    public uint alpha_incr_flag;
    public uint alpha_opaque_value;
    public uint alpha_transparent_value;

    public bool seq_scaling_matrix_present_flag;
    public fixed bool seq_scaling_list_present_flag[8];
    public fixed byte seq_scaling_list_4x4[6*16];
    public fixed byte seq_scaling_list_8x8[2*64];
    public fixed bool use_default_scaling_matrix_flag[8];

    // vui part
    public bool aspect_ratio_info_present_flag;
    public byte aspect_ratio_idc;
    public uint	sar_width;
    public uint sar_height;

    public bool overscan_info_present_flag;
    public bool overscan_appropriate_flag;

    public bool video_signal_type_present_flag;
    public byte video_format;
    public bool video_full_range_flag;

    public bool colour_description_present_flag;
    public byte colour_primaries;
    public byte transfer_characteristics;
    public byte matrix_coefficients;

    public bool chroma_loc_info_present_flag;
    public byte chroma_sample_loc_type_top_field;
    public byte chroma_sample_loc_type_bottom_field;

    public bool timing_info_present_flag;
    public uint num_units_in_tick;
    public uint time_scale;
    public bool fixed_frame_rate_flag;

    public bool low_delay_hrd_flag;

    public bool pic_struct_present_flag;
    public bool bitstream_restriction_flag;
    public bool motion_vectors_over_pic_boundaries_flag;
    public byte max_bytes_per_pic_denom;
    public byte max_bits_per_mb_denom;
    public byte log2_max_mv_length_horizontal;
    public byte log2_max_mv_length_vertical;
    public byte num_reorder_frames;
    public byte max_dec_frame_buffering;

    public H264HrdParams nal_hrd;
    public H264HrdParams vcl_hrd;

    public bool pack_sequence_extension;
    public bool qpprime_y_zero_transform_bypass_flag;
    public bool residual_colour_transform_flag;
    public bool additional_extension_flag;
    
    public void Decode(byte [] buf)
    {
        var br = new H264BitReader(buf);

        //nalu_byte = (byte)br.Get_Bits(8);
        profile_idc = (byte)br.Get_Bits(8);

        //if (!IsValidProfile(profile_idc))
        //    return SET_ERROR(MPG_E_STREAM_SYNTAX_ERROR, "Incorrect profile_idx %d", profile_idc);

        constrained_set0_flag = br.Get_Bool();
        constrained_set1_flag = br.Get_Bool();
        constrained_set2_flag = br.Get_Bool();
        constrained_set3_flag = br.Get_Bool();
        constrained_set4_flag = br.Get_Bool();
        constrained_set5_flag = br.Get_Bool();

        if (br.Get_Bits(2) != 0)
            ;//    return SET_ERROR(MPG_E_STREAM_SYNTAX_ERROR);

        level_idc = (byte)br.Get_Bits(8);
        //if (!IsValidLevel(level_idc))
        //    return SET_ERROR(MPG_E_STREAM_SYNTAX_ERROR, "Incorrect level_idc %d", level_idc);

        // id
        seq_parameter_set_id = (byte)br.Get_UE();
        //if (seq_parameter_set_id > MAX_NUM_SEQ_PARAM_SETS - 1)
        //    return SET_ERROR(MPG_E_STREAM_SYNTAX_ERROR);

        if (profile_idc == 100 ||  // High profile
            profile_idc == 110 ||  // High10 profile
            profile_idc == 122 ||  // High422 profile
            profile_idc == 244 ||  // High444 Predictive profile
            profile_idc == 44 ||  // Cavlc444 profile
            profile_idc == 83 ||  // Scalable Constrained High profile (SVC)
            profile_idc == 86 ||  // Scalable High Intra profile (SVC)
            profile_idc == 118 ||  // Stereo High profile (MVC)
            profile_idc == 128 ||  // Multiview High profile (MVC)
            profile_idc == 138 ||  // Multiview Depth High profile (MVCD)
            profile_idc == 144)    // old High444 profile
        {
            chroma_format_idc = (byte)br.Get_UE();

            //if (chroma_format_idc > 3)
            //    return SET_ERROR(MPG_E_STREAM_SYNTAX_ERROR, "chroma_format_idc (%u) us out of range", chroma_format_idc);

            if (chroma_format_idc == 3)
            {
                if (residual_colour_transform_flag = br.Get_Bool())
                    ;// return SET_ERROR(MPG_E_STREAM_TYPE_NOT_SUPPORTED);
            }

            bit_depth_luma = (byte)(br.Get_UE() + 8);
            bit_depth_chroma = (byte)(br.Get_UE() + 8);

            qpprime_y_zero_transform_bypass_flag = br.Get_Bool();

            fixed (bool *list_present_flag = seq_scaling_list_present_flag, use_default_flag = use_default_scaling_matrix_flag)
            fixed (byte *list_4x4 = seq_scaling_list_4x4, list_8x8 = seq_scaling_list_8x8)
                if (seq_scaling_matrix_present_flag = br.Get_Bool())
                {
                    for (var i = 0; i < 8; i++)
                    {
                        if (list_present_flag[i] = br.Get_Bool())
                        {
                            if (i < 6)
                                ReadScalingList(ref br, i, list_4x4 + i*16, 16, use_default_flag + i);
                            else
                                ReadScalingList(ref br, i, list_8x8 + (i-6)*64, 64, use_default_flag + i);
                        }
                    }
                }
        }
        else
        {
            chroma_format_idc = 1;
            bit_depth_luma = 8;
            bit_depth_chroma = 8;
        }

        // log2 max frame num (bitstream contains value - 4)
        log2_max_frame_num = (byte)(br.Get_UE() + 4);
        //if (log2_max_frame_num < 4 || log2_max_frame_num > 16)
        //    return SET_ERROR(MPG_E_STREAM_SYNTAX_ERROR);

        //MaxFrameNum = (1 << log2_max_frame_num);

        // pic order cnt type (0..2)
        pic_order_cnt_type = (byte)br.Get_UE();
        //if (pic_order_cnt_type > 2)
        //    return SET_ERROR(MPG_E_STREAM_SYNTAX_ERROR);

        log2_max_pic_order_cnt_lsb = 4;
        switch (pic_order_cnt_type)
        {
            case 0:
                // log2 max pic order count lsb (bitstream contains value - 4)
                log2_max_pic_order_cnt_lsb = (byte)(br.Get_UE() + 4);
                //if (log2_max_pic_order_cnt_lsb < 4 || log2_max_pic_order_cnt_lsb > 16)
                //    return SET_ERROR(MPG_E_STREAM_SYNTAX_ERROR);

                //MaxPicOrderCntLsb = (1 << log2_max_pic_order_cnt_lsb);

                //TempRefShift = 1;
                //MaxTempRef = MaxPicOrderCntLsb >> 1;
                break;
            case 1:
            {
                delta_pic_order_always_zero_flag = br.Get_Bool();
                offset_for_non_ref_pic = br.Get_SE();
                offset_for_top_to_bottom_field = br.Get_SE();
                num_ref_frames_in_pic_order_cnt_cycle = br.Get_UE();

                //assert(num_ref_frames_in_pic_order_cnt_cycle <= _countof(offset_for_ref_frame));

                for (var i = 0; i < num_ref_frames_in_pic_order_cnt_cycle; i++)
                    offset_for_ref_frame[i] = br.Get_SE();
                break;
            }
            case 2:
                //MaxTempRef = MaxFrameNum;
                break;
        }

        // num ref frames
        num_ref_frames = br.Get_UE();
        //if (num_ref_frames > 16)
        //    return SET_ERROR(MPG_E_STREAM_SYNTAX_ERROR);

        gaps_in_frame_num_value_allowed_flag = br.Get_Bool();

        // picture width in MBs (bitstream contains value - 1)
        frame_width_in_mbs = br.Get_UE() + 1;

        // picture height in MBs (bitstream contains value - 1)
        frame_height_in_map_units = br.Get_UE() + 1;

        frame_mbs_only_flag = br.Get_Bool();
        frame_height_in_mbs = (2u - (frame_mbs_only_flag?1u:0u)) * frame_height_in_map_units;

        if (!frame_mbs_only_flag)
            mb_adaptive_frame_field_flag = br.Get_Bool();

        //	  if (pic_order_cnt_type == 0 && frame_mbs_only_flag)
        //		MaxTempRef <<= 1;

        direct_8x8_inference_flag = br.Get_Bool();
        //if (!frame_mbs_only_flag)
        //    CC_ASSERT(direct_8x8_inference_flag == true);

        if (frame_cropping_flag = br.Get_Bool())
        {
            frame_cropping_rect_left_offset = br.Get_UE();
            frame_cropping_rect_right_offset = br.Get_UE();
            frame_cropping_rect_top_offset = br.Get_UE();
            frame_cropping_rect_bottom_offset = br.Get_UE();
        }

        if (vui_parameters_present_flag = br.Get_Bool())
        {
            if (aspect_ratio_info_present_flag = br.Get_Bool())
            {
                aspect_ratio_idc = (byte)br.Get_Bits(8);

                if (aspect_ratio_idc == 255)
                {
                    sar_width = br.Get_Bits(16);
                    sar_height = br.Get_Bits(16);
                }
            }

            if (overscan_info_present_flag = br.Get_Bool())
                overscan_appropriate_flag = br.Get_Bool();

            if (video_signal_type_present_flag = br.Get_Bool())
            {
                video_format = (byte)br.Get_Bits(3);
                video_full_range_flag = br.Get_Bool();

                if (colour_description_present_flag = br.Get_Bool())
                {
                    colour_primaries = (byte)br.Get_Bits(8);
                    transfer_characteristics = (byte)br.Get_Bits(8);
                    matrix_coefficients = (byte)br.Get_Bits(8);
                }
            }

            if (chroma_loc_info_present_flag = br.Get_Bool())
            {
                chroma_sample_loc_type_top_field = (byte)br.Get_UE();
                chroma_sample_loc_type_bottom_field = (byte)br.Get_UE();
            }

            if (timing_info_present_flag = br.Get_Bool())
            {
                num_units_in_tick = br.Get_Bits(32);
                time_scale = br.Get_Bits(32);
                fixed_frame_rate_flag = br.Get_Bool();
            }

            nal_hrd.Read(ref br);
            vcl_hrd.Read(ref br);

            if (nal_hrd.present_flag || vcl_hrd.present_flag)
                low_delay_hrd_flag = br.Get_Bool();

            pic_struct_present_flag = br.Get_Bool();

            if (bitstream_restriction_flag = br.Get_Bool())
            {
                motion_vectors_over_pic_boundaries_flag = br.Get_Bool();

                max_bytes_per_pic_denom = (byte)br.Get_UE();
                max_bits_per_mb_denom = (byte)br.Get_UE();
                log2_max_mv_length_horizontal = (byte)br.Get_UE();
                log2_max_mv_length_vertical = (byte)br.Get_UE();
                num_reorder_frames = (byte)br.Get_UE();
                max_dec_frame_buffering = (byte)br.Get_UE();
            }
        }
    }

    public byte[] Encode()
    {
        var bitStream = new byte[1024];
        var bw = new H264BitWriter(bitStream);

        bw.Put_Bits(nalu_byte, 8);
        bw.Put_Bits(profile_idc, 8);
        bw.Put_Bool(constrained_set0_flag);
        bw.Put_Bool(constrained_set1_flag);
        bw.Put_Bool(constrained_set2_flag);
        bw.Put_Bool(constrained_set3_flag);
        bw.Put_Bool(constrained_set4_flag);
        bw.Put_Bool(constrained_set5_flag);
        bw.Put_Bits(0, 2);
        bw.Put_Bits(level_idc, 8);

        // id
        bw.Put_UE(seq_parameter_set_id);

        if (profile_idc == 100 ||  // High profile
            profile_idc == 110 ||  // High10 profile
            profile_idc == 122 ||  // High422 profile
            profile_idc == 244 ||  // High444 Predictive profile
            profile_idc == 44 ||  // Cavlc444 profile
            profile_idc == 83 ||  // Scalable Constrained High profile (SVC)
            profile_idc == 86 ||  // Scalable High Intra profile (SVC)
            profile_idc == 118 ||  // Stereo High profile (MVC)
            profile_idc == 128 ||  // Multiview High profile (MVC)
            profile_idc == 138 ||  // Multiview Depth High profile (MVCD)
            profile_idc == 144)    // old High444 profile
        {
            bw.Put_UE(chroma_format_idc);

            if (chroma_format_idc == 3)
            {
                bw.Put_Bool(residual_colour_transform_flag);
                // todo
            }

            bw.Put_UE(bit_depth_luma - 8);
            bw.Put_UE(bit_depth_chroma - 8);

            bw.Put_Bool(qpprime_y_zero_transform_bypass_flag);

            fixed (bool* list_present_flag = seq_scaling_list_present_flag, use_default_flag = use_default_scaling_matrix_flag)
            fixed (byte* list_4x4 = seq_scaling_list_4x4, list_8x8 = seq_scaling_list_8x8)
                if (bw.Put_Bool(seq_scaling_matrix_present_flag))
                {
                    for (var i = 0; i < 8; i++)
                    {
                        if (bw.Put_Bool(list_present_flag[i]))
                        {
                            if (i < 6)
                                WriteScalingList(ref bw, i, list_4x4 + i * 16, 16, use_default_flag + i);
                            else
                                WriteScalingList(ref bw, i, list_8x8 + (i - 6) * 64, 64, use_default_flag + i);
                        }
                    }
                }
        }

        // log2 max frame num (bitstream contains value - 4)
        bw.Put_UE(log2_max_frame_num - 4u);
        // pic order cnt type (0..2)
        bw.Put_UE(pic_order_cnt_type);

        if (pic_order_cnt_type == 0)
        {
            // log2 max pic order count lsb (bitstream contains value - 4)
            bw.Put_UE(log2_max_pic_order_cnt_lsb - 4u);
        }
        else if (pic_order_cnt_type == 1)
        {
            bw.Put_Bool(delta_pic_order_always_zero_flag);
            bw.Put_SE(offset_for_non_ref_pic);
            bw.Put_SE(offset_for_top_to_bottom_field);
            bw.Put_UE(num_ref_frames_in_pic_order_cnt_cycle);

            fixed (int* offset = offset_for_ref_frame)
                for (var i = 0; i < num_ref_frames_in_pic_order_cnt_cycle; i++)
                    bw.Put_SE(offset[i]);
        }
        else if (pic_order_cnt_type == 2)
        {
        }

        // num ref frames
        bw.Put_UE(num_ref_frames);
        bw.Put_Bool(gaps_in_frame_num_value_allowed_flag);
        // picture width in MBs (bitstream contains value - 1)
        bw.Put_UE(frame_width_in_mbs - 1u);
        // picture height in MBs (bitstream contains value - 1)
        bw.Put_UE(frame_height_in_map_units - 1);
        if(!bw.Put_Bool(frame_mbs_only_flag))
            bw.Put_Bool(mb_adaptive_frame_field_flag);

        bw.Put_Bool(direct_8x8_inference_flag);

        if (bw.Put_Bool(frame_cropping_flag))
        {
            bw.Put_UE(frame_cropping_rect_left_offset);
            bw.Put_UE(frame_cropping_rect_right_offset);
            bw.Put_UE(frame_cropping_rect_top_offset);
            bw.Put_UE(frame_cropping_rect_bottom_offset);
        }

        if (bw.Put_Bool(vui_parameters_present_flag))
        {
            if (bw.Put_Bool(aspect_ratio_info_present_flag))
            {
                bw.Put_Bits(aspect_ratio_idc, 8);

                if (aspect_ratio_idc == 255)
                {
                    bw.Put_Bits(sar_width, 16);
                    bw.Put_Bits(sar_height, 16);
                }
            }

            if (bw.Put_Bool(overscan_info_present_flag))
                bw.Put_Bool(overscan_appropriate_flag);

            if (bw.Put_Bool(video_signal_type_present_flag))
            {
                bw.Put_Bits(video_format, 3);
                bw.Put_Bool(video_full_range_flag);

                if (bw.Put_Bool(colour_description_present_flag))
                {
                    bw.Put_Bits(colour_primaries, 8);
                    bw.Put_Bits(transfer_characteristics, 8);
                    bw.Put_Bits(matrix_coefficients, 8);
                }
            }

            if (bw.Put_Bool(chroma_loc_info_present_flag))
            {
                bw.Put_UE(chroma_sample_loc_type_top_field);
                bw.Put_UE(chroma_sample_loc_type_bottom_field);
            }

            if (bw.Put_Bool(timing_info_present_flag))
            {
                bw.Put_Bits(num_units_in_tick, 32);
                bw.Put_Bits(time_scale, 32);
                bw.Put_Bool(fixed_frame_rate_flag);
            }

            nal_hrd.Write(ref bw);
            vcl_hrd.Write(ref bw);

            if (nal_hrd.present_flag || vcl_hrd.present_flag)
                bw.Put_Bool(low_delay_hrd_flag);

            bw.Put_Bool(pic_struct_present_flag);

            if (bw.Put_Bool(bitstream_restriction_flag))
            {
                bw.Put_Bool(motion_vectors_over_pic_boundaries_flag);

                bw.Put_UE(max_bytes_per_pic_denom);
                bw.Put_UE(max_bits_per_mb_denom);
                bw.Put_UE(log2_max_mv_length_horizontal);
                bw.Put_UE(log2_max_mv_length_vertical);
                bw.Put_UE(num_reorder_frames);
                bw.Put_UE(max_dec_frame_buffering);
            }
        }

        bw.Put_Bool(true);
        bw.Align();

        var result = new byte[bw.BytesInBuffer];
        Array.Copy(bitStream, result, bw.BytesInBuffer);
        return result;
    }

    static byte[] scan4x4 =
    {
        0,  1,  4,  8,
        5,  2,  3,  6,
        9, 12, 13, 10,
        7, 11, 14, 15
    };

    static byte[] scan8x8 =
    {
        0,  1,  8, 16,  9,  2,  3, 10,
        17, 24, 32, 25, 18, 11,  4,  5,
        12, 19, 26, 33, 40, 48, 41, 34,
        27, 20, 13,  6,  7, 14, 21, 28,
        35, 42, 49, 56, 57, 50, 43, 36,
        29, 22, 15, 23, 30, 37, 44, 51,
        58, 59, 52, 45, 38, 31, 39, 46,
        53, 60, 61, 54, 47, 55, 62, 63
    };

    static byte[][] default_matrix_4x4 = 
    {   new byte[]
        { 6, 13, 20, 28, 
            13, 20, 28, 32, 
            20, 28, 32, 37, 
            28, 32, 37, 42 },
        new byte[]
        {10, 14, 20, 24, 
            14, 20, 24, 27, 
            20, 24, 27, 30, 
            24, 27, 30, 34 }
    };

    static byte[][] default_matrix_8x8 =
    {   new byte[]
        { 6, 10, 13, 16, 18, 23, 25, 27,
            10, 11, 16, 18, 23, 25, 27, 29,
            13, 16, 18, 23, 25, 27, 29, 31,
            16, 18, 23, 25, 27, 29, 31, 33,
            18, 23, 25, 27, 29, 31, 33, 36,
            23, 25, 27, 29, 31, 33, 36, 38,
            25, 27, 29, 31, 33, 36, 38, 40,
            27, 29, 31, 33, 36, 38, 40, 42 },
        new byte[]
        { 9, 13, 15, 17, 19, 21, 22, 24,
            13, 13, 17, 19, 21, 22, 24, 25,
            15, 17, 19, 21, 22, 24, 25, 27,
            17, 19, 21, 22, 24, 25, 27, 28,
            19, 21, 22, 24, 25, 27, 28, 30,
            21, 22, 24, 25, 27, 28, 30, 32,
            22, 24, 25, 27, 28, 30, 32, 33,
            24, 25, 27, 28, 30, 32, 33, 35 }
    };

    private static void ReadScalingList(ref H264BitReader br, int id, byte* scaling_list, int list_size, bool* use_default_matrix)
    {
        var scan = id < 6 ? scan4x4 : scan8x8;

        byte lastScale = 8, nextScale = 8;

        for (var j = 0; j < list_size; j++)
        {
            if (nextScale != 0)
            {
                var delta_scale = br.Get_SE();
                nextScale = (byte)(lastScale + delta_scale);
                *use_default_matrix = (j == 0 && nextScale == 0);
            }

            scaling_list[scan[j]] = lastScale = (nextScale == 0) ? lastScale : nextScale;
        }

        if (!(*use_default_matrix)) return;

        if (id < 6)
            Marshal.Copy(default_matrix_4x4[id / 3], 0, (IntPtr)scaling_list, list_size);
        else
            Marshal.Copy(default_matrix_8x8[id & 1], 0, (IntPtr)scaling_list, list_size);
    }
    
    private static void WriteScalingList(ref H264BitWriter bw, int id, byte* scaling_list, int list_size, bool* use_default_matrix)
    {
        var scan = id < 6 ? scan4x4 : scan8x8;

        var lastScale = 8;

        if (*use_default_matrix)
            bw.Put_SE(0);

        else for (var j = 0; j < list_size; j++)
        {
            var nextScale = scaling_list[scan[j]];
            bw.Put_SE(nextScale - lastScale);
            lastScale = nextScale;
        }
    }
}