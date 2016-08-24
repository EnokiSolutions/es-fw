using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Es.Fw
{
    public sealed unsafe class ByteBuffer
    {
        public const int DefaultMaxPacketSize = 2*1024*1024;

        // HACK
        public static int MaxPacketSize = DefaultMaxPacketSize;

        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToUniversalTime();
        private static readonly DateTime EpochLocal = new DateTime(1970, 1, 1, 0, 0, 0);

        // Because: Time Zones
        private static readonly double MaxDelta = DateTime.MaxValue.Subtract(EpochLocal.AddDays(1)).TotalMilliseconds;
        private static readonly double MinDelta = DateTime.MinValue.Subtract(EpochLocal.AddDays(-1)).TotalMilliseconds;


        // only used for "object" read/write
        internal static readonly ConcurrentDictionary<string, ObjectInfo> ObjectInfoByTypeName =
            new ConcurrentDictionary<string, ObjectInfo>();

        internal static readonly ConcurrentDictionary<byte, ObjectInfo> ObjectInfoByTypeId =
            new ConcurrentDictionary<byte, ObjectInfo>();

        public byte[] Bytes;
        public int ReadPosition;
        public int WriteCommitPosition;
        public int WritePosition;

        public bool Writing;

        static ByteBuffer()
        {
            RegisterType(TypeId.String, WriteString, ReadString);
            RegisterType(TypeId.Byte, WriteByte, ReadByte);
            RegisterType(TypeId.SByte, WriteSByte, ReadSByte);
            RegisterType(TypeId.Char, WriteChar, ReadChar);
            RegisterType(TypeId.Short, WriteShort, ReadShort);
            RegisterType(TypeId.Ushort, WriteUshort, ReadUshort);
            RegisterType(TypeId.Int, WriteInt, ReadInt);
            RegisterType(TypeId.Uint, WriteUint, ReadUint);
            RegisterType(TypeId.Long, WriteLong, ReadLong);
            RegisterType(TypeId.Ulong, WriteUlong, ReadUlong);
            RegisterType(TypeId.DateTime, WriteDateTime, ReadDateTime);
            RegisterType(TypeId.Float, WriteFloat, ReadFloat);
            RegisterType(TypeId.Double, WriteDouble, ReadDouble);
            RegisterType(TypeId.Bool, WriteBool, ReadBool);
            RegisterType(TypeId.Decimal, WriteDecimal, ReadDecimal);
            RegisterType(TypeId.Guid, WriteGuid, ReadGuid);
            RegisterType(TypeId.Dso, WriteDso, ReadDso);

            {
                var name = typeof(object[]).FullName;
                byte typeId = TypeId.FlagHeterogeneous | TypeId.FlagArray;
                var objectInfo = new ObjectInfo(typeId, (bb, ts) => WriteArrayOf(bb, (object[]) ts, WriteObject),
                    bb => ReadArrayOf(bb, ReadObject));
                ObjectInfoByTypeName[name] = objectInfo;
                ObjectInfoByTypeId[typeId] = objectInfo;
            }
            {
                var name = typeof(List<object>).FullName;
                byte typeId = TypeId.FlagHeterogeneous | TypeId.FlagList;
                var objectInfo = new ObjectInfo(typeId, (bb, ts) => WriteListOf(bb, (List<object>) ts, WriteObject),
                    bb => ReadListOf(bb, ReadObject));
                ObjectInfoByTypeName[name] = objectInfo;
                ObjectInfoByTypeId[typeId] = objectInfo;
            }
        }

        public ByteBuffer(int initialSize = 0)
        {
            if (initialSize == 0)
                initialSize = MaxPacketSize;

            Bytes = new byte[initialSize];
            ReadPosition = 0;
            WritePosition = 0;
            WriteCommitPosition = 0;
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return WriteCommitPosition - ReadPosition; }
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong DoubleToUlong(double d)
        {
            var p = &d;
            {
                return *(ulong*) p;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EndPeelPacket(ByteBuffer peeled)
        {
            peeled.Bytes = null;
            peeled.ReadPosition = 0;
            peeled.WriteCommitPosition = 0;
            peeled.WritePosition = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EndReadPacket(ByteBuffer bb, int endingPos)
        {
            bb.ReadPosition = endingPos;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EndWriteInnerPacket(ByteBuffer bb, int startingPos)
        {
            var endingPos = bb.WritePosition;
            bb.WritePosition = startingPos;
            var packetLengthWithoutSizePrefix = endingPos - startingPos - 4;
            if (packetLengthWithoutSizePrefix > MaxPacketSize)
                throw new Exception("Packet too large, aborting.");
            WriteInt(bb, packetLengthWithoutSizePrefix);
            bb.WritePosition = endingPos;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EndWritePacket(ByteBuffer bb, int startingPos)
        {
            var endingPos = bb.WritePosition;
            bb.WritePosition = startingPos;
            var packetLengthWithoutSizePrefix = endingPos - startingPos - 4;
            if (packetLengthWithoutSizePrefix > MaxPacketSize)
                throw new Exception($"Packet too large({packetLengthWithoutSizePrefix} > {MaxPacketSize}), aborting.");
            WriteInt(bb, packetLengthWithoutSizePrefix);
            bb.WritePosition = endingPos;

            var hash = bb.Bytes.XxHash(startingPos, packetLengthWithoutSizePrefix);
            WriteUlong(bb, hash);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EndWritePacket(ByteBuffer bb, int startingPos, ulong k)
        {
            var endingPos = bb.WritePosition;
            bb.WritePosition = startingPos;
            var packetLengthWithoutSizePrefix = endingPos - startingPos - 4;
            if (packetLengthWithoutSizePrefix > MaxPacketSize)
                throw new Exception("Packet too large, aborting.");
            WriteInt(bb, packetLengthWithoutSizePrefix);
            bb.WritePosition = endingPos;

            var hash = bb.Bytes.XxHash(startingPos, packetLengthWithoutSizePrefix, k);
            WriteUlong(bb, hash);
        }

        public void Ensure(int newWritePos)
        {
            if (Bytes.Length >= newWritePos)
                return;

            var tmp = Bytes;
            var newLength = Bytes.Length*2;
            if (newLength < newWritePos)
                newLength = newWritePos;
            Bytes = new byte[newLength];
            Buffer.BlockCopy(tmp, 0, Bytes, 0, tmp.Length);
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
        public static bool PacketHasMore(ByteBuffer bb, int endingPos)
        {
            return bb.ReadPosition + 8 < endingPos;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool PeeledPacketHasMore(ByteBuffer peeled)
        {
            return peeled.ReadPosition + 8 < peeled.WriteCommitPosition;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T[] ReadArrayOf<T>(ByteBuffer bb, Func<ByteBuffer, T> reader)
        {
            var ts = new T[ReadInt(bb)];
            for (var i = 0; i < ts.Length; ++i)
                ts[i] = reader(bb);
            return ts;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ReadBool(ByteBuffer bb)
        {
            Debug.Assert(bb.WriteCommitPosition - bb.ReadPosition >= 1);
            byte v;
            fixed (byte* b = bb.Bytes)
            {
                v = *(b + bb.ReadPosition);
            }
            bb.ReadPosition += 1;
            return v != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ReadByte(ByteBuffer bb)
        {
            Debug.Assert(bb.WriteCommitPosition - bb.ReadPosition >= 1);
            byte v;
            fixed (byte* b = bb.Bytes)
            {
                v = *(b + bb.ReadPosition);
            }
            bb.ReadPosition += 1;
            return v;
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
        public static char ReadChar(ByteBuffer bb)
        {
            return (char) ReadUshort(bb);
        }

        public static void ReadCommit(ByteBuffer bb)
        {
            if (bb.ReadPosition == 0)
                return;

            if (bb.ReadPosition == bb.WriteCommitPosition && bb.WriteCommitPosition == bb.WritePosition)
            {
                bb.ReadPosition = 0;
                bb.WritePosition = 0;
                bb.WriteCommitPosition = 0;
                return;
            }

            Debug.Assert(bb.WritePosition >= bb.WriteCommitPosition);
            Debug.Assert(bb.WriteCommitPosition >= bb.ReadPosition);

            var size = bb.WritePosition - bb.ReadPosition;
            if (size == 0)
                return;

            Debug.Assert(size > 0);

            if (size > 128 && (bb.Bytes.Length - bb.WritePosition > MaxPacketSize))
                return;

            bb.WritePosition = size;
            bb.WriteCommitPosition -= bb.ReadPosition;

            //Console.WriteLine("Partial shift, {0} bytes copied.", WritePosition);
            Buffer.BlockCopy(bb.Bytes, bb.ReadPosition, bb.Bytes, 0, size);
            bb.ReadPosition = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTime ReadDateTime(ByteBuffer bb)
        {
            var delta = ReadDouble(bb);
            if (delta > MaxDelta) return DateTime.MaxValue;
            if (delta < MinDelta) return DateTime.MinValue;
            return Epoch.AddMilliseconds(delta);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal ReadDecimal(ByteBuffer bb)
        {
            var ints = new int[4];
            ints[0] = ReadInt(bb);
            ints[1] = ReadInt(bb);
            ints[2] = ReadInt(bb);
            ints[3] = ReadInt(bb);
            return new decimal(ints);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ReadDouble(ByteBuffer bb)
        {
            return UlongToDouble(ReadUlong(bb));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Dictionary<string, object> ReadDso(ByteBuffer bb)
        {
            return ReadDso(bb, StringComparer.Ordinal);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Dictionary<string, object> ReadDso(ByteBuffer bb, StringComparer stringComparer)
        {
            if (!ReadBool(bb))
                return null;

            var dso = new Dictionary<string, object>(stringComparer);

            var count = ReadInt(bb);
            for (var i = 0; i < count; ++i)
            {
                var key = ReadString(bb);
                var value = ReadObject(bb);
                dso[key] = value;
            }
            return dso;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ReadFixedBytes(ByteBuffer bb, int length)
        {
            var b = new byte[length];
            Buffer.BlockCopy(bb.Bytes, bb.ReadPosition, b, 0, length);
            bb.ReadPosition += length;
            return b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ReadFloat(ByteBuffer bb)
        {
            return UintToFloat(ReadUint(bb));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Guid ReadGuid(ByteBuffer bb)
        {
            var bytes = ReadFixedBytes(bb, 16);
            return new Guid(bytes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadInt(ByteBuffer bb)
        {
            Debug.Assert(bb.WriteCommitPosition - bb.ReadPosition >= 4);
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
        private static List<T> ReadListOf<T>(ByteBuffer bb, Func<ByteBuffer, T> reader)
        {
            var count = ReadInt(bb);
            var ts = new List<T>(count);
            for (var i = 0; i < count; ++i)
                ts.Add(reader(bb));
            return ts;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ReadLong(ByteBuffer bb)
        {
            return (long) ReadUlong(bb);
        }

        internal static object ReadObject(ByteBuffer bb)
        {
            var typeId = ReadByte(bb);
            if (typeId == TypeId.Null)
                return null;

            ObjectInfo objectInfo;
            if (!ObjectInfoByTypeId.TryGetValue(typeId, out objectInfo))
            {
                throw new Exception("Unhandled object typeId: " + typeId);
            }
            Debug.Assert(typeId == objectInfo.TypeId);
            return objectInfo.Reader(bb);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte ReadSByte(ByteBuffer bb)
        {
            return (sbyte) ReadByte(bb);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ReadShort(ByteBuffer bb)
        {
            Debug.Assert(bb.WriteCommitPosition - bb.ReadPosition >= 2);
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
        public static string ReadString(ByteBuffer bb)
        {
            var isValid = ReadBool(bb);
            if (!isValid)
                return null;

            var length = ReadInt(bb);
            Debug.Assert(bb.WriteCommitPosition - bb.ReadPosition >= 2*length);
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
        public static uint ReadUint(ByteBuffer bb)
        {
            Debug.Assert(bb.WriteCommitPosition - bb.ReadPosition >= 4);
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
        public static ulong ReadUlong(ByteBuffer bb)
        {
            Debug.Assert(bb.WriteCommitPosition - bb.ReadPosition >= 8);
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
        public static ushort ReadUshort(ByteBuffer bb)
        {
            Debug.Assert(bb.WriteCommitPosition - bb.ReadPosition >= 2);
            ushort u;
            fixed (byte* b = bb.Bytes)
            {
                var x = b + bb.ReadPosition;
                u = (ushort) ((*x << 8) | *++x);
            }
            bb.ReadPosition += 2;
            return u;
        }

        private static void RegisterType<T>(byte typeId, Action<ByteBuffer, T> writer, Func<ByteBuffer, T> reader)
        {
            {
                var name = typeof(T).FullName;
                var objectInfo = new ObjectInfo(typeId, (bb, t) => writer(bb, (T) t), bb => reader(bb));
                ObjectInfoByTypeName[name] = objectInfo;
                ObjectInfoByTypeId[typeId] = objectInfo;
            }
            {
                var name = typeof(T[]).FullName;
                var arrTypeId = (byte) (typeId | TypeId.FlagArray);
                var objectInfo = new ObjectInfo(arrTypeId, (bb, ts) => WriteArrayOf(bb, (T[]) ts, writer),
                    bb => ReadArrayOf(bb, reader));
                ObjectInfoByTypeName[name] = objectInfo;
                ObjectInfoByTypeId[arrTypeId] = objectInfo;
            }
            {
                var name = typeof(List<T>).FullName;
                var listTypeId = (byte) (typeId | TypeId.FlagList);
                var objectInfo = new ObjectInfo(listTypeId, (bb, ts) => WriteListOf(bb, (List<T>) ts, writer),
                    bb => ReadListOf(bb, reader));
                ObjectInfoByTypeName[name] = objectInfo;
                ObjectInfoByTypeId[listTypeId] = objectInfo;
            }
        }

        public static void RegisterUserDefinedType<T>(byte userTypeId, Action<ByteBuffer, T> writer,
            Func<ByteBuffer, T> reader)
        {
            var typeId = (byte) ((userTypeId & 0x63) | TypeId.FlagUser);
            RegisterType(typeId, writer, reader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Reset(ByteBuffer bb)
        {
            bb.ReadPosition = 0;
            bb.WritePosition = 0;
            bb.WriteCommitPosition = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartTryReadInnerPacket(ByteBuffer bb, out int endPos)
        {
            endPos = bb.ReadPosition;

            if (bb.WriteCommitPosition - bb.ReadPosition < 4)
                return false;

            var startingPos = bb.ReadPosition;
            var length = ReadInt(bb);
            if (length > MaxPacketSize || length < 0)
                throw new Exception("Packet length too large or negative! Corrupt packet data?");

            endPos = bb.ReadPosition + length;
            if (bb.WriteCommitPosition >= endPos)
                return true;

            bb.ReadPosition = startingPos; // unread length
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartTryReadPacket(ByteBuffer bb, out int endPos)
        {
            endPos = bb.ReadPosition;

            if (bb.WriteCommitPosition - bb.ReadPosition < 4)
                return false;

            var startingPos = bb.ReadPosition;
            var length = ReadInt(bb);
            if (length > MaxPacketSize)
                throw new Exception($"Packet length too large ({length} > {MaxPacketSize})! Corrupt packet data?");
            if (length < 0)
                throw new Exception($"Packet length negative ({length})! Corrupt packet data?");

            var endingHash = bb.ReadPosition + length;
            endPos = endingHash + 8;
            if (bb.WriteCommitPosition < endPos)
            {
                bb.ReadPosition = startingPos; // unread length
                return false;
            }
            var targetHash = bb.Bytes.XxHash(startingPos, length);
            bb.ReadPosition = endingHash;
            var hash = ReadUlong(bb);

            if (targetHash != hash)
                throw new Exception("Data corruption detected in packet!");

            bb.ReadPosition = startingPos + 4;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartTryReadPacket(ByteBuffer bb, out int endPos, ulong k)
        {
            endPos = bb.ReadPosition;

            if (bb.WriteCommitPosition - bb.ReadPosition < 4)
                return false;

            var startingPos = bb.ReadPosition;
            var length = ReadInt(bb);
            if (length > MaxPacketSize || length < 0)
                throw new Exception("Packet length too large or negative! Corrupt packet data?");

            var endingHash = bb.ReadPosition + length;
            endPos = endingHash + 8;
            if (bb.WriteCommitPosition < endPos)
            {
                bb.ReadPosition = startingPos; // unread length
                return false;
            }
            var targetHash = bb.Bytes.XxHash(startingPos, length, k);
            bb.ReadPosition = endingHash;
            var hash = ReadUlong(bb);

            if (targetHash != hash)
                throw new Exception("Data corruption detected in packet!");

            bb.ReadPosition = startingPos + 4;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int StartWritePacket(ByteBuffer bb)
        {
            ReadCommit(bb);
            var startingPos = bb.WritePosition;
            WriteInt(bb, 0);
            return startingPos;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryReadPacket(ByteBuffer bb, Action<ByteBuffer> reader, ulong k)
        {
            int endPos;
            if (!StartTryReadPacket(bb, out endPos, k)) return false;
            reader(bb);
            EndReadPacket(bb, endPos);
            return true;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryStartPeelPacket(ByteBuffer bb, ByteBuffer peeled)
        {
            int endPos;
            if (!StartTryReadInnerPacket(bb, out endPos))
                return false;

            peeled.Bytes = bb.Bytes;
            peeled.ReadPosition = bb.ReadPosition;
            peeled.WriteCommitPosition = endPos;
            peeled.WritePosition = endPos;
            return true;
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
        private static double UlongToDouble(ulong i)
        {
            var p = &i;
            {
                return *(double*) p;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteArrayOf<T>(ByteBuffer bb, T[] ts, Action<ByteBuffer, T> writer)
        {
            WriteInt(bb, ts.Length);
            foreach (var t in ts)
                writer(bb, t);
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
        public static void WriteBytes(ByteBuffer bb, byte[] b)
        {
            var length = b.Length;
            WriteInt(bb, length);
            bb.Ensure(bb.WritePosition + length);
            Buffer.BlockCopy(b, 0, bb.Bytes, bb.WritePosition, length);
            bb.WritePosition += length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(ByteBuffer bb, byte[] b, int offset, int count)
        {
            WriteInt(bb, count);
            bb.Ensure(bb.WritePosition + count);
            Buffer.BlockCopy(b, offset, bb.Bytes, bb.WritePosition, count);
            bb.WritePosition += count;
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
        public static void WriteChar(ByteBuffer bb, char v)
        {
            WriteUshort(bb, v);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteCommit(ByteBuffer bb)
        {
            bb.WriteCommitPosition = bb.WritePosition;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteDateTime(ByteBuffer bb, DateTime dt)
        {
            if (dt != DateTime.MaxValue && dt != DateTime.MinValue)
                if (dt.Kind != DateTimeKind.Utc)
                {
                    // throw new Exception("DateTime MUST BE UTC.");
                    dt = dt.ToUniversalTime();
                }

            var delta = dt.Subtract(Epoch).TotalMilliseconds;
            WriteDouble(bb, delta);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteDecimal(ByteBuffer bb, decimal d)
        {
            var ints = decimal.GetBits(d);
            WriteInt(bb, ints[0]);
            WriteInt(bb, ints[1]);
            WriteInt(bb, ints[2]);
            WriteInt(bb, ints[3]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteDouble(ByteBuffer bb, double d)
        {
            WriteUlong(bb, DoubleToUlong(d));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteDso(ByteBuffer bb, Dictionary<string, object> dso)
        {
            if (dso == null)
            {
                WriteBool(bb, false);
                return;
            }
            WriteBool(bb, true);
            WriteInt(bb, dso.Count);

            foreach (var kvp in dso)
            {
                WriteString(bb, kvp.Key);
                WriteObject(bb, kvp.Value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteFixedBytes(ByteBuffer bb, byte[] b)
        {
            var length = b.Length;
            bb.Ensure(bb.WritePosition + length);
            Buffer.BlockCopy(b, 0, bb.Bytes, bb.WritePosition, length);
            bb.WritePosition += length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteFloat(ByteBuffer bb, float f)
        {
            WriteUint(bb, FloatToUint(f));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteGuid(ByteBuffer bb, Guid g)
        {
            var bytes = g.ToByteArray();
            WriteFixedBytes(bb, bytes);
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
        private static void WriteListOf<T>(ByteBuffer bb, List<T> ts, Action<ByteBuffer, T> writer)
        {
            WriteInt(bb, ts.Count);
            foreach (var t in ts)
                writer(bb, t);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteLong(ByteBuffer bb, long l)
        {
            WriteUlong(bb, (ulong) l);
        }

        internal static void WriteObject(ByteBuffer bb, object obj)
        {
            if (obj == null)
            {
                WriteByte(bb, TypeId.Null);
                return;
            }
            ObjectInfo objectInfo;
            var fullName = obj.GetType().FullName;
            if (!ObjectInfoByTypeName.TryGetValue(fullName, out objectInfo))
            {
                throw new Exception("Unhandled object type: " + fullName);
            }
            WriteByte(bb, objectInfo.TypeId);
            objectInfo.Writer(bb, obj);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteSByte(ByteBuffer bb, sbyte v)
        {
            WriteByte(bb, (byte) v);
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

        public static class TypeId
        {
            public const byte Null = 0;
            public const byte String = 1;
            public const byte Byte = 2;
            public const byte SByte = 3;
            public const byte Char = 4;
            public const byte Short = 5;
            public const byte Ushort = 6;
            public const byte Int = 7;
            public const byte Uint = 8;
            public const byte Long = 9;
            public const byte Ulong = 10;
            public const byte DateTime = 11;
            public const byte Float = 12;
            public const byte Double = 13;
            public const byte Bool = 14;
            public const byte Decimal = 15;
            public const byte Guid = 16;
            public const byte KeyValuePair = 17;
            public const byte Dso = 18;
            // 18-31 is reserved.
            public const byte Reserved = 31;
            public const byte FlagMask = 128 + 64 + 32; // top 3 bits

            public const byte FlagHeterogeneous = 32;
            // for array of object or list of object, i.e. heterogeneous array or list.

            public const byte FlagArray = 64;
            // 010 - array of one type, 011 - array where each element carries its own type

            public const byte FlagList = 128; // 100/101 - list like array above ^
            public const byte FlagUser = 64 + 128; // 110 - user type from 0-63
            public const byte FlagExtended = 32 + 64 + 128; // 111 - for future use.
        }

        internal sealed class ObjectInfo
        {
            public readonly Func<ByteBuffer, object> Reader;
            public readonly byte TypeId;
            public readonly Action<ByteBuffer, object> Writer;

            public ObjectInfo(byte typeId, Action<ByteBuffer, object> writer, Func<ByteBuffer, object> reader)
            {
                TypeId = typeId;
                Writer = writer;
                Reader = reader;
            }
        }
    }
}