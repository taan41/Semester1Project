namespace BLL.Server.DataModels
{
    [Serializable]
    public class User
    {
        public int UserID { get; set; } = -1;
        public string Username { get; set; } = "";
        public string Nickname { get; set; } = "";
        public string Email { get; set; } = "";
        public PasswordSet? PwdSet { get; set; }

        public User() {}

        public User(string username, string nickname, string email, string password)
        {
            Username = username;
            Nickname = nickname;
            Email = email;
            PwdSet = new(password);
        }

        public User(User other)
        {
            UserID = other.UserID;
            Username = other.Username;
            Nickname = other.Nickname;
            Email = other.Email;
            PwdSet = other.PwdSet;
        }
        
        public override string ToString()
            => $"User(ID: {UserID})";

        public string ToString(bool showFullInfo)
            => showFullInfo ?
                $"User(ID: {UserID}, Username: {Username}, Nickname: {Nickname})" :
                ToString();
    }
}