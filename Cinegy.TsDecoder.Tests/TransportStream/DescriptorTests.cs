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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Cinegy.TsDecoder.Descriptors;
using Cinegy.TsDecoder.Tables;
using Cinegy.TsDecoder.TransportStream;
using NUnit.Framework;

namespace Cinegy.TsDecoder.Tests.TransportStream
{
    [TestFixture]
    public class DescriptorTests
    {

        [TestCase(@"TestStreams/11954-2017-07-09-16-05-06.ts")]
        [TestCase(@"TestStreams/11720-2017-07-09-16-01-43.ts")]
        //[TestCase(@"C:\Users\lewis\OneDrive\Temp\BBCHD-DVBS2Mux-19thSept2016-20160919-105050.ts")]
        public void ReadDescriptorsFromTestStreams(string file)
        {
            var testFile = Path.Combine(TestContext.CurrentContext.TestDirectory, file);
            ProcessFileForDescriptors(testFile);
        }

        private static void ProcessFileForDescriptors(string sourceFileName)
        {
            const int readFragmentSize = 1316;

            var stream = File.Open(sourceFileName, FileMode.Open);

            if (stream == null) Assert.Fail("Unable to read test file: " + sourceFileName);

            var data = new byte[readFragmentSize];

            var readCount = stream.Read(data, 0, readFragmentSize);

            var decoder = new TsDecoder.TransportStream.TsDecoder();
            decoder.TableChangeDetected += Decoder_TableChangeDetected;

            while (readCount > 0)
            {
                try
                {
                    if (readCount < readFragmentSize)
                    {
                        var tmpArr = new byte[readCount];
                        Buffer.BlockCopy(data, 0, tmpArr, 0, readCount);
                        data = new byte[readCount];
                        Buffer.BlockCopy(tmpArr, 0, data, 0, readCount);
                    }

                    decoder.AddData(data);

                    if (decoder.ServiceDescriptionTable != null && decoder.ServiceDescriptionTable.ItemsIncomplete != true)
                    {
                        foreach (var program in decoder.ProgramMapTables)
                        {
                            Console.WriteLine(decoder.GetServiceDescriptorForProgramNumber(program.ProgramNumber).ServiceName);
                            foreach (var esStream in program.EsStreams)
                            {
                                Console.WriteLine($"\t0x{esStream.ElementaryPid:X4} - {DescriptorDictionaries.ShortElementaryStreamTypeDescriptions[esStream.StreamType]}");

                                //only check type 6 privately defined streams
                                if (esStream.StreamType != 6) continue;

                                foreach (var desc in esStream.Descriptors)
                                {
                                    if (desc is ExtendedEventDescriptor extDesc)
                                    {
                                        Console.WriteLine($"{extDesc.TextChar.Value}");
                                    }
                                    else
                                    {
                                        Console.WriteLine($"\t {desc}");
                                    }
                                }
                            }
                        }
                        return;
                    }


                    if (stream.Position < stream.Length)
                    {
                        readCount = stream.Read(data, 0, readFragmentSize);
                    }
                    else
                    {
                        Assert.Fail("Reached end of file without completing descriptor scan");
                    }
                }
                catch (Exception ex)
                {
                    Assert.Fail($"Problem reading file: {ex.Message}");
                }
            }
        }

        private static void Decoder_TableChangeDetected(object sender, TableChangedEventArgs args)
        {
            var fact = sender as TableFactory;

            if (fact is ServiceDescriptionTableFactory sdtFact)
            {
                Console.WriteLine($"{args.Message} - {sdtFact.ServiceDescriptionItems.Count}");
            }
        }
    }

}

