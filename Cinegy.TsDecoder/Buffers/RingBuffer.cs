/* Copyright 2017 Cinegy GmbH.

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
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Cinegy.TsDecoder.Buffers
{
    public class RingBuffer
    {
        private byte[][] _buffer;
        private ulong[] _timestamp;
        private int[] _dataLength;

        private ushort _lastAddPos;
        private ushort _lastRemPos;

        private const int PacketSize = 1500;
        private readonly object _lockObj = new object();

        private long TimerFreq { get; } = Stopwatch.Frequency/1000;

        public RingBuffer()
        {
            ResetBuffers();
        }
        
        private void ResetBuffers()
        {
            lock (_lockObj)
            {
                //allocate buffer and zero
                _buffer = new byte[ushort.MaxValue + 1][];
                _timestamp = new ulong[ushort.MaxValue + 1];
                _dataLength = new int[ushort.MaxValue + 1];

                for (var n = 0; n <= ushort.MaxValue; ++n)
                {
                    _buffer[n] = new byte[PacketSize];
                }
            }
        }

        /// <summary>
        /// Add a packet into the ring buffer.
        /// </summary>
        /// <param name="data"></param>
        public void Add(ref byte[] data)
        {
           Add(ref data, (ulong)(Stopwatch.GetTimestamp() / TimerFreq));
        }

        /// <summary>
        /// Add a packet into the ring buffer.
        /// </summary>
        /// <param name="data">Bytes to store inside ringbuffer slot</param>
        /// <param name="timestamp">Timestamp value to associate with entry</param>
        public void Add(ref byte[] data, ulong timestamp)
        {
            lock (_lockObj)
            {
                if (data.Length <= PacketSize)
                {
                    //good data size
                    Buffer.BlockCopy(data, 0, _buffer[_lastAddPos], 0, data.Length);
                    _dataLength[_lastAddPos] = data.Length;
                    _timestamp[_lastAddPos++] = timestamp;
                }
                else
                {
                    throw new InvalidDataException("Jumbo packets not supported");
                }
            }
        }

        /// <summary>
        /// Get the oldest element from the ring buffer - blocks if no data is yet available
        /// </summary>
        /// <returns></returns>
        public int Remove(ref byte[] dataBuffer,out int dataLength, out ulong timestamp)
        {
            while(true)
            {
                lock (_lockObj)
                {
                    if (_lastRemPos != _lastAddPos)
                    {
                        dataLength = _dataLength[_lastRemPos];
                        timestamp = _timestamp[_lastRemPos];

                        if (dataBuffer.Length < dataLength)
                            return dataLength;
                        
                        Buffer.BlockCopy(_buffer[_lastRemPos++], 0, dataBuffer, 0, dataLength);
                        return 0;
                    }
                }
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Provides the current timestamp, which would be attached to any new buffer entries added at that moment
        /// </summary>
        /// <returns>Current timestamp, calculated using the Stopwatch timestamp divided by (Stopwatch Timerfrequency / 1000)</returns>
        public ulong CurrentTimestamp()
        {
            return (ulong)(Stopwatch.GetTimestamp() / TimerFreq);
        }

        /// <summary>
        /// Returns the number of items in the buffer awaiting collection
        /// </summary>
        /// <returns></returns>
        public int BufferFullness
        {
            get
            {
                //todo: make this smarter with internal non-ushort trackers so we don't dump a whole buffer load on overflow
                return (ushort) (_lastAddPos - _lastRemPos);
            }
        }
    }
}