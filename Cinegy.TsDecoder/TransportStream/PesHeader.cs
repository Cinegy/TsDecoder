namespace Cinegy.TsDecoder.TransportStream
{
    public struct PesHeader
    {
        public uint StartCode;
        public byte StreamId;
        public ushort PacketLength;
        public long Pts;
        public long Dts;
        public byte[] Payload;
        public byte HeaderLength;
    }
}