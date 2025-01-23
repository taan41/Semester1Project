using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DAL.Persistence.DataTransferObjects;

namespace BLL
{
    static class ServerHelper
    {
        public static string ToJson(object obj)
            => JsonSerializer.Serialize(obj);

        public static T? FromJson<T>(string json)
            => JsonSerializer.Deserialize<T>(json);

        public static class Encode
        {
            public static string ToString(byte[] data) => Encoding.UTF8.GetString(data);

            public static string ToString(byte[] data, int index, int length) => Encoding.UTF8.GetString(data, index, length);

            public static byte[] ToBytes(string content) => Encoding.UTF8.GetBytes(content);
        }

        public static class Security
        {
            // Hash & verify password
            public static (byte[] PwdHash, byte[] Salt) HashPassword(string pwd)
            {
                byte[] salt = new byte[16];
                RandomNumberGenerator.Fill(salt);

                using var pbkdf2 = new Rfc2898DeriveBytes(Encode.ToBytes(pwd), salt, 10000, HashAlgorithmName.SHA256);
                byte[] pwdHash = pbkdf2.GetBytes(32);

                return (pwdHash, salt);
            }

            public static bool VerifyPassword(string pwd, byte[] storedPwdHash, byte[] storedSalt)
            {
                using var pbkdf2 = new Rfc2898DeriveBytes(Encode.ToBytes(pwd), storedSalt, 10000, HashAlgorithmName.SHA256);
                byte[] pwdHash = pbkdf2.GetBytes(32);

                return pwdHash.SequenceEqual(storedPwdHash);
            }

            public static bool VerifyPassword(string pwd, PasswordSet pwdSet)
                => VerifyPassword(pwd, pwdSet.PwdHash, pwdSet.PwdSalt);
        }
    }
}