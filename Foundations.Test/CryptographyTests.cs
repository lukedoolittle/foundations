using Org.BouncyCastle.Crypto.Digests;
using Xunit;

namespace Foundations.Test
{
    public class CryptographyTests
    {
        [Fact]
        public void CallingCreateCryptoSeveralTimesProducesRandomString()
        {
            var crypto1 = Cryptography.Cryptography.CreateCryptographicallyStrongString<Sha512Digest>(32);
            var crypto2 = Cryptography.Cryptography.CreateCryptographicallyStrongString<Sha512Digest>(32);
            var crypto3 = Cryptography.Cryptography.CreateCryptographicallyStrongString<Sha512Digest>(32);

            Assert.Equal(32, crypto1.Length);
            Assert.Equal(32, crypto2.Length);
            Assert.Equal(32, crypto3.Length);

            Assert.NotEqual(crypto1, crypto2);
            Assert.NotEqual(crypto2, crypto3);
        }
    }
}
