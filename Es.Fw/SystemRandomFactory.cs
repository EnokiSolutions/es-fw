using System;
using Es.FwI;

namespace Es.Fw
{
    internal sealed class SystemRandomFactory : IRandomFactory, IRandomWithSeedFactory
    {
        IRandom IRandomFactory.Create()
        {
            return new SystemRandom();
        }

        IRandom IRandomWithSeedFactory.Create(int seed)
        {
            return new SystemRandom(new Random(seed));
        }
    }
}