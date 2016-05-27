/***************************************************************************
=========================================================================
DireenTech Inc. (www.DireenTech.com)

  File Name:      ByteArrayQueue.cs
  Created:        Aug. 30, 2012
  Author:		  Harry Direen, DireenTech
  Description:    
	
=========================================================================
               Copyright (c) 2012 - 2015 DireenTech Inc.
All or Portions of This Software are Copyrighted by DireenTech Inc.
                        Company Proprietary
=========================================================================
***************************************************************************/

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Rabit.Utils
{
    /// <summary>
    /// A Thread Safe, Fixed size Byte Array Queue
    /// Fixed size queue is used for message passing where data
    /// is copied into and out of internal buffers.  The use
    /// off this queue prevents Instantiating and Distroying 
    /// object buffers which can be time-consuming.
    /// This object is good for Small messages... It would not be efficient for
    /// large messages because of the data copying.
    /// </summary>
    public class ByteArrayQueue
    {
            public readonly int ByteArraySize;
            public readonly int QueueSize;
            private byte[][] _queue;
            private int _head;
            private int _tail;
            private bool _tossOldestOnQueueFull;

            public ByteArrayQueue(int byteArraySize, int queueSize, bool tossOldest)
            {
                ByteArraySize = byteArraySize;
                QueueSize = queueSize;
                _tossOldestOnQueueFull = tossOldest;
                _head = 0;
                _tail = 0;
                _queue = new byte[QueueSize][];
                for (int i = 0; i < QueueSize; i++)
                    _queue[i] = new byte[ByteArraySize];
            }

            public void Clear()
            {
                lock (_queue.SyncRoot)
                {
                    _head = 0;
                    _tail = 0;
                }
            }

            /// <summary>
            /// Adds the byte array message to the Queue.
            /// The message input, msg, is copied byte-for-byte into the
            /// queue, so a reference to the message is not kept.
            /// </summary>
            /// <param name="msg">The MSG.</param>
            /// <returns></returns>
            public bool addMessage(byte[] msg)
            {
                return addMessage(msg, 0, msg.Length);
            }

            public bool addMessage(byte[] msg, int startIdx, int NoBytes)
            {
                bool queueFull = false;     //Error if Queue is full.
                int i = 0;
                lock (_queue.SyncRoot)
                {
                    int nextHead = _head + 1;
                    nextHead = nextHead < QueueSize ? nextHead : 0;
                    if (nextHead == _tail && _tossOldestOnQueueFull)
                    {
                        queueFull = true;   //Let the user know...
                        _tail = ++_tail < QueueSize ? _tail : 0;
                    }
                    if (nextHead != _tail)
                    {
                        int N = NoBytes;
                        int M = ByteArraySize - NoBytes;
                        if( M < 0 )
                        {
                            N = ByteArraySize;
                            M = 0;
                        }
                        for (i = 0; i < N; i++)
                            _queue[_head][i] = msg[startIdx + i];

                        for (; i < M; i++)
                            _queue[_head][i] = 0;   //Clear any Extra

                        _head = nextHead;
                    }
                    else
                    {
                        queueFull = true;   //Queue is full.
                    }
                }
                return queueFull;
            }


            /// <summary>
            /// Gets a copy of the next available message
            /// from the queue.  The message is copied into the byte array provided.
            /// Returns zero if the queue is empty.
            /// </summary>
            /// <param name="msg">The MSG.</param>
            /// <returns>The number of Bytes copied into the msg.</returns>
            public int getNextMessage(byte[] msg)
            {
                int NoBytesObtained = 0;
                lock (_queue.SyncRoot)
                {
                    if (_tail != _head)
                    {
                        int N = msg.Length < ByteArraySize ? msg.Length : ByteArraySize;
                        for (int i = 0; i < N; i++)
                            msg[i] = _queue[_tail][i];

                        NoBytesObtained = N;
                        _tail = ++_tail < QueueSize ? _tail : 0;
                    }
                }
                return NoBytesObtained;
            }

            public int getNoItemsInQueue()
            {
                int N = 0;
                lock (_queue.SyncRoot)
                {
                    if (_head >= _tail)
                        N = _head - _tail;
                    else
                        N = QueueSize - (_tail - _head);
                }
                return N;
            }
    }
}
