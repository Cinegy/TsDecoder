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

using System.Collections.Generic;

namespace Cinegy.TsDecoder.Descriptors
{
    /// <summary>
    /// A Logical Channel Number Descriptor 0x83 <see cref="Descriptor"/>.
    /// </summary>
    /// <remarks>
    /// For details please refer to the original documentation,
    /// e.g. <i>ETSI EN 300 468 V1.15.1 (2016-03)</i> or alternate versions.
    /// </remarks>
    public class LcnDescriptor : Descriptor
    {
        public class LogicalchannelnumberItem
        {
            private ushort serviceID;
            private bool visibleServiceFlag;
            private byte reserved;
            private ushort logicalChannelNumber;

            public ushort ServiceID { get { return serviceID; } set { serviceID = value; } }
            public bool VisibleServiceFlag { get { return visibleServiceFlag; } set { visibleServiceFlag = value; } }
            public byte Reserved { get { return reserved; } set { reserved = value; } }
            public ushort LogicalChannelNumber { get { return logicalChannelNumber; } set { logicalChannelNumber = value; } }
        }

        private List<LogicalchannelnumberItem> logicalChannelNumbers = new List<LogicalchannelnumberItem>();

        public LcnDescriptor(byte[] stream, int start)
            : base(stream, start)
        {

            for (int idx = start + 2; idx < start + DescriptorLength - 1; idx += 4)
            {
                LogicalchannelnumberItem lcnItem = new LogicalchannelnumberItem();
                lcnItem.ServiceID = (ushort)(stream[idx] << 8 | stream[idx + 1]);
                lcnItem.VisibleServiceFlag = (stream[idx + 2] >> 7 & 0x01) != 0; ;
                lcnItem.Reserved = (byte)(stream[idx + 2] >> 2 & 0x1F);
                lcnItem.LogicalChannelNumber = (ushort)((stream[idx + 2] & 0x02) << 8 | stream[idx + 3]);
                logicalChannelNumbers.Add(lcnItem);
            }
            LogicalChannelNumbers = logicalChannelNumbers;
        }

        public IEnumerable<LogicalchannelnumberItem> LogicalChannelNumbers { get; }
    }
}
