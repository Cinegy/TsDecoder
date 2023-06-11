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
using System.Collections.Generic;
using Cinegy.TsDecoder.TransportStream;

namespace Cinegy.TsDecoder.Video
{
    public class NalUnitFactory
    {

        public void AddPes(Pes pes, PesHdr tsPacketPesHeader, ushort streamType)
        {
            if (pes.PacketStartCodePrefix != Pes.DefaultPacketStartCodePrefix) return;
            
            var startOfData = 6;
            
            if (pes.OptionalPesHeader?.MarkerBits == 2) //optional PES header exists - minimum length is 3
            {
                startOfData += (ushort)(3 + pes.OptionalPesHeader.PesHeaderLength);
            }

            var dataBufSize = pes.Data.Length - startOfData;

            List<INalUnit> entities = null;
            if (streamType == 0x1b)
            {
                entities = GetEntitiesFromData<H264NalUnit>(pes.Data[startOfData..], dataBufSize);
            }
            else if (streamType == 0x24)
            {
                entities = GetEntitiesFromData<H265NalUnit>(pes.Data[startOfData..], dataBufSize);
            }
            
            OnVideoReady(entities);
        }
        
        public static List<INalUnit> GetEntitiesFromData<T>(byte[] sourceData, int dataLen) where T : INalUnit, new()
        {
            var videoEntities = new List<INalUnit>();
            var sourceDataPos = 0;
            
            while (sourceDataPos < dataLen)
            {
                var nalUnit = new T();
                nalUnit.Init(sourceData, sourceDataPos, dataLen);
                sourceDataPos += nalUnit.ReadBytes;
                videoEntities.Add(nalUnit);
            }

            return videoEntities;
        }
        
        public event NalUnitsReadyEventHandler NalUnitsReady;

        private void OnVideoReady(List<INalUnit> nalUnits)
        {
            NalUnitsReady?.Invoke(this, new NalUnitReadyEventArgs(nalUnits));
        }
    }
}
