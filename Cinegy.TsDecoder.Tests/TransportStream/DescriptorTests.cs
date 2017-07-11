using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Cinegy.TsDecoder.TransportStream;

namespace Cinegy.TsDecoder.Tests.TransportStream
{
    [TestClass()]
    public class DescriptorTests
    {

        [TestMethod()]
        public void ReadDescriptorsFromTestStreams()
        {
            ProcessFileForDescriptors(@"..\..\TestStreams\11954-2017-07-09-16-05-06.ts");
            //ProcessFileForDescriptors(@"..\..\TestStreams\11720-2017-07-09-16-01-43.ts");
        }

        private void ProcessFileForDescriptors(string sourceFileName)
        {
            const int readFragmentSize = 1316;

            var stream = File.Open(sourceFileName, FileMode.Open);

            if (stream == null) Assert.Fail("Unable to read test file: " + sourceFileName);

            var data = new byte[readFragmentSize];

            var readCount = stream.Read(data, 0, readFragmentSize);

            var decoder = new TsDecoder.TransportStream.TsDecoder();

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
                        if (decoder.ServiceDescriptionTable.TableId != 0x42) continue;
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
                                    if (desc is ExtendedEventDescriptor)
                                    {
                                        var extDesc = desc as ExtendedEventDescriptor;
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

    }

}

