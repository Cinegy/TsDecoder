using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
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

        [TestCase(true)]
        [TestCase(false)]
        public void OverflowBufferTest(bool allowOverflow)
        {
            var bufferSize = 12;
            var dataCount = 16;
            var dataSize = 1;
            var didExcept = false;

            Console.WriteLine($"Testing buffer addition for buffer size {bufferSize} with {dataCount} elements");
            try
            {
                var buffer = new RingBuffer(bufferSize, 1, allowOverflow);

                FillBufferWithFakeData(buffer, dataSize, dataCount);
                var checkValueBuffer = new byte[1];
                buffer.Peek(dataCount-bufferSize-2, ref checkValueBuffer, out int dataPeekLength);

                if (checkValueBuffer[0] != dataCount -1)
                    Assert.Fail($"Unexpected value at circled buffer location - expected {dataCount -1}, found {checkValueBuffer[0]}");

                for(int i = 4; i<dataCount; i++)
                {
                    buffer.Remove(ref checkValueBuffer, out int dataLength, out ulong timestamp);
                    if(checkValueBuffer[0] != i)
                        Assert.Fail($"Unexpected value at circled buffer location - expected {dataCount - 1}, found {checkValueBuffer[0]}");
                }
            }
            catch (Exception ex)
            {
                if(!(ex is OverflowException))
                    Assert.Fail($"Failed LoopedBufferTest with buffer size {dataCount} - {ex.Message}");

                didExcept = true;
            }

            if(allowOverflow && didExcept)
                Assert.Fail($"Failed LoopedBufferTest with buffer size {dataCount} - overflow should be permitted, but OverflowException was thrown.");

        }

        [Test]
        public void ThreadedLoopedTest()
        {
            var bufferSize = 12;
            var dataSize = 1;
            var cycleCount = 255;

            var data = new byte[dataSize];
            var buffer = new RingBuffer(bufferSize, 1, true);

            var ts = new ThreadStart(delegate
            {
                AddingThread(buffer, cycleCount);
            });

            var addingThread = new Thread(ts) { Priority = ThreadPriority.Highest };

            addingThread.Start();

            for (int i = 0; i < 255; i++)
            {
                data[0] = (byte)i;
                buffer.Remove(ref data, out int dataLength, out ulong timestamp);

                if(data[0]!=i)
                    Assert.Fail($"Failed ThreadedLoopedBufferTest with buffer size {bufferSize} - expected value {i}, got {data[0]}.");
            }
        }


        [Test]
        public void ThreadedLoopedPeekTest()
        {
            //this test creates a thread to reliably add a buffer sample with a new element added every 5 milliseconds.
            //it then consumes the buffer, taking a randomized time between 1 and 8 ms to eat a sample - it should trigger the
            //buffer to fill and empty with some realistic behaviour

            var bufferSize = 48;
            var dataSize = 1;
            var cycleCount = 409;

            var data = new byte[dataSize];
            var buffer = new RingBuffer(bufferSize, 1, true);

            var ts = new ThreadStart(delegate
            {
                AddingThread(buffer, cycleCount);
            });

            var addingThread = new Thread(ts) { Priority = ThreadPriority.Highest };

            addingThread.Start();

            var readPosition = 0;
            var iterationCount = 0;
            byte expectedVal = 0;

            var randomGenerator = new Random();
            var stringResult = new StringBuilder(1024);
            var sleepCount = 0;

            while (addingThread.IsAlive)
            {
                var randSleep = randomGenerator.Next(1, 8);

                if (readPosition > buffer.BufferSize)
                    readPosition = 0;

                if (readPosition == buffer.NextAddPosition)
                {
                    Thread.Sleep(randSleep);
                    sleepCount++;
                    continue;
                }

                data[0] = 0;
                try
                {
                    buffer.Peek(readPosition, ref data, out int dataLength);
                }
                catch(Exception ex)
                {
                    Assert.Fail($"Failed ThreadedLoopedBufferTest on read iteration {iterationCount} - exception: {ex.Message}.");
                }
                var result = data[0];

                if (result != expectedVal)
                    Assert.Fail($"Failed ThreadedLoopedBufferTest on read iteration {iterationCount} - expected value {expectedVal}, got {result}.");
                    
                stringResult.Append($"{result},");

                expectedVal = ++result;
                readPosition++;
                iterationCount++;
                
            }

            Console.WriteLine($"Sleep count: {sleepCount}");
            Console.WriteLine(stringResult.ToString());
        }

        private static void AddingThread(RingBuffer buffer, int cycleCount)
        {
            var dataSize = 1;
            var data = new byte[dataSize];

            for (int i = 0; i < cycleCount; i++)
            {
                data[0] = (byte)i;
                buffer.Add(ref data);
             
                Thread.Sleep(5);
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