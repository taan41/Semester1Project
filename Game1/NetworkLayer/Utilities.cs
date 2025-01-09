using System.Security.Cryptography;
using System.Text;

static class Utilities
{
    public static class DataConstants
    {
        public const int
            usernameMin = 4, usernameMax = 50,
            passwordMin = 6, passwordMax = 50,
            nicknameMin = 1, nicknameMax = 25,
            pwdHashLen = 32, pwdSaltLen = 16,
            emailLen = 255,
            inputLimit = 500,
            bufferSize = 2048;
    }

    public static class Encode
    {
        public static string GetString(byte[] data) => Encoding.UTF8.GetString(data);

        public static string GetString(byte[] data, int index, int length) => Encoding.UTF8.GetString(data, index, length);

        public static byte[] GetBytes(string content) => Encoding.UTF8.GetBytes(content);
    }

    public static class Security
    {
        // Hash & verify password
        public static (byte[] PwdHash, byte[] Salt) HashPassword(string pwd)
        {
            byte[] salt = new byte[DataConstants.pwdSaltLen];
            RandomNumberGenerator.Fill(salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(Encode.GetBytes(pwd), salt, 10000, HashAlgorithmName.SHA256);
            byte[] pwdHash = pbkdf2.GetBytes(DataConstants.pwdHashLen);

            return (pwdHash, salt);
        }

        public static bool VerifyPassword(string pwd, byte[] storedPwdHash, byte[] storedSalt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(Encode.GetBytes(pwd), storedSalt, 10000, HashAlgorithmName.SHA256);
            byte[] pwdHash = pbkdf2.GetBytes(DataConstants.pwdHashLen);

            return pwdHash.SequenceEqual(storedPwdHash);
        }

        public static bool VerifyPassword(string pwd, PasswordSet pwdSet)
            => VerifyPassword(pwd, pwdSet.PwdHash, pwdSet.PwdSalt);
    }
}