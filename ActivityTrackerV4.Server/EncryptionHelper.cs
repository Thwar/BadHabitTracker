using System.Security.Cryptography;
using System.Text;

namespace BlazorAuthAPI
{
    public static class EncryptionHelper
    {
        private static readonly string Key = "qHu00Y08nMHegNdpgquf4CMkonAMEvfbO-gt0PT3UlI"; // Replace with your key (32 chars for AES-256)

        private static byte[] GetKey()
        {
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(Key));
        }

        public static string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            var keyBytes = Encoding.UTF8.GetBytes(Key);
            aes.Key = GetKey();
            aes.IV = new byte[16]; // Default IV (all zeros)

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        public static string Decrypt(string cipherText)
        {
            using var aes = Aes.Create();
            var keyBytes = Encoding.UTF8.GetBytes(Key);
            aes.Key = GetKey();
            aes.IV = new byte[16]; // Default IV (all zeros)

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }
    }
}
