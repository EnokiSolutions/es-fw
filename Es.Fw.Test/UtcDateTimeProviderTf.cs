using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;

namespace Es.Fw.Test
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public sealed class UtcDateTimeProviderTf
    {
        [Test]
        public void Test()
        {
            var dtp = Default.UtcDateTimeProvider;
            var utcNow = dtp.UtcNow;
            Assert.Greater(utcNow,dtp.Epoch);
        }
    }
}