using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cinegy.TsDecoder.Buffers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinegy.TsDecoder.Buffers.Tests
{
    [TestClass()]
    public class RingBufferTests
    {
        private readonly List<int> _bufferSizes = new List<int> { 188, 376, 512, 564, 1024, 1316, 1500, 2048, 16000, 32000, 64000, ushort.MaxValue, ushort.MaxValue + 1, ushort.MaxValue + 2, 2 ^ 17, 2 ^ 17 + 1, 2 ^ 18, 2 ^ 19 };

        [TestMethod()]
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

        [TestMethod()]
        public void AddTest()
        {
            foreach (var size in _bufferSizes)
            {
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

        [TestMethod()]
        public void RemoveTest()
        {
            foreach (var size in _bufferSizes)
            {
                try
                {
                    var buffer = new RingBuffer(size);

                    FillBufferWithFakeData(buffer, 1316, size);

                    var data = new byte[1316];
                    int dataLen;
                    ulong tstamp;
                    buffer.Remove(ref data, out dataLen, out tstamp);
                    buffer.Remove(ref data, out dataLen, out tstamp);
                    buffer.Remove(ref data, out dataLen, out tstamp);
                    buffer.Remove(ref data, out dataLen, out tstamp);
                    FillBufferWithFakeData(buffer, 3, 4);

                    while (buffer.BufferFullness > 1)
                    {
                        buffer.Remove(ref data, out dataLen, out tstamp);
                    }

                    buffer.Remove(ref data, out dataLen, out tstamp);

                    //TODO: Clean up test to check after wrapping now added

                    //if (data[0] != 3) Assert.Fail("Unexpected value after pushing through RingBuffer");
                    //if (data[1] != 3) Assert.Fail("Unexpected value after pushing through RingBuffer");
                    //if (data[2] != 3) Assert.Fail("Unexpected value after pushing through RingBuffer");
                    //if (data[3] != 254) Assert.Fail("Unexpected value after pushing through RingBuffer");

                }
                catch (Exception ex)
                {
                    Assert.Fail($"Failed AddTest with buffer size {size} - {ex.Message}");
                }
            }
        }

        [TestMethod()]
        public void BufferFullnessTest()
        {
            var buffer = new RingBuffer();

            FillBufferWithFakeData(buffer, 1316, 10);

            CheckBufferFullness(buffer, 10);

            FillBufferWithFakeData(buffer, 1316, ushort.MaxValue - 11);

            CheckBufferFullness(buffer, ushort.MaxValue - 1);

            FillBufferWithFakeData(buffer, 1316, 1);

            //check with non-standard size buffer
            buffer = new RingBuffer(2 ^ 17);

            FillBufferWithFakeData(buffer, 1316, 2 ^ 17 - 11);

            CheckBufferFullness(buffer, 2 ^ 17 - 11);

            //stop here until reimplementing buffer position management (overflow causes wrap now)

            /*
            CheckBufferFullness(buffer, ushort.MaxValue);

            FillBufferWithFakeData(buffer, 1316, 1);

            CheckBufferFullness(buffer, ushort.MaxValue);
            
            FillBufferWithFakeData(buffer, 1316, 10);

            CheckBufferFullness(buffer, ushort.MaxValue);

            var data = new byte[1316];
            int dataLen;
            ulong tstamp;
            buffer.Remove(ref data,out dataLen,out tstamp );

            CheckBufferFullness(buffer, ushort.MaxValue-1);
            */
        }

        private void CheckBufferFullness(RingBuffer buffer, int expectedVal)
        {
            if (buffer.BufferFullness != expectedVal)
                Assert.Fail($"Full ring buffer value is wrong when filled - expected {expectedVal}, found {buffer.BufferFullness}.");
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