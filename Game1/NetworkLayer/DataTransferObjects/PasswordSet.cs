using System.Text.Json;

[Serializable]
class PasswordSet
{
    public byte[] PwdHash { get; set; } = new byte[Utilities.DataConstants.pwdHashLen];
    public byte[] PwdSalt { get; set; } = new byte[Utilities.DataConstants.pwdSaltLen];

    public PasswordSet() {}

    public PasswordSet(string password)
    {
        (PwdHash, PwdSalt) = Utilities.Security.HashPassword(password);
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