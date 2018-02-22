using System;
using System.Collections.Generic;
using Cinegy.TsDecoder.Buffers;
using NUnit.Framework;

namespace Cinegy.TsDecoder.Tests.Buffers
{
    [TestFixture]
    public class RingBufferTests
    {
        private readonly List<int> _bufferSizes = new List<int> { 12, 188, 376, 512, 564, 1024, 1316, 1500, 2048, 16000, 32000, 64000, ushort.MaxValue, ushort.MaxValue + 1, ushort.MaxValue + 2 }; //, 2 ^ 17, 2 ^ 17 + 1, 2 ^ 18, 2 ^ 19 };

        [Test]
        public void RingBufferTest()
        {
            foreach (var size in _bufferSizes)
            {
                try
                {
                    var buffer = new RingBuffer(size);
                }
                catch (Exception)
                {
                    Assert.Fail();
                }
            }
        }

        [Test]
        public void OverflowBufferTest()
        {
            var dataCount = 12;
            var dataSize = 1;

            Console.WriteLine($"Testing buffer addition for buffer size {dataCount}");
            try
            {
                var buffer = new RingBuffer(dataCount, 1);

                FillBufferWithFakeData(buffer, dataSize, dataCount + 2);
            }
            catch (Exception ex)
            {
                if(!(ex is OverflowException))
                    Assert.Fail($"Failed LoopedBufferTest with buffer size {dataCount} - {ex.Message}");
            }
        }

        [Test]
        public void JaggedAddTest()
        {
            var dataCount = 8;
            var dataSize = 1;
            int outDataSize;
            ulong outTimestamp;
            

            var data = new byte[dataSize];
            
            try
            {
                var buffer = new RingBuffer(dataCount,dataSize);
                
                for (var i = 1; i < 40; i++)
                {
                    data[0] = (byte)i;
                    buffer.Add(ref data);
                    buffer.Add(ref data);
                    buffer.Add(ref data);
                    buffer.Add(ref data);
                    buffer.Remove(ref data,out outDataSize, out outTimestamp);
                    if(data[0]!=i) Assert.Fail("Returned value does not match expected value");
                    buffer.Remove(ref data, out outDataSize, out outTimestamp);
                    if (data[0] != i) Assert.Fail("Returned value does not match expected value");
                    buffer.Remove(ref data, out outDataSize, out outTimestamp);
                    if (data[0] != i) Assert.Fail("Returned value does not match expected value");
                    buffer.Remove(ref data, out outDataSize, out outTimestamp);
                    if (data[0] != i) Assert.Fail("Returned value does not match expected value");
                }

            }
            catch (Exception ex)
            {
                Assert.Fail($"Exception in {nameof(JaggedAddTest)}: {ex.Message}");
            }
        }

        [Test]
        public void AddTest()
        {
            foreach (var size in _bufferSizes)
            {
                Console.WriteLine($"Testing buffer addition for buffer size {size}");
                try
                {
                    var buffer = new RingBuffer(size);

                    FillBufferWithFakeData(buffer, 1316, size);
                }
                catch (Exception ex)
                {
                    Assert.Fail($"Failed AddTest with buffer size {size} - {ex.Message}");
                }
            }
        }

        [Test]
        public void RemoveTest()
        {
            foreach (var size in _bufferSizes)
            {
                Console.WriteLine($"Testing buffer removal for buffer size {size}");
                try
                {
                    var buffer = new RingBuffer(size);

                    FillBufferWithFakeData(buffer, 1316, size);

                    var data = new byte[1316];
                    int dataLen;
                    ulong tstamp;

                    for (var j = 0; j < size; j++)
                    {
                        buffer.Remove(ref data, out dataLen, out tstamp);
                        if (data[0] != j % 256)
                        {
                            Assert.Fail($"Unexpected value after pushing first data through RingBuffer (expected {j % 256}, got {data[0]})");
                        }
                    }

                    const int addCount = 4;
                    FillBufferWithFakeData(buffer, 1, addCount);

                    for (var j = 0; j < addCount; j++)
                    {
                        buffer.Remove(ref data, out dataLen, out tstamp);
                        if (data[0] != j) Assert.Fail($"Unexpected value after pushing second data through RingBuffer (expected {j}, got {data[0]}");
                    }

                }
                catch (Exception ex)
                {
                    Assert.Fail($"Failed AddTest with buffer size {size} - {ex.Message}");
                }
            }
        }

        [Test]
        public void BufferFullnessTest()
        {
            foreach (var size in _bufferSizes)
            {
                Console.WriteLine($"Testing buffer fullness for buffer size {size}");
                try
                {
                    var buffer = new RingBuffer(size);
                   
                    FillBufferWithFakeData(buffer, 1316, 10);

                    CheckBufferFullness(buffer, 10);

                    var addCount = size - 11;
                    var sum = addCount + 10;

                    FillBufferWithFakeData(buffer, 2, addCount);

                    CheckBufferFullness(buffer, size - 1);

                    FillBufferWithFakeData(buffer, 1316, 1);
                    //should now be full
                    CheckBufferFullness(buffer, size);

                    //remove some, check level is size - loop count
                    var data = new byte[1316];

                    for (var j = 1; j < 5; j++)
                    {
                        int dataLen;
                        ulong tstamp;
                        buffer.Remove(ref data, out dataLen, out tstamp);
                        CheckBufferFullness(buffer, size -j);
                    }


                    for (var j = 1; j < 5; j++)
                    {
                        FillBufferWithFakeData(buffer,1316,1);
                        CheckBufferFullness(buffer, size - 4 + j);
                    }

                }
                catch (Exception ex)
                {
                    Assert.Fail($"Failed AddTest with buffer size {size} - {ex.Message}");
                }
            }
        }

        private void CheckBufferFullness(RingBuffer buffer, int expectedVal)
        {
            if (buffer.BufferFullness != expectedVal)
                Assert.Fail($"Ring buffer fullness value is wrong - expected {expectedVal}, found {buffer.BufferFullness}.");
        }

        private void FillBufferWithFakeData(RingBuffer buffer, int dataLength, int dataCount)
        {
            for (var i = 0; i < dataCount; i++)
            {
                var fakeTsData = new byte[dataLength];

                for (var n = 0; n < fakeTsData.Length; n++)
                {
                    fakeTsData[n] = (byte)i;
                }

                buffer.Add(ref fakeTsData);
            }

        }
    }
}