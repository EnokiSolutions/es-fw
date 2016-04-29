using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;

namespace Es.Fw.Test
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public sealed class ByteArrayComparerTf
    {
        [Test]
        public void Test()
        {
            var d = new Dictionary<byte[], ulong>(ByteArrayComparer.Instance);

            d.Add(new byte[] {}, 0);
            d.Add(new byte[] { 1 }, 1);
            Assert.Throws<ArgumentException>(()=>
            {
                d.Add(new byte[] {1}, 2);
            });
            Assert.False(d.ContainsKey(new byte[] {2}));
            ulong x;
            Assert.True(d.TryGetValue(new byte[] {1}, out x));
            Assert.AreEqual(1, x);
        }
    }
}