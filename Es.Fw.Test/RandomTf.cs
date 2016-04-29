using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using Es.FwI;
using NUnit.Framework;

namespace Es.Fw.Test
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public sealed class RandomTf
    {
        private readonly IRandomWithSeedFactory[] _rfss =
        {
            Default.SystemRandomWithSeedFactory,
            Default.XorShift1024RandomWithSeedFactory
        };

        private readonly IRandomFactory[] _rfs = {Default.SystemRandomFactory, Default.XorShift1024RandomFactory};

        private static IDictionary<byte, int> CollectCounts(IRandom r, byte[] bytes)
        {
            Contract.Requires(r != null);
            Contract.Requires(bytes != null);
            Contract.Ensures(Contract.Result<IDictionary<byte, int>>() != null);
            var counts = new Dictionary<byte, int>();
            Contract.Assert(counts != null);
            r.Fill(new ArraySegment<byte>(bytes));

            foreach (var b in bytes)
            {
                counts[b] = counts.VivifyDefault(b) + 1;
            }
            return counts;
        }

        [Test]
        public void TestByteArraysModZeroToEight()
        {
            var r = Default.RandomFactory.Create();
            Assert.NotNull(r);
            Contract.Assume(r != null);

            for (var i = 0; i < 8; ++i)
            {
                var bytes = new byte[4096 + i];
                var counts = CollectCounts(r, bytes);
                Assert.AreEqual(counts.Count, 256);
            }
        }

        [Test]
        public void TestDefaultIsXor1024()
        {
            Assert.NotNull(Default.XorShift1024RandomFactory);
            Assert.AreEqual(Default.XorShift1024RandomFactory, Default.RandomFactory);
            Assert.NotNull(Default.XorShift1024RandomWithSeedFactory);
            Assert.AreEqual(Default.XorShift1024RandomWithSeedFactory, Default.RandomWithSeedFactory);
        }

        [Test]
        public void TestRandomDouble()
        {
            var r = Default.RandomFactory.Create();
            Assert.NotNull(r);
            Contract.Assume(r != null);

            var samples = new HashSet<double>();
            for (var n = 0; n < 128; ++n)
            {
                samples.Add(r.Double());
            }
            Assert.Greater(samples.Count, 1);
        }

        [Test]
        public void TestRandomInt()
        {
            var r = Default.RandomFactory.Create();
            Assert.NotNull(r);
            Contract.Assume(r != null);
            var samples = new HashSet<int>();
            for (var n = 0; n < 128; ++n)
            {
                samples.Add(r.Int());
            }
            Assert.Greater(samples.Count, 1);
        }

        [Test]
        public void TestRandomLong()
        {
            foreach (var rf in _rfss)
            {
                var r = rf.Create(0);
                Assert.NotNull(r);
                Contract.Assume(r != null);
                var samples = new HashSet<long>();
                for (var n = 0; n < 128; ++n)
                {
                    samples.Add(r.Long());
                }
                Assert.Greater(samples.Count, 1);
            }
        }

        [Test]
        public void TestRandomLotsOfBytes()
        {
            foreach (var rf in _rfs)
            {
                var r = rf.Create();
                Assert.NotNull(r);
                Contract.Assume(r != null);

                var bytes = new byte[1024*1024];
                var counts = CollectCounts(r, bytes);
                Assert.AreEqual(counts.Count, 256);
            }
        }

        [Test]
        public void TestRandomUint()
        {
            var r = Default.RandomFactory.Create();
            Assert.NotNull(r);
            Contract.Assume(r != null);

            var samples = new HashSet<uint>();
            for (var n = 0; n < 128; ++n)
            {
                samples.Add(r.Uint());
            }
            Assert.Greater(samples.Count, 1);
        }

        [Test]
        public void TestRandomUlong()
        {
            var r = Default.RandomFactory.Create();
            Assert.NotNull(r);
            Contract.Assume(r != null);

            var samples = new HashSet<ulong>();
            for (var n = 0; n < 128; ++n)
            {
                samples.Add(r.Ulong());
            }
            Assert.Greater(samples.Count, 1);
        }

        [Test]
        public void TestRandomWithSeedProducesTheSameResultsForTheSameSeed()
        {
            foreach (var rf in _rfss)
            {
                var r1 = rf.Create(0);
                Assert.NotNull(r1);
                Contract.Assume(r1 != null);

                var r2 = rf.Create(0);
                Assert.NotNull(r2);
                Contract.Assume(r2 != null);

                var samples = new HashSet<int>();
                for (var n = 0; n < 128; ++n)
                {
                    var s1 = r1.Int();
                    var s2 = r2.Int();
                    Assert.AreEqual(s1, s2);
                    samples.Add(s1);
                }
                Assert.Greater(samples.Count, 1);
            }
        }
    }
}