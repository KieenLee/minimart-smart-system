using MS2.ServerApp.Models;

namespace MS2.ServerApp.Business.Interfaces
{
    public interface ISessionManager
    {
        //Tạo session mới cho user
        string CreateSession(UserSession session);
       
        // Lấy session theo SessionId
        UserSession? GetSession(string sessionId);

        // Xoá session khi logout
        bool RemoveSession(string sessionId);

        // Kiểm tra session có tồn tại không
        bool IsValidSession(string sessionId);
       
        // Lấy tất cả active sessions
        IEnumerable<UserSession> GetAllSessions();

        // Xoá sessions hết hạn (optional, cho future)
        int RemoveExpiredSessions(TimeSpan expirationTime);
    }
}