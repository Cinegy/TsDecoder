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

using System;
using System.Text;
using Cinegy.TsDecoder.TransportStream;

namespace Cinegy.TsDecoder.Descriptors
{
    /// <summary>
    /// A Short Event Descriptor <see cref="Descriptor"/>.
    /// </summary>
    /// <remarks>
    /// For details please refer to the original documentation,
    /// e.g. <i>ETSI EN 300 468 V1.15.1 (2016-03)</i> or alternate versions.
    /// </remarks>
    public class ShortEventDescriptor : Descriptor
    {
        public ShortEventDescriptor(byte[] stream, int start) : base(stream, start)
        {
            int lastindex = start + 2;
            try
            {

                Iso639LanguageCode = Encoding.UTF8.GetString(stream, lastindex, 3);
                lastindex += Iso639LanguageCode.Length;

                EventNameLength = stream[lastindex];
                lastindex++;

                if (EventNameLength != 0)
                {
                    EventNameChar = new Text(stream, lastindex, EventNameLength);
                    lastindex += EventNameLength;
                    //Debug.WriteLine(EventNameChar);
                }

                TextLength = stream[lastindex];
                lastindex++;
                if (TextLength != 0)
                {
                    TextChar = new Text(stream, lastindex, TextLength);
                    lastindex += TextLength;
                    //Debug.WriteLine(TextChar);
                }
            }
            catch (IndexOutOfRangeException)
            {
                throw new ArgumentOutOfRangeException("Index was outside the bounds of the array."); 
            }
        }

        public string Iso639LanguageCode { get; }
        public byte EventNameLength { get; }
        public Text EventNameChar { get; }
        public byte TextLength { get; }
        public Text TextChar { get; }
    }
}
