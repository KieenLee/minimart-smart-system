namespace MS2.DesktopApp.Models;

public class TcpClientSettings
{
    public string Host { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 5000;
    public int ConnectTimeoutMs { get; set; } = 5000;
    public int ReadTimeoutMs { get; set; } = 30000;
}
