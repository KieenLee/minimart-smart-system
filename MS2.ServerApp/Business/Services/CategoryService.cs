using MS2.DataAccess.Interfaces;
using MS2.Models.TCP;
using MS2.ServerApp.Business.Interfaces;
using System.Text.Json;

namespace MS2.ServerApp.Business.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISessionManager _sessionManager;

        public CategoryService(IUnitOfWork unitOfWork, ISessionManager sessionManager)
        {
            _unitOfWork = unitOfWork;
            _sessionManager = sessionManager;
        }

        public async Task<TcpResponse> GetCategoriesAsync(TcpMessage message)
        {
            try
            {
                if (!_sessionManager.IsValidSession(message.SessionId))
                {
                    return TcpResponse.CreateError("Invalid session", message.RequestId);
                }

                var categories = await _unitOfWork.Categories.GetAllAsync();

                return TcpResponse.CreateSuccess(categories, "Get categories successful", message.RequestId);
            }
            catch (Exception ex)
            {
                return TcpResponse.CreateError($"Get categories error: {ex.Message}", message.RequestId);
            }
        }

        public async Task<TcpResponse> GetRootCategoriesAsync(TcpMessage message)
        {
            try
            {
                if (!_sessionManager.IsValidSession(message.SessionId))
                {
                    return TcpResponse.CreateError("Invalid session", message.RequestId);
                }

                var rootCategories = await _unitOfWork.Categories.GetRootCategoriesAsync();

                return TcpResponse.CreateSuccess(rootCategories, "Get root categories successful", message.RequestId);
            }
            catch (Exception ex)
            {
                return TcpResponse.CreateError($"Get root categories error: {ex.Message}", message.RequestId);
            }
        }

        public async Task<TcpResponse> GetSubCategoriesAsync(TcpMessage message)
        {
            try
            {
                if (!_sessionManager.IsValidSession(message.SessionId))
                {
                    return TcpResponse.CreateError("Invalid session", message.RequestId);
                }

                var categoryData = JsonSerializer.Deserialize<Dictionary<string, int>>(
                    JsonSerializer.Serialize(message.Data));

                int parentId = categoryData?["ParentId"] ?? 0;

                var subCategories = await _unitOfWork.Categories.GetSubCategoriesAsync(parentId);

                return TcpResponse.CreateSuccess(subCategories, "Get sub categories successful", message.RequestId);
            }
            catch (Exception ex)
            {
                return TcpResponse.CreateError($"Get sub categories error: {ex.Message}", message.RequestId);
            }
        }
    }
}