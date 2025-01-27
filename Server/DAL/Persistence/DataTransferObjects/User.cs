namespace DAL.Persistence.DataTransferObjects
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

        public User(User other)
        {
            UserID = other.UserID;
            Username = other.Username;
            Nickname = other.Nickname;
            Email = other.Email;
            PwdSet = other.PwdSet;
        }
        
        public override string ToString()
            => $"User({(UserID != -1 ? $"ID: {UserID}, " : "")}Username: {Username})";

        public string ToString(bool showFullInfo)
            => showFullInfo ? $"User(ID: {UserID}, Username: {Username}, Nickname: {Nickname}, Email: {Email})" : ToString();
    }
}