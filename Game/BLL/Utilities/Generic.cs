using System.Text;
using System.Text.Json;

namespace BLL.Utilities
{
    public static class GenericUtilities
    {
        // JSON Serialization
        public static string ToJson<T>(T obj) => JsonSerializer.Serialize(obj);
        public static T? FromJson<T>(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(json);
            }
            catch
            {
                return default;
            }
        }

        // String <-> Bytes
        public static string BytesToString(byte[] data) => Encoding.UTF8.GetString(data);
        public static string BytesToString(byte[] data, int index, int length) => Encoding.UTF8.GetString(data, index, length);
        public static byte[] StringToBytes(string content) => Encoding.UTF8.GetBytes(content);
    }
}