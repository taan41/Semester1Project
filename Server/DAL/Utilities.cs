using System.Text.Json;

namespace DAL
{
    static class Utitlities
    {
        public static string ToJson(object obj)
            => JsonSerializer.Serialize(obj);

        public static T? FromJson<T>(string json)
            => JsonSerializer.Deserialize<T>(json);
    }
}