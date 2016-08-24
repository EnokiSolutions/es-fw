using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Es.Fw
{
    public static class ByteBufferEx
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Read(this ByteBuffer bb)
        {
            bb.Writing = false;
            return bb;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Write(this ByteBuffer bb)
        {
            bb.Writing = true;
            return bb;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref ulong u)
        {
            if (bb.Writing)
            {
                ByteBuffer.WriteUlong(bb, u);
            }
            else
            {
                u = ByteBuffer.ReadUlong(bb);
            }
            return bb;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref ulong? u)
        {
            if (bb.Writing)
            {
                ByteBuffer.WriteBool(bb, u.HasValue);
                if (u.HasValue)
                    ByteBuffer.WriteUlong(bb, u.Value);
            }
            else
            {
                if (ByteBuffer.ReadBool(bb))
                    u = ByteBuffer.ReadUlong(bb);
            }
            return bb;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref ulong[] u)
        {
            return bb.StreamTs(ref u, Stream);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref List<ulong> u)
        {
            return bb.StreamTs(ref u, Stream);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref long u)
        {
            if (bb.Writing)
            {
                ByteBuffer.WriteLong(bb, u);
            }
            else
            {
                u = ByteBuffer.ReadLong(bb);
            }
            return bb;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref long? u)
        {
            if (bb.Writing)
            {
                ByteBuffer.WriteBool(bb, u.HasValue);
                if (u.HasValue)
                    ByteBuffer.WriteLong(bb, u.Value);
            }
            else
            {
                if (ByteBuffer.ReadBool(bb))
                    u = ByteBuffer.ReadLong(bb);
            }
            return bb;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref long[] u)
        {
            return bb.StreamTs(ref u, Stream);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref List<long> u)
        {
            return bb.StreamTs(ref u, Stream);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref uint u)
        {
            if (bb.Writing)
            {
                ByteBuffer.WriteUint(bb, u);
            }
            else
            {
                u = ByteBuffer.ReadUint(bb);
            }
            return bb;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref uint? u)
        {
            if (bb.Writing)
            {
                ByteBuffer.WriteBool(bb, u.HasValue);
                if (u.HasValue)
                    ByteBuffer.WriteUint(bb, u.Value);
            }
            else
            {
                if (ByteBuffer.ReadBool(bb))
                    u = ByteBuffer.ReadUint(bb);
            }
            return bb;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref uint[] u)
        {
            return bb.StreamTs(ref u, Stream);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref List<uint> u)
        {
            return bb.StreamTs(ref u, Stream);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref int u)
        {
            if (bb.Writing)
            {
                ByteBuffer.WriteInt(bb, u);
            }
            else
            {
                u = ByteBuffer.ReadInt(bb);
            }
            return bb;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref int? u)
        {
            if (bb.Writing)
            {
                ByteBuffer.WriteBool(bb, u.HasValue);
                if (u.HasValue)
                    ByteBuffer.WriteInt(bb, u.Value);
            }
            else
            {
                if (ByteBuffer.ReadBool(bb))
                    u = ByteBuffer.ReadInt(bb);
            }
            return bb;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref int[] u)
        {
            return bb.StreamTs(ref u, Stream);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref List<int> u)
        {
            return bb.StreamTs(ref u, Stream);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref ushort u)
        {
            if (bb.Writing)
            {
                ByteBuffer.WriteUshort(bb, u);
            }
            else
            {
                u = ByteBuffer.ReadUshort(bb);
            }
            return bb;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref ushort? u)
        {
            if (bb.Writing)
            {
                ByteBuffer.WriteBool(bb, u.HasValue);
                if (u.HasValue)
                    ByteBuffer.WriteUshort(bb, u.Value);
            }
            else
            {
                if (ByteBuffer.ReadBool(bb))
                    u = ByteBuffer.ReadUshort(bb);
            }
            return bb;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref ushort[] u)
        {
            return bb.StreamTs(ref u, Stream);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref List<ushort> u)
        {
            return bb.StreamTs(ref u, Stream);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref short u)
        {
            if (bb.Writing)
            {
                ByteBuffer.WriteShort(bb, u);
            }
            else
            {
                u = ByteBuffer.ReadShort(bb);
            }
            return bb;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref short? u)
        {
            if (bb.Writing)
            {
                ByteBuffer.WriteBool(bb, u.HasValue);
                if (u.HasValue)
                    ByteBuffer.WriteShort(bb, u.Value);
            }
            else
            {
                if (ByteBuffer.ReadBool(bb))
                    u = ByteBuffer.ReadShort(bb);
            }
            return bb;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref short[] u)
        {
            return bb.StreamTs(ref u, Stream);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref List<short> u)
        {
            return bb.StreamTs(ref u, Stream);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref byte u)
        {
            if (bb.Writing)
            {
                ByteBuffer.WriteByte(bb, u);
            }
            else
            {
                u = ByteBuffer.ReadByte(bb);
            }
            return bb;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref byte? u)
        {
            if (bb.Writing)
            {
                ByteBuffer.WriteBool(bb, u.HasValue);
                if (u.HasValue)
                    ByteBuffer.WriteByte(bb, u.Value);
            }
            else
            {
                if (ByteBuffer.ReadBool(bb))
                    u = ByteBuffer.ReadByte(bb);
            }
            return bb;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref byte[] u)
        {
            return bb.StreamTs(ref u, Stream);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref List<byte> u)
        {
            return bb.StreamTs(ref u, Stream);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref char u)
        {
            if (bb.Writing)
            {
                ByteBuffer.WriteChar(bb, u);
            }
            else
            {
                u = ByteBuffer.ReadChar(bb);
            }
            return bb;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref char? u)
        {
            if (bb.Writing)
            {
                ByteBuffer.WriteBool(bb, u.HasValue);
                if (u.HasValue)
                    ByteBuffer.WriteChar(bb, u.Value);
            }
            else
            {
                if (ByteBuffer.ReadBool(bb))
                    u = ByteBuffer.ReadChar(bb);
            }
            return bb;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref char[] u)
        {
            return bb.StreamTs(ref u, Stream);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref List<char> u)
        {
            return bb.StreamTs(ref u, Stream);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref sbyte u)
        {
            if (bb.Writing)
            {
                ByteBuffer.WriteSByte(bb, u);
            }
            else
            {
                u = ByteBuffer.ReadSByte(bb);
            }
            return bb;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref sbyte? u)
        {
            if (bb.Writing)
            {
                ByteBuffer.WriteBool(bb, u.HasValue);
                if (u.HasValue)
                    ByteBuffer.WriteSByte(bb, u.Value);
            }
            else
            {
                if (ByteBuffer.ReadBool(bb))
                    u = ByteBuffer.ReadSByte(bb);
            }
            return bb;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref sbyte[] u)
        {
            return bb.StreamTs(ref u, Stream);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref List<sbyte> u)
        {
            return bb.StreamTs(ref u, Stream);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref double u)
        {
            if (bb.Writing)
            {
                ByteBuffer.WriteDouble(bb, u);
            }
            else
            {
                u = ByteBuffer.ReadDouble(bb);
            }
            return bb;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref double? u)
        {
            if (bb.Writing)
            {
                ByteBuffer.WriteBool(bb, u.HasValue);
                if (u.HasValue)
                    ByteBuffer.WriteDouble(bb, u.Value);
            }
            else
            {
                if (ByteBuffer.ReadBool(bb))
                    u = ByteBuffer.ReadDouble(bb);
            }
            return bb;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref double[] u)
        {
            return bb.StreamTs(ref u, Stream);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref List<double> u)
        {
            return bb.StreamTs(ref u, Stream);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref bool u)
        {
            if (bb.Writing)
            {
                ByteBuffer.WriteBool(bb, u);
            }
            else
            {
                u = ByteBuffer.ReadBool(bb);
            }
            return bb;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref bool? u)
        {
            if (bb.Writing)
            {
                ByteBuffer.WriteBool(bb, u.HasValue);
                if (u.HasValue)
                    ByteBuffer.WriteBool(bb, u.Value);
            }
            else
            {
                if (ByteBuffer.ReadBool(bb))
                    u = ByteBuffer.ReadBool(bb);
            }
            return bb;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref bool[] u)
        {
            return bb.StreamTs(ref u, Stream);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref List<bool> u)
        {
            return bb.StreamTs(ref u, Stream);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref float u)
        {
            if (bb.Writing)
            {
                ByteBuffer.WriteFloat(bb, u);
            }
            else
            {
                u = ByteBuffer.ReadFloat(bb);
            }
            return bb;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref float? u)
        {
            if (bb.Writing)
            {
                ByteBuffer.WriteBool(bb, u.HasValue);
                if (u.HasValue)
                    ByteBuffer.WriteFloat(bb, u.Value);
            }
            else
            {
                if (ByteBuffer.ReadBool(bb))
                    u = ByteBuffer.ReadFloat(bb);
            }
            return bb;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref float[] u)
        {
            return bb.StreamTs(ref u, Stream);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref List<float> u)
        {
            return bb.StreamTs(ref u, Stream);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref decimal u)
        {
            if (bb.Writing)
            {
                ByteBuffer.WriteDecimal(bb, u);
            }
            else
            {
                u = ByteBuffer.ReadDecimal(bb);
            }
            return bb;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref decimal? u)
        {
            if (bb.Writing)
            {
                ByteBuffer.WriteBool(bb, u.HasValue);
                if (u.HasValue)
                    ByteBuffer.WriteDecimal(bb, u.Value);
            }
            else
            {
                if (ByteBuffer.ReadBool(bb))
                    u = ByteBuffer.ReadDecimal(bb);
            }
            return bb;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref decimal[] u)
        {
            return bb.StreamTs(ref u, Stream);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref List<decimal> u)
        {
            return bb.StreamTs(ref u, Stream);
        }

        public static ByteBuffer Stream(this ByteBuffer bb, ref DateTime u)
        {
            if (bb.Writing)
            {
                ByteBuffer.WriteDateTime(bb, u);
            }
            else
            {
                u = ByteBuffer.ReadDateTime(bb);
            }
            return bb;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref DateTime? u)
        {
            if (bb.Writing)
            {
                ByteBuffer.WriteBool(bb, u.HasValue);
                if (u.HasValue)
                    ByteBuffer.WriteDateTime(bb, u.Value);
            }
            else
            {
                if (ByteBuffer.ReadBool(bb))
                    u = ByteBuffer.ReadDateTime(bb);
            }
            return bb;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref DateTime[] u)
        {
            return bb.StreamTs(ref u, Stream);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref List<DateTime> u)
        {
            return bb.StreamTs(ref u, Stream);
        }


        public static ByteBuffer Stream(this ByteBuffer bb, ref Guid u)
        {
            if (bb.Writing)
            {
                ByteBuffer.WriteGuid(bb, u);
            }
            else
            {
                u = ByteBuffer.ReadGuid(bb);
            }
            return bb;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref Guid? u)
        {
            if (bb.Writing)
            {
                ByteBuffer.WriteBool(bb, u.HasValue);
                if (u.HasValue)
                    ByteBuffer.WriteGuid(bb, u.Value);
            }
            else
            {
                if (ByteBuffer.ReadBool(bb))
                    u = ByteBuffer.ReadGuid(bb);
            }
            return bb;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref Guid[] u)
        {
            return bb.StreamTs(ref u, Stream);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref List<Guid> u)
        {
            return bb.StreamTs(ref u, Stream);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref string u)
        {
            if (bb.Writing)
            {
                ByteBuffer.WriteString(bb, u);
            }
            else
            {
                u = ByteBuffer.ReadString(bb);
            }
            return bb;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref string[] u)
        {
            return bb.StreamTs(ref u, Stream);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref List<string> u)
        {
            return bb.StreamTs(ref u, Stream);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref object u)
        {
            if (bb.Writing)
            {
                ByteBuffer.WriteObject(bb, u);
            }
            else
            {
                u = ByteBuffer.ReadObject(bb);
            }
            return bb;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref object[] u)
        {
            return bb.StreamTs(ref u, Stream);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer Stream(this ByteBuffer bb, ref List<object> u)
        {
            return bb.StreamTs(ref u, Stream);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer StreamTs<T>(this ByteBuffer bb, ref T[] ts, Action<ByteBuffer, T> writer, Func<ByteBuffer, T> reader)
        {
            if (bb.Writing)
            {
                ByteBuffer.WriteBool(bb, ts != null);
                if (ts == null) return bb;
                ByteBuffer.WriteInt(bb, ts.Length);
                foreach (var t in ts)
                    writer(bb, t);
            }
            else
            {
                if (!ByteBuffer.ReadBool(bb))
                {
                    ts = null;
                    return bb;
                }
                ts = new T[ByteBuffer.ReadInt(bb)];
                for (var i = 0; i < ts.Length; ++i)
                    ts[i] = reader(bb);
            }
            return bb;
        }

        internal static ByteBuffer StreamKvps<TK, TV>(
            this ByteBuffer bb,
            ref KeyValuePair<TK, TV>[] kvpArr,
            Streamer<TK> keyStreamer,
            Streamer<TV> valueStreamer)// where TK : class, new() where TV : class, new()
        {
            return bb.StreamTs(
                ref kvpArr,
                (bb1, kvp) =>
                {
                    var key = kvp.Key;
                    var value = kvp.Value;
                    keyStreamer(bb1, ref key);
                    valueStreamer(bb1, ref value);
                },
                bb1 =>
                {
                    var key = default(TK);
                    var value = default(TV);
                    keyStreamer(bb1, ref key);
                    valueStreamer(bb1, ref value);
                    return new KeyValuePair<TK, TV>(key, value);
                });
        }

        internal static ByteBuffer Stream(this ByteBuffer bb, ref Dictionary<string, object> dso, StringComparer stringComparer = null)
        {
            if (bb.Writing)
            {
                ByteBuffer.WriteDso(bb, dso);
            }
            else
            {
                dso = ByteBuffer.ReadDso(bb, stringComparer);
            }
            return bb;
        }

        public delegate ByteBuffer Streamer<T>(ByteBuffer bb, ref T t); // where T : class, new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer StreamTs<T>(this ByteBuffer bb, ref T[] ts, Streamer<T> streamer)// where T:class, new()
        {
            if (bb.Writing)
            {
                ByteBuffer.WriteBool(bb, ts != null);
                if (ts == null)
                    return bb;

                ByteBuffer.WriteInt(bb, ts.Length);
                foreach (var t in ts)
                {
                    var t1 = t;
                    streamer(bb, ref t1);
                }
            }
            else
            {
                if (!ByteBuffer.ReadBool(bb))
                {
                    ts = null;
                    return bb;
                }
                ts = new T[ByteBuffer.ReadInt(bb)];
                for (var i = 0; i < ts.Length; ++i)
                {
                    //ts[i] = default(T);
                    streamer(bb, ref ts[i]);
                }
            }
            return bb;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer StreamTs<T>(this ByteBuffer bb, ref List<T> ts, Streamer<T> streamer)// where T : class, new()
        {
            if (bb.Writing)
            {
                ByteBuffer.WriteBool(bb, ts != null);
                if (ts == null)
                    return bb;

                ByteBuffer.WriteInt(bb, ts.Count);
                foreach (var t in ts)
                {
                    var t1 = t;
                    streamer(bb, ref t1);
                }
            }
            else
            {
                if (!ByteBuffer.ReadBool(bb))
                {
                    ts = null;
                    return bb;
                }
                var count = ByteBuffer.ReadInt(bb);
                ts = new List<T>(count);
                for (var i = 0; i < count; ++i)
                {
                    var t = default(T);
                    streamer(bb, ref t);
                    ts.Add(t);
                }
            }
            return bb;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteBuffer StreamT<T>(this ByteBuffer bb, ref T t, Streamer<T> streamer) // where T:class, new()

        {
            var b = t != null;
            bb.Stream(ref b);
            if (!b)
                return bb;

            streamer(bb, ref t);
            return bb;
        }
    }
}