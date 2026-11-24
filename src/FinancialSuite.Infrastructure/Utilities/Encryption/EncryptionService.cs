using System.Security.Cryptography;
using System.Text;
using FinancialSuite.Domain.Interfaces.Utilities;

namespace FinancialSuite.Infrastructure.Utilities.Encryption
{
    // AES-256-GCM. Output is base64 of nonce + tag + cipher text, so each value carries what it needs to be decrypted.
    public class EncryptionService : IEncryptionService
    {
        private const int KeySize = 32;
        private const int NonceSize = 12;
        private const int TagSize = 16;

        private readonly byte[] key;

        public EncryptionService(byte[] key)
        {
            if (key.Length != KeySize) throw new ArgumentException($"Encryption key must be {KeySize} bytes.", nameof(key));
            this.key = key;
        }

        public string Encrypt(string plainText)
        {
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var nonce = RandomNumberGenerator.GetBytes(NonceSize);
            var tag = new byte[TagSize];
            var cipherBytes = new byte[plainBytes.Length];

            using var aes = new AesGcm(key, TagSize);
            aes.Encrypt(nonce, plainBytes, cipherBytes, tag);

            return Convert.ToBase64String([.. nonce, .. tag, .. cipherBytes]);
        }

        public string Decrypt(string cipherText)
        {
            var data = Convert.FromBase64String(cipherText);
            var nonce = data.AsSpan(0, NonceSize);
            var tag = data.AsSpan(NonceSize, TagSize);
            var cipherBytes = data.AsSpan(NonceSize + TagSize);
            var plainBytes = new byte[cipherBytes.Length];

            using var aes = new AesGcm(key, TagSize);
            aes.Decrypt(nonce, cipherBytes, tag, plainBytes);

            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}
