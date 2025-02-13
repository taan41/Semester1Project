using System.Text;
using System.Text.Json;

namespace BLL.Utilities;

public static class GenericUtilities
{
    // JSON serialization
    public static string ToJson(object obj)
        => JsonSerializer.Serialize(obj);
    public static T? FromJson<T>(string json)
        => JsonSerializer.Deserialize<T>(json);

    // String encoding
    public static string BytesToString(byte[] data) => Encoding.UTF8.GetString(data);
    public static string BytesToString(byte[] data, int index, int length) => Encoding.UTF8.GetString(data, index, length);
    public static byte[] StringToBytes(string content) => Encoding.UTF8.GetBytes(content);
}