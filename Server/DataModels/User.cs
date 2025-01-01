using System.Text;
using System.Text.Json;

[Serializable]
class User
{
    public int UserID { get; set; } = -1;
    public string Username { get; set; } = "";
    public string Nickname { get; set; } = "";
    public string Email { get; set; } = "";
    public PasswordSet? PwdSet { get; set; }

    public User() {}

    public User(User userToCopy)
    {
        UserID = userToCopy.UserID;
        Username = userToCopy.Username;
        Nickname = userToCopy.Nickname;
        Email = userToCopy.Email;
        PwdSet = userToCopy.PwdSet;
    }
    
    public override string ToString()
        => $"User(ID: {UserID})";

    public string ToString(bool showFullInfo)
        => showFullInfo ? $"User(ID: {UserID}, Username: {Username}, Nickname: {Nickname})" : ToString();

    // public string Serialize()
    //     => JsonSerializer.Serialize(this);

    // public static User? Deserialize(string data) =>
    //     JsonSerializer.Deserialize<User>(data);
}