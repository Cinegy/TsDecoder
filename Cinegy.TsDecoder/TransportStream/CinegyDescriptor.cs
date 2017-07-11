using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinegy.TsDecoder.TransportStream
{
    public class CinegyDescriptor : RegistrationDescriptor //if format-identifier / organization tag == 2LND (proposed to change to CNGY)
    {
        public CinegyDescriptor(byte[] stream, int start) : base(stream, start)
        {
            if (AdditionalIdentificationInfo?.Length >= 2)
            {
                switch (AdditionalIdentificationInfo[0])
                {
                    case (0x01): //D2 Video sub-descriptor
                        SubDescriptor = new CinegyDaniel2SubDescriptor(AdditionalIdentificationInfo);
                        break;
                    case (0x02): //Technical Metadata sub-descriptor
                        SubDescriptor = new CinegyTechMetadataSubDescriptor(AdditionalIdentificationInfo);
                        break;
                }
            }
        }

        public CinegySubDescriptor SubDescriptor { get; set; }

        public static Dictionary<byte, string> CinegySubDescriptorTypes = new Dictionary<byte, string>(){
            {0x00, "reserved for future use"},
            {0x01, "DANIEL2 Video Stream Descriptor"},
            {0x02, "Cinegy Technical Metadata Descriptor"},
            {0x03, "reserved for future use"},
            {0x04, "reserved for future use" }
        };

    }

    public class CinegySubDescriptor
    {
        public CinegySubDescriptor(byte[] data)
        {
            SubDescriptorTag = data[0];
            SubDescriptorLength = data[1];
        }
        public byte SubDescriptorTag { get; }
        public byte SubDescriptorLength { get; }
    }

    public class CinegyDaniel2SubDescriptor : CinegySubDescriptor
    {
        public CinegyDaniel2SubDescriptor(byte[] data) : base(data)
        {
            if (data.Length == 3) //D2 sub-descriptors must be 3
            {
                CompatibilityLevelByte = data[3];
            }
        }

        public byte CompatibilityLevelByte {get;}

    }

    public class CinegyTechMetadataSubDescriptor : CinegySubDescriptor
    {
        public CinegyTechMetadataSubDescriptor(byte[] data) : base(data)
        {
            if (data.Length > 4) 
            {
                //todo: set to extracted values
                CinecoderVersion = "3.28.22";
                MLVersion = "6.7.1";
                AppVersion = "12.0.3.2112";
                AppName = "PlayoutExApp";
            }
        }

        public string CinecoderVersion { get; }
        public string MLVersion { get; }
        public string AppVersion { get; }
        public string AppName { get; }


    }
}
