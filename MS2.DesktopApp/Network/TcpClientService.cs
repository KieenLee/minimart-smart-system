using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MS2.DesktopApp.Models;
using MS2.Models.TCP;

namespace MS2.DesktopApp.Network;

public class TcpClientService : IDisposable
{
    private readonly string _host;
    private readonly int _port;
    private TcpClient? _client;
    private NetworkStream? _stream;
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    private bool _isConnected;

    public bool IsConnected => _isConnected && _client?.Connected == true;
    public string? CurrentSessionId { get; set; }

    public TcpClientService(TcpClientSettings settings)
    {
        _host = settings.Host;
        _port = settings.Port;
        Console.WriteLine($"TcpClientService initialized with {_host}:{_port}");
    }

    /// <summary>
    /// Kết nối tới TCP Server
    /// </summary>
    public async Task<bool> ConnectAsync()
    {
        try
        {
            if (IsConnected)
            {
                Console.WriteLine("Already connected to server.");
                return true;
            }

            _client = new TcpClient();
            await _client.ConnectAsync(_host, _port);
            _stream = _client.GetStream();
            _isConnected = true;

            Console.WriteLine($"Connected to TCP Server at {_host}:{_port}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection failed: {ex.Message}");
            _isConnected = false;
            return false;
        }
    }

    /// <summary>
    /// Ngắt kết nối
    /// </summary>
    public void Disconnect()
    {
        try
        {
            _stream?.Close();
            _client?.Close();
            _isConnected = false;
            Console.WriteLine("Disconnected from server.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during disconnect: {ex.Message}");
        }
    }

    /// <summary>
    /// Gửi message và nhận response
    /// </summary>
    public async Task<TcpResponse?> SendMessageAsync(string action, object? data = null, string? sessionId = null)
    {
        if (!IsConnected)
        {
            throw new InvalidOperationException("Not connected to server. Call ConnectAsync() first.");
        }

        await _sendLock.WaitAsync();
        try
        {
            // Tạo message
            var message = new TcpMessage
            {
                Action = action,
                Data = data,
                SessionId = sessionId ?? CurrentSessionId,
                RequestId = Guid.NewGuid().ToString()
            };

            // Gửi message
            byte[] messageBytes = message.ToBytes();
            await _stream!.WriteAsync(messageBytes, 0, messageBytes.Length);

            Console.WriteLine($"Sent: {action} (RequestId: {message.RequestId})");

            // Đọc response
            var response = await ReadResponseAsync();
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Send message failed: {ex.Message}");
            _isConnected = false;
            throw;
        }
        finally
        {
            _sendLock.Release();
        }
    }

    /// <summary>
    /// Đọc response từ server (length-prefix protocol)
    /// </summary>
    private async Task<TcpResponse?> ReadResponseAsync()
    {
        try
        {
            // Đọc 4 bytes length prefix
            byte[] lengthBuffer = new byte[4];
            int bytesRead = 0;
            while (bytesRead < 4)
            {
                int read = await _stream!.ReadAsync(lengthBuffer, bytesRead, 4 - bytesRead);
                if (read == 0)
                {
                    throw new Exception("Connection closed by server.");
                }
                bytesRead += read;
            }

            int messageLength = BitConverter.ToInt32(lengthBuffer, 0);

            // Đọc message body
            byte[] messageBuffer = new byte[messageLength];
            bytesRead = 0;
            while (bytesRead < messageLength)
            {
                int read = await _stream!.ReadAsync(messageBuffer, bytesRead, messageLength - bytesRead);
                if (read == 0)
                {
                    throw new Exception("Connection closed by server.");
                }
                bytesRead += read;
            }

            // Deserialize
            string json = Encoding.UTF8.GetString(messageBuffer);
            var response = TcpResponse.FromJson(json);

            Console.WriteLine($"Received: {(response?.Success == true ? "Success" : "Error")} - {response?.Message}");
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Read response failed: {ex.Message}");
            throw;
        }
    }

    public void Dispose()
    {
        Disconnect();
        _sendLock?.Dispose();
    }
}
