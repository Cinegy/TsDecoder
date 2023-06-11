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
using System.Collections.Generic;
using System.Text;
using Cinegy.TsDecoder.TransportStream;

namespace Cinegy.TsDecoder.Descriptors
{
    /// <summary>
    /// A Extended Event Descriptor <see cref="Descriptor"/>.
    /// </summary>
    /// <remarks>
    /// For details please refer to the original documentation,
    /// e.g. <i>ETSI EN 300 468 V1.15.1 (2016-03)</i> or alternate versions.
    /// </remarks>
    public class ExtendedEventDescriptor : Descriptor
    {
        public class Item
        {
            public Item()
            {
            }

            public Item(Item item)
            {
                ItemDescriptionLength = item.ItemDescriptionLength;
                ItemDescriptionChar = new Text(item.ItemDescriptionChar);
                ItemLength = item.ItemLength;
                ItemChar = new Text(item.ItemChar);
            }

            public byte ItemDescriptionLength { get; internal set; }
            public Text ItemDescriptionChar { get; internal set; }
            public byte ItemLength { get; internal set; }
            public Text ItemChar { get; internal set; }
        }

        public ExtendedEventDescriptor(byte[] stream, int start) : base(stream, start)
        {
            int lastindex = start + 2;
            try
            {
                DescriptorNumber = (byte)(stream[lastindex] >> 4 & 0x0F);
                LastDescriptorNumber = (byte)(stream[lastindex] & 0x0F);
                lastindex++;
                Iso639LanguageCode = Encoding.UTF8.GetString(stream, lastindex, 3);
                lastindex += Iso639LanguageCode.Length;
                LengthOfItems = stream[lastindex];
                lastindex++;
                if (LengthOfItems != 0)
                {
                    var items = new List<Item>();
                    while (LengthOfItems != 0)
                    {
                        var item = new Item { ItemDescriptionLength = stream[lastindex] };
                        lastindex++;
                        if (item.ItemDescriptionLength != 0)
                        {
                            item.ItemDescriptionChar = new Text(stream, lastindex, item.ItemDescriptionLength);
                            lastindex += item.ItemDescriptionLength;
                        }

                        item.ItemLength = stream[lastindex];
                        lastindex++;
                        if (item.ItemLength != 0)
                        {
                            item.ItemChar = new Text(stream, lastindex, item.ItemLength);
                            lastindex += item.ItemLength;
                        }

                        items.Add(item);
                        LengthOfItems -= item.ItemDescriptionLength + item.ItemLength + 2;
                    }

                    Items = items;
                }

                TextLength = stream[lastindex];
                lastindex++;
                if (TextLength != 0)
                {
                    TextChar = new Text(stream, lastindex, TextLength);
                    lastindex = +TextLength;
                }
            }
            catch (Exception)
            {
                throw new ArgumentOutOfRangeException("Index was outside the bounds of the array for ExtendedEventDescriptor");
            }
        }

        public byte DescriptorNumber { get; }
        public byte LastDescriptorNumber { get; }
        public string Iso639LanguageCode { get; }
        public int LengthOfItems { get; }
        public IEnumerable<Item> Items { get; }
        public byte TextLength { get; }
        public Text TextChar { get; }
    }
}
