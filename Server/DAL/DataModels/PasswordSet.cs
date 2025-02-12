namespace DAL.DataModels
{
    [Serializable]
    public class PasswordSet(byte[] pwdHash, byte[] pwdSalt)
    {
        public byte[] PwdHash { get; set; } = pwdHash;
        public byte[] PwdSalt { get; set; } = pwdSalt;
    }
}