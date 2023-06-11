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
using Cinegy.TsDecoder.TransportStream;
//using Microsoft.VisualBasic.CompilerServices;

namespace Cinegy.TsDecoder.Video
{
    public class VideoTsDecoder
    {
        private Pes _currentVideoPes;
        private bool _foundPps;
        private bool _foundSps;

        public long LastPts { get; private set; }

        public bool FoundSpsAndPps => _foundPps && _foundSps;

        public VideoTsService TsService { get; private set; }
        
        /// <summary>
        /// The Program Number of the service that is used as source for video data - can be set by constructor only, otherwise default program will be used.
        /// </summary>
        public ushort ProgramNumber { get; private set; }

        public ushort StreamType { get; private set; } = 0;
        
        public bool PreserveSourceData { get; private set; }

        public VideoTsDecoder()
        {
            TsService = new VideoTsService();
            TsService.OnVideoNalUnitsReady += TsService_OnVideoNalUnitsReady;
        }

        public VideoTsDecoder(int streamType, ushort programNumber = 0)
        {
            StreamType = (ushort)streamType;
            ProgramNumber = programNumber;
            TsService = new VideoTsService();
            TsService.OnVideoNalUnitsReady += TsService_OnVideoNalUnitsReady;
        }

        private void TsService_OnVideoNalUnitsReady(object sender, NalUnitReadyEventArgs args)
        {
            foreach (var nalUnit in args.NalUnits)
            {
                if (nalUnit is not H264NalUnit h264Nal) continue;
                switch (h264Nal.UnitType)
                {
                    case H264NalUnitType.PictureParameterSet:
                        _foundPps = true;
                        break;
                    case H264NalUnitType.SequenceParameterSet:
                        if (_foundSps) continue;
                        _foundSps = true;
                        var sps = new H264SeqParamSet();
                        sps.Decode(h264Nal.RbspData);
                        break;
                }
            }

            if (FoundSpsAndPps)
                TsService.OnVideoNalUnitsReady -= TsService_OnVideoNalUnitsReady;
        }

        public bool FindVideoService(TransportStream.TsDecoder tsDecoder, out EsInfo esStreamInfo)
        {
            if (tsDecoder == null) throw new InvalidOperationException("Null reference to TS Decoder");

            esStreamInfo = null;

            lock (tsDecoder)
            {
                if (ProgramNumber == 0)
                {
                    var pmt = tsDecoder.GetSelectedPmt(ProgramNumber);
                    if (pmt != null)
                    {
                        ProgramNumber = pmt.ProgramNumber;
                    }
                }

                if (ProgramNumber == 0) return false;

                TsService.ProgramNumber = ProgramNumber;

                if (StreamType > 0)
                {
                    esStreamInfo = tsDecoder.GetFirstEsStreamForProgramNumber(ProgramNumber, StreamType);
                }
                else
                {
                    //first check for H264
                    esStreamInfo = tsDecoder.GetFirstEsStreamForProgramNumber(ProgramNumber, 0x1B);
                    if (esStreamInfo == null)
                    {
                        //now check for HEVC (MPEG 2 is not supported currently)
                        esStreamInfo = tsDecoder.GetFirstEsStreamForProgramNumber(ProgramNumber, 0x24);
                        if (esStreamInfo != null)
                        {
                            StreamType = 0x24;
                            Console.WriteLine("Found HEVC stream");
                        }
                    }
                    else
                    {
                        StreamType = 0x1B;
                        Console.WriteLine("Found H264 stream");
                    }
                }

                return esStreamInfo != null;
            }
        }

        private void Setup(TransportStream.TsDecoder tsDecoder)
        {
            if (FindVideoService(tsDecoder, out var esStreamInfo))
            {
                Setup(esStreamInfo.ElementaryPid);
            }
        }
        
        public void Setup(ushort videoPid)
        {
            TsService.VideoPid = videoPid;
        }

        public void AddPacket(TsPacket tsPacket, TransportStream.TsDecoder tsDecoder = null)
        {
            if (TsService?.VideoPid == null)
            {
                if (tsDecoder != null)
                {
                    Setup(tsDecoder);
                }
            }

            if (tsPacket.Pid != TsService?.VideoPid) return;
            
            if (tsPacket.PayloadUnitStartIndicator)
            {
                if (tsPacket.PesHeader.Pts > -1)
                    LastPts = tsPacket.PesHeader.Pts;

                if (_currentVideoPes != null)
                {
                    _currentVideoPes.Decode();
                    TsService.AddData(_currentVideoPes, tsPacket.PesHeader, StreamType);
                }
                _currentVideoPes = new Pes(tsPacket);
                
            }
            else
            {
                _currentVideoPes?.Add(tsPacket);
            }
        }
    }
}
