using System.Text.Json;

namespace NetworkLL.DataTransferObjects
{
    [Serializable]
    public class PasswordSet
    {
        public byte[] PwdHash { get; set; } = new byte[NetworkUtilities.Security.PwdHashLen];
        public byte[] PwdSalt { get; set; } = new byte[NetworkUtilities.Security.PwdSaltLen];

        public PasswordSet() {}

        public PasswordSet(string password)
        {
            (PwdHash, PwdSalt) = NetworkUtilities.Security.HashPassword(password);
        }

        public PasswordSet(byte[] pwdHash, byte[] pwdSalt)
        {
            PwdHash = pwdHash;
            PwdSalt = pwdSalt;
        }

        public string ToJson()
            => JsonSerializer.Serialize(this);

        public static PasswordSet? FromJson(string data) =>
            JsonSerializer.Deserialize<PasswordSet>(data);
    }
}