using System;
using System.Text;

namespace Es.Fw
{
    public static class ByteArrayEx
    {
        // EncodedString is meant to provide a "selectable" string on all OSs, Mail Clients, etc. Thus it is limited to a-zA-Z0-9 (62 symbols)
        public static string ToEncodedString(this ArraySegment<byte> bytes)
        {
            return bytes.Array.ToEncodedString(bytes.Offset, bytes.Count);
        }

        public static string ToEncodedString(this byte[] bytes)
        {
            return bytes.ToEncodedString(0, bytes.Length);
        }

        public static string ToEncodedString(this byte[] bytes, int offset, int length)
        {
            return Convert.ToBase64String(bytes, offset, length)
                .Replace("Z", "Zz")
                .Replace("+", "Zp")
                .Replace("/", "Zs")
                .Replace("=", "Ze")
                ;
        }

        public static byte[] FromEncodedString(this string base64String)
        {
            return Convert.FromBase64String(
                base64String
                    .Replace("Ze", "=")
                    .Replace("Zs", "/")
                    .Replace("Zp", "+")
                    .Replace("Zz", "Z")
                );
        }

        public static string ToHexString(this byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (var b in bytes)
            {
                var hex = b.ToString("x2");
                sb.Append(hex);
            }
            return sb.ToString();
        }

        public static string ToHexString(this ArraySegment<byte> bytes)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < bytes.Count; i++)
            {
                var b = bytes.Array[bytes.Offset + i];
                var hex = b.ToString("x2");
                sb.Append(hex);
            }
            return sb.ToString();
        }

        public static byte[] FromHexString(this string hexString)
        {
            var length = hexString.Length;

            if (length%2 == 1)
                throw new FormatException("hexString");

            var arrLength = length/2;
            var bytes = new byte[arrLength];

            for (var i = 0; i < arrLength; ++i)
            {
                var highChar = (int) hexString[i*2];
                var low = (int) hexString[i*2 + 1];
                var highValue = highChar - (highChar < 58 ? 48 : (highChar < 97 ? 55 : 87));
                var lowValue = low - (low < 58 ? 48 : (low < 97 ? 55 : 87));

                if (highValue < 0 || highValue > 15) throw new FormatException("hexString");
                if (lowValue < 0 || lowValue > 15) throw new FormatException("hexString");

                bytes[i] = (byte) ((highValue << 4) + lowValue);
            }

            return bytes;
        }
    }
}