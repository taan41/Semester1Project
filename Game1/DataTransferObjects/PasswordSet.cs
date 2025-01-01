using System.Text.Json;

[Serializable]
class PasswordSet(byte[] pwdHash, byte[] pwdSalt)
{
    public byte[] PwdHash { get; set; } = pwdHash;
    public byte[] PwdSalt { get; set; } = pwdSalt;

    // public string Serialize()
    //     => JsonSerializer.Serialize(this);

    // public static PasswordSet? Deserialize(string data) =>
    //     JsonSerializer.Deserialize<PasswordSet>(data);
}