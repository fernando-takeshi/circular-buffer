#region Copyright & License
/*************************************************************************
 * 
 * The MIT License (MIT)
 * 
 * Copyright (c) 2014 Roman Atachiants (kelindar@gmail.com)
 *               2014 Fernando Takeshi Sato (fernando.takeshi@gmail.com)
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
*************************************************************************/
#endregion


namespace System.Collections
{
    /// <summary>
    /// Defines a class that represents a resizable circular byte queue
    /// </summary>
    public sealed class ByteQueue
    {
        #region Properties

        private int fHead;
        private int fTail;
        private int fSize;
        private int fSizeUntilCut;
        private byte[] fInternalBuffer;

        /// <summary>
        /// Gets the length of the byte queue
        /// </summary>
        public int Length
        {
            get { return fSize; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new instance of a byte queue
        /// </summary>
        public ByteQueue()
        {
            fInternalBuffer = new byte[2048];
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Extends the capacity of the bytequeue
        /// </summary>
        private void SetCapacity(int pCapacity)
        {
            byte[] newBuffer = new byte[pCapacity];

            if (fSize > 0)
            {
                if (fHead < fTail)
                {
                    Buffer.BlockCopy(fInternalBuffer, fHead, newBuffer, 0, fSize);
                }
                else
                {
                    Buffer.BlockCopy(fInternalBuffer, fHead, newBuffer, 0, fInternalBuffer.Length - fHead);
                    Buffer.BlockCopy(fInternalBuffer, 0, newBuffer, fInternalBuffer.Length - fHead, fTail);
                }
            }

            fHead = 0;
            fTail = fSize;
            fInternalBuffer = newBuffer;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Clears the byte queue
        /// </summary>
        public void Clear()
        {
            fHead = 0;
            fTail = 0;
            fSize = 0;
            fSizeUntilCut = fInternalBuffer.Length;
        }

        /// <summary>
        /// Clears the byte queue
        /// </summary>
        public void Clear(int pSize)
        {
            lock (this)
            {
                if (pSize > fSize)
                    pSize = fSize;

                if (pSize == 0)
                    return;

                fHead = (fHead + pSize) % fInternalBuffer.Length;
                fSize -= pSize;

                if (fSize == 0)
                {
                    fHead = 0;
                    fTail = 0;
                }

                fSizeUntilCut = fInternalBuffer.Length - fHead;
                return;
            }
        }

        /// <summary>
        /// Enqueues a buffer to the queue
        /// </summary>
        /// <param name="pBuffer">Buffer to enqueue</param>
        /// <param name="pOffset">Zero-based byte offset in the buffer</param>
        /// <param name="pSize">Number of bytes to enqueue</param>
        public void Enqueue(byte[] pBuffer, int pOffset, int pSize)
        {
            if (pSize == 0)
                return;

            lock (this)
            {
                if ((fSize + pSize) > fInternalBuffer.Length)
                    SetCapacity((fSize + pSize + 2047) & ~2047);

                if (fHead < fTail)
                {
                    int rightLength = (fInternalBuffer.Length - fTail);

                    if (rightLength >= pSize)
                    {
                        Buffer.BlockCopy(pBuffer, pOffset, fInternalBuffer, fTail, pSize);
                    }
                    else
                    {
                        Buffer.BlockCopy(pBuffer, pOffset, fInternalBuffer, fTail, rightLength);
                        Buffer.BlockCopy(pBuffer, pOffset + rightLength, fInternalBuffer, 0, pSize - rightLength);
                    }
                }
                else
                {
                    Buffer.BlockCopy(pBuffer, pOffset, fInternalBuffer, fTail, pSize);
                }

                fTail = (fTail + pSize) % fInternalBuffer.Length;
                fSize += pSize;
                fSizeUntilCut = fInternalBuffer.Length - fHead;
            }
        }

        /// <summary>
        /// Enqueues a buffer to the queue
        /// </summary>
        /// <param name="pBuffer">Buffer to enqueue</param>
        public void Enqueue(byte[] pBuffer)
        {
            if (pBuffer == null)
            {
                throw new ArgumentException("Buffer cannot be null", "pBuffer");
            }
            else
            {
                Enqueue(pBuffer, 0, pBuffer.Length);
            }
        }

        /// <summary>
        /// Dequeues a buffer from the queue
        /// </summary>
        /// <param name="pBuffer">Buffer to dequeue</param>
        /// <param name="pOffset">Zero-based byte offset in the buffer</param>
        /// <param name="pSize">Number of bytes to dequeue</param>
        /// <returns>Number of bytes dequeued</returns>
        public int Dequeue(byte[] pBuffer, int pOffset, int pSize)
        {
            lock (this)
            {
                if (pSize > fSize)
                    pSize = fSize;

                if (pSize == 0)
                    return 0;

                if (fHead < fTail)
                {
                    Buffer.BlockCopy(fInternalBuffer, fHead, pBuffer, pOffset, pSize);
                }
                else
                {
                    int rightLength = (fInternalBuffer.Length - fHead);

                    if (rightLength >= pSize)
                    {
                        Buffer.BlockCopy(fInternalBuffer, fHead, pBuffer, pOffset, pSize);
                    }
                    else
                    {
                        Buffer.BlockCopy(fInternalBuffer, fHead, pBuffer, pOffset, rightLength);
                        Buffer.BlockCopy(fInternalBuffer, 0, pBuffer, pOffset + rightLength, pSize - rightLength);
                    }
                }

                fHead = (fHead + pSize) % fInternalBuffer.Length;
                fSize -= pSize;

                if (fSize == 0)
                {
                    fHead = 0;
                    fTail = 0;
                }

                fSizeUntilCut = fInternalBuffer.Length - fHead;
                return pSize;
            }
        }

        /// <summary>
        /// Dequeues a buffer from the queue
        /// </summary>
        /// <param name="pBuffer">Buffer to dequeue</param>
        /// <returns></returns>
        public int Dequeue(byte[] pBuffer)
        {
            if (pBuffer == null)
            {
                throw new ArgumentException("Buffer cannot be null", "pBuffer");
            }
            else
            {
                return Dequeue(pBuffer, 0, pBuffer.Length);
            }
        }

        /// <summary>
        /// Dequeues a buffer from the queue
        /// </summary>
        /// <param name="pBuffer">Buffer to dequeue</param>
        /// <param name="pSize">Number of bytes to dequeue</param>
        /// <returns></returns>
        public int Dequeue(out byte[] pBuffer, int pSize)
        {
            pBuffer = new byte[pSize];
            return Dequeue(pBuffer);
        }

        #endregion
    }
}
