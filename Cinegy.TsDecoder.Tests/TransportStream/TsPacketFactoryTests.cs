using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Cinegy.TsDecoder.TransportStream;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cinegy.TsDecoder.Tests.TransportStream
{
    [TestClass()]
    public class TsPacketFactoryTests
    {
        [TestMethod()]
        public void GetTsPacketsFromDataTest()
        {
            const string filename = @"..\..\TestStreams\SD-H264-1mbps-Bars.ts";

            const int expectedPacketCount = 10493;
            var sizes = new List<int> { 188, 376, 512, 564, 1024, 1316, 1500, 2048 };

            foreach (var size in sizes)
            {
                Console.WriteLine($"Testing file {filename} with block size {size}");
                PerformUnalignedDataTest(filename, expectedPacketCount, size);
            }
        }

        [TestMethod()]
        public void ReadServiceNamesFromDataTest()
        {
            ProcessFileForServiceNames(@"..\..\TestStreams\cut-2ts.ts");
            ProcessFileForServiceNames(@"..\..\TestStreams\cut-bbchd-dvbs2mux.ts");
        }

        [TestMethod()]
        public void ReadEsFromStream()
        {
            ProcessFileForStreams(@"..\..\TestStreams\D2-TS-20170706-163400.ts");
        }

        private void ProcessFileForStreams(string sourceFileName)
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
                    
                    if(decoder.ProgramMapTables!=null && decoder.ProgramMapTables.Count > 0)
                    {
                        foreach(var esStream in decoder.ProgramMapTables[0].EsStreams)
                        {
                            if(esStream.Descriptors!=null && esStream.Descriptors.Count > 0)
                            {
                                Debug.WriteLine($"Terminating read at position {stream.Position} after detection of a PMT.");
                                return;
                            }
                        }
                    }

                    if (stream.Position < stream.Length)
                    {
                        readCount = stream.Read(data, 0, readFragmentSize);
                    }
                    else
                    {
                        Assert.Fail("Reached end of file without completing SDT scan");
                    }
                }
                catch (Exception ex)
                {
                    Assert.Fail($"Problem reading file: {ex.Message}");
                }
            }
        }


        private void ProcessFileForServiceNames(string sourceFileName)
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

                    if (decoder.ServiceDescriptionTable?.ItemsIncomplete == false)
                    {
                        Debug.WriteLine($"Terminating read at position {stream.Position} after detection of embedded service names completed.");
                        break;
                    }

                    if (stream.Position < stream.Length)
                    {
                        readCount = stream.Read(data, 0, readFragmentSize);
                    }
                    else
                    {
                        Assert.Fail("Reached end of file without completing SDT scan");
                    }
                }
                catch (Exception ex)
                {
                    Assert.Fail($"Problem reading file: {ex.Message}");
                }
            }
        }

        private void Decoder_TableChangeDetected(object sender, TableChangedEventArgs args)
        {
            //filter to SDT events, since we are looking for the SDT to complete
            if (args.TableType != TableType.Sdt)
                return;

            var decoder = sender as TsDecoder.TransportStream.TsDecoder;

            if (decoder?.ServiceDescriptionTable?.ItemsIncomplete != false) return;

            foreach (var serviceDescriptionItem in decoder.ServiceDescriptionTable.Items)
            {
                Debug.WriteLine(decoder.GetServiceDescriptorForProgramNumber(serviceDescriptionItem.ServiceId).ServiceName);
            }
        }

        private static void PerformUnalignedDataTest(string filename, int expectedPacketCount, int readFragmentSize)
        {
            try
            {
                var factory = new TsPacketFactory();

                //load some data from test file
                using (var stream = File.Open(filename, FileMode.Open))
                {
                    var packetCounter = 0;

                    var data = new byte[readFragmentSize];

                    var readCount = stream.Read(data, 0, readFragmentSize);

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

                            var tsPackets = factory.GetTsPacketsFromData(data);

                            if (tsPackets == null) break;

                            packetCounter += tsPackets.Length;

                            if (stream.Position < stream.Length)
                            {
                                readCount = stream.Read(data, 0, readFragmentSize);
                            }
                            else
                            {
                                break;
                            }

                        }
                        catch (Exception ex)
                        {
                            Assert.Fail($@"Unhandled exception reading sample file: {ex.Message}");
                        }
                    }

                    if (packetCounter != expectedPacketCount)
                    {
                        Assert.Fail($"Failed to read expected number of packets in sample file - expected {expectedPacketCount}, " +
                                    $"got {packetCounter}, blocksize: {readFragmentSize}");
                    }
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

    }
}