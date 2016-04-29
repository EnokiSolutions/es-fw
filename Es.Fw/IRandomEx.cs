using System;
using Es.FwI;

namespace Es.Fw
{
    // ReSharper disable once InconsistentNaming
    public static class IRandomEx
    {
        public static uint Uint(this IRandom r)
        {
            unchecked
            {
                return (uint) r.Int();
            }
        }

        public static int Int(this IRandom r)
        {
            var b = new byte[4];
            r.Fill(new ArraySegment<byte>(b));
            return (b[0] << 24)
                   | (b[1] << 16)
                   | (b[2] << 8)
                   | b[3];
        }

        public static long Long(this IRandom r)
        {
            return (long) r.Ulong();
        }

        public static double Double(this IRandom r)
        {
            var v = r.Ulong();
            const ulong maxIntergerForIeeeDouble = 1UL << 53;
            v &= maxIntergerForIeeeDouble - 1; // mask
            return v/(double) maxIntergerForIeeeDouble;
        }
    }
}