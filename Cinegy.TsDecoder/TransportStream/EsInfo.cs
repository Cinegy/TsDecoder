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

using System.Collections.Generic;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Cinegy.TsDecoder.TransportStream
{
    public class EsInfo
    {
        public byte StreamType { get; set; }
        public short ElementaryPid { get; set; }
        public ushort EsInfoLength { get; set; }
        public ICollection<Descriptor> Descriptors { get; set; }
        public byte[] SourceData { get; set; }
    }
}
