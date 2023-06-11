/* Copyright 2017-2023 Cinegy GmbH.

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
        private bool _wrapped;
        private readonly bool _allowOverflow;

        private readonly object _lockObj = new();
        private readonly int _packetSize = 1500;

        private static readonly EventWaitHandle WaitHandle = new AutoResetEvent(false);

        public RingBuffer()
        {
            ResetBuffers();
        }

        public RingBuffer(int bufferSize, int packetSize = 1500, bool allowOverflow = false)
        {
            BufferSize = bufferSize;
            _packetSize = packetSize;
            _allowOverflow = allowOverflow;
            ResetBuffers();
        }
        
        private void ResetBuffers()
        {
            lock (_lockObj)
            {
                //allocate buffer and zero
                _buffer = new byte[BufferSize + 1][];
                _timestamp = new ulong[BufferSize + 1];
                _dataLength = new int[BufferSize + 1];

                for (var n = 0; n <= BufferSize; ++n)
                {
                    _buffer[n] = new byte[_packetSize];
                }
            }
        }

        /// <summary>
        /// Add a packet into the ring buffer.
        /// </summary>
        /// <param name="data"></param>
        public void Add(byte[] data)
        {
           Add(data, data.Length);
        }
        
        /// <summary>
        /// Add a packet into the ring buffer.
        /// </summary>
        /// <param name="data">Bytes to store inside ringbuffer slot</param>
        /// <param name="dataLen">Length of the data within the buffer to consider valid</param>
        public void Add(byte[] data, int dataLen)
        {
           Add(data, dataLen, (ulong)Stopwatch.GetTimestamp());
        }
        
        /// <summary>
        /// Add a packet into the ring buffer.
        /// </summary>
        /// <param name="data">Bytes to store inside ringbuffer slot</param>
        /// <param name="dataLen">Length of the data within the buffer to consider valid</param>
        /// <param name="timestamp">Timestamp value to associate with entry</param>
        public void Add(byte[] data, int dataLen, ulong timestamp)
        {
            lock (_lockObj)
            {
                if (BufferFullness == BufferSize)
                {
                    if(!_allowOverflow){
                        throw new OverflowException("Ringbuffer has overflowed");
                    }

                    _lastRemPos++;
                }

                if (dataLen <= _packetSize)
                {
                    //good data size
                    Array.Copy(data,_buffer[_nextAddPos],dataLen);
                    _dataLength[_nextAddPos] = dataLen;
                    _timestamp[_nextAddPos++] = timestamp;
                    
                    if (_nextAddPos > BufferSize)
                    {
                        _nextAddPos = (_nextAddPos % BufferSize - 1);
                        _wrapped = true;
                    }

                    WaitHandle.Set();
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
        public int Peek(int position, byte[] dataBuffer, out int dataLength)
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
        public int Remove(byte[] dataBuffer,out int dataLength, out ulong timestamp, CancellationToken? cancellationToken = null)
        {
            cancellationToken?.ThrowIfCancellationRequested();

            try
            {
                while (cancellationToken?.IsCancellationRequested != true)
                {
                    lock (_lockObj)
                    {
                        if (_lastRemPos > BufferSize)
                        {
                            _lastRemPos = _lastRemPos % BufferSize - 1;
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

                    WaitHandle.WaitOne(100);
                }
            }
            catch
            {
            }

            dataLength = 0;
            timestamp = 0;
            return 0;
        }

        /// <summary>
        /// Provides the current timestamp, which would be attached to any new buffer entries added at that moment
        /// </summary>
        /// <returns>Current timestamp, calculated using the Stopwatch timestamp/returns>
        public ulong CurrentTimestamp()
        {
            return (ulong)Stopwatch.GetTimestamp();
        }

        /// <summary>
        /// Returns the number of items in the buffer awaiting collection
        /// </summary>
        /// <returns></returns>
        public int BufferFullness
        {
            get
            {
                lock (_lockObj)
                {
                    var fullness = _nextAddPos - _lastRemPos;
                    if (fullness > -1) return fullness;

                    fullness = fullness + BufferSize + 1;
                    return fullness;
                }
            }
        }

        public int BufferSize { get; } = ushort.MaxValue;

        /// <summary>
        /// Returns the position of the ring-buffer indicating the array position of the next data will be entered into the buffer
        /// </summary>
        public int NextAddPosition
        {
            get
            {
                lock (_lockObj)
                {
                    return _nextAddPos;
                }
            }
        }

        /// <summary>
        /// Returns the position of the ring-buffer indicating the array position of the last data 'removed' from the buffer
        /// </summary>
        public int LastRemovedPosition
        {
            get
            {
                lock (_lockObj)
                {
                    return _lastRemPos;
                }
            }
        } 
    }
}