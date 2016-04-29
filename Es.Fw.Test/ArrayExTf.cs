using System;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;

namespace Es.Fw.Test
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public sealed class ArrayExTf
    {
        [Test]
        public void TestArraySegementToArray()
        {
            var a = new[] {1, 2, 3};
            var aseg = new ArraySegment<int>(a);
            var b = aseg.ToArray();
            Assert.AreNotSame(a, b);
            CollectionAssert.AreEquivalent(a, b);
            var cseg = new ArraySegment<int>(a, 1, 1);
            var c = cseg.ToArray();
            Assert.AreEqual(1, c.Length);
            Assert.AreEqual(2, c[0]);
        }

        [Test]
        public void TestArrayEq()
        {
            var ia = new[] {0, 0};
            var ias = new ArraySegment<int>(new[] {0, 0});
            var ib = new[] {0, 0};
            var ibs = new ArraySegment<int>(new[] {0, 0});
            var ic = new[] {0, 0, 0};
            var ics = new ArraySegment<int>(new[] {0, 0, 0});
            var id = new[] {0, 1};
            var ids = new ArraySegment<int>(new[] {0, 1});

            Assert.IsTrue(ia.Eq(ib));
            Assert.IsTrue(ia.Eq(ias));
            Assert.IsFalse(ia.Eq(ic));
            Assert.IsFalse(ia.Eq(id));
            Assert.IsFalse(ia.Eq(ics));
            Assert.IsFalse(ia.Eq(ids));

            Assert.IsTrue(ias.Eq(ia));
            Assert.IsTrue(ias.Eq(ibs));
            Assert.IsFalse(ias.Eq(ic));
            Assert.IsFalse(ias.Eq(id));
            Assert.IsFalse(ias.Eq(ics));
            Assert.IsFalse(ias.Eq(ids));
        }
    }
}