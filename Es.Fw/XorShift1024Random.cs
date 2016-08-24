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
        [ThreadStatic]
        private static XorShift1024RandomState _threadSpecificRandomState;

        private static XorShift1024RandomState ThreadSpecificRandomState => _threadSpecificRandomState ?? (_threadSpecificRandomState = new XorShift1024RandomState());
        private static readonly IRandom SystemRandomForSeeds = Default.SystemRandomFactory.Create();

        private XorShift1024RandomState _state;

        public XorShift1024Random()
        {
            _state = null;
        }

        public XorShift1024Random(int seed)
        {
            _state = new XorShift1024RandomState(seed);
        }

        private sealed class XorShift1024RandomState
        {
            public readonly ulong[] S;
            public int P;

            public XorShift1024RandomState()
            {
                S = new ulong[16];

                var systemRandomForSeeds = SystemRandomForSeeds;
                for (var i = 0; i < S.Length; ++i)
                {
                    S[i] = systemRandomForSeeds.Ulong();
                }
            }

            public XorShift1024RandomState(int seed)
            {
                S = new ulong[16];

                unchecked
                {
                    var s = (ulong) seed;

                    if (s == 0)
                        s = 0x85cc55cc5c5c5c5eUL;

                    var i = 0;
                    S[i++] = s++;
                    S[i++] = s++;
                    S[i++] = s++;
                    S[i++] = s++;
                    S[i++] = s++;
                    S[i++] = s++;
                    S[i++] = s++;
                    S[i++] = s++;
                    S[i++] = s++;
                    S[i++] = s++;
                    S[i++] = s++;
                    S[i++] = s++;
                    S[i++] = s++;
                    S[i++] = s++;
                    S[i++] = s++;
                    S[i] = s;
                }
            }
        }

        [SuppressMessage("Microsoft.Contracts", "TestAlwaysEvaluatingToAConstant")]
        [ExcludeFromCodeCoverage]
        // only because in release mode coverage doesn't recognise the switch statement. Once https://youtrack.jetbrains.com/issue/TW-44821 is fixed we should be able to re-enable coverage for this method.
        void IRandom.Fill(ArraySegment<byte> toFill)
        {
            var count = toFill.Count;
            var offset = toFill.Offset;
            var state = _state ?? ThreadSpecificRandomState;

            ulong r = 0;
            fixed (byte* ap = toFill.Array)
            {
                var rp = (byte*) &r;

                while (count >= 8)
                {
                    r = Ulong(state);
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

                r = Ulong(state);

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


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ulong IRandom.Ulong()
        {
            var state = _state ?? ThreadSpecificRandomState;
            unchecked
            {
                fixed (ulong* sp = state.S)
                {
                    var s0 = sp[state.P];
                    state.P = (state.P + 1) & 15;
                    var s1 = sp[state.P];
                    s1 ^= s1 << 31;
                    sp[state.P] = s1 ^ s0 ^ (s1 >> 11) ^ (s0 >> 30);
                    return sp[state.P] * 1181783497276652981UL;
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong Ulong(XorShift1024RandomState state)
        {
            unchecked
            {
                fixed (ulong* sp = state.S)
                {
                    var s0 = sp[state.P];
                    state.P = (state.P + 1) & 15;
                    var s1 = sp[state.P];
                    s1 ^= s1 << 31;
                    sp[state.P] = s1 ^ s0 ^ (s1 >> 11) ^ (s0 >> 30);
                    return sp[state.P] * 1181783497276652981UL;
                }
            }
        }
    }
}