using System;
using System.Collections.Generic;

namespace Es.Fw
{
    public static class VivifyEx
    {
        public static TV Vivify<TK, TV>(this IDictionary<TK, TV> dict, TK key, Func<TV> valueFactory)
        {
            TV v;
            if (dict.TryGetValue(key, out v))
                return v;

            v = valueFactory();
            dict[key] = v;
            return v;
        }

        public static TV Vivify<TK, TV>(this IDictionary<TK, TV> dict, TK key) where TV : class, new()
        {
            TV v;
            if (dict.TryGetValue(key, out v))
                return v;

            v = new TV();
            dict[key] = v;
            return v;
        }

        public static TV VivifyDefault<TK, TV>(this IDictionary<TK, TV> dict, TK key)
        {
            TV v;
            if (dict.TryGetValue(key, out v))
                return v;

            v = default(TV);
            dict[key] = v;
            return v;
        }
    }
}