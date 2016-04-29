using System;
using System.Diagnostics;
using NUnit.Framework;

namespace Es.Fw.Test
{
    [TestFixture]
    public sealed class ByteArrayExTf
    {
        [Test]
        public void Test()
        {
            for (var b = 0; b < 256; ++b)
            {
                var ba = new[] { (byte)b };
                var encodedString = ba.ToEncodedString();
                Debug.WriteLine("{0} {1}", b, encodedString);
                var fromEncodedString = encodedString.FromEncodedString();
                CollectionAssert.AreEquivalent(ba, fromEncodedString);
            }
        }

        [Test]
        public void TestHexString()
        {
            CollectionAssert.AreEquivalent(new byte[]{0x00,0xff,0x0f,0xf0,0xab},"00ff0ff0ab".FromHexString());
            Assert.AreEqual("00ff0ff0ab", new byte[] { 0x00, 0xff, 0x0f, 0xf0, 0xab }.ToHexString());
        }

        [Test]
        public void TestFromHexStringException()
        {
            Assert.Throws<FormatException>(() => "a".FromHexString());
            Assert.Throws<FormatException>(() => "xx".FromHexString());
            Assert.Throws<FormatException>(() => "ax".FromHexString());
            Assert.Throws<FormatException>(() => "xa".FromHexString());
        }

        [Test]
        public void TestToHexString()
        {
            Assert.AreEqual("000ff0ff", new byte[] { 0, 0x0F, 0xF0, 0xFF }.ToHexString());
            Assert.AreEqual("000ff0ff", new ArraySegment<byte>(new byte[] { 0, 0x0F, 0xF0, 0xFF }).ToHexString());
        }

        [Test]
        public void TestEncodedString()
        {
            Assert.AreEqual("AAAZe", new byte[] { 0, 0 }.ToEncodedString());
            Assert.AreEqual("AAAZe", new ArraySegment<byte>(new byte[] { 0, 0 }).ToEncodedString());

            var r = new Random(0);
            for (var i = 0; i < 16384; ++i)
            {
                var b = new byte[r.Next(1, 999)];
                r.NextBytes(b);
                Assert.AreEqual(b, b.ToEncodedString().FromEncodedString());
            }
        }

        [Test]
        public void TestFromEncodedStringException()
        {
            Assert.Throws<FormatException>(() => "_".FromEncodedString());
        }
    }
}