using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using BLL.Server.DataModels;

using static BLL.Utilities.GenericUtilities;

namespace BLL.Utilities
{
    public static class NetworkUtilities
    {
        #region Miscellaneous
        public static bool CheckIPv4(string? ipAddress)
        {
            if (!IPAddress.TryParse(ipAddress, out _))
                return false;

            string[] parts = ipAddress.Split('.');
            if (parts.Length != 4) return false;

            foreach(string part in parts)
            {
                if (!int.TryParse(part, out int number))
                    return false;

                if (number < 0 || number > 255)
                    return false;

                if (part.Length > 1 && part[0] == '0')
                    return false;
            }

            return true;
        }
        #endregion

        public static class Security
        {
            #region AES encryption
            public static void InitAESClient(NetworkStream stream, out Aes aes)
            {
                using RSACryptoServiceProvider rsa = new();
                byte[] rsaPublicKey = rsa.ExportRSAPublicKey();
                stream.Write(rsaPublicKey);

                byte[] encryptedAesKey = new byte[256];
                int bytesRead = stream.Read(encryptedAesKey, 0, 256);
                byte[] aesKey = rsa.Decrypt(encryptedAesKey[..bytesRead], RSAEncryptionPadding.Pkcs1);

                byte[] encryptedAesIV = new byte[256];
                bytesRead = stream.Read(encryptedAesIV, 0, 256);
                byte[] aesIV = rsa.Decrypt(encryptedAesIV[..bytesRead], RSAEncryptionPadding.Pkcs1);

                aes = Aes.Create();
                aes.Key = aesKey;
                aes.IV = aesIV;
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
                => Encrypt(StringToBytes(data), aes);

            public static byte[] Decrypt(byte[] data, Aes aes)
            {
                using var decryptor = aes.CreateDecryptor();
                using var ms = new MemoryStream(data);
                using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using var br = new BinaryReader(cs);

                return br.ReadBytes(data.Length);
            }

            public static string DecryptString(byte[] data, Aes aes)
                => BytesToString(Decrypt(data, aes));
            #endregion
            
            #region Password hashing
            public const int PwdHashLen = 32;
            public const int PwdSaltLen = 16;

            public static (byte[] PwdHash, byte[] Salt) HashPassword(string pwd)
            {
                byte[] salt = new byte[PwdSaltLen];
                RandomNumberGenerator.Fill(salt);

                using var pbkdf2 = new Rfc2898DeriveBytes(StringToBytes(pwd), salt, 10000, HashAlgorithmName.SHA256);
                byte[] pwdHash = pbkdf2.GetBytes(PwdHashLen);

                return (pwdHash, salt);
            }

            public static bool VerifyPassword(string pwd, byte[] storedPwdHash, byte[] storedSalt)
            {
                using var pbkdf2 = new Rfc2898DeriveBytes(StringToBytes(pwd), storedSalt, 10000, HashAlgorithmName.SHA256);
                byte[] pwdHash = pbkdf2.GetBytes(PwdHashLen);

                return pwdHash.SequenceEqual(storedPwdHash);
            }

            public static bool VerifyPassword(string pwd, PasswordSet pwdSet)
                => VerifyPassword(pwd, pwdSet.PwdHash, pwdSet.PwdSalt);
            #endregion
        }
    }
}