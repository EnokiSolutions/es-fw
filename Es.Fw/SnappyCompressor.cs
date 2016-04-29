using System;
using System.Runtime.CompilerServices;
using Es.FwI;

namespace Es.Fw
{
    internal sealed class SnappyCompressor : ICompress
    {
        ICompressInfo ICompress.Analyze(ArraySegment<byte> input)
        {
            return new SnappyCompressInfo(Compressor.MaxCompressedLength(input.Count));
        }

        int ICompress.Compress(ArraySegment<byte> input, ArraySegment<byte> output, ICompressInfo info)
        {
            return Compressor.Compress(input.Array, input.Offset, input.Count, output.Array, output.Offset);
        }

        // based on the csharp port with some minor fixes. mostly covered except for some weird cases.
        internal static class Compressor
        {
            public static int MaxCompressedLength(int sourceLength)
            {
                // So says the code from Google.
                return 32 + sourceLength + sourceLength/6;
            }

            public static int Compress(byte[] uncompressed, int uncompressedOffset, int uncompressedLength,
                byte[] compressed, int compressedOffset)
            {
                if (uncompressedLength < 0)
                    throw new ArgumentException("uncompressedLength");

                var compressedIndex = WriteUncomressedLength(compressed, compressedOffset, uncompressedLength);
                var headLength = compressedIndex - compressedOffset;
                return headLength +
                       CompressInternal(uncompressed, uncompressedOffset, uncompressedLength, compressed,
                           compressedIndex);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static uint ReadUint(byte[] source, int index)
            {
                return ((uint) source[index] << 24) | ((uint) source[index + 1] << 16) | ((uint) source[index + 2] << 8) |
                       source[index + 3];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static int CompressInternal(byte[] uncompressed, int uncompressedOffset, int uncompressedLength,
                byte[] compressed, int compressedOffset)
            {
                // first time through set to offset.
                var compressedIndex = compressedOffset;
                var hashTable = GetHashTable(uncompressedLength);

                for (var read = 0; read < uncompressedLength; read += BLOCK_SIZE)
                {
                    // Get encoding table for compression
                    Array.Clear(hashTable, 0, hashTable.Length);

                    compressedIndex = CompressFragment(
                        uncompressed,
                        uncompressedOffset + read,
                        Math.Min(uncompressedLength - read, BLOCK_SIZE),
                        compressed,
                        compressedIndex,
                        hashTable);
                }
                return compressedIndex - compressedOffset;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            // Function from http://aggregate.org/MAGIC/
            private static uint Log2Floor(uint x)
            {
                x |= x >> 1;
                x |= x >> 2;
                x |= x >> 4;
                x |= x >> 8;
                x |= x >> 16;
                // here Log2Floor(0) = 0
                return NumberOfOnes(x >> 1);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            // Function from http://aggregate.org/MAGIC/
            private static uint NumberOfOnes(uint x)
            {
                x -= (x >> 1) & 0x55555555;
                x = ((x >> 2) & 0x33333333) + (x & 0x33333333);
                x = ((x >> 4) + x) & 0x0f0f0f0f;
                x += x >> 8;
                x += x >> 16;
                return x & 0x0000003f;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static int CompressFragment(byte[] input, int inputOffset, int inputSize, byte[] output,
                int outputIndex, short[] hashTable)
            {
                // "ip" is the input pointer, and "op" is the output pointer.
                var inputIndex = inputOffset;
                var shift = (int) (32 - Log2Floor((uint) hashTable.Length));
                //DCHECK_EQ(static_cast<int>(kuint32max >> shift), table_size - 1);
                var inputEnd = inputOffset + inputSize;
                var baseInputIndex = inputIndex;
                // Bytes in [next_emit, ip) will be emitted as literal bytes.  Or
                // [next_emit, ip_end) after the main loop.
                var nextEmitIndex = inputIndex;

                if (inputSize >= INPUT_MARGIN_BYTES)
                {
                    var ipLimit = inputOffset + inputSize - INPUT_MARGIN_BYTES;

                    var currentIndexBytes = ReadUint(input, ++inputIndex);
                    for (var nextHash = Hash(currentIndexBytes, shift);;)
                    {
                        uint skip = 32;

                        var nextIp = inputIndex;
                        int candidate;
                        do
                        {
                            inputIndex = nextIp;
                            var hash = nextHash;
                            nextIp = (int) (inputIndex + (skip++ >> 5));
                            if (nextIp > ipLimit)
                            {
                                goto emit_remainder;
                            }
                            currentIndexBytes = ReadUint(input, nextIp);
                            nextHash = Hash(currentIndexBytes, shift);
                            candidate = baseInputIndex + hashTable[hash];

                            hashTable[hash] = (short) (inputIndex - baseInputIndex);
                        } while (ReadUint(input, inputIndex) != ReadUint(input, candidate));

                        outputIndex = EmitLiteral(output, outputIndex, input, nextEmitIndex, inputIndex - nextEmitIndex);

                        uint candidateBytes;
                        int insertTail;

                        do
                        {
                            var baseIndex = inputIndex;
                            var matched = 4 + FindMatchLength64(input, candidate + 4, inputIndex + 4, inputEnd);
                            inputIndex += matched;
                            var offset = baseIndex - candidate;
                            outputIndex = EmitCopy(output, outputIndex, offset, matched);
                            insertTail = inputIndex - 1;
                            nextEmitIndex = inputIndex;
                            if (inputIndex >= ipLimit)
                            {
                                goto emit_remainder;
                            }
                            var prevHash = Hash(ReadUint(input, insertTail), shift);
                            hashTable[prevHash] = (short) (inputIndex - baseInputIndex - 1);
                            var curHash = Hash(ReadUint(input, insertTail + 1), shift);
                            candidate = baseInputIndex + hashTable[curHash];
                            candidateBytes = ReadUint(input, candidate);
                            hashTable[curHash] = (short) (inputIndex - baseInputIndex);
                        } while (ReadUint(input, insertTail + 1) == candidateBytes);

                        nextHash = Hash(ReadUint(input, insertTail + 2), shift);
                        ++inputIndex;
                    }
                }

                emit_remainder:
                // Emit the remaining bytes as a literal
                if (nextEmitIndex < inputEnd)
                {
                    outputIndex = EmitLiteral(output, outputIndex, input, nextEmitIndex, inputEnd - nextEmitIndex);
                }

                return outputIndex;
            }

            private static int EmitCopyLessThan64(byte[] output, int outputIndex, int offset, int length)
            {
                var writeIndex = outputIndex;
                if ((length < 12) && (offset < 2048))
                {
                    var lenMinus4 = length - 4;
                    output[writeIndex++] = (byte) (COPY_1_BYTE_OFFSET | (lenMinus4 << 2) | ((offset >> 8) << 5));
                    output[writeIndex++] = (byte) offset;
                }
                else
                {
                    output[writeIndex++] = (byte) (COPY_2_BYTE_OFFSET | ((length - 1) << 2));
                    output[writeIndex++] = (byte) offset;
                    output[writeIndex++] = (byte) (offset >> 8);
                }
                return writeIndex;
            }

            private static int EmitCopy(byte[] compressed, int compressedIndex, int offset, int length)
            {
                // Emit 64 byte copies but make sure to keep at least four bytes reserved
                while (length >= 68)
                {
                    compressedIndex = EmitCopyLessThan64(compressed, compressedIndex, offset, 64);
                    length -= 64;
                }

                // Emit an extra 60 byte copy if have too much data to fit in one copy
                if (length > 64)
                {
                    // this case doesn't appear to happen, which is why I'm exluding coverage here.
                    compressedIndex = EmitCopyLessThan64(compressed, compressedIndex, offset, 60);
                    length -= 60;
                }

                // Emit remainder
                compressedIndex = EmitCopyLessThan64(compressed, compressedIndex, offset, length);
                return compressedIndex;
            }

            // 64-bit optimized version of above
            private static unsafe int FindMatchLength64(byte[] s1, int s1Index, int s2Index, int s2Limit)
            {
                var matched = 0;

                fixed (byte* sp = s1) // avoid array bounds checks
                {
                    while (s2Index < s2Limit)
                    {
                        if (sp[s1Index + matched] != sp[s2Index])
                        {
                            break;
                        }
                        ++s2Index;
                        ++matched;
                    }
                }
                return matched;
            }


            private static int EmitLiteral(byte[] output, int outputIndex, byte[] literal, int literalIndex, int length)
            {
                var n = length - 1;
                outputIndex = EmitLiteralTag(output, outputIndex, n);
                Buffer.BlockCopy(literal, literalIndex, output, outputIndex, length);
                return outputIndex + length;
            }

            private static int EmitLiteralTag(byte[] output, int outputIndex, int size)
            {
                if (size < 60)
                {
                    output[outputIndex++] = (byte) (LITERAL | (size << 2));
                }
                else
                {
                    var baseIndex = outputIndex;
                    outputIndex++;

                    var count = 0;
                    while (size > 0)
                    {
                        output[outputIndex++] = (byte) (size & 0xff);
                        size >>= 8;
                        ++count;
                    }

                    output[baseIndex] = (byte) (LITERAL | ((59 + count) << 2));
                }
                return outputIndex;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static uint Hash(uint bytes, int shift)
            {
                const int kMul = 0x1e35a7bd;
                return (bytes*kMul) >> shift;
            }

            private static int WriteUncomressedLength(byte[] compressed, int compressedOffset, int uncompressedLength)
            {
                const int bitMask = 0x80;

                // A little-endian varint. 
                // From doc:
                // Varints consist of a series of bytes, where the lower 7 bits are data and the upper bit is set iff there are more bytes to read.
                // In other words, an uncompressed length of 64 would be stored as 0x40, and an uncompressed length of 2097150 (0x1FFFFE) would
                // be stored as 0xFE 0XFF 0X7F
                while (uncompressedLength >= bitMask)
                {
                    compressed[compressedOffset++] = (byte) (uncompressedLength | bitMask);
                    uncompressedLength = uncompressedLength >> 7;
                }
                compressed[compressedOffset++] = (byte) uncompressedLength;

                return compressedOffset;
            }

            private static short[] GetHashTable(int inputLength)
            {
                var tableSize = 256;
                while (tableSize < MAX_HASH_TABLE_SIZE && tableSize < inputLength)
                {
                    tableSize <<= 1;
                }

                return new short[tableSize];
            }

            // ReSharper disable InconsistentNaming
            private const int LITERAL = 0;
            private const int COPY_1_BYTE_OFFSET = 1; // 3 bit length + 3 bits of offset in opcode
            private const int COPY_2_BYTE_OFFSET = 2;
            //private const int COPY_4_BYTE_OFFSET = 3;

            private const int BLOCK_LOG = 15;
            private const int BLOCK_SIZE = 1 << BLOCK_LOG;

            private const int INPUT_MARGIN_BYTES = 15;

            private const int MAX_HASH_TABLE_BITS = 16;
            private const int MAX_HASH_TABLE_SIZE = 1 << MAX_HASH_TABLE_BITS;
            // ReSharper restore InconsistentNaming
        }

        private sealed class SnappyCompressInfo : ICompressInfo
        {
            public SnappyCompressInfo(int maxCompressedSize)
            {
                MaxCompressedSize = maxCompressedSize;
            }

            public int MaxCompressedSize { get; }
        }
    }
}