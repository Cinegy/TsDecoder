/* Copyright 2016-2023 Cinegy GmbH.

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
*/

using System;
using System.Data;
using System.IO;
using Cinegy.TsDecoder.DataAccess;

namespace Cinegy.TsDecoder.Video;

public struct SeiMessage
{
    public uint Type { get; set; }

    public uint PayloadSize { get; set; }

    public byte[] Payload { get; set; }

    public void Init(RbspBitReader bitReader)
    {
        byte currentByte = 0xFF;
        while (currentByte == 0xFF)
        {
            currentByte = (byte)bitReader.Get_Bits(8);
            Type += currentByte;
        }
           
        currentByte = 0xFF;
        while (currentByte == 0xFF)
        {
            currentByte = (byte)bitReader.Get_Bits(8);
            PayloadSize += currentByte;
        }

        Payload = new byte[PayloadSize];
        
        for(var i = 0; i < PayloadSize; i++){
            Payload[i] = (byte)bitReader.Get_Bits(8);
        }
    }
    
    public void Init(byte[] data, int offset, int length)
    {
        var dataPos = offset;
        var dataEndPos = offset + length;

        byte currentByte = 0xFF;
        while (currentByte == 0xFF)
        {
            if (dataPos >= dataEndPos)
            {
                throw new InvalidDataException(
                    "Corrupted SEI message buffer - attempted to read SEI type past end of buffer");
            }

            currentByte = data[dataPos++];
            Type += currentByte;
        }
            
        currentByte = 0xFF;
        while (currentByte == 0xFF)
        {
            if (dataPos >= dataEndPos)
            {
                throw new InvalidDataException(
                    "Corrupted SEI message buffer - attempted to read SEI payload size past end of buffer");
            }
            currentByte = data[dataPos++];
            PayloadSize += currentByte;
        }

        if (dataPos + PayloadSize != dataEndPos)
        {
            throw new InvalidDataException(
                "Corrupted SEI message buffer - payload size indicated does not fit final extent of data buffer provided");
        }

        Buffer.BlockCopy(data[dataPos..dataEndPos],0,Payload,0,(int)PayloadSize);
    }
    
}