#region Copyright & License
/*************************************************************************
 * 
 * The MIT License (MIT)
 * 
 * Copyright (c) 2014 Fernando Takeshi Sato (fernando.takeshi@gmail.com)
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

using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ByteQueueTests
{
    [TestClass]
    public class ByteQueueTests
    {
        private const int K = 1024;

        [TestMethod]
        public void ByteQueueCreationAndSize()
        {
            ByteQueue vQueue = new ByteQueue();
            byte[] vTestData = new byte[5 * K];
            byte[] vDequeuedData = null;
            int vDequeuedBytes;

            Action<int> vCompareBuffers = (pOffset) =>
            {
                Parallel.For(0, vDequeuedData.Length, (pIndex) =>
                {
                    Assert.AreEqual(vTestData[pIndex + pOffset], vDequeuedData[pIndex], "Dequeued data differs from original source");
                });
            };

            Random vRandom = new Random();
            vRandom.NextBytes(vTestData);

            // Simple enqueue / dequeue
            vQueue.Enqueue(vTestData, 0, K);
            Assert.AreEqual(vQueue.Length, K, "Queue size is not " + K);
            vDequeuedBytes = vQueue.Dequeue(out vDequeuedData, K);
            Assert.IsNotNull(vDequeuedData, "Dequeued byte array was not initialized");
            Assert.AreEqual(vDequeuedBytes, K, "Dequeued size is not " + K);
            Assert.AreEqual(vQueue.Length, 0, "Queue size is not 0");
            vCompareBuffers(0);

            // Double enqueue / dequeue
            vQueue.Enqueue(vTestData, 0, K);
            vQueue.Enqueue(vTestData, K, K);
            Assert.AreEqual(vQueue.Length, 2 * K, "Queue size is not " + 2 * K);
            vDequeuedBytes = vQueue.Dequeue(out vDequeuedData, K);
            Assert.AreEqual(vQueue.Length, K, "Queue size is not " + K);
            Assert.IsNotNull(vDequeuedData, "Dequeued byte array was not initialized");
            Assert.AreEqual(vDequeuedBytes, K, "Dequeued size is not " + K);
            vCompareBuffers(0);
            vDequeuedBytes = vQueue.Dequeue(out vDequeuedData, K);
            Assert.IsNotNull(vDequeuedData, "Dequeued byte array was not initialized");
            Assert.AreEqual(vDequeuedBytes, K, "Dequeued size is not " + K);
            Assert.AreEqual(vQueue.Length, 0, "Queue size is not 0");
            vCompareBuffers(K);

            // Enqueue that forces the queue's internal buffer to expand (intially 2048)
            vQueue.Enqueue(vTestData, 0, 3 * K);
            Assert.AreEqual(vQueue.Length, 3 * K, "Queue size is not " + 3 * K);
            vDequeuedBytes = vQueue.Dequeue(out vDequeuedData, 3 * K);
            Assert.IsNotNull(vDequeuedData, "Dequeued byte array was not initialized");
            Assert.AreEqual(vDequeuedBytes, 3 * K, "Dequeued size is not " + 3 * K);
            Assert.AreEqual(vQueue.Length, 0, "Queue size is not 0");
            vCompareBuffers(0);

            // Dequeueing all bytes into a buffer larger than stored queue
            vQueue.Enqueue(vTestData, 0, K);
            Assert.AreEqual(vQueue.Length, K, "Queue size is not " + K);
            vDequeuedData = new byte[2 * K];
            vDequeuedBytes = vQueue.Dequeue(vDequeuedData);
            Assert.AreEqual(vDequeuedBytes, K, "Dequeued size is not " + K);
            Assert.AreEqual(vQueue.Length, 0, "Queue size is not 0");
            Parallel.For(0, vDequeuedData.Length, (pIndex) =>
            {
                if (pIndex < K)
                {
                    Assert.AreEqual(vTestData[pIndex], vDequeuedData[pIndex], "Dequeued data differs from original source");
                }
                else
                {
                    Assert.AreEqual(vDequeuedData[pIndex], 0, "Dequeued array has data out of supposed range");
                }
            });

            // Successive enqueueing to force another buffer expansion
            vQueue.Enqueue(vTestData, 0, 3 * K);
            Assert.AreEqual(vQueue.Length, 3 * K, "Queue size is not " + 3 * K);
            vQueue.Enqueue(vTestData, 3 * K, K);
            Assert.AreEqual(vQueue.Length, 4 * K, "Queue size is not " + 4 * K);
            vQueue.Enqueue(vTestData, 4 * K, K);
            Assert.AreEqual(vQueue.Length, 5 * K, "Queue size is not " + 5 * K);
            vDequeuedBytes = vQueue.Dequeue(out vDequeuedData, 5 * K);
            Assert.IsNotNull(vDequeuedData, "Dequeued byte array was not initialized");
            Assert.AreEqual(vDequeuedBytes, 5 * K, "Dequeued size is not " + 5 * K);
            Assert.AreEqual(vQueue.Length, 0, "Queue size is not 0");
            vCompareBuffers(0);

            // Clearing the buffer
            vQueue.Enqueue(vTestData);
            Assert.AreEqual(vQueue.Length, 5 * K, "Queue size is not " + 5 * K);
            vQueue.Clear(K);
            Assert.AreEqual(vQueue.Length, 4 * K, "Queue size is not " + 4 * K);
            vQueue.Clear();
            Assert.AreEqual(vQueue.Length, 0, "Queue size is not 0");
        }
    }
}
