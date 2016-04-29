using Es.FwI;

namespace Es.Fw
{
    internal sealed class XorShift1024RandomFactory : IRandomFactory, IRandomWithSeedFactory
    {
        IRandom IRandomFactory.Create()
        {
            return new XorShift1024Random();
        }

        IRandom IRandomWithSeedFactory.Create(int seed)
        {
            return new XorShift1024Random(seed);
        }
    }
}