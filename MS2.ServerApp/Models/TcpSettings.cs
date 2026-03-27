namespace MS2.ServerApp.Models
{
    public class TcpSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public int MaxConnections { get; set; }
    }
}