using System;
using System.Collections.Generic;
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
            const string resourceName = "Cinegy.TsDecoder.Tests.TestStreams.SD-H264-1mbps-Bars.ts";
            const int expectedPacketCount = 10493;
            var sizes = new List<int> { 188, 376, 512, 564, 1024, 1316, 1500, 2048 };

            foreach (var size in sizes)
            {
                Console.WriteLine($"Testing file {resourceName} with block size {size}");
                PerformUnalignedDataTest(resourceName, expectedPacketCount, size);
            }
        }

        private static void PerformUnalignedDataTest(string resourceName, int expectedPacketCount, int readFragmentSize)
        {
            try
            {
                var factory = new TsPacketFactory();

                //load some data from test file
                var assembly = Assembly.GetExecutingAssembly();

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) Assert.Fail("Unable to read test resource: " + resourceName);

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