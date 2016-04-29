using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Es.Fw.Test
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public sealed class AtomicAppendPeelQueueTf
    {
        private const int NumPerProducer = 100000;

        private sealed class Data : AtomicAppendPeelQueueNode
        {
            public int N;
            public int P;
        }

        private async Task TestAsync()
        {
            var aapq = new AtomicAppendPeelQueue<Data>();
            var cts = new CancellationTokenSource();

            var lc1 = new List<Data>();
            var tc1 = Consumer(aapq, lc1, cts.Token);

            var lc2 = new List<Data>();
            var tc2 = Consumer(aapq, lc2, cts.Token);

            var pc1 = Producer(aapq, 1, cts.Token);
            var pc2 = Producer(aapq, 2, cts.Token);

            await pc1;
            await pc2;

            cts.Cancel();
            await tc1;
            await tc2;

            Assert.Greater(lc1.Count, 0);
            Assert.Greater(lc2.Count, 0);
            Assert.AreEqual(NumPerProducer*2, lc1.Count + lc2.Count);
            Assert.AreEqual(NumPerProducer, lc1.Count(x => x.P == 1) + lc2.Count(x => x.P == 1));
            Assert.AreEqual(NumPerProducer, lc1.Count(x => x.P == 2) + lc2.Count(x => x.P == 2));
            Assert.AreEqual(4999950000, lc1.Where(x => x.P == 1).Sum(x=>(long)x.N) + lc2.Where(x => x.P == 1).Sum(x => (long)x.N));
            Assert.AreEqual(4999950000, lc1.Where(x => x.P == 2).Sum(x => (long)x.N) + lc2.Where(x => x.P == 2).Sum(x => (long)x.N));
        }

        private static async Task Consumer(AtomicAppendPeelQueue<Data> q, ICollection<Data> d, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var l = q.Peel();
                if (l == null)
                {
                    await Task.Yield();
                    continue;
                }
                while (l != null)
                {
                    d.Add(l);
                    l = (Data) l.Next;
                }
            }
        }

        private static async Task Producer(AtomicAppendPeelQueue<Data> q, int p, CancellationToken token)
        {
            await Task.Yield();

            var n = 0;
            while (n < NumPerProducer && !token.IsCancellationRequested)
            {
                for (var i = 1; i < 5; ++i)
                {
                    Data head = null;
                    Data tail = null;
                    for (var j = 0; j < i; ++j)
                    {
                        AtomicAppendPeelQueueNode.Push(ref head, ref tail, new Data {P = p, N = n++});
                        if (n == NumPerProducer)
                            break;
                    }
                    q.AppendList(head, tail);
                }
            }
        }

        [Test]
        public void Test()
        {
            TestAsync().Wait();
        }
    }
}