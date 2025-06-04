using System.Security.Cryptography;
using System.Text;

namespace Tracker.Services.Utility
{
    /// <summary>
    /// Cipher utils
    /// </summary>
    public class CipherUtils
    {
        /// <summary>
        /// Generate key.
        /// </summary>
        /// <param name="keySizeInBits">Key size in bits</param>
        /// <returns></returns>
        public static string GenerateKey(int keySizeInBits = 256)
        {
            var random = new Random();

            byte[] keyBytes = new byte[keySizeInBits / 8];
            random.NextBytes(keyBytes);
            return Convert.ToBase64String(keyBytes);
        }


        private const string KeyFileName = "encryption_key.bin";

        /// <summary>
        /// Save key.
        /// </summary>
        /// <param name="key">Key</param>
        public static void SaveKey(string key)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] encryptedKey = ProtectedData.Protect(keyBytes, null, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(KeyFileName, encryptedKey);
        }

        public static string LoadKey()
        {
            if (!File.Exists(KeyFileName))
            {
                return null;
            }

            byte[] encryptedKey = File.ReadAllBytes(KeyFileName);
            byte[] keyBytes = ProtectedData.Unprotect(encryptedKey, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(keyBytes);
        }
    }
}
