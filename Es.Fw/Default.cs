using Es.FwI;

namespace Es.Fw
{
    public static class Default
    {
        public static readonly IRandomFactory RandomFactory;
        public static readonly IRandomWithSeedFactory RandomWithSeedFactory;
        public static readonly ICompress SnappyCompressor;
        public static readonly IDecompress SnappyDecompressor;
        public static readonly IUtcDateTimeProvider UtcDateTimeProvider;

        internal static readonly IRandomFactory SystemRandomFactory;
        internal static IRandomWithSeedFactory SystemRandomWithSeedFactory;

        internal static readonly IRandomFactory XorShift1024RandomFactory;
        internal static IRandomWithSeedFactory XorShift1024RandomWithSeedFactory;

        public static IEncrypt CreateEncrypt(byte[] key, IRandom random) { return new Blowfish(key, random); }
        public static IDecrypt CreateDecrypt(byte[] key) { return new Blowfish(key); }

        static Default()
        {
            var systemRandomFactory = new SystemRandomFactory();
            SystemRandomFactory = systemRandomFactory;
            SystemRandomWithSeedFactory = systemRandomFactory;  

            var xorShift1024RandomFactory = new XorShift1024RandomFactory();
            XorShift1024RandomFactory = xorShift1024RandomFactory;
            XorShift1024RandomWithSeedFactory = xorShift1024RandomFactory;
            RandomWithSeedFactory = xorShift1024RandomFactory;
            RandomFactory = xorShift1024RandomFactory;

            SnappyCompressor = new SnappyCompressor();
            SnappyDecompressor = new SnappyDecompressor();
            UtcDateTimeProvider = new UtcDateTimeProvider();
        }
    }
}