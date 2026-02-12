using MS2.Models.DTOs.Auth;
using System.Net.Sockets;

namespace MS2.ServerApp.Models
{
    public class UserSession
    {
        public string SessionId { get; set; } = string.Empty;
        public UserDto User { get; set; } = null!;
        public DateTime LoginTime { get; set; }
        public TcpClient? TcpClient { get; set; }

        public TimeSpan GetSessionDuration()
        {
            return DateTime.Now - LoginTime;
        }
    }
}