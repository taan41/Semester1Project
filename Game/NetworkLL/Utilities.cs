using System.Security.Cryptography;
using System.Text;
using NetworkLL.DataTransferObjects;

namespace NetworkLL
{
    public static class Utilities
    {
        public static class Encode
        {
            public static string GetString(byte[] data) => Encoding.UTF8.GetString(data);

            public static string GetString(byte[] data, int index, int length) => Encoding.UTF8.GetString(data, index, length);

            public static byte[] GetBytes(string content) => Encoding.UTF8.GetBytes(content);
        }

        public static class Security
        {
            public const int PwdHashLen = 32;
            public const int PwdSaltLen = 16;
            
            // Hash & verify password
            public static (byte[] PwdHash, byte[] Salt) HashPassword(string pwd)
            {
                byte[] salt = new byte[PwdSaltLen];
                RandomNumberGenerator.Fill(salt);

                using var pbkdf2 = new Rfc2898DeriveBytes(Encode.GetBytes(pwd), salt, 10000, HashAlgorithmName.SHA256);
                byte[] pwdHash = pbkdf2.GetBytes(PwdHashLen);

                return (pwdHash, salt);
            }

            public static bool VerifyPassword(string pwd, byte[] storedPwdHash, byte[] storedSalt)
            {
                using var pbkdf2 = new Rfc2898DeriveBytes(Encode.GetBytes(pwd), storedSalt, 10000, HashAlgorithmName.SHA256);
                byte[] pwdHash = pbkdf2.GetBytes(PwdHashLen);

                return pwdHash.SequenceEqual(storedPwdHash);
            }

            public static bool VerifyPassword(string pwd, PasswordSet pwdSet)
                => VerifyPassword(pwd, pwdSet.PwdHash, pwdSet.PwdSalt);
        }
    }
}