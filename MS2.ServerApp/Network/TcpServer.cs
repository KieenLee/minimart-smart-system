using MS2.Models.TCP;
using MS2.ServerApp.Models;
using System.Net;
using System.Net.Sockets;

namespace MS2.ServerApp.Network
{
    public class TcpServer
    {
        private readonly TcpSettings _settings;
        private readonly TcpMessageRouter _router;
        private TcpListener? _listener;
        private readonly List<Task> _clientTasks = new();
        private CancellationTokenSource? _cancellationTokenSource;
        private int _connectedClients = 0;

        public TcpServer(TcpSettings settings, TcpMessageRouter router)
        {
            _settings = settings;
            _router = router;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                var ipAddress = IPAddress.Parse(_settings.Host);
                _listener = new TcpListener(ipAddress, _settings.Port);
                _listener.Start();

                Console.WriteLine($"Server started | {_settings.Host}:{_settings.Port}");

                await AcceptClientsAsync(_cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting TCP Server: {ex.Message}");
                throw;
            }
        }

        private async Task AcceptClientsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var client = await _listener!.AcceptTcpClientAsync(cancellationToken);

                    Interlocked.Increment(ref _connectedClients);
                    Console.WriteLine($"{client.Client.RemoteEndPoint} | Connected | Online: {_connectedClients}");

                    // Handle client trong Task riêng
                    var clientTask = Task.Run(() => HandleClientAsync(client, cancellationToken), cancellationToken);
                    _clientTasks.Add(clientTask);

                    // Cleanup completed tasks
                    _clientTasks.RemoveAll(t => t.IsCompleted);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accepting client: {ex.Message}");
                }
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
            var endpoint = client.Client.RemoteEndPoint?.ToString() ?? "Unknown";

            try
            {
                using var stream = client.GetStream();
                var buffer = new byte[8192]; // 8KB buffer

                while (!cancellationToken.IsCancellationRequested && client.Connected)
                {
                    try
                    {
                        // Đọc 4 bytes đầu (message length)
                        int lengthBytesRead = await stream.ReadAsync(buffer.AsMemory(0, 4), cancellationToken);

                        if (lengthBytesRead == 0) break; // Client disconnected

                        int messageLength = BitConverter.ToInt32(buffer, 0);

                        if (messageLength <= 0 || messageLength > buffer.Length)
                        {
                            break; // Invalid length, close connection
                        }

                        // Đọc message body
                        int totalRead = 0;
                        while (totalRead < messageLength)
                        {
                            int bytesRead = await stream.ReadAsync(
                                buffer.AsMemory(totalRead, messageLength - totalRead),
                                cancellationToken);

                            if (bytesRead == 0) break;
                            totalRead += bytesRead;
                        }

                        if (totalRead < messageLength)
                        {
                            break; // Incomplete message
                        }

                        // Deserialize message
                        var messageBytes = new byte[messageLength];
                        Array.Copy(buffer, 0, messageBytes, 0, messageLength);

                        var messageJson = System.Text.Encoding.UTF8.GetString(messageBytes);
                        var tcpMessage = TcpMessage.FromJson(messageJson);

                        if (tcpMessage == null) continue;

                        // Log request & response trên 1 dòng
                        var queryInfo = tcpMessage.Data != null ? $" - {tcpMessage.Data.ToString()?.Substring(0, Math.Min(40, tcpMessage.Data.ToString()?.Length ?? 0))}..." : "";

                        // Route và xử lý message
                        var response = await _router.RouteMessageAsync(tcpMessage);

                        // Log: IP:Port | Request | Action - Query | Status | Records | Online
                        var recordCount = GetRecordCount(response.Data);
                        var status = response.Success ? "SUCCESS" : "FAILED";
                        Console.WriteLine($"{endpoint} | Request | {tcpMessage.Action}{queryInfo} | {status} | Records: {recordCount} | Online: {_connectedClients}");

                        // Send response
                        var responseBytes = response.ToBytes();
                        await stream.WriteAsync(responseBytes, cancellationToken);
                        await stream.FlushAsync(cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        // Send error response
                        var errorResponse = TcpResponse.CreateError($"Server error: {ex.Message}");
                        var errorBytes = errorResponse.ToBytes();
                        await stream.WriteAsync(errorBytes, cancellationToken);
                    }
                }
            }
            catch (Exception)
            {
                // Silent error handling
            }
            finally
            {
                client.Close();
                Interlocked.Decrement(ref _connectedClients);
                Console.WriteLine($"{endpoint} | Disconnected | Online: {_connectedClients}");
            }
        }

        private int GetRecordCount(object? data)
        {
            if (data == null) return 0;

            // Check if data is array/list
            if (data is System.Collections.IEnumerable enumerable and not string)
            {
                int count = 0;
                foreach (var _ in enumerable) count++;
                return count;
            }

            return 1; // Single object
        }

        public async Task StopAsync()
        {
            Console.WriteLine("Stopping server...");

            _cancellationTokenSource?.Cancel();
            _listener?.Stop();

            // Đợi tất cả client tasks hoàn thành (timeout 5s)
            await Task.WhenAny(
                Task.WhenAll(_clientTasks),
                Task.Delay(5000)
            );

            Console.WriteLine("Server stopped");
        }
    }
}