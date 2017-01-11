#Cinegy Transport Stream Decoder Library

Use this library to decode MPEGTS (including DVB) transport streams - tested with various streams, from Multicast and File sources, with single- and multi-program streams.

##How easy is it?

Assuming you have some data from a source as a byte array (e.g. from a UDP packet or from a file), get a collection of Transport Stream packets (TSPackets) using the factory feeding that data in:

```
var tsPackets = TsPacketFactory.GetTsPacketsFromData(data);

foreach (var tsPacket in tsPackets)
{
	_tsDecoder.AddPacket(tsPacket);
}

```

Once you start feeding in packets, the class will start to populate with data - allowing you to explore the Program Allocation Table, Service Descriptors, or even start grabbing the data from an individual elementary stream.

See all of this in action inside the Cinegy TS Analyser tool here: [GitHub] [https://github.com/cinegy/tsanalyser]
    
##Getting the library

Just to make your life easier, we auto-build this using AppVeyor and push to NuGet - here is how we are doing right now: 

[![Build status](https://ci.appveyor.com/api/projects/status/mmqiy7vr5f01lhcx?svg=true)](https://ci.appveyor.com/project/cinegy/tsdecoder)

You can check out the latest compiled binary from the master or pre-master code here:

[AppVeyor TtxDecoder Project Builder](https://ci.appveyor.com/project/cinegy/tsdecoder/build/artifacts)

Available on NuGet here:

[NuGet](https://www.nuget.org/packages/Cinegy.TsDecoder/)
