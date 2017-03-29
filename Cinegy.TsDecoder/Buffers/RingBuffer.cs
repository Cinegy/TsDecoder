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

        private int _nextAddPos;
        private int _lastRemPos;
        private bool _wrapped = false;

        private readonly object _lockObj = new object();

        private readonly int _bufferSize = ushort.MaxValue;
        private readonly int _packetSize = 1500;

        private long TimerFreq { get; } = Stopwatch.Frequency/1000;

        public RingBuffer()
        {
            ResetBuffers();
        }

        public RingBuffer(int bufferSize, int packetSize = 1500)
        {
            _bufferSize = bufferSize;
            _packetSize = packetSize;
            ResetBuffers();
        }
        
        private void ResetBuffers()
        {
            lock (_lockObj)
            {
                //allocate buffer and zero
                _buffer = new byte[_bufferSize + 1][];
                _timestamp = new ulong[_bufferSize + 1];
                _dataLength = new int[_bufferSize + 1];

                for (var n = 0; n <= _bufferSize; ++n)
                {
                    _buffer[n] = new byte[_packetSize];
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
                if (data.Length <= _packetSize)
                {
                    if (_nextAddPos > _bufferSize)
                    {
                        _nextAddPos = (_nextAddPos%_bufferSize-1);
                        _wrapped = true;
                    }
                    //good data size
                    Buffer.BlockCopy(data, 0, _buffer[_nextAddPos], 0, data.Length);
                    _dataLength[_nextAddPos] = data.Length;
                    _timestamp[_nextAddPos++] = timestamp;

                   
                    
                }
                else
                {
                    throw new InvalidDataException("Data stored is greater than predefined maximum size (jumbo packet?)");
                }
            }
        }

        /// <summary>
        /// Get the any element from the ring buffer without advancing any position elements
        /// </summary>
        /// <returns>Required size of reference buffer, if passed-in buffer was too small to accomodate data. Otherwise returns zero.</returns>
        public int Peek(int position, ref byte[] dataBuffer, out int dataLength)
        {
            lock (_lockObj)
            {
                dataLength = _dataLength[position];

                if (dataBuffer.Length < dataLength)
                    return dataLength;

                Buffer.BlockCopy(_buffer[position], 0, dataBuffer, 0, dataLength);
                return 0;
            }
        }

        /// <summary>
        /// Get the oldest element from the ring buffer and advances the removal position - blocks if no data is yet available
        /// </summary>
        /// <returns>Required size of reference buffer, if passed-in buffer was too small to accomodate data. Otherwise returns zero.</returns>
        public int Remove(ref byte[] dataBuffer,out int dataLength, out ulong timestamp)
        {
            while(true)
            {
                lock (_lockObj)
                {
                    if (_lastRemPos > _bufferSize)
                    {
                        _lastRemPos = _lastRemPos%_bufferSize - 1;
                    }

                    if (_lastRemPos != _nextAddPos || _wrapped)
                    {
                        if (_wrapped) _wrapped = false;

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
                //todo: double check this after we move from ushort sizing
                var fullness = _nextAddPos - _lastRemPos;
                if(fullness>-1) return fullness;

                fullness = fullness + _bufferSize + 1; 
                return fullness;
            }
        }

        public int BufferSize => _bufferSize;

        /// <summary>
        /// Returns the position of the ring-buffer indicating the array position of the next data will be entered into the buffer
        /// </summary>
        public int NextAddPosition => _nextAddPos;

        /// <summary>
        /// Returns the position of the ring-buffer indicating the array position of the last data 'removed' from the buffer
        /// </summary>
        public int LastRemovedPosition => _lastRemPos;
    }
}