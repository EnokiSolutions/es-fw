//exclude from duplicate code check

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NUnit.Framework;

namespace Es.Fw.Test
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public sealed class SnappyCompressorTextFixture
    {
        // CollectionAssert.AreEquivalent is horridly slow btw.
        private static void FailIfByteArraysDiffer(byte[] b1, byte[] b2)
        {
            for (var i = 0; i < b1.Length; ++i)
                if (b2[i] != b1[i])
                {
                    Console.WriteLine("Differ at {0}: {1} {2}", i, b1[i], b2[i]);
                    Assert.Fail();
                }
        }

        [Test]
        public void TestCorruptLiteralLength()
        {
            var compressor = Default.SnappyCompressor;

            var data = new byte[]
            {
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4,
                5, 6, 7, 8, 9
            };

            var uncompressedInput = new ArraySegment<byte>(data);
            var compressInfo = compressor.Analyze(uncompressedInput);
            var compressedBytes = new byte[compressInfo.MaxCompressedSize];
            var compressedLength = compressor.Compress(uncompressedInput, new ArraySegment<byte>(compressedBytes),
                compressInfo);
            var compressed = new ArraySegment<byte>(compressedBytes, 0, compressedLength);

            compressedBytes[1] = 0xfc;

            var decompressor = Default.SnappyDecompressor;

            Assert.Throws<ArgumentException>(
                () =>
                {
                    var info = decompressor.Analyze(compressed);
                    var decompressed = new ArraySegment<byte>(new byte[info.UncompressedSize]);
                    decompressor.Decompress(compressed, decompressed, info);
                });
        }

        [Test]
        [TestCase(
            new byte[]
            {
                40, 36, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 118, 10, 0
            },
            new byte[]
            {
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4,
                5,
                6, 7, 8, 9
            })]
        [TestCase(
            new byte[]
            {
                40, 36, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 110, 10, 0, 4, 8, 9
            },
            new byte[]
            {
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4,
                5,
                6, 7, 8, 9
            })]
        [TestCase(
            new byte[]
            {
                82, 28, 5, 0, 5, 0, 6, 0, 6, 0, 1, 8, 5, 4, 5, 14, 5, 10, 5, 6, 5, 6, 5, 6, 8, 5, 0, 4, 5, 8, 5, 14, 5,
                12,
                5, 10, 5, 24, 44, 5, 0, 5, 0, 5, 0, 5, 0, 5, 0, 5, 0
            },
            new byte[]
            {
                5, 0, 5, 0, 6, 0, 6, 0, 5, 0, 5, 0, 5, 0, 5, 0, 5, 0, 6, 0, 6, 0, 5, 0, 5, 0, 5, 0, 5, 0, 5, 0, 5, 0, 5,
                0,
                5, 0, 5, 0, 5, 0, 5, 0, 4, 0, 5, 0, 5, 0, 5, 0, 5, 0, 5, 0, 4, 0, 5, 0, 5, 0, 5, 0, 5, 0, 5, 0, 4, 0, 5,
                0, 5, 0, 5, 0, 5, 0, 5, 0, 5, 0
            })]
        [TestCase(
            new byte[]
            {
                82, 28, 5, 0, 5, 0, 6, 0, 6, 0, 1, 8, 9, 4, 54, 14, 0, 9, 20, 9, 6, 0, 4, 13, 8, 1, 14, 94, 12, 0, 4, 5,
                0
            },
            new byte[]
            {
                5, 0, 5, 0, 6, 0, 6, 0, 5, 0, 5, 0, 5, 0, 5, 0, 5, 0, 6, 0, 6, 0, 5, 0, 5, 0, 5, 0, 5, 0, 5, 0, 5, 0, 5,
                0,
                5, 0, 5, 0, 5, 0, 5, 0, 4, 0, 5, 0, 5, 0, 5, 0, 5, 0, 5, 0, 4, 0, 5, 0, 5, 0, 5, 0, 5, 0, 5, 0, 4, 0, 5,
                0, 5, 0, 5, 0, 5, 0, 5, 0, 5, 0
            })]
        public void TestDecompressOldCompressionResult(byte[] compressed, byte[] uncompressed)
        {
            var decompressor = Default.SnappyDecompressor;
            var compressedAseg = new ArraySegment<byte>(compressed);
            var info = decompressor.Analyze(compressedAseg);
            var decompressed = new ArraySegment<byte>(new byte[info.UncompressedSize]);
            decompressor.Decompress(compressedAseg, decompressed, info);
            FailIfByteArraysDiffer(uncompressed, decompressed.Array);
        }

        [Test]
        public void TestInvalidDataLength()
        {
            Assert.Throws<ArgumentException>(() => SnappyCompressor.Compressor.Compress(null, 0, -1, null, -1));
        }

        [Test]
        public void TestLarge()
        {
            var compressor = Default.SnappyCompressor;
            var decompressor = Default.SnappyDecompressor;

            var j = 0;
            for (var size = 1024; size <= 128*1024*1024; size += 1024 + (size << 1))
            {
                var n = size + j++;
                Console.Write(n);
                var data = new byte[n];
                for (var i = 0; i < data.Length; ++i)
                    data[i] = (byte) (i & 0xff);

                var uncompressedInput = new ArraySegment<byte>(data);
                var compressInfo = compressor.Analyze(uncompressedInput);
                var compressedBytes = new byte[compressInfo.MaxCompressedSize];
                var swc = Stopwatch.StartNew();
                var compressedLength = compressor.Compress(uncompressedInput, new ArraySegment<byte>(compressedBytes),
                    compressInfo);
                swc.Stop();
                var swd = Stopwatch.StartNew();
                var compressed = new ArraySegment<byte>(compressedBytes, 0, compressedLength);
                var info = decompressor.Analyze(compressed);
                var decompressedBytes = new byte[info.UncompressedSize];
                var decompressed = new ArraySegment<byte>(decompressedBytes);
                decompressor.Decompress(compressed, decompressed, info);
                swd.Stop();
                FailIfByteArraysDiffer(data, decompressedBytes);
                Console.WriteLine(" compress={0}ms decompress={1}ms ratio={2:F3}", swc.ElapsedMilliseconds,
                    swd.ElapsedMilliseconds, (double) compressedLength/data.Length);
            }
        }

        [Test]
        public void TestRange()
        {
            var compressor = Default.SnappyCompressor;
            var decompressor = Default.SnappyDecompressor;

            for (var size = 1; size <= 1024; ++size)
            {
                Console.Write(size);
                Console.Write(" ");
                var data = new byte[size];
                for (var i = 0; i < data.Length; ++i)
                    data[i] = (byte) (i & 0xff);

                var uncompressedInput = new ArraySegment<byte>(data);
                var compressInfo = compressor.Analyze(uncompressedInput);
                var compressedBytes = new byte[compressInfo.MaxCompressedSize];
                var compressedLength = compressor.Compress(uncompressedInput, new ArraySegment<byte>(compressedBytes),
                    compressInfo);
                var compressed = new ArraySegment<byte>(compressedBytes, 0, compressedLength);
                var decompressionInfo = decompressor.Analyze(compressed);
                var decompressed = new byte[decompressionInfo.UncompressedSize];

                decompressor.Decompress(compressed, new ArraySegment<byte>(decompressed), decompressionInfo);

                FailIfByteArraysDiffer(data, decompressed);
            }
        }

        [Test]
        public void TestSimple()
        {
            var compressor = Default.SnappyCompressor;
            var decompressor = Default.SnappyDecompressor;

            var uncompressed = new byte[]
            {
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4,
                5,
                6, 7, 8, 9
            };
            var uncompressedInput = new ArraySegment<byte>(uncompressed);
            var compressedInfo = compressor.Analyze(uncompressedInput);
            var compressedBytes = new byte[compressedInfo.MaxCompressedSize];
            var compressedLength = compressor.Compress(uncompressedInput, new ArraySegment<byte>(compressedBytes),
                compressedInfo);

            Console.WriteLine("var compressed = new byte[] {" + string.Join(",", compressedBytes.Take(compressedLength)) +
                              "}");
            var compressed = new ArraySegment<byte>(compressedBytes, 0, compressedLength);

            var info = decompressor.Analyze(compressed);
            var decompressed = new byte[info.UncompressedSize];

            decompressor.Decompress(compressed, new ArraySegment<byte>(decompressed), info);

            FailIfByteArraysDiffer(uncompressed, decompressed);

            var data2 = new byte[]
            {
                5, 0, 5, 0, 6, 0, 6, 0, 5, 0, 5, 0, 5, 0, 5, 0, 5, 0, 6, 0, 6, 0, 5, 0, 5, 0, 5, 0, 5, 0, 5, 0, 5, 0, 5,
                0, 5, 0, 5, 0, 5, 0, 5, 0, 4, 0, 5, 0, 5, 0, 5, 0, 5, 0, 5, 0, 4, 0, 5, 0, 5, 0, 5, 0, 5, 0, 5, 0, 4, 0,
                5, 0, 5, 0, 5, 0, 5, 0, 5, 0, 5, 0
            };

            var compressedBytes2 = new byte[SnappyCompressor.Compressor.MaxCompressedLength(uncompressed.Length)];
            var compressedLength2 = SnappyCompressor.Compressor.Compress(data2, 0, data2.Length, compressedBytes2, 0);
            Console.WriteLine("var compressed2 = new byte[] {" +
                              string.Join(",", compressedBytes2.Take(compressedLength2)) + "}");
            var compressed2 = new ArraySegment<byte>(compressedBytes2, 0, compressedLength2);
            var info2 = decompressor.Analyze(compressed2);
            var decompressed2 = new byte[info2.UncompressedSize];
            decompressor.Decompress(compressed2, new ArraySegment<byte>(decompressed2), info2);

            FailIfByteArraysDiffer(data2, decompressed2);
        }
    }
}