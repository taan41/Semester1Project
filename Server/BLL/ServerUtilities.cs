using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DAL.Persistence.DataTransferObjects;

namespace BLL
{
    static class ServerUtilities
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
            // AES encryption
            

            public static void InitAESServer(NetworkStream stream, out Aes aes)
            {
                byte[] buffer = new byte[256];

                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                byte[] rsaPubKey = buffer[..bytesRead];

                using RSACryptoServiceProvider rsa = new();
                rsa.ImportRSAPublicKey(rsaPubKey, out _);

                aes = Aes.Create();
                aes.GenerateKey();
                aes.GenerateIV();

                byte[] encryptedKey = rsa.Encrypt(aes.Key, RSAEncryptionPadding.Pkcs1);
                byte[] encryptedIV = rsa.Encrypt(aes.IV, RSAEncryptionPadding.Pkcs1);

                stream.Write(encryptedKey, 0, encryptedKey.Length);
                stream.Write(encryptedIV, 0, encryptedIV.Length);
            }

            public static byte[] Encrypt(byte[] data, Aes aes)
            {
                using var encryptor = aes.CreateEncryptor();
                using var ms = new MemoryStream();
                using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
                using var bw = new BinaryWriter(cs);

                bw.Write(data);
                cs.FlushFinalBlock();

                return ms.ToArray();
            }

            public static byte[] EncryptString(string data, Aes aes)
                => Encrypt(Encode.ToBytes(data), aes);

            public static byte[] Decrypt(byte[] data, Aes aes)
            {
                using var decryptor = aes.CreateDecryptor();
                using var ms = new MemoryStream(data);
                using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using var br = new BinaryReader(cs);

                return br.ReadBytes(data.Length);
            }

            public static string DecryptString(byte[] data, Aes aes)
                => Encode.ToString(Decrypt(data, aes));

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