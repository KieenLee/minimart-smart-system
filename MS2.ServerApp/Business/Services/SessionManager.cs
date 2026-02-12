using MS2.ServerApp.Business.Interfaces;
using MS2.ServerApp.Models;
using System.Collections.Concurrent;

namespace MS2.ServerApp.Business.Services
{
    public class SessionManager : ISessionManager
    {
        private readonly ConcurrentDictionary<string, UserSession> _sessions = new();

        public string CreateSession(UserSession session)
        {
            session.SessionId = Guid.NewGuid().ToString();
            session.LoginTime = DateTime.Now;

            _sessions[session.SessionId] = session;

            return session.SessionId;
        }

        public UserSession? GetSession(string sessionId)
        {
            _sessions.TryGetValue(sessionId, out var session);
            return session;
        }

        public bool RemoveSession(string sessionId)
        {
            return _sessions.TryRemove(sessionId, out _);
        }

        public bool IsValidSession(string sessionId)
        {
            return _sessions.ContainsKey(sessionId);
        }

        public IEnumerable<UserSession> GetAllSessions()
        {
            return _sessions.Values;
        }

        public int RemoveExpiredSessions(TimeSpan expirationTime)
        {
            var expiredSessions = _sessions
                .Where(kvp => kvp.Value.GetSessionDuration() > expirationTime)
                .Select(kvp => kvp.Key)
                .ToList();

            int count = 0;
            foreach (var sessionId in expiredSessions)
            {
                if (RemoveSession(sessionId))
                    count++;
            }

            return count;
        }
    }
}