using Cinegy.TsDecoder.TransportStream;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinegy.TsDecoder.Tests.DescriptorTests
{
    [TestFixture]
    public class DescriptorDeserializerTests
    {
        [TestCase(new byte[] { 0x4A, 0x14, 0x00, 0x12, 0x06, 0x00, 0x45, 0x44, 0x05, 0x44, 0x56, 0x52, 0x31, 0x04, 0xB0, 0x01, 0x10, 0x35, 0x00, 0x00, 0x09, 0xB7 })]
        public void LinkageDescriptorTest(byte[] hexBytes)
        {
            var descriptor = DescriptorFactory.DescriptorFromData(hexBytes, 0) as LinkageDescriptor;
            
            Assert.IsNotNull(descriptor);
            Assert.AreEqual(descriptor.TransportStreamId,0x12);
            Assert.AreEqual(descriptor.OriginalNetworkId, 0x600);
        }
    }
}
