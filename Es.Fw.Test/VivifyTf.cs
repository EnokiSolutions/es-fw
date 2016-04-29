using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;

namespace Es.Fw.Test
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    public sealed class VivifyTf
    {
        private sealed class TestClass
        {
            public readonly int X;

            public TestClass()
            {
                X = 2;
            }

            public TestClass(int x)
            {
                X = x;
            }
        }

        [Test]
        public void TestClassType()
        {
            var d = new Dictionary<int, TestClass>() as IDictionary<int, TestClass>;
            {
                var expected = d.Vivify(0).X;
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected, 2);
            }
            {
                var testClass = d.Vivify(1, () => new TestClass(4));
                Assert.IsNotNull(testClass);
                Assert.AreEqual(testClass.X, 4);
            }
            {
                var testClass = d.Vivify(0, () => new TestClass(4));
                Assert.IsNotNull(testClass);
                Assert.AreEqual(testClass.X, 2);
            }

            Assert.IsNull(d.VivifyDefault(2));
            Assert.IsNull(d.Vivify(2));
            Assert.IsNull(d.Vivify(2, () => new TestClass(4)));

            d[2] = new TestClass(4);
            {
                var testClass = d.Vivify(2);
                Assert.IsNotNull(testClass);
                Assert.AreEqual(testClass.X, 4);
            }
            {
                var vivifyDefault = d.VivifyDefault(2);
                Assert.IsNotNull(vivifyDefault);
                Assert.AreEqual(vivifyDefault.X, 4);
            }
        }

        [Test]
        public void TestValueType()
        {
            var d = new Dictionary<int, int>() as IDictionary<int, int>;
            Assert.AreEqual(d.Vivify(0, () => 2), 2);
            Assert.AreEqual(d.Vivify(1, () => 4), 4);
            Assert.AreEqual(d.Vivify(0, () => 4), 2);

            Assert.AreEqual(d.VivifyDefault(2), default(int));
            Assert.AreEqual(d.Vivify(2, () => 4), default(int));
            d[2] = 4;
            Assert.AreEqual(d.VivifyDefault(2), 4);
        }
    }
}