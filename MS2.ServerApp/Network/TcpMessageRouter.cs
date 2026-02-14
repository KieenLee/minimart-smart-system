using Microsoft.Extensions.DependencyInjection;
using MS2.Models.TCP;
using MS2.ServerApp.Business.Interfaces;

namespace MS2.ServerApp.Network
{
    public class TcpMessageRouter
    {
        private readonly IServiceProvider _serviceProvider;

        public TcpMessageRouter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<TcpResponse> RouteMessageAsync(TcpMessage message)
        {
            try
            {
                // Tạo scope mới cho mỗi request (để có fresh DbContext)
                using var scope = _serviceProvider.CreateScope();

                return message.Action switch
                {
                    // Auth Actions
                    TcpActions.LOGIN => await scope.ServiceProvider
                        .GetRequiredService<IAuthService>()
                        .LoginAsync(message),

                    TcpActions.LOGOUT => await scope.ServiceProvider
                        .GetRequiredService<IAuthService>()
                        .LogoutAsync(message),

                    // Product Actions
                    TcpActions.GET_PRODUCTS => await scope.ServiceProvider
                        .GetRequiredService<IProductService>()
                        .GetProductsAsync(message),

                    TcpActions.SEARCH_PRODUCTS => await scope.ServiceProvider
                        .GetRequiredService<IProductService>()
                        .SearchProductsAsync(message),

                    TcpActions.GET_PRODUCT_BY_BARCODE => await scope.ServiceProvider
                        .GetRequiredService<IProductService>()
                        .GetProductByBarcodeAsync(message),

                    TcpActions.UPDATE_PRODUCT_PRICE => await scope.ServiceProvider
                        .GetRequiredService<IProductService>()
                        .UpdateProductPriceAsync(message),

                    TcpActions.UPDATE_PRODUCT_STOCK => await scope.ServiceProvider
                        .GetRequiredService<IProductService>()
                        .UpdateProductStockAsync(message),

                    TcpActions.GET_LOW_STOCK_PRODUCTS => await scope.ServiceProvider
                        .GetRequiredService<IProductService>()
                        .GetLowStockProductsAsync(message),

                    // Order Actions
                    TcpActions.CREATE_ORDER => await scope.ServiceProvider
                        .GetRequiredService<IOrderService>()
                        .CreateOrderAsync(message),

                    TcpActions.GET_ORDERS => await scope.ServiceProvider
                        .GetRequiredService<IOrderService>()
                        .GetOrdersAsync(message),

                    TcpActions.GET_ORDER_DETAILS => await scope.ServiceProvider
                        .GetRequiredService<IOrderService>()
                        .GetOrderDetailsAsync(message),

                    TcpActions.GET_SALES_REPORT => await scope.ServiceProvider
                        .GetRequiredService<IOrderService>()
                        .GetSalesReportAsync(message),

                    // Category Actions
                    TcpActions.GET_CATEGORIES => await scope.ServiceProvider
                        .GetRequiredService<ICategoryService>()
                        .GetCategoriesAsync(message),

                    // User/Employee Actions
                    TcpActions.GET_EMPLOYEES => await scope.ServiceProvider
                        .GetRequiredService<IUserService>()
                        .GetEmployeesAsync(message),

                    TcpActions.GET_USERS_BY_ROLE => await scope.ServiceProvider
                        .GetRequiredService<IUserService>()
                        .GetUsersByRoleAsync(message),

                    TcpActions.SEARCH_USERS => await scope.ServiceProvider
                        .GetRequiredService<IUserService>()
                        .SearchUsersAsync(message),

                    TcpActions.CREATE_USER => await scope.ServiceProvider
                        .GetRequiredService<IUserService>()
                        .CreateUserAsync(message),

                    TcpActions.UPDATE_USER_PROFILE => await scope.ServiceProvider
                        .GetRequiredService<IUserService>()
                        .UpdateUserProfileAsync(message),

                    // Unknown Action
                    _ => TcpResponse.CreateError($"Unknown action: {message.Action}", message.RequestId)
                };
            }
            catch (Exception ex)
            {
                return TcpResponse.CreateError($"Routing error: {ex.Message}", message.RequestId);
            }
        }
    }
}