using System.Collections.Generic;

namespace Es.Fw
{
    public sealed class ByteArrayComparer : IEqualityComparer<byte[]>
    {
        private ByteArrayComparer()
        {
            // Use the Instance
        }

        public static readonly IEqualityComparer<byte[]> Instance = new ByteArrayComparer();

        public bool Equals(byte[] x, byte[] y)
        {
            return x.Eq(y, EqualityComparer<byte>.Default);
        }

        public int GetHashCode(byte[] x)
        {
            return (int) (x.XxHash(0, x.Length) & 0xffffffff);
        }
    }
}