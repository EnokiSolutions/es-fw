//exclude from duplicate code check

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Es.FwI;

namespace Es.Fw
{
    // based on http://xorshift.di.unimi.it/xorshift1024star.c
    internal sealed unsafe class XorShift1024Random : IRandom
    {
        private static readonly IRandom SystemRandomForSeeds = Default.SystemRandomFactory.Create();
        private readonly ulong[] _s;
        private int _p;

        public XorShift1024Random()
        {
            _s = new ulong[16];

            for (var i = 0; i < _s.Length; ++i)
                _s[i] = SystemRandomForSeeds.Ulong();
        }

        public XorShift1024Random(int seed)
        {
            _s = new ulong[16];

            unchecked
            {
                var s = (ulong) seed;

                if (s == 0)
                    s = 0x85cc55cc5c5c5c5eUL;

                var i = 0;
                _s[i++] = s++;
                _s[i++] = s++;
                _s[i++] = s++;
                _s[i++] = s++;
                _s[i++] = s++;
                _s[i++] = s++;
                _s[i++] = s++;
                _s[i++] = s++;
                _s[i++] = s++;
                _s[i++] = s++;
                _s[i++] = s++;
                _s[i++] = s++;
                _s[i++] = s++;
                _s[i++] = s++;
                _s[i++] = s++;
                _s[i] = s;
            }
        }

        [SuppressMessage("Microsoft.Contracts", "TestAlwaysEvaluatingToAConstant")]
        [ExcludeFromCodeCoverage]
        // only because in release mode coverage doesn't recognise the switch statement. Once https://youtrack.jetbrains.com/issue/TW-44821 is fixed we should be able to re-enable coverage for this method.
        public void Fill(ArraySegment<byte> toFill)
        {
            var count = toFill.Count;
            var offset = toFill.Offset;
            ulong r = 0;
            fixed (byte* ap = toFill.Array)
            {
                var rp = (byte*) &r;

                while (count >= 8)
                {
                    r = Ulong();
                    ap[offset] = rp[0];
                    ap[offset + 1] = rp[1];
                    ap[offset + 2] = rp[2];
                    ap[offset + 3] = rp[3];
                    ap[offset + 4] = rp[4];
                    ap[offset + 5] = rp[5];
                    ap[offset + 6] = rp[6];
                    ap[offset + 7] = rp[7];
                    offset += 8;
                    count -= 8;
                }

                if (count == 0)
                    return;

                r = Ulong();

                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (count)
                {
                    case 7:
                        ap[offset + 6] = rp[6];
                        goto case 6;
                    case 6:
                        ap[offset + 5] = rp[5];
                        goto case 5;
                    case 5:
                        ap[offset + 4] = rp[4];
                        goto case 4;
                    case 4:
                        ap[offset + 3] = rp[3];
                        goto case 3;
                    case 3:
                        ap[offset + 2] = rp[2];
                        goto case 2;
                    case 2:
                        ap[offset + 1] = rp[1];
                        goto case 1;
                    case 1:
                        ap[offset] = rp[0];
                        break;
                }
            }
        }

        [SuppressMessage("Microsoft.Contracts", "TestAlwaysEvaluatingToAConstant")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong Ulong()
        {
            unchecked
            {
                fixed (ulong* sp = _s)
                {
                    var s0 = sp[_p];
                    _p = (_p + 1) & 15;
                    var s1 = sp[_p];
                    s1 ^= s1 << 31;
                    sp[_p] = s1 ^ s0 ^ (s1 >> 11) ^ (s0 >> 30);
                    return sp[_p]*1181783497276652981UL;
                }
            }
        }
    }
}