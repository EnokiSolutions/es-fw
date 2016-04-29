#define HASH_PACKETS

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Es.Fw
{
    // TODO add package versioning support

    public sealed unsafe class ByteBuffer
    {
        public byte[] Bytes;
        public int ReadPosition;
        public int WritePosition;
        public int WriteCommit;
        public const int MaxPacketSize = 1024*1024;

        public static readonly bool AntiCorruptionEnabled =
#if HASH_PACKETS
            true
#else
            false
#endif
            ;

        public ByteBuffer(int initialSize = MaxPacketSize)
        {
            Bytes = new byte[initialSize];
            ReadPosition = 0;
            WritePosition = 0;
            WriteCommit = 0;
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return WriteCommit - ReadPosition; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Reset(ByteBuffer bb)
        {
            bb.ReadPosition = 0;
            bb.WritePosition = 0;
            bb.WriteCommit = 0;
        }

        public void Ensure(int pos)
        {
            if (Bytes.Length > pos)
                return;

            var tmp = Bytes;
            var newLength = Bytes.Length*2;
            if (newLength < pos)
                newLength = pos;
            Bytes = new byte[newLength];
            Buffer.BlockCopy(tmp, 0, Bytes, 0, tmp.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt(ByteBuffer bb, int i)
        {
            var nwp = bb.WritePosition + 4;
            bb.Ensure(nwp);

            fixed (byte* b = bb.Bytes)
            {
                var x = b + bb.WritePosition;
                *x = (byte) ((i & 0xFF000000) >> 24);
                *++x = (byte) ((i & 0x00FF0000) >> 16);
                *++x = (byte) ((i & 0x0000FF00) >> 8);
                *++x = (byte) (i & 0x000000FF);
            }

            bb.WritePosition = nwp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Commit(ByteBuffer bb)
        {
            bb.WriteCommit = bb.WritePosition;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadInt(ByteBuffer bb)
        {
            Debug.Assert(bb.WriteCommit - bb.ReadPosition >= 4);
            int i;
            fixed (byte* b = bb.Bytes)
            {
                var x = b + bb.ReadPosition;

                i = (*x << 24)
                    | (*++x << 16)
                    | (*++x << 8)
                    | *++x;
            }
            bb.ReadPosition += 4;
            return i;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUint(ByteBuffer bb, uint u)
        {
            var nwp = bb.WritePosition + 4;
            bb.Ensure(nwp);
            fixed (byte* b = bb.Bytes)
            {
                var x = b + bb.WritePosition;
                *x = (byte) ((u & 0xFF000000) >> 24);
                *++x = (byte) ((u & 0x00FF0000) >> 16);
                *++x = (byte) ((u & 0x0000FF00) >> 8);
                *++x = (byte) (u & 0x000000FF);
            }
            bb.WritePosition = nwp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadUint(ByteBuffer bb)
        {
            Debug.Assert(bb.WriteCommit - bb.ReadPosition >= 4);
            uint u;
            fixed (byte* b = bb.Bytes)
            {
                var x = b + bb.ReadPosition;
                u = (uint) (*x << 24)
                    | ((uint) *++x << 16)
                    | ((uint) *++x << 8)
                    | *++x;
            }
            bb.ReadPosition += 4;
            return u;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUshort(ByteBuffer bb, ushort u)
        {
            var nwp = bb.WritePosition + 2;
            bb.Ensure(nwp);
            fixed (byte* b = bb.Bytes)
            {
                var x = b + bb.WritePosition;
                *x = (byte) ((u & 0x0000FF00) >> 8);
                *++x = (byte) (u & 0x000000FF);
            }
            bb.WritePosition = nwp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReadUshort(ByteBuffer bb)
        {
            Debug.Assert(bb.WriteCommit - bb.ReadPosition >= 2);
            ushort u;
            fixed (byte* b = bb.Bytes)
            {
                var x = b + bb.ReadPosition;
                u = (ushort) ((*x << 8) | *++x);
            }
            bb.ReadPosition += 2;
            return u;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteShort(ByteBuffer bb, short u)
        {
            var nwp = bb.WritePosition + 2;
            bb.Ensure(nwp);
            fixed (byte* b = bb.Bytes)
            {
                var x = b + bb.WritePosition;
                *x = (byte) ((u & 0x0000FF00) >> 8);
                *++x = (byte) (u & 0x000000FF);
            }
            bb.WritePosition = nwp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ReadShort(ByteBuffer bb)
        {
            Debug.Assert(bb.WriteCommit - bb.ReadPosition >= 2);
            short u;
            fixed (byte* b = bb.Bytes)
            {
                var x = b + bb.ReadPosition;
                u = (short) ((*x << 8) | *++x);
            }
            bb.ReadPosition += 2;
            return u;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteChar(ByteBuffer bb, char v)
        {
            WriteUshort(bb, v);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char ReadChar(ByteBuffer bb)
        {
            return (char) ReadUshort(bb);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteByte(ByteBuffer bb, byte v)
        {
            var nwp = bb.WritePosition + 1;
            bb.Ensure(nwp);
            fixed (byte* b = bb.Bytes)
            {
                *(b + bb.WritePosition) = v;
            }
            bb.WritePosition = nwp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ReadByte(ByteBuffer bb)
        {
            Debug.Assert(bb.WriteCommit - bb.ReadPosition >= 1);
            byte v;
            fixed (byte* b = bb.Bytes)
            {
                v = *(b + bb.ReadPosition);
            }
            bb.ReadPosition += 1;
            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteSByte(ByteBuffer bb, sbyte v)
        {
            WriteByte(bb, (byte) v);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte ReadSByte(ByteBuffer bb)
        {
            return (sbyte) ReadByte(bb);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBool(ByteBuffer bb, bool v)
        {
            var nwp = bb.WritePosition + 1;
            bb.Ensure(nwp);
            fixed (byte* b = bb.Bytes)
            {
                *(b + bb.WritePosition) = v ? (byte) 1 : (byte) 0;
            }
            bb.WritePosition = nwp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ReadBool(ByteBuffer bb)
        {
            Debug.Assert(bb.WriteCommit - bb.ReadPosition >= 1);
            byte v;
            fixed (byte* b = bb.Bytes)
            {
                v = *(b + bb.ReadPosition);
            }
            bb.ReadPosition += 1;
            return v != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint FloatToUint(float f)
        {
            var p = &f;
            {
                return *(uint*) p;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float UintToFloat(uint i)
        {
            var p = &i;
            {
                return *(float*) p;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong DoubleToUlong(double d)
        {
            var p = &d;
            {
                return *(ulong*) p;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double UlongToDouble(ulong i)
        {
            var p = &i;
            {
                return *(double*) p;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteFloat(ByteBuffer bb, float f)
        {
            WriteUint(bb, FloatToUint(f));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ReadFloat(ByteBuffer bb)
        {
            return UintToFloat(ReadUint(bb));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteDouble(ByteBuffer bb, double d)
        {
            WriteUlong(bb, DoubleToUlong(d));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ReadDouble(ByteBuffer bb)
        {
            return UlongToDouble(ReadUlong(bb));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteLong(ByteBuffer bb, long l)
        {
            WriteUlong(bb, (ulong) l);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUlong(ByteBuffer bb, ulong ul)
        {
            var nwp = bb.WritePosition + 8;
            bb.Ensure(nwp);
            fixed (byte* b = bb.Bytes)
            {
                var x = b + bb.WritePosition;
                *x = (byte) ((ul & 0xFF00000000000000L) >> 56);
                *++x = (byte) ((ul & 0x00FF000000000000L) >> 48);
                *++x = (byte) ((ul & 0x0000FF0000000000L) >> 40);
                *++x = (byte) ((ul & 0x000000FF00000000L) >> 32);
                *++x = (byte) ((ul & 0x00000000FF000000L) >> 24);
                *++x = (byte) ((ul & 0x0000000000FF0000L) >> 16);
                *++x = (byte) ((ul & 0x000000000000FF00L) >> 8);
                *++x = (byte) (ul & 0x00000000000000FFL);
            }
            bb.WritePosition = nwp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ReadUlong(ByteBuffer bb)
        {
            Debug.Assert(bb.WriteCommit - bb.ReadPosition >= 8);
            ulong ul;
            fixed (byte* b = bb.Bytes)
            {
                var x = b + bb.ReadPosition;

                ul = (ulong) *x << 56
                     | ((ulong) *++x << 48)
                     | ((ulong) *++x << 40)
                     | ((ulong) *++x << 32)
                     | ((ulong) *++x << 24)
                     | ((ulong) *++x << 16)
                     | ((ulong) *++x << 8)
                     | *++x;
            }
            bb.ReadPosition += 8;
            return ul;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ReadLong(ByteBuffer bb)
        {
            return (long) ReadUlong(bb);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(ByteBuffer bb, byte[] b)
        {
            var length = b.Length;
            WriteInt(bb, length);
            bb.Ensure(bb.WritePosition + length);
            Buffer.BlockCopy(b, 0, bb.Bytes, bb.WritePosition, length);
            bb.WritePosition += length;
        }


        public static void WriteBytes(ByteBuffer bb, ArraySegment<byte> b)
        {
            var length = b.Count;
            WriteInt(bb, length);
            bb.Ensure(bb.WritePosition + length);
            Buffer.BlockCopy(b.Array, b.Offset, bb.Bytes, bb.WritePosition, length);
            bb.WritePosition += length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ReadBytes(ByteBuffer bb)
        {
            var length = ReadInt(bb);
            var b = new byte[length];
            Buffer.BlockCopy(bb.Bytes, bb.ReadPosition, b, 0, length);
            bb.ReadPosition += length;
            return b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteString(ByteBuffer bb, string s)
        {
            if (s == null)
            {
                WriteBool(bb, false);
                return;
            }
            WriteBool(bb, true);
            var length = s.Length;

            WriteInt(bb, length);
            var nwp = bb.WritePosition + length*2;
            bb.Ensure(nwp);
            fixed (byte* b = bb.Bytes)
            {
                var x = b + bb.WritePosition;
                fixed (char* c = s)
                {
                    var y = c;
                    for (var i = 0; i < length; ++i)
                    {
                        ushort us = *y;
                        ++y;

                        *x = (byte) (us >> 8);
                        ++x;
                        *x = (byte) (us & 0xff);
                        ++x;
                    }
                }
            }
            bb.WritePosition = nwp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReadString(ByteBuffer bb)
        {
            var isNull = ReadBool(bb);
            if (!isNull)
                return null;

            var length = ReadInt(bb);
            Debug.Assert(bb.WriteCommit - bb.ReadPosition >= 2*length);
            var s = new string('\0', length);

            fixed (byte* b = bb.Bytes)
            {
                var x = b + bb.ReadPosition;
                fixed (char* c = s)
                {
                    var y = c;

                    for (var i = 0; i < length; ++i)
                    {
                        var h = *x << 8;
                        ++x;
                        var l = *x;
                        ++x;

                        *y = (char) (h | l);
                        ++y;
                    }
                }
            }
            bb.ReadPosition += 2*length;
            return s;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int StartWritePacket(ByteBuffer bb)
        {
            Shift(bb);
            var startingPos = bb.WritePosition;
            WriteInt(bb, 0);
            return startingPos;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EndWritePacket(ByteBuffer bb, int startingPos)
        {
            var endingPos = bb.WritePosition;
            bb.WritePosition = startingPos;
            var packetLengthWithoutSizePrefix = endingPos - startingPos - 4;
            if (packetLengthWithoutSizePrefix > MaxPacketSize)
                throw new Exception("Packet too large, aborting.");
            WriteInt(bb, packetLengthWithoutSizePrefix);
            bb.WritePosition = endingPos;

#if HASH_PACKETS
            var hash = bb.Bytes.XxHash(startingPos, packetLengthWithoutSizePrefix);
            WriteUlong(bb, hash);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartTryReadPacket(ByteBuffer bb, out int endPos)
        {
            endPos = bb.ReadPosition;

            if (bb.WriteCommit - bb.ReadPosition < 4)
                return false;

            var startingtPos = bb.ReadPosition;
            var length = ReadInt(bb);
            if (length > MaxPacketSize || length < 0)
                throw new Exception("Packet length too large or negative! Corrupt packet data?");

#if HASH_PACKETS
            var endingHash = bb.ReadPosition + length;
            endPos = endingHash + 8;
#else
            endPos = bb.ReadPosition + length;
#endif
            if (bb.WriteCommit < endPos)
            {
                bb.ReadPosition = startingtPos; // unread length
                return false;
            }
#if HASH_PACKETS
            var targetHash = bb.Bytes.XxHash(startingtPos, length);
            bb.ReadPosition = endingHash;
            var hash = ReadUlong(bb);

            if (targetHash != hash)
                throw new Exception("Data corruption detected in packet!");
#endif

            bb.ReadPosition = startingtPos + 4;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EndReadPacket(ByteBuffer bb, int endingPos)
        {
            bb.ReadPosition = endingPos;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryReadPacket(ByteBuffer bb, Action<ByteBuffer> reader)
        {
            int endPos;
            if (!StartTryReadPacket(bb, out endPos)) return false;
            reader(bb);
            EndReadPacket(bb, endPos);
            return true;
        }

        public static void Shift(ByteBuffer bb)
        {
            if (bb.ReadPosition == 0)
                return;

            if (bb.ReadPosition == bb.WriteCommit && bb.WriteCommit == bb.WritePosition)
            {
                bb.ReadPosition = 0;
                bb.WritePosition = 0;
                bb.WriteCommit = 0;
                return;
            }

            Debug.Assert(bb.WritePosition >= bb.WriteCommit);
            Debug.Assert(bb.WriteCommit >= bb.ReadPosition);

            var size = bb.WritePosition - bb.ReadPosition;

            Debug.Assert(size > 0);

            if (size > 128 && (bb.Bytes.Length - bb.WritePosition > MaxPacketSize))
                return;

            bb.WritePosition = size;
            bb.WriteCommit -= bb.ReadPosition;

            //Console.WriteLine("Partial shift, {0} bytes copied.", WritePosition);
            Buffer.BlockCopy(bb.Bytes, bb.ReadPosition, bb.Bytes, 0, size);
            bb.ReadPosition = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Append(ByteBuffer bb, ByteBuffer byteBuffer)
        {
            var count = byteBuffer.Count;
            bb.Ensure(bb.WritePosition + count);
            Buffer.BlockCopy(byteBuffer.Bytes, byteBuffer.ReadPosition, bb.Bytes, bb.WritePosition, count);
            bb.WritePosition += count;
            Reset(byteBuffer);
        }

        private static readonly DateTime Epoc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToUniversalTime();
        private static readonly DateTime EpocLocal = new DateTime(1970, 1, 1, 0, 0, 0);

        // Because: Time Zones
        private static readonly double MaxDelta = DateTime.MaxValue.Subtract(EpocLocal.AddDays(1)).TotalMilliseconds;
        private static readonly double MinDelta = DateTime.MinValue.Subtract(EpocLocal.AddDays(-1)).TotalMilliseconds;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteDateTime(ByteBuffer bb, DateTime dt)
        {
            var delta = dt.ToEpochTimeMilliseconds();
            WriteDouble(bb, delta);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static DateTime ReadDateTime(ByteBuffer bb)
        {
            var delta = ReadDouble(bb);
            if (delta > MaxDelta) return DateTime.MaxValue;
            if (delta < MinDelta) return DateTime.MinValue;
            return DateTimeEx.Epoch.AddMilliseconds(delta);
        }

        public static class TypeId
        {
            public const byte Null = 0;
            public const byte String = 1;
            public const byte Byte = 2;
            public const byte SByte = 3;
            public const byte Char = 4;
            public const byte Int16 = 5;
            public const byte UInt16 = 6;
            public const byte Int32 = 7;
            public const byte UInt32 = 8;
            public const byte Int64 = 9;
            public const byte UInt64 = 10;
            public const byte DateTime = 11;
            public const byte Single = 12;
            public const byte Double = 13;
            public const byte Boolean = 14;
            // TODO:
            //  byte[] (array support in general?)
            //  Guid
            //  Decimal
            public const byte Reserved = 255;
        }

        // TODO add array support

        internal static void WriteObject(ByteBuffer bb, object obj)
        {
            if (obj == null)
            {
                WriteByte(bb, TypeId.Null);
                return;
            }

            var typeName = obj.GetType().FullName;
            switch (typeName)
            {
                case "System.String":
                    WriteByte(bb, TypeId.String);
                    WriteString(bb, (string) obj);
                    break;
                case "System.Byte":
                    WriteByte(bb, TypeId.Byte);
                    WriteByte(bb, (byte) obj);
                    break;
                case "System.SByte":
                    WriteByte(bb, TypeId.SByte);
                    WriteSByte(bb, (sbyte) obj);
                    break;
                case "System.Char":
                    WriteByte(bb, TypeId.Char);
                    WriteChar(bb, (char) obj);
                    break;
                case "System.Int16":
                    WriteByte(bb, TypeId.Int16);
                    WriteShort(bb, (short) obj);
                    break;
                case "System.UInt16":
                    WriteByte(bb, TypeId.UInt16);
                    WriteUshort(bb, (ushort) obj);
                    break;
                case "System.Int32":
                    WriteByte(bb, TypeId.Int32);
                    WriteInt(bb, (int) obj);
                    break;
                case "System.UInt32":
                    WriteByte(bb, TypeId.UInt32);
                    WriteUint(bb, (uint) obj);
                    break;
                case "System.Int64":
                    WriteByte(bb, TypeId.Int64);
                    WriteLong(bb, (long) obj);
                    break;
                case "System.UInt64":
                    WriteByte(bb, TypeId.UInt64);
                    WriteUlong(bb, (ulong) obj);
                    break;
                case "System.DateTime":
                    WriteByte(bb, TypeId.DateTime);
                    var dt = (DateTime) obj;
                    WriteDateTime(bb, dt);
                    break;
                case "System.Single":
                    WriteByte(bb, TypeId.Single);
                    WriteFloat(bb, (float) obj);
                    break;
                case "System.Double":
                    WriteByte(bb, TypeId.Double);
                    WriteDouble(bb, (double) obj);
                    break;
                case "System.Boolean":
                    WriteByte(bb, TypeId.Boolean);
                    WriteBool(bb, (bool) obj);
                    break;
                default:
                    throw new Exception("Unsupported Type: " + typeName);
            }
        }

        internal static object ReadObject(ByteBuffer bb)
        {
            var typeId = ReadByte(bb);

            switch (typeId)
            {
                case TypeId.Null:
                    return null;
                case TypeId.String:
                    return ReadString(bb);
                case TypeId.Byte:
                    return ReadByte(bb);
                case TypeId.SByte:
                    return ReadSByte(bb);
                case TypeId.Char:
                    return ReadChar(bb);
                case TypeId.Int16:
                    return ReadShort(bb);
                case TypeId.UInt16:
                    return ReadUshort(bb);
                case TypeId.Int32:
                    return ReadInt(bb);
                case TypeId.UInt32:
                    return ReadUint(bb);
                case TypeId.Int64:
                    return ReadLong(bb);
                case TypeId.UInt64:
                    return ReadUlong(bb);
                case TypeId.DateTime:
                    return ReadDateTime(bb);
                case TypeId.Single:
                    return ReadFloat(bb);
                case TypeId.Double:
                    return ReadDouble(bb);
                case TypeId.Boolean:
                    return ReadBool(bb);
                default:
                    throw new Exception("Unsupported Type Id: " + typeId);
            }
        }
    }
}