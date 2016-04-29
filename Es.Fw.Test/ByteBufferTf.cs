using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;

namespace Es.Fw.Test
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public sealed class ByteBufferTestFixture
    {
        private interface IWriteToByteBuffer
        {
            void Write(ByteBuffer bb);
        }

        private interface IReadFromByteBuffer
        {
            bool TryRead(ByteBuffer bb);
        }

        private interface ITestByteBuffer : IReadFromByteBuffer, IWriteToByteBuffer
        {
        }

        private sealed class SomeTestHandler : ITestByteBuffer
        {
            private readonly string _s = new string('x', 1024);

            public void Write(ByteBuffer bb)
            {
                var startingPos = ByteBuffer.StartWritePacket(bb);
                ByteBuffer.WriteUlong(bb, 0xddddccccbbbbaaaaL);
                ByteBuffer.WriteString(bb, _s);
                ByteBuffer.WriteInt(bb, 45);
                ByteBuffer.WriteUint(bb, 33);
                ByteBuffer.WriteUlong(bb, 0xFDECBA9876543210L);
                ByteBuffer.WriteInt(bb, 0); // not read by reader
                ByteBuffer.EndWritePacket(bb, startingPos);
            }

            public bool TryRead(ByteBuffer bb)
            {
                int endPos;
                if (!ByteBuffer.StartTryReadPacket(bb, out endPos))
                    return false;

                ByteBuffer.ReadUlong(bb);
                ByteBuffer.ReadString(bb);
                ByteBuffer.ReadInt(bb);
                ByteBuffer.ReadUint(bb);
                ByteBuffer.ReadUlong(bb);

                ByteBuffer.EndReadPacket(bb, endPos);
                return true;
            }
        }

        [Test]
        public void TestUnsupportedObjectType()
        {
            var bb = new ByteBuffer(4);
            Assert.Throws<Exception>(() => { ByteBuffer.WriteObject(bb, new NotImplementedException()); });

            ByteBuffer.WriteObject(bb, true);
            ByteBuffer.Commit(bb);
            bb.Bytes[0] = ByteBuffer.TypeId.Reserved;
            Assert.Throws<Exception>(() => { ByteBuffer.ReadObject(bb); });

        }
        [Test]
        public void Test()
        {
            var bytes = new byte[] {3, 4, 5, 6};
            var byteSeg = new ArraySegment<byte>(bytes,1,2);

            const string thisIsATest = "this is a test \u3333 \x22";
            const double d = 1.2345e-200;
            const float f = 1.2345e22f;
            const ulong ul = 0xddddccccbbbbaaaaL;
            const long l = 0x4dddcc3cbb2baa1aL;

            var bb = new ByteBuffer(4);

            for (var i = 0; i < 4; ++i)
            {
                Console.WriteLine(i);
                var startingPos = ByteBuffer.StartWritePacket(bb);
                ByteBuffer.WriteUlong(bb, ul);
                ByteBuffer.WriteUshort(bb, 2345);
                ByteBuffer.WriteBool(bb, true);
                ByteBuffer.WriteLong(bb, l);
                ByteBuffer.WriteBool(bb, false);
                ByteBuffer.WriteByte(bb, 0x44);
                ByteBuffer.WriteDouble(bb, d);
                ByteBuffer.WriteFloat(bb, f);
                ByteBuffer.WriteInt(bb, 45);
                ByteBuffer.WriteUint(bb, 33);
                ByteBuffer.WriteBytes(bb, bytes);
                ByteBuffer.WriteBytes(bb, byteSeg);
                ByteBuffer.WriteString(bb, thisIsATest);
                ByteBuffer.WriteString(bb, null);
                ByteBuffer.WriteUlong(bb, 0xFDECBA9876543210L);
                ByteBuffer.WriteInt(bb, 0); // not read by reader
                ByteBuffer.EndWritePacket(bb, startingPos);

                var startingPos1 = ByteBuffer.StartWritePacket(bb);
                ByteBuffer.WriteInt(bb, 55);
                ByteBuffer.EndWritePacket(bb, startingPos1);

                Assert.IsFalse(ByteBuffer.TryReadPacket(bb, r => { }));

                ByteBuffer.Commit(bb);

                Assert.IsTrue(ByteBuffer.TryReadPacket(bb, r =>
                {
                    Assert.AreEqual(ul, ByteBuffer.ReadUlong(r));
                    Assert.AreEqual(2345, ByteBuffer.ReadUshort(r));
                    Assert.AreEqual(true, ByteBuffer.ReadBool(r));
                    Assert.AreEqual(l, ByteBuffer.ReadLong(r));
                    Assert.AreEqual(false, ByteBuffer.ReadBool(r));
                    Assert.AreEqual(0x44, ByteBuffer.ReadByte(r));
                    Assert.AreEqual(d, ByteBuffer.ReadDouble(r));
                    Assert.AreEqual(f, ByteBuffer.ReadFloat(r));
                    Assert.AreEqual(45, ByteBuffer.ReadInt(r));
                    Assert.AreEqual(33, ByteBuffer.ReadUint(r));
                    var readBytes = ByteBuffer.ReadBytes(r);
                    Assert.IsTrue(bytes.Eq(readBytes));
                    readBytes = ByteBuffer.ReadBytes(r);
                    Assert.IsTrue(byteSeg.Eq(readBytes));
                    var readString = ByteBuffer.ReadString(r);
                    Assert.AreEqual(thisIsATest, readString);
                    Assert.IsNull(ByteBuffer.ReadString(r));
                    Assert.AreEqual(0xFDECBA9876543210L, ByteBuffer.ReadUlong(r));
                }));

                var rp = bb.ReadPosition;
                Assert.Greater(rp, 0);
                ByteBuffer.Shift(bb);
                Assert.Greater(bb.WriteCommit, 0);
                Assert.AreEqual(0, bb.ReadPosition);
                ByteBuffer.Shift(bb);

                Assert.IsTrue(ByteBuffer.TryReadPacket(bb, r => { Assert.AreEqual(55, ByteBuffer.ReadInt(r)); }));

                ByteBuffer.Shift(bb);
                Assert.AreEqual(0, bb.ReadPosition);
                Assert.AreEqual(0, bb.WritePosition);
                Assert.AreEqual(0, bb.WriteCommit);

                var x = ByteBuffer.StartWritePacket(bb);
                ByteBuffer.WriteUlong(bb, 0xddddccccbbbbaaaaL);
                ByteBuffer.WriteString(bb, thisIsATest);
                ByteBuffer.WriteInt(bb, 45);
                ByteBuffer.WriteUint(bb, 33);
                ByteBuffer.WriteUlong(bb, 0xFDECBA9876543210L);
                ByteBuffer.WriteInt(bb, 0); // not read by reader
                ByteBuffer.EndWritePacket(bb, x);
                ByteBuffer.Commit(bb);
                int ep;
                ByteBuffer.StartTryReadPacket(bb, out ep);
                Assert.AreEqual(0xddddccccbbbbaaaaL, ByteBuffer.ReadUlong(bb));
                Assert.AreEqual(thisIsATest, ByteBuffer.ReadString(bb));
                Assert.AreEqual(45, ByteBuffer.ReadInt(bb));
                Assert.AreEqual(33, ByteBuffer.ReadUint(bb));
                Assert.AreEqual(0xFDECBA9876543210L, ByteBuffer.ReadUlong(bb));
                ByteBuffer.EndReadPacket(bb, ep);

                ByteBuffer.Shift(bb);
                Assert.AreEqual(0, bb.ReadPosition);
                Assert.AreEqual(0, bb.WritePosition);
                Assert.AreEqual(0, bb.WriteCommit);
            }
        }

        [Test]
        public void TestAppend()
        {
            var bb1 = new ByteBuffer(4);
            var bb2 = new ByteBuffer(4);

            var startingPos = ByteBuffer.StartWritePacket(bb1);
            ByteBuffer.WriteUlong(bb1, 0xddddccccbbbbaaaaL);
            ByteBuffer.EndWritePacket(bb1, startingPos);

            ByteBuffer.Commit(bb1);

            var startingPos1 = ByteBuffer.StartWritePacket(bb2);
            ByteBuffer.WriteLong(bb2, 0x1d3d3cccb3bba3aaL);
            ByteBuffer.EndWritePacket(bb2, startingPos1);

            ByteBuffer.Commit(bb2);

            ByteBuffer.Append(bb1, bb2);

            ByteBuffer.Commit(bb1);

            Assert.AreEqual(ByteBuffer.AntiCorruptionEnabled ? 40 : 24, bb1.Count);

            ByteBuffer.TryReadPacket(bb1, bb => { Assert.AreEqual(0xddddccccbbbbaaaaL, ByteBuffer.ReadUlong(bb)); });
            ByteBuffer.TryReadPacket(bb1, bb => { Assert.AreEqual(0x1d3d3cccb3bba3aaL, ByteBuffer.ReadLong(bb)); });
        }

        [Test]
        public void TestCorruptPacketDetection()
        {
            if (!ByteBuffer.AntiCorruptionEnabled)
                return;

            var bb = new ByteBuffer(16);

            var startingPos = ByteBuffer.StartWritePacket(bb);
            ByteBuffer.WriteUlong(bb, 0xddddccccbbbbaaaaL);
            ByteBuffer.WriteBool(bb, true);
            ByteBuffer.EndWritePacket(bb, startingPos);

            ByteBuffer.Commit(bb);

            bb.Bytes[6] ^= 1;

            Assert.Throws<Exception>(
                () => ByteBuffer.TryReadPacket(bb,
                    r => { Assert.AreEqual(0xddddccccbbbbaaaaL, ByteBuffer.ReadUlong(r)); }
                    )
                );
        }

        [Test]
        public void TestCorruptPacketSizeDetection()
        {
            var bb = new ByteBuffer(16);

            var startingPos = ByteBuffer.StartWritePacket(bb);
            ByteBuffer.WriteUlong(bb, 0xddddccccbbbbaaaaL);
            ByteBuffer.EndWritePacket(bb, startingPos);

            ByteBuffer.Commit(bb);

            bb.Bytes[0] ^= 1;

            Assert.Throws<Exception>(
                () => ByteBuffer.TryReadPacket(bb,
                    r => { Assert.AreEqual(0xddddccccbbbbaaaaL, ByteBuffer.ReadUlong(r)); }
                    )
                );
        }

        [Test]
        public void TestEnsure()
        {
            var bb = new ByteBuffer(16);
            bb.Ensure(1024);
            Assert.GreaterOrEqual(1024, bb.Bytes.Length);
        }

        [Test]
        public void TestFragment()
        {
            var bb = new ByteBuffer(4);

            var startingPos = ByteBuffer.StartWritePacket(bb);
            ByteBuffer.WriteUlong(bb, 0xddddccccbbbbaaaaL);
            ByteBuffer.WriteInt(bb, 45);
            ByteBuffer.WriteUint(bb, 33);
            ByteBuffer.WriteUlong(bb, 0xFDECBA9876543210L);
            ByteBuffer.WriteInt(bb, 0); // not read by reader
            ByteBuffer.EndWritePacket(bb, startingPos);
            ByteBuffer.Commit(bb);
            ByteBuffer.Shift(bb);

            bb.WriteCommit -= 1;

            Assert.IsFalse(ByteBuffer.TryReadPacket(bb, r => { }));
            ByteBuffer.Shift(bb);

            Assert.AreEqual(0, bb.ReadPosition);
            Assert.AreNotEqual(0, bb.WritePosition);
            Assert.AreNotEqual(0, bb.WriteCommit);

            bb.WriteCommit = bb.WritePosition;
            Assert.IsTrue(ByteBuffer.TryReadPacket(bb, r => { }));

            ByteBuffer.Shift(bb);
            Assert.AreEqual(0, bb.ReadPosition);
            Assert.AreEqual(0, bb.WritePosition);
            Assert.AreEqual(0, bb.WriteCommit);
        }

        [Test]
        public void TestObject()
        {
            var bb = new ByteBuffer();
            ByteBuffer.WriteObject(bb, null);
            ByteBuffer.WriteObject(bb, "string");
            ByteBuffer.WriteObject(bb, true);
            ByteBuffer.WriteObject(bb, false);
            ByteBuffer.WriteObject(bb, (byte) 0);
            ByteBuffer.WriteObject(bb, (sbyte) 0);
            ByteBuffer.WriteObject(bb, 'c');
            ByteBuffer.WriteObject(bb, (short) 0);
            ByteBuffer.WriteObject(bb, (ushort) 0);
            ByteBuffer.WriteObject(bb, 0);
            ByteBuffer.WriteObject(bb, (uint) 0);
            ByteBuffer.WriteObject(bb, (long) 0);
            ByteBuffer.WriteObject(bb, (ulong) 0);
            ByteBuffer.WriteObject(bb, (float) 0);
            ByteBuffer.WriteObject(bb, (double) 0);

            ByteBuffer.WriteObject(bb, byte.MinValue);
            ByteBuffer.WriteObject(bb, byte.MaxValue);
            ByteBuffer.WriteObject(bb, char.MinValue);
            ByteBuffer.WriteObject(bb, char.MaxValue);
            ByteBuffer.WriteObject(bb, sbyte.MinValue);
            ByteBuffer.WriteObject(bb, sbyte.MaxValue);
            ByteBuffer.WriteObject(bb, ushort.MinValue);
            ByteBuffer.WriteObject(bb, ushort.MaxValue);
            ByteBuffer.WriteObject(bb, short.MinValue);
            ByteBuffer.WriteObject(bb, short.MaxValue);
            ByteBuffer.WriteObject(bb, uint.MinValue);
            ByteBuffer.WriteObject(bb, uint.MaxValue);
            ByteBuffer.WriteObject(bb, int.MinValue);
            ByteBuffer.WriteObject(bb, int.MaxValue);
            ByteBuffer.WriteObject(bb, ulong.MinValue);
            ByteBuffer.WriteObject(bb, ulong.MaxValue);
            ByteBuffer.WriteObject(bb, long.MinValue);
            ByteBuffer.WriteObject(bb, long.MaxValue);
            ByteBuffer.WriteObject(bb, float.MinValue);
            ByteBuffer.WriteObject(bb, float.MaxValue);
            ByteBuffer.WriteObject(bb, double.MinValue);
            ByteBuffer.WriteObject(bb, double.MaxValue);

            ByteBuffer.WriteObject(bb, DateTime.MinValue);
            ByteBuffer.WriteObject(bb, DateTime.MaxValue);
            var dateTime = new DateTime(2000,1,1);
            ByteBuffer.WriteObject(bb, dateTime);

            ByteBuffer.Commit(bb);

            Assert.IsNull(ByteBuffer.ReadObject(bb));
            Assert.AreEqual("string", ByteBuffer.ReadObject(bb));
            Assert.AreEqual(true, ByteBuffer.ReadObject(bb));
            Assert.AreEqual(false, ByteBuffer.ReadObject(bb));
            Assert.AreEqual((byte) 0, ByteBuffer.ReadObject(bb));
            Assert.AreEqual((sbyte) 0, ByteBuffer.ReadObject(bb));
            Assert.AreEqual('c', ByteBuffer.ReadObject(bb));
            Assert.AreEqual((short) 0, ByteBuffer.ReadObject(bb));
            Assert.AreEqual((ushort) 0, ByteBuffer.ReadObject(bb));
            Assert.AreEqual(0, ByteBuffer.ReadObject(bb));
            Assert.AreEqual((uint) 0, ByteBuffer.ReadObject(bb));
            Assert.AreEqual((long) 0, ByteBuffer.ReadObject(bb));
            Assert.AreEqual((ulong) 0, ByteBuffer.ReadObject(bb));
            Assert.AreEqual((float) 0, ByteBuffer.ReadObject(bb));
            Assert.AreEqual((double) 0, ByteBuffer.ReadObject(bb));

            Assert.AreEqual(byte.MinValue, ByteBuffer.ReadObject(bb));
            Assert.AreEqual(byte.MaxValue, ByteBuffer.ReadObject(bb));
            Assert.AreEqual(char.MinValue, ByteBuffer.ReadObject(bb));
            Assert.AreEqual(char.MaxValue, ByteBuffer.ReadObject(bb));
            Assert.AreEqual(sbyte.MinValue, ByteBuffer.ReadObject(bb));
            Assert.AreEqual(sbyte.MaxValue, ByteBuffer.ReadObject(bb));
            Assert.AreEqual(ushort.MinValue, ByteBuffer.ReadObject(bb));
            Assert.AreEqual(ushort.MaxValue, ByteBuffer.ReadObject(bb));
            Assert.AreEqual(short.MinValue, ByteBuffer.ReadObject(bb));
            Assert.AreEqual(short.MaxValue, ByteBuffer.ReadObject(bb));
            Assert.AreEqual(uint.MinValue, ByteBuffer.ReadObject(bb));
            Assert.AreEqual(uint.MaxValue, ByteBuffer.ReadObject(bb));
            Assert.AreEqual(int.MinValue, ByteBuffer.ReadObject(bb));
            Assert.AreEqual(int.MaxValue, ByteBuffer.ReadObject(bb));
            Assert.AreEqual(ulong.MinValue, ByteBuffer.ReadObject(bb));
            Assert.AreEqual(ulong.MaxValue, ByteBuffer.ReadObject(bb));
            Assert.AreEqual(long.MinValue, ByteBuffer.ReadObject(bb));
            Assert.AreEqual(long.MaxValue, ByteBuffer.ReadObject(bb));
            Assert.AreEqual(float.MinValue, ByteBuffer.ReadObject(bb));
            Assert.AreEqual(float.MaxValue, ByteBuffer.ReadObject(bb));
            Assert.AreEqual(double.MinValue, ByteBuffer.ReadObject(bb));
            Assert.AreEqual(double.MaxValue, ByteBuffer.ReadObject(bb));

            Assert.AreEqual(DateTime.MinValue, ByteBuffer.ReadObject(bb));
            Assert.AreEqual(DateTime.MaxValue, ByteBuffer.ReadObject(bb));
            Assert.AreEqual(dateTime,ByteBuffer.ReadObject(bb));
        }


        [Test]
        public void TestPacketSizeLimit()
        {
            var bb = new ByteBuffer(1024*1024*2);

            Assert.Throws<Exception>(
                () =>
                {
                    var startingPos = ByteBuffer.StartWritePacket(bb);
                    for (ulong i = 0; i < 1024*1024/8 + 1; ++i)
                        ByteBuffer.WriteUlong(bb, i);
                    ByteBuffer.EndWritePacket(bb, startingPos);
                });
        }

        [Test]
        [Ignore]
        public void TestPerf()
        {
            var s = new string('x', 1024);

            var swd = new Stopwatch();
            var swv = new Stopwatch();
            var bb = new ByteBuffer(32*1024*1024);

            IDictionary<int, ITestByteBuffer> dict = new Dictionary<int, ITestByteBuffer> {{0, new SomeTestHandler()}};

            var vrbb = dict[0];

            for (var j = 0; j < 100; ++j)
            {

                {
                    swd.Start();
                    for (var i = 0; i < 10000; ++i)
                    {
                        var x = ByteBuffer.StartWritePacket(bb);
                        ByteBuffer.WriteUlong(bb, 0xddddccccbbbbaaaaL);
                        ByteBuffer.WriteString(bb, s);
                        ByteBuffer.WriteInt(bb, 45);
                        ByteBuffer.WriteUint(bb, 33);
                        ByteBuffer.WriteUlong(bb, 0xFDECBA9876543210L);
                        ByteBuffer.WriteInt(bb, 0); // not read by reader
                        ByteBuffer.EndWritePacket(bb, x);
                        ByteBuffer.Commit(bb);
                        int ep;
                        ByteBuffer.StartTryReadPacket(bb, out ep);
                        ByteBuffer.ReadUlong(bb);
                        ByteBuffer.ReadString(bb);
                        ByteBuffer.ReadInt(bb);
                        ByteBuffer.ReadUint(bb);
                        ByteBuffer.ReadUlong(bb);
                        ByteBuffer.EndReadPacket(bb, ep);
                        ByteBuffer.Reset(bb);
                    }

                    swd.Stop();
                }

                {
                    swv.Start();
                    for (var i = 0; i < 10000; ++i)
                    {
                        vrbb.Write(bb);
                        ByteBuffer.Commit(bb);
                        vrbb.TryRead(bb);
                        ByteBuffer.Reset(bb);
                    }

                    swv.Stop();
                }
            }

            Console.WriteLine("direct {0}ms", swd.ElapsedMilliseconds);
            Console.WriteLine("vdispl {0}ms", swv.ElapsedMilliseconds);
        }

        [Test]
        public void TestShiftAvoidance()
        {
            var bb = new ByteBuffer(1024*1024*2);

            var s = new string('x', 1024);
            var startingPos = ByteBuffer.StartWritePacket(bb);
            ByteBuffer.WriteString(bb, s);
            ByteBuffer.EndWritePacket(bb, startingPos);
            var startingPos1 = ByteBuffer.StartWritePacket(bb);
            ByteBuffer.WriteUlong(bb, 1);
            ByteBuffer.EndWritePacket(bb, startingPos1);
            var startingPos2 = ByteBuffer.StartWritePacket(bb);
            ByteBuffer.WriteString(bb, s);
            ByteBuffer.EndWritePacket(bb, startingPos2);
            var startingPos3 = ByteBuffer.StartWritePacket(bb);
            ByteBuffer.WriteUlong(bb, 2);
            ByteBuffer.EndWritePacket(bb, startingPos3);
            ByteBuffer.Commit(bb);
            ByteBuffer.TryReadPacket(bb, r => { Assert.AreEqual(s, ByteBuffer.ReadString(r)); });
            ByteBuffer.TryReadPacket(bb, r => { Assert.AreEqual(1, ByteBuffer.ReadUlong(r)); });
            ByteBuffer.Shift(bb);
            Assert.AreNotEqual(0, bb.ReadPosition);
            ByteBuffer.TryReadPacket(bb, r => { Assert.AreEqual(s, ByteBuffer.ReadString(r)); });
            var startingPos4 = ByteBuffer.StartWritePacket(bb);
            ByteBuffer.WriteUlong(bb, 3);
            ByteBuffer.EndWritePacket(bb, startingPos4);
            ByteBuffer.Commit(bb);
            ByteBuffer.TryReadPacket(bb, r => { Assert.AreEqual(2, ByteBuffer.ReadUlong(r)); });
            Assert.AreNotEqual(0, bb.ReadPosition);
            ByteBuffer.TryReadPacket(bb, r => { Assert.AreEqual(3, ByteBuffer.ReadUlong(r)); });
            ByteBuffer.Shift(bb);
            Assert.AreEqual(0, bb.ReadPosition);
        }
    }
}