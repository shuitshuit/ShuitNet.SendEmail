using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;

namespace ShuitNet.SendEmail
{
    public static class PasswordManager
    {
        public static string EncryptPassword(string plainPassword)
        {
            if (string.IsNullOrEmpty(plainPassword))
                return string.Empty;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return EncryptWithDpapi(plainPassword);
            else
                return EncryptWithAes(plainPassword);
        }

        public static string DecryptPassword(string encryptedPassword)
        {
            if (string.IsNullOrEmpty(encryptedPassword))
                return string.Empty;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return DecryptWithDpapi(encryptedPassword);
            else
                return DecryptWithAes(encryptedPassword);
        }

        private static string EncryptWithDpapi(string plainPassword)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                throw new PlatformNotSupportedException("DPAPI is not supported on this platform.");
            var data = Encoding.UTF8.GetBytes(plainPassword);
            var encryptedData = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedData);
        }

        private static string DecryptWithDpapi(string encryptedPassword)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                throw new PlatformNotSupportedException("DPAPI is not supported on this platform.");
            var encryptedData = Convert.FromBase64String(encryptedPassword);
            var data = ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(data);
        }

        private static string EncryptWithAes(string plainPassword)
        {
            using var aes = Aes.Create();
            var key = GetUserSpecificKey();
            aes.Key = key;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainPassword);
            var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            var result = new byte[aes.IV.Length + encryptedBytes.Length];
            Array.Copy(aes.IV, 0, result, 0, aes.IV.Length);
            Array.Copy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

            return Convert.ToBase64String(result);
        }

        private static string DecryptWithAes(string encryptedPassword)
        {
            var encryptedData = Convert.FromBase64String(encryptedPassword);
            using var aes = Aes.Create();
            var key = GetUserSpecificKey();
            aes.Key = key;

            var iv = new byte[aes.IV.Length];
            var encryptedBytes = new byte[encryptedData.Length - iv.Length];
            Array.Copy(encryptedData, 0, iv, 0, iv.Length);
            Array.Copy(encryptedData, iv.Length, encryptedBytes, 0, encryptedBytes.Length);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
            return Encoding.UTF8.GetString(decryptedBytes);
        }

        private static byte[] GetUserSpecificKey()
        {
            var userInfo = Environment.UserName + Environment.MachineName;
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(userInfo));
        }
    }
}