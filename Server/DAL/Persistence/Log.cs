namespace DAL.Persistence
{
    public class Log(DateTime? _time, string _source, string _content)
    {
        private readonly DateTime time = _time ?? DateTime.Now;
        private readonly string source = _source;
        private readonly string content = _content;

        public override string ToString()
            => $"[{time:dd/MM/yy HH:mm:ss}] ({source}) {content}";
    }
}