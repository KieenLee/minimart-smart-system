using System.Text;
using System.Text.Json;

namespace MS2.Models.TCP;

public class TcpMessage
{
    public string Action { get; set; } = null!;
    public string? Token { get; set; }
    public object? Data { get; set; }
    public string RequestId { get; set; } = Guid.NewGuid().ToString();

    // Serialize to JSON
    public string ToJson()
    {
        return JsonSerializer.Serialize(this);
    }

    // Deserialize from JSON
    public static TcpMessage? FromJson(string json)
    {
        return JsonSerializer.Deserialize<TcpMessage>(json);
    }

    // Convert to bytes with length prefix
    public byte[] ToBytes()
    {
        string json = ToJson();
        byte[] messageBytes = Encoding.UTF8.GetBytes(json);
        byte[] lengthBytes = BitConverter.GetBytes(messageBytes.Length);

        // Combine length prefix (4 bytes) + message
        byte[] result = new byte[4 + messageBytes.Length];
        Buffer.BlockCopy(lengthBytes, 0, result, 0, 4);
        Buffer.BlockCopy(messageBytes, 0, result, 4, messageBytes.Length);

        return result;
    }

    // Read from bytes with length prefix
    public static TcpMessage? FromBytes(byte[] bytes)
    {
        if (bytes.Length < 4) return null;

        int messageLength = BitConverter.ToInt32(bytes, 0);
        if (bytes.Length < 4 + messageLength) return null;

        byte[] messageBytes = new byte[messageLength];
        Buffer.BlockCopy(bytes, 4, messageBytes, 0, messageLength);

        string json = Encoding.UTF8.GetString(messageBytes);
        return FromJson(json);
    }
}