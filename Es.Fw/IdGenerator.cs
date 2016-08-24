using System;
using Es.FwI;

namespace Es.Fw
{
    internal sealed class IdGenerator : IIdGenerator
    {
        private readonly IRandom _random;

        public IdGenerator(IRandom random)
        {
            _random = random;
        }

        Id IIdGenerator.Create()
        {
            return new Id((ulong)DateTime.UtcNow.Ticks, _random.Ulong());
        }

        void IIdGenerator.Create(Id id)
        {
            id.Ulongs[0] = (ulong)DateTime.UtcNow.Ticks;
            id.Ulongs[1] = _random.Ulong();
        }
    }
}