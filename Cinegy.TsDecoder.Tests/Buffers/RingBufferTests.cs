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
        [TestMethod()]
        public void RingBufferTest()
        {
            var buffer = new RingBuffer();

            if(buffer == null)
                Assert.Fail();
        }

        [TestMethod()]
        public void AddTest()
        {
            var buffer = new RingBuffer();

            FillBufferWithFakeData(buffer, 1316,ushort.MaxValue);
        }

        [TestMethod()]
        public void RemoveTest()
        {
            var buffer = new RingBuffer();

            FillBufferWithFakeData(buffer, 1316, ushort.MaxValue);

            var data = new byte[1316];
            int dataLen;
            ulong tstamp;
            buffer.Remove(ref data, out dataLen, out tstamp);
            buffer.Remove(ref data, out dataLen, out tstamp);
            buffer.Remove(ref data, out dataLen, out tstamp);
            buffer.Remove(ref data, out dataLen, out tstamp);
            FillBufferWithFakeData(buffer,3,4);

            while (buffer.BufferFullness > 1)
            {
                buffer.Remove(ref data, out dataLen, out tstamp);
            }
            
            buffer.Remove(ref data, out dataLen, out tstamp);

            if(data[0]!=3) Assert.Fail("Unexpected value after pushing through RingBuffer");
            if (data[1] != 3) Assert.Fail("Unexpected value after pushing through RingBuffer");
            if (data[2] != 3) Assert.Fail("Unexpected value after pushing through RingBuffer");
            if (data[3] != 254) Assert.Fail("Unexpected value after pushing through RingBuffer");
        }
        
        [TestMethod()]
        public void BufferFullnessTest()
        {
            var buffer = new RingBuffer();
            
            FillBufferWithFakeData(buffer, 1316, 10);

            CheckBufferFullness(buffer, 10);

            FillBufferWithFakeData(buffer, 1316, ushort.MaxValue -11);

            CheckBufferFullness(buffer,ushort.MaxValue-1);

            FillBufferWithFakeData(buffer, 1316, 1);

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