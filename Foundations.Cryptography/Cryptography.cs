using System;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Security;

namespace Foundations.Cryptography
{
    public static class Cryptography
    {
        public static Guid CreateGuidFromData(string data)
        {
            MD5Digest digest = new MD5Digest();

            byte[] msgBytes = Encoding.UTF8.GetBytes(data);
            digest.BlockUpdate(msgBytes, 0, msgBytes.Length);
            byte[] result = new byte[digest.GetDigestSize()];
            digest.DoFinal(result, 0);
            return new Guid(result);
        }

        public static string CreateCryptographicallyStrongString<TDigest>(
            int stringLength = 32)
            where TDigest : IDigest, new()
        {
            var randomBytes = BitConverter.GetBytes(
                new SecureRandom().NextInt());

            var digest = new TDigest();
            digest.BlockUpdate(
                randomBytes,
                0,
                randomBytes.Length);
            var randomDigest = new DigestRandomGenerator(digest);

            var cryptoData = new byte[digest.GetDigestSize()];
            randomDigest.NextBytes(cryptoData);

            return Convert.ToBase64String(cryptoData)
                .Substring(
                    0, 
                    stringLength);
        }
    }
}
