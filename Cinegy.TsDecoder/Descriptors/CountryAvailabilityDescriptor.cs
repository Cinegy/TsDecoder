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
using System.Collections.ObjectModel;
using System.Text;

namespace Cinegy.TsDecoder.Descriptors
{
    /// <summary>
    /// A Country Availability Descriptor <see cref="Descriptor"/>.
    /// </summary>
    /// <remarks>
    /// For details please refer to the original documentation,
    /// e.g. <i>ETSI EN 300 468 V1.15.1 (2016-03)</i> or alternate versions.
    /// </remarks>
    public class CountryAvailabilityDescriptor : Descriptor
    {
        public CountryAvailabilityDescriptor(byte[] stream, int start) : base(stream, start)
        {
            var lastIndex = start + 2;

            try
            {
                if (DescriptorLength != 0)
                {
                    AvailabilityFlag = (stream[lastIndex] & 0x80) != 0;
                    lastIndex++;

                    int countryCount = (DescriptorLength - 1) / 3;

                    if (countryCount != 0)
                    {
                        CountryCodes = new Collection<string>();

                        while (CountryCodes.Count != countryCount)
                        {
                            var countryCode = Encoding.UTF8.GetString(stream, lastIndex, 3);
                            CountryCodes.Add(countryCode);
                            lastIndex += 3;
                        }
                    }
                }
            }
            catch (IndexOutOfRangeException)
            {
                throw new ArgumentOutOfRangeException("The Country Availability Descriptor message is short");
            }
        }

        public bool AvailabilityFlag { get; private set; }

        public Collection<string> CountryCodes { get; private set; }
    }
}
