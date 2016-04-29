using System;
using System.Collections.Generic;

namespace Es.Fw
{
    public static class ArrayEx
    {
        public static T[] ToArray<T>(this ArraySegment<T> seg)
        {
            var arr = new T[seg.Count];
            for (var i = 0; i < seg.Count; ++i)
                arr[i] = seg.Array[i + seg.Offset];

            return arr;
        }

        public static bool Eq<T>(this T[] sa, ArraySegment<T> sb)
        {
            return sa.Eq(sb, EqualityComparer<T>.Default);
        }

        public static bool Eq<T>(this ArraySegment<T> sa, ArraySegment<T> sb)
        {
            return sa.Eq(sb, EqualityComparer<T>.Default);
        }

        public static bool Eq<T>(this ArraySegment<T> sa, T[] sb)
        {
            return sa.Eq(sb, EqualityComparer<T>.Default);
        }

        public static bool Eq<T>(this T[] sa, T[] sb)
        {
            return sa.Eq(sb, EqualityComparer<T>.Default);
        }

        public static bool Eq<T>(this T[] sa, ArraySegment<T> sb, IEqualityComparer<T> eq)
        {
            if (sa.Length != sb.Count) return false;
            var count = sa.Length;
            for (var i = 0; i < count; ++i)
                if (!eq.Equals(sa[i], sb.Array[sb.Offset + i])) return false;
            return true;
        }

        public static bool Eq<T>(this ArraySegment<T> sa, T[] sb, IEqualityComparer<T> eq)
        {
            if (sa.Count != sb.Length) return false;
            var count = sa.Count;
            for (var i = 0; i < count; ++i)
                if (!eq.Equals(sa.Array[sa.Offset + i], sb[i])) return false;
            return true;
        }

        public static bool Eq<T>(this ArraySegment<T> sa, ArraySegment<T> sb, IEqualityComparer<T> eq)
        {
            if (sa.Count != sb.Count) return false;
            var count = sa.Count;
            for (var i = 0; i < count; ++i)
                if (!eq.Equals(sa.Array[sa.Offset + i], sb.Array[sb.Offset + i])) return false;
            return true;
        }

        public static bool Eq<T>(this T[] sa, T[] sb, IEqualityComparer<T> eq)
        {
            if (sa.Length != sb.Length) return false;
            var count = sa.Length;
            for (var i = 0; i < count; ++i)
                if (!eq.Equals(sa[i], sb[i]))
                    return false;
            return true;
        }
    }
}