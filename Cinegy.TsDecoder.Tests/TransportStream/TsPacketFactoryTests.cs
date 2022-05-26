using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Cinegy.TsDecoder.TransportStream;
using NUnit.Framework;

namespace Cinegy.TsDecoder.Tests.TransportStream
{
    [TestFixture]
    public class TsPacketFactoryTests
    {
        [TestCase(188)]
        [TestCase(376)]
        [TestCase(512)]
        [TestCase(564)]
        [TestCase(1024)]
        [TestCase(1316)]
        [TestCase(1500)]
        [TestCase(2048)]
        public void GetTsPacketsFromDataTest(int AlignmentSize)
        {
            const string filename = @"TestStreams\SD-H264-1mbps-Bars.ts";
            var testFile = Path.Combine(TestContext.CurrentContext.TestDirectory, filename);

            const int expectedPacketCount = 10493;
            Console.WriteLine($"Testing file {filename} with block size {AlignmentSize}");
            PerformUnalignedDataTest(testFile, expectedPacketCount, AlignmentSize);
        }

        [TestCase(@"TestStreams\cut-2ts.ts")]
        [TestCase(@"TestStreams\cut-bbchd-dvbs2mux.ts")]
        public void ReadServiceNamesFromDataTest(string filename)
        {
            var testFile = Path.Combine(TestContext.CurrentContext.TestDirectory, filename);
            ProcessFileForServiceNames(testFile);
        }

        [Test]
        public void ReadEsFromStream()
        {
            //ProcessFileForStreams(@"D:\Data\OneDrive\Temp\DVBT2-HEVC-EAC3-SHORT.ts");
            var testFile = Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestStreams\D2-TS-HD-AC3-Blue-45mbps.ts");
            ProcessFileForStreams(testFile);
        }

        private void ProcessFileForStreams(string sourceFileName)
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
                    
                    if(decoder.ProgramMapTables!=null && decoder.ProgramMapTables.Count > 0)
                    {
                        
                        foreach(var esStream in decoder.ProgramMapTables[0].EsStreams)
                        {
                            Console.WriteLine($"0x{esStream.ElementaryPid:X4} - {DescriptorDictionaries.ShortElementaryStreamTypeDescriptions[esStream.StreamType]}");
                            
                            //only check type 6 privately defined streams
                            if (esStream.StreamType != 6) continue;

                            var lang = esStream.Descriptors.OfType<Iso639LanguageDescriptor>().FirstOrDefault()?.Language;
                            
                            if (esStream.Descriptors.OfType<RegistrationDescriptor>().FirstOrDefault() != null)
                            {
                                var regDesc = esStream.Descriptors.OfType<RegistrationDescriptor>().First();
                                var msg = $"0x{esStream.ElementaryPid:X4} - {regDesc.Name} Descriptor: {regDesc.Organization}";
                                Console.WriteLine(msg);
                            }

                            if (esStream.Descriptors.OfType<Ac3Descriptor>().FirstOrDefault() != null)
                            {
                                var msg = $"0x{esStream.ElementaryPid:X4} - AC3 Audio";
                                if (!string.IsNullOrWhiteSpace(lang)) msg += $" [{lang}]";
                                Console.WriteLine(msg);
                            }

                            if (esStream.Descriptors.OfType<Eac3Descriptor>().FirstOrDefault() != null)
                            {
                                var msg = $"0x{esStream.ElementaryPid:X4} - EAC3 Audio";
                                if (!string.IsNullOrWhiteSpace(lang)) msg += $" [{lang}]";
                                Console.WriteLine(msg);
                            }

                            if (esStream.Descriptors.OfType<SubtitlingDescriptor>().FirstOrDefault() != null)
                            {
                                var subDesc = esStream.Descriptors.OfType<SubtitlingDescriptor>().First();
                                var msg = $"0x{esStream.ElementaryPid:X4} - DVB Subtitles";
                                foreach (var language in subDesc.Languages)
                                {
                                    msg += $" [{language.Iso639LanguageCode}/{language.SubtitlingType}]";
                                }
                                Console.WriteLine(msg);
                            }

                            if (esStream.Descriptors.OfType<TeletextDescriptor>().FirstOrDefault() != null)
                            {
                                var ttxDesc = esStream.Descriptors.OfType<TeletextDescriptor>().First();
                                var msg = $"0x{esStream.ElementaryPid:X4} - Teletext Subtitles";
                                var foundSubs = false;
                                foreach (var language in ttxDesc.Languages)
                                {
                                    if (language.TeletextType != 2) continue;
                                    var magNum = language.TeletextMagazineNumber;
                                    if (magNum == 0) magNum = 8;
                                    msg += $" [{magNum}{language.TeletextPageNumber:X2}-{language.Iso639LanguageCode}/{language.TeletextType}]";
                                    foundSubs = true;
                                }
                                if (foundSubs)
                                {
                                    Console.WriteLine(msg);
                                }
                            }
                        }

                        //finished scan
                        return;
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
                        Console.WriteLine($"Terminating read at position {stream.Position} after detection of embedded service names completed.");
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
                Console.WriteLine(decoder.GetServiceDescriptorForProgramNumber(serviceDescriptionItem.ServiceId).ServiceName);
            }
        }

        private static void PerformUnalignedDataTest(string filename, int expectedPacketCount, int readFragmentSize)
        {
            try
            {
                var factory = new TsPacketFactory();

                //load some data from test file
                using var stream = File.Open(filename, FileMode.Open);
                var packetCounter = 0;

                var data = new byte[readFragmentSize];

                var readCount = stream.Read(data, 0, readFragmentSize);

                var analyzer = new TsAnalysis.Analyzer();
                analyzer.DiscontinuityDetected += TsAnalysis_DiscontinuityDetected;

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

                        analyzer.AnalyzePackets(tsPackets);

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
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        private static void TsAnalysis_DiscontinuityDetected(object sender, TransportStreamEventArgs args)
        {
            Console.WriteLine($"CC error in stream for PID: {args.TsPid}");
        }
    }
}