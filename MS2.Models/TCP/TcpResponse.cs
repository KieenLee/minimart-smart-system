using System.Text;
using System.Text.Json;

namespace MS2.Models.TCP;

public class TcpResponse
{
    public bool Success { get; set; }
    public object? Data { get; set; }
    public string Message { get; set; } = null!;
    public string RequestId { get; set; } = null!;

    // Factory methods
    public static TcpResponse CreateSuccess(object? data, string message = "Success", string requestId = "")
    {
        return new TcpResponse
        {
            Success = true,
            Data = data,
            Message = message,
            RequestId = requestId
        };
    }

    public static TcpResponse CreateError(string message, string requestId = "")
    {
        return new TcpResponse
        {
            Success = false,
            Data = null,
            Message = message,
            RequestId = requestId
        };
    }

    // Serialize to JSON
    public string ToJson()
    {
        return JsonSerializer.Serialize(this);
    }

    // Deserialize from JSON
    public static TcpResponse? FromJson(string json)
    {
        return JsonSerializer.Deserialize<TcpResponse>(json);
    }

    // Convert to bytes with length prefix
    public byte[] ToBytes()
    {
        string json = ToJson();
        byte[] messageBytes = Encoding.UTF8.GetBytes(json);
        byte[] lengthBytes = BitConverter.GetBytes(messageBytes.Length);

        byte[] result = new byte[4 + messageBytes.Length];
        Buffer.BlockCopy(lengthBytes, 0, result, 0, 4);
        Buffer.BlockCopy(messageBytes, 0, result, 4, messageBytes.Length);

        return result;
    }
}