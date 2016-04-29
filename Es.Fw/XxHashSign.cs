using System;
using System.Linq;
using Es.FwI;

namespace Es.Fw
{
    /// <summary>
    ///     I know this is not cyptographically secure.
    /// </summary>
    internal sealed class XxHashSign : ISign
    {
        private readonly ulong _k0;
        private readonly ulong _k1;

        public XxHashSign(ulong k0, ulong k1)
        {
            _k0 = k0;
            _k1 = k1;
        }

        public int SignatureBytesCount => sizeof(ulong);

        public byte[] Sign(ArraySegment<byte> payload)
        {
            var bb = new ByteBuffer(payload.Count + 16);
            ByteBuffer.WriteUlong(bb, _k0);
            ByteBuffer.WriteBytes(bb, payload);
            ByteBuffer.WriteUlong(bb, _k1);
            ByteBuffer.Commit(bb);

            var hash = bb.Bytes.XxHash(bb.ReadPosition, bb.Count);
            ByteBuffer.Reset(bb);
            ByteBuffer.WriteUlong(bb, hash);
            ByteBuffer.Commit(bb);

            return bb.Bytes.Take(8).ToArray();
        }
    }
}