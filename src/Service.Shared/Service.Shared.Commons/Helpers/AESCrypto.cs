// "Một sản phẩm của HieuDV"

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Service.Shared.Commons.Helpers
{
    public static class AESCrypto
    {
        private static readonly string DefaultKey = "TanAnEnterpriseSecretAESKey2026!"; // 32 chars for AES-256

        public static string Encrypt(string plainText, string? keyStr = null)
        {
            if (string.IsNullOrEmpty(plainText)) return string.Empty;

            var key = Encoding.UTF8.GetBytes((keyStr ?? DefaultKey).PadRight(32).Substring(0, 32));
            using var aes = Aes.Create();
            aes.Key = key;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            ms.Write(aes.IV, 0, aes.IV.Length);

            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        public static string EncryptNoAutoGen(string plainText, string? keyStr = null)
        {
            if (string.IsNullOrEmpty(plainText)) return string.Empty;

            var key = Encoding.UTF8.GetBytes((keyStr ?? DefaultKey).PadRight(32).Substring(0, 32));
            var iv = Encoding.UTF8.GetBytes("1234567890123456"); // Fixed IV for deterministic hash comparison

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();

            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        public static string Decrypt(string cipherText, string? keyStr = null)
        {
            if (string.IsNullOrEmpty(cipherText)) return string.Empty;

            var fullBytes = Convert.FromBase64String(cipherText);
            var key = Encoding.UTF8.GetBytes((keyStr ?? DefaultKey).PadRight(32).Substring(0, 32));

            using var aes = Aes.Create();
            aes.Key = key;

            byte[] iv = new byte[16];
            byte[] cipherBytes;

            if (fullBytes.Length > 16)
            {
                Array.Copy(fullBytes, 0, iv, 0, 16);
                cipherBytes = new byte[fullBytes.Length - 16];
                Array.Copy(fullBytes, 16, cipherBytes, 0, cipherBytes.Length);
            }
            else
            {
                iv = Encoding.UTF8.GetBytes("1234567890123456");
                cipherBytes = fullBytes;
            }

            aes.IV = iv;
            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(cipherBytes);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }

        public static bool IsBase64String(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            Span<byte> buffer = new Span<byte>(new byte[value.Length]);
            return Convert.TryFromBase64String(value, buffer, out _);
        }
    }
}
