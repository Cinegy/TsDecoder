/* Copyright 2017 Cinegy GmbH.

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

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Cinegy.TsDecoder.Tables
{
    public class EventInformationTable : Table
    {
        public ushort SericeId { get; set; }      
        public byte VersionNumber { get; set; }
        public bool CurrentNextIndicator { get; set; }
        public byte SectionNumber { get; set; }
        public byte LastSectionNumber { get; set; }
        public ushort TransportStreamId { get; set; }
        public ushort OriginalNetworkId { get; set; }
        public byte SegmentLastSectionNumber { get; set; }
        public byte LastTableId { get; set; }
        public List<EventInformationItem> Items { get; set; }
    }

    public class EventInformationItem
    {
        public static String[] RunningStatusDescription = new String[] { "undefined", "not running", "Starts in a few seconds", "pausing", "running", "reserved for future use", "reserved for future use" };
        public UInt16 EventId { get; set; }// 16     uimsbf
        public UInt64 StartTime { get; set; }// 40     bslbf
        public String StartTimeString
        {
            get
            {
                ulong mjd = StartTime >> 24;
                long y, m, d, k;

                y = (long)((mjd - 15078.2) / 365.25);
                m = (long)((mjd - 14956.1 - (long)(y * 365.25)) / 30.6001);
                d = (long)(mjd - 14956 - (ulong)(y * 365.25) - (ulong)(m * 30.6001));
                k = (m == 14 || m == 15) ? 1 : 0;
                y = y + k + 1900;
                m = m - 1 - k * 12;

                return String.Format("{3,00}-{4,00}-{5,00} {0:x}:{1:x}:{2:x}", ((StartTime >> 16) & 0xFF), ((StartTime >> 8) & 0xFF), (StartTime & 0xFF), y, m, d, 0);
            }
        }
        public uint Duration { get; set; }// 24     uimsbf
        public String DurationString { get { return String.Format("{0:x}:{1:x}:{2:x}", ((Duration >> 16) & 0xFF), ((Duration >> 8) & 0xFF), (Duration & 0xFF)); } }
        public byte RunningStatus { get; set; }// 3     uimsbf
        public String RunningStatusString { get { return RunningStatusDescription[RunningStatus]; } }// 3     uimsbf
        public bool FreeCAMode { get; set; }// 1     bslbf
        public ushort DescriptorsLoopLength { get; set; }// 12     uimsbf
        public IEnumerable<Descriptor> Descriptors { get; set; }
    }
}
