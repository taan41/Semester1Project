namespace DAL.Persistence.DataTransferObjects
{
    [Serializable]
    public class Command
    {
        public enum Type
        {
            Empty, Ping, Error, GetAES,
            CheckUsername, Register,
            GetUserPwd, Login, Logout, ValidateEmail, ResetPwd,
            ChangeNickname, ChangeEmail, ChangePassword,
            GameConfig, ServerConfig, DatabaseConfig,
            UpdateEquip, UpdateSkill, UpdateMonster,
            UploadSave, DownloadSave,
            UploadScore, PersonalScores, MonthlyScores, AllTimeScores,
            Disconnect
        }

        public Type CommandType { get; set; } = Type.Empty;
        public string Payload { get; set; } = "";

        public Command() {}

        public Command(Type cmdType, string? payload = null)
        {
            CommandType = cmdType;
            Payload = payload ?? "";
        }

        public void Set(Type cmdType, string? payload = null)
        {
            CommandType = cmdType;
            Payload = payload ?? "";
        }

        public void SetError(string? payload = null)
            => Set(Type.Error, payload);

        public string Name()
            => CommandType.ToString();
    }
}