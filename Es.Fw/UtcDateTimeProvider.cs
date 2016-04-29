using System;
using Es.FwI;

namespace Es.Fw
{
    internal sealed class UtcDateTimeProvider : IUtcDateTimeProvider
    {
        public DateTime Epoch => DateTimeEx.Epoch;
        public DateTime UtcNow => DateTime.UtcNow;
    }
}