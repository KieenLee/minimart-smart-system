using Microsoft.Extensions.Logging;
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
        private readonly ILogger<TcpServer> _logger;
        private TcpListener? _listener;
        private readonly List<Task> _clientTasks = new();
        private CancellationTokenSource? _cancellationTokenSource;

        public TcpServer(TcpSettings settings, TcpMessageRouter router, ILogger<TcpServer> logger)
        {
            _settings = settings;
            _router = router;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                var ipAddress = IPAddress.Parse(_settings.Host);
                _listener = new TcpListener(ipAddress, _settings.Port);
                _listener.Start(_settings.MaxConnections);

                _logger.LogInformation("TCP Server started on {Host}:{Port}", _settings.Host, _settings.Port);
                _logger.LogInformation("Max connections: {MaxConnections}", _settings.MaxConnections);

                await AcceptClientsAsync(_cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting TCP Server");
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
                    _logger.LogInformation("Client connected: {Endpoint}", client.Client.RemoteEndPoint);

                    // Handle client trong Task riêng
                    var clientTask = Task.Run(() => HandleClientAsync(client, cancellationToken), cancellationToken);
                    _clientTasks.Add(clientTask);

                    // Cleanup completed tasks
                    _clientTasks.RemoveAll(t => t.IsCompleted);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Server is shutting down...");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error accepting client");
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
                            _logger.LogWarning("Invalid message length: {Length} from {Endpoint}", messageLength, endpoint);
                            break;
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
                            _logger.LogWarning("Incomplete message from {Endpoint}", endpoint);
                            break;
                        }

                        // Deserialize message
                        var messageBytes = new byte[messageLength];
                        Array.Copy(buffer, 0, messageBytes, 0, messageLength);

                        var messageJson = System.Text.Encoding.UTF8.GetString(messageBytes);
                        var tcpMessage = TcpMessage.FromJson(messageJson);

                        if (tcpMessage == null)
                        {
                            _logger.LogWarning("Failed to deserialize message from {Endpoint}", endpoint);
                            continue;
                        }

                        _logger.LogInformation("Received [{Action}] from {Endpoint}", tcpMessage.Action, endpoint);
                        // Route và xử lý message
                        var response = await _router.RouteMessageAsync(tcpMessage);

                        // Send response
                        var responseBytes = response.ToBytes();
                        await stream.WriteAsync(responseBytes, cancellationToken);
                        await stream.FlushAsync(cancellationToken);

                        _logger.LogInformation("Sent response [{Success}] to {Endpoint}", response.Success, endpoint);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing message from {Endpoint}", endpoint);
                        // Send error response
                        var errorResponse = TcpResponse.CreateError($"Server error: {ex.Message}");
                        var errorBytes = errorResponse.ToBytes();
                        await stream.WriteAsync(errorBytes, cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling client {Endpoint}", endpoint);
            }
            finally
            {
                client.Close();
                _logger.LogInformation("Client disconnected: {Endpoint}", endpoint);
            }
        }

        public async Task StopAsync()
        {
            _logger.LogInformation("Stopping TCP Server...");

            _cancellationTokenSource?.Cancel();
            _listener?.Stop();

            // Đợi tất cả client tasks hoàn thành (timeout 5s)
            await Task.WhenAny(
                Task.WhenAll(_clientTasks),
                Task.Delay(5000)
            );

            _logger.LogInformation("TCP Server stopped");
        }
    }
}