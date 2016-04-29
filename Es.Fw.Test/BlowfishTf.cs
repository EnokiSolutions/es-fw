using System;
using System.Diagnostics.CodeAnalysis;
using Es.FwI;
using NUnit.Framework;
// ReSharper disable MemberCanBePrivate.Local

namespace Es.Fw.Test
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public sealed class BlowfishTf
    {
        private sealed class MockRandom : IRandom
        {
            private int _x;

            public int FillCalled { get; private set; }

            public int UlongCalled { get; private set; }

            public void Fill(ArraySegment<byte> toFill)
            {
                for (var i = 0; i < toFill.Count; ++i)
                    toFill.Array[toFill.Offset + i] = (byte) ++_x;
                ++FillCalled;
            }

            public ulong Ulong()
            {
                ++_x;
                ++UlongCalled;
                return (uint) _x;
            }
        }

        private static bool AllSame(int count, byte[] out1, byte[] out2)
        {
            var allSame = true;
            for (var j = 0; j < count; ++j)
            {
                if (out1[j] == out2[j])
                    continue;

                allSame = false;
                break;
            }
            return allSame;
        }

        [Test]
        public void Simple()
        {
            var r = new MockRandom();

            var key = new byte[54];
            r.Fill(new ArraySegment<byte>(key));

            var de = Default.CreateDecrypt(key);
            var en = Default.CreateEncrypt(key, r);

            const int maxSize = 1024*1024*16;
            var input = new byte[maxSize];
            r.Fill(new ArraySegment<byte>(input));
            var out1 = new byte[maxSize];
            var out2 = new byte[maxSize];
            var tmp = new byte[maxSize];

            for (var i = 1; i < maxSize; i = 3*i + 1)
            {
                Console.WriteLine(i);
                var inas = new ArraySegment<byte>(input, 0, i);
                var enInfo = en.Analyze(inas);
                var out1As = new ArraySegment<byte>(out1, 0, enInfo.EncryptedMaxSize);
                var out2As = new ArraySegment<byte>(out2, 0, enInfo.EncryptedMaxSize);
                var enCount1 = en.Encrypt(inas, out1As, enInfo);
                var enCount2 = en.Encrypt(inas, out2As, enInfo);

                Assert.AreEqual(enCount1, enCount2);
                Assert.False(AllSame(enCount1, out1, out2));

                {
                    var en1As = new ArraySegment<byte>(out1, 0, enCount1);
                    var de1Info = de.Analyze(en1As);
                    var tmpAs = new ArraySegment<byte>(tmp, 0, de1Info.DecryptedMaxSize);
                    var deCount1 = de.Decrypt(en1As, tmpAs, de1Info);
                    Assert.AreEqual(i, deCount1);
                    Assert.True(AllSame(deCount1, input, tmp));
                }

                {
                    var en2As = new ArraySegment<byte>(out2, 0, enCount2);
                    var de2Info = de.Analyze(en2As);
                    var tmpAs = new ArraySegment<byte>(tmp, 0, de2Info.DecryptedMaxSize);
                    var deCount2 = de.Decrypt(en2As, tmpAs, de2Info);
                    Assert.AreEqual(i, deCount2);
                    Assert.True(AllSame(deCount2, input, tmp));
                }
            }
        }

        [Test]
        public void TestInvalidKeys()
        {
            foreach (var key in new[]
            {
                new byte[] {0, 1, 2, 3}, // too short
                new byte[]
                {
                    0, 1, 2, 3, 4, 5, 6, 7, 8,
                    0, 1, 2, 3, 4, 5, 6, 7, 8,
                    0, 1, 2, 3, 4, 5, 6, 7, 8,
                    0, 1, 2, 3, 4, 5, 6, 7, 8,
                    0, 1, 2, 3, 4, 5, 6, 7, 8,
                    0, 1, 2, 3, 4, 5, 6, 7, 8,
                    0, 1, 2, 3, 4, 5, 6, 7, 8,
                    0 // too long
                }
            })
            {
                Assert.Throws<ArgumentException>(() =>
                {
                    var en = Default.CreateEncrypt(key, new MockRandom());
                    var input = new byte[512];
                    var out1 = new byte[1024];
                    var inas = new ArraySegment<byte>(input);
                    var enInfo = en.Analyze(inas);
                    var out1As = new ArraySegment<byte>(out1, 0, enInfo.EncryptedMaxSize);
                    en.Encrypt(inas, out1As, enInfo);
                });
            }
        }
    }
}