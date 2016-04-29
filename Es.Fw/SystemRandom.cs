using System;
using Es.FwI;

namespace Es.Fw
{
    // This is here in case we decide to move away from XorShift1024
    internal sealed class SystemRandom : IRandom
    {
        // Normally I'd disallow the use of a thread static, but since selecting a seed to create a new random generator
        // presents a problem (using time isn't a good idea since several generators may be requested at the same time)
        // The use of a thread static sacrifics some performance to avoid the programs with seed generation.
        [ThreadStatic] private static Random _commonRandom;
        private readonly byte[] _bytes;

        private readonly Random _random;
        private int _used;

        public SystemRandom(Random random = null)
        {
            _random = random;
            _bytes = new byte[4096];
                // we buffer to avoid excessive thread local accesses in the case of using the common generator
            _used = _bytes.Length;
        }

        private static Random ThreadSpecificRandom => _commonRandom ?? (_commonRandom = new Random());

        public void Fill(ArraySegment<byte> toFill)
        {
            Random r = null;
            var offset = toFill.Offset;
            var count = toFill.Count;

            for (;;)
            {
                var remaining = _bytes.Length - _used;
                if (remaining == 0)
                {
                    // only acquire the base random generator if we need it, as it may require creation and/or thread local access
                    r = r ?? _random ?? ThreadSpecificRandom;
                    r.NextBytes(_bytes);
                    remaining = _bytes.Length;
                    _used = 0;
                }

                if (count <= remaining)
                {
                    Buffer.BlockCopy(_bytes, _used, toFill.Array, offset, count);
                    _used += count;
                    return;
                }

                Buffer.BlockCopy(_bytes, _used, toFill.Array, offset, remaining);
                _used += remaining;
                offset += remaining;
                count -= remaining;
            }
        }

        public ulong Ulong()
        {
            if (_random != null)
                // i.e. we're not using the thread static, so we can call random without incurring TLS access
                return ((ulong) (uint) _random.Next() << 32) | (uint) _random.Next();

            var b = new byte[8];
            Fill(new ArraySegment<byte>(b));

            return ((ulong) b[0] << 56)
                   | ((ulong) b[1] << 48)
                   | ((ulong) b[2] << 40)
                   | ((ulong) b[3] << 32)
                   | ((ulong) b[4] << 24)
                   | ((ulong) b[5] << 16)
                   | ((ulong) b[6] << 8)
                   | b[7]
                ;
        }
    }
}