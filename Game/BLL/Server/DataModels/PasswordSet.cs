using static BLL.Utilities.NetworkUtilities;

namespace BLL.Server.DataModels
{
    [Serializable]
    public class PasswordSet
    {
        public byte[] PwdHash { get; set; } = new byte[Security.PwdHashLen];
        public byte[] PwdSalt { get; set; } = new byte[Security.PwdSaltLen];

        public PasswordSet() {}

        public PasswordSet(string password)
        {
            (PwdHash, PwdSalt) = Security.HashPassword(password);
        }

        public PasswordSet(byte[] pwdHash, byte[] pwdSalt)
        {
            PwdHash = pwdHash;
            PwdSalt = pwdSalt;
        }
    }
}