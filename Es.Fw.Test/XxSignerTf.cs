using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NUnit.Framework;

namespace Es.Fw.Test
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public sealed class XxHashSignTf
    {
        [Test]
        public void Test()
        {
            var signer = new XxHashSign(10, 11);

            var payload1 = new byte[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9};
            var signature1 = signer.Sign(new ArraySegment<byte>(payload1));
            Console.WriteLine(signature1.ToHexString());

            Assert.AreEqual(signature1.Length, signer.SignatureBytesCount);
            var expected = "1967874272bef9e1".FromHexString();
            CollectionAssert.AreEquivalent(expected, signature1);
            
            var payload2 = new byte[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 5};
            var signature2 = signer.Sign(new ArraySegment<byte>(payload2));
            Console.WriteLine(signature2.ToHexString());

            Assert.AreEqual(signature2.Length, signer.SignatureBytesCount);
            CollectionAssert.AreNotEquivalent(signature1, signature2);

            var signature1Alt1 = signer.Sign(new ArraySegment<byte>(payload1));
            CollectionAssert.AreEquivalent(signature1, signature1Alt1);

            var sigAppendedPayload1 = signature1.Concat(payload1).ToArray();
            var signature1Alt2 =
                signer.Sign(new ArraySegment<byte>(sigAppendedPayload1, signer.SignatureBytesCount,
                    sigAppendedPayload1.Length - signer.SignatureBytesCount));

            CollectionAssert.AreEquivalent(signature1, signature1Alt2);
        }
    }
}