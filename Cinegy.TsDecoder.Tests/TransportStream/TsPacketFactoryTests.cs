using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cinegy.TsDecoder.TransportStream;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cinegy.TsDecoder.TransportStream.Tests
{
    [TestClass()]
    public class TsPacketFactoryTests
    {
        [TestMethod()]
        public void TsPacketFactoryTest()
        {
            try
            {
                var factory = new TsPacketFactory();
                if (factory.TsPacketCallbackNumber < 0)
                {
                    Assert.Fail("TsPacketFactory has negative default buffer size - this is unexpected");
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod()]
        public void GetTsPacketsFromDataAsyncTest()
        {
            try
            {
                var tsDecoder = new TsDecoder();

                var factory = new TsPacketFactory();

                //load some data from test file
                var assembly = Assembly.GetExecutingAssembly();

                const string resourceName = "Cinegy.TsDecoder.Tests.TestStreams.SD-H264-1mbps-Bars.ts";
                const int expectedPacketCount = 10493;
                
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) Assert.Fail("Unable to read test resource: " + resourceName);

                    var packetCounter = 0;

                    var data = new byte[1024];

                    var fileName = $"streamerror-{DateTime.UtcNow.ToFileTime()}.ts";

                    var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);

                    var errorStream = new BinaryWriter(fs);

                    var expectedCc = 15;

                    while (stream.Read(data, 0, 1024) > 0)
                    {
                        try
                        {
                            var tsPackets = factory.GetTsPacketsFromData(data);
                            
                            if (tsPackets == null) break;

                            if (tsPackets.Length > 6)
                            {
                                Debug.Assert(true,"Not five packets");
                            }

                            packetCounter += tsPackets.Length;


                            //foreach (var tsPacket in tsPackets)
                            //{
                            //    if (tsPacket.Pid == 4096)
                            //    {
                            //        if(tsPacket.ContinuityCounter != expectedCc)
                            //            Console.WriteLine($"{packetCounter} - {tsPacket.Pid}:{tsPacket.ContinuityCounter}");

                            //        expectedCc = tsPacket.ContinuityCounter+1;

                            //        if (expectedCc == 16) expectedCc = 0;
                            //    }
                            //}


                            if (packetCounter > (expectedPacketCount - 15))
                            {
                                foreach (var tsPacket in tsPackets)
                                {
                                    Console.WriteLine($"{packetCounter} - {tsPacket.Pid}:{tsPacket.ContinuityCounter}");
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            Assert.Fail($@"Unhandled exception reading sample file: {ex.Message}");
                        }
                    }
                    
                    if (packetCounter != expectedPacketCount)
                    {
                      //  Assert.Fail($"Failed to read expected number of packets in sample file - expected {expectedPacketCount}, got {packetCounter}");
                    }
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod()]
        public void GetTsPacketsFromDataTest()
        {
            try
            {
                var factory = new TsPacketFactory();

                //load some data from test file
                var assembly = Assembly.GetExecutingAssembly();

                const string resourceName = "Cinegy.TsDecoder.Tests.TestStreams.SD-H264-1mbps-Bars.ts";
                const int expectedPacketCount = 10493;
                var expectedCc = 15;

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) Assert.Fail("Unable to read test resource: " + resourceName);

                    var packetCounter = 0;

                    var data = new byte[188];

                    while (stream.Read(data, 0, 188) > 0)
                    {
                        try
                        {
                            var tsPackets = factory.GetTsPacketsFromData(data);

                            if (tsPackets == null) break;

                            packetCounter += tsPackets.Length;

                            if (packetCounter > (expectedPacketCount - 25))
                            {
                                foreach (var tsPacket in tsPackets)
                                {
                                    Console.WriteLine($"{packetCounter} - {tsPacket.Pid}:{tsPacket.ContinuityCounter}");
                                }
                            }


                        }
                        catch (Exception ex)
                        {
                            Assert.Fail($@"Unhandled exception reading sample file: {ex.Message}");
                        }
                    }

                    if (packetCounter != expectedPacketCount)
                    {
                        Assert.Fail($"Failed to read expected number of packets in sample file - expected {expectedPacketCount}, got {packetCounter}");
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