using System;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;

namespace Es.Fw.Test
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public sealed class DateTimeExTf
    {
        [Test]
        public void Test()
        {
            var dt = new DateTime(2000,1,1);
            var dtse = dt.ToEpochTimeSeconds();

            Assert.AreEqual(946684800, dtse);

            var dtmse = dt.ToEpochTimeMilliseconds();

            Assert.AreEqual(946684800000, dtmse);

            var dtlc = DateTime.Parse("2000-01-01 00:00:00 -100");
            var dtu = dtlc.AsUtc();
            Assert.AreEqual("2000-01-01 01:00:00Z", dtu.ToString("u"));

            DateTime? dtn = null;
            Assert.IsNull(dtn.AsUtc());
        }
    }
}