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

using System.Collections.Generic;
using Cinegy.TsDecoder.Descriptors;
using Cinegy.TsDecoder.TransportStream;

namespace Cinegy.TsDecoder.Video
{
    public class VideoTsService
    {
        private readonly NalUnitFactory _nalUnitFactory;

        public VideoTsService()
        {
            _nalUnitFactory = new NalUnitFactory();
            _nalUnitFactory.NalUnitsReady  += NalUnitFactoryOnNalUnitsReady;
        }

        private void NalUnitFactoryOnNalUnitsReady(object sender, NalUnitReadyEventArgs args)
        {
             OnVideoNalUnitsReady?.Invoke(this, args);
        }
        
        /// <summary>
        /// Reference PTS, used to calculate and display relative time offsets for data within stream
        /// </summary>
        public long ReferencePts { get; set; }

        /// <summary>
        /// The TS Packet ID that has been selected as the elementary stream containing KLV data
        /// </summary>
        public ushort? VideoPid { get; set; } = null;

        /// <summary>
        /// The Program Number ID to which the selected KLV PID belongs, if any
        /// </summary>
        public ushort ProgramNumber { get; set; } = 0;

        /// <summary>
        /// The associated Descriptor for the service, if any
        /// </summary>
        public Descriptor AssociatedDescriptor { get; set; }
 
        public void AddData(Pes pes, PesHdr tsPacketPesHeader, ushort streamType)
        {
            //update / store any reference PTS for displaying easy relative values
            if (ReferencePts == 0) ReferencePts = tsPacketPesHeader.Pts;
            if (ReferencePts > 0 && tsPacketPesHeader.Pts < ReferencePts) ReferencePts = tsPacketPesHeader.Pts;
            _nalUnitFactory.AddPes(pes, tsPacketPesHeader, streamType);
        }
        
        public event NalUnitsReadyEventHandler OnVideoNalUnitsReady;

        internal virtual void OnVideoEntitiesReady(List<INalUnit> nalUnits)
        { 
            OnVideoNalUnitsReady?.Invoke(this, new NalUnitReadyEventArgs(nalUnits));
        }
    }
}
