using MS2.DataAccess.Interfaces;
using MS2.Models.DTOs.Product;
using MS2.Models.TCP;
using MS2.ServerApp.Business.Interfaces;
using System.Text.Json;

namespace MS2.ServerApp.Business.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISessionManager _sessionManager;

        public ProductService(IUnitOfWork unitOfWork, ISessionManager sessionManager)
        {
            _unitOfWork = unitOfWork;
            _sessionManager = sessionManager;
        }

        public async Task<TcpResponse> GetProductsAsync(TcpMessage message)
        {
            try
            {
                // Validate session
                if (!_sessionManager.IsValidSession(message.SessionId))
                {
                    return TcpResponse.CreateError("Invalid session", message.RequestId);
                }

                var products = await _unitOfWork.Products.GetAllAsync();

                var productDtos = products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category?.Name ?? "",
                    Barcode = p.Barcode,
                    Price = p.Price,
                    Stock = p.Stock,
                    ImageUrl = p.ImageUrl,
                    Description = p.Description,
                    CreatedAt = p.CreatedAt,
                    IsActive = p.IsActive
                }).ToList();

                return TcpResponse.CreateSuccess(productDtos, "Get products successful", message.RequestId);
            }
            catch (Exception ex)
            {
                return TcpResponse.CreateError($"Get products error: {ex.Message}", message.RequestId);
            }
        }

        public async Task<TcpResponse> SearchProductsAsync(TcpMessage message)
        {
            try
            {
                if (!_sessionManager.IsValidSession(message.SessionId))
                {
                    return TcpResponse.CreateError("Invalid session", message.RequestId);
                }

                var searchData = JsonSerializer.Deserialize<Dictionary<string, string>>(
                    JsonSerializer.Serialize(message.Data));

                string keyword = searchData?["Keyword"] ?? "";

                var products = await _unitOfWork.Products.SearchAsync(keyword);

                var productDtos = products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category?.Name ?? "",
                    Barcode = p.Barcode,
                    Price = p.Price,
                    Stock = p.Stock,
                    ImageUrl = p.ImageUrl,
                    Description = p.Description,
                    CreatedAt = p.CreatedAt,
                    IsActive = p.IsActive
                }).ToList();

                return TcpResponse.CreateSuccess(productDtos, "Search successful", message.RequestId);
            }
            catch (Exception ex)
            {
                return TcpResponse.CreateError($"Search error: {ex.Message}", message.RequestId);
            }
        }

        public async Task<TcpResponse> GetProductByBarcodeAsync(TcpMessage message)
        {
            try
            {
                if (!_sessionManager.IsValidSession(message.SessionId))
                {
                    return TcpResponse.CreateError("Invalid session", message.RequestId);
                }

                var barcodeData = JsonSerializer.Deserialize<Dictionary<string, string>>(
                    JsonSerializer.Serialize(message.Data));

                string barcode = barcodeData?["Barcode"] ?? "";

                var product = await _unitOfWork.Products.GetByBarcodeAsync(barcode);

                if (product == null)
                {
                    return TcpResponse.CreateError("Product not found", message.RequestId);
                }

                var productDto = new ProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Barcode = product.Barcode,
                    CategoryId = product.CategoryId,
                    CategoryName = product.Category?.Name ?? "",
                    Price = product.Price,
                    Stock = product.Stock,
                    ImageUrl = product.ImageUrl,
                    Description = product.Description,
                    CreatedAt = product.CreatedAt,
                    IsActive = product.IsActive
                };

                return TcpResponse.CreateSuccess(productDto, "Product found", message.RequestId);
            }
            catch (Exception ex)
            {
                return TcpResponse.CreateError($"Get product error: {ex.Message}", message.RequestId);
            }
        }

        public async Task<TcpResponse> UpdateProductPriceAsync(TcpMessage message)
        {
            try
            {
                if (!_sessionManager.IsValidSession(message.SessionId))
                {
                    return TcpResponse.CreateError("Invalid session", message.RequestId);
                }

                var updateDto = JsonSerializer.Deserialize<UpdatePriceDto>(
                    JsonSerializer.Serialize(message.Data));

                if (updateDto == null)
                {
                    return TcpResponse.CreateError("Invalid data", message.RequestId);
                }

                var product = await _unitOfWork.Products.GetByIdAsync(updateDto.ProductId);
                if (product == null)
                {
                    return TcpResponse.CreateError("Product not found", message.RequestId);
                }

                product.Price = updateDto.NewPrice;
                _unitOfWork.Context.Products.Update(product);
                await _unitOfWork.SaveChangesAsync();

                return TcpResponse.CreateSuccess(null, "Price updated", message.RequestId);
            }
            catch (Exception ex)
            {
                return TcpResponse.CreateError($"Update price error: {ex.Message}", message.RequestId);
            }
        }

        public async Task<TcpResponse> UpdateProductStockAsync(TcpMessage message)
        {
            try
            {
                if (!_sessionManager.IsValidSession(message.SessionId))
                {
                    return TcpResponse.CreateError("Invalid session", message.RequestId);
                }

                var updateDto = JsonSerializer.Deserialize<UpdateStockDto>(
                    JsonSerializer.Serialize(message.Data));

                if (updateDto == null)
                {
                    return TcpResponse.CreateError("Invalid data", message.RequestId);
                }

                var product = await _unitOfWork.Products.GetByIdAsync(updateDto.ProductId);
                if (product == null)
                {
                    return TcpResponse.CreateError("Product not found", message.RequestId);
                }

                product.Stock = updateDto.NewStock;
                _unitOfWork.Context.Products.Update(product);
                await _unitOfWork.SaveChangesAsync();

                return TcpResponse.CreateSuccess(null, "Stock updated", message.RequestId);
            }
            catch (Exception ex)
            {
                return TcpResponse.CreateError($"Update stock error: {ex.Message}", message.RequestId);
            }
        }

        public async Task<TcpResponse> GetLowStockProductsAsync(TcpMessage message)
        {
            try
            {
                if (!_sessionManager.IsValidSession(message.SessionId))
                {
                    return TcpResponse.CreateError("Invalid session", message.RequestId);
                }

                var lowStockData = JsonSerializer.Deserialize<Dictionary<string, int>>(
                    JsonSerializer.Serialize(message.Data));

                int threshold = lowStockData?["Threshold"] ?? 10;

                var products = await _unitOfWork.Products.GetLowStockProductsAsync(threshold);

                var productDtos = products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category?.Name ?? "",
                    Barcode = p.Barcode,
                    Price = p.Price,
                    Stock = p.Stock,
                    ImageUrl = p.ImageUrl,
                    Description = p.Description,
                    CreatedAt = p.CreatedAt,
                    IsActive = p.IsActive
                }).ToList();

                return TcpResponse.CreateSuccess(productDtos, "Low stock products retrieved", message.RequestId);
            }
            catch (Exception ex)
            {
                return TcpResponse.CreateError($"Get low stock error: {ex.Message}", message.RequestId);
            }
        }

        public async Task<TcpResponse> CreateProductAsync(TcpMessage message)
        {
            try
            {
                if (!_sessionManager.IsValidSession(message.SessionId))
                {
                    return TcpResponse.CreateError("Invalid session", message.RequestId);
                }

                var createDto = JsonSerializer.Deserialize<CreateProductDto>(
                    JsonSerializer.Serialize(message.Data));

                if (createDto == null)
                {
                    return TcpResponse.CreateError("Invalid product data", message.RequestId);
                }

                if (!string.IsNullOrEmpty(createDto.Barcode))
                {
                    bool exists = await _unitOfWork.Products.BarcodeExistsAsync(createDto.Barcode);
                    if (exists)
                        return TcpResponse.CreateError("Barcode đã tồn tại", message.RequestId);
                }

                var product = new MS2.Models.Entities.Product
                {
                    Name = createDto.Name,
                    CategoryId = createDto.CategoryId,
                    Price = createDto.Price,
                    Stock = createDto.Stock,
                    Barcode = createDto.Barcode,
                    Description = createDto.Description,
                    ImageUrl = createDto.ImageUrl,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                await _unitOfWork.Products.AddAsync(product);
                await _unitOfWork.SaveChangesAsync();

                return TcpResponse.CreateSuccess(null, "Product created successfully", message.RequestId);
            }
            catch (Exception ex)
            {
                return TcpResponse.CreateError($"Create product error: {ex.Message}", message.RequestId);
            }
        }

        public async Task<TcpResponse> BatchUpsertProductsAsync(TcpMessage message)
        {
            try
            {
                if (!_sessionManager.IsValidSession(message.SessionId))
                    return TcpResponse.CreateError("Invalid session", message.RequestId);

                var rows = JsonSerializer.Deserialize<List<CreateProductDto>>(
                    JsonSerializer.Serialize(message.Data),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (rows == null || rows.Count == 0)
                    return TcpResponse.CreateError("Không có dữ liệu để import", message.RequestId);

                int added = 0, updated = 0;

                foreach (var row in rows)
                {
                    if (string.IsNullOrWhiteSpace(row.Name)) continue;

                    // Tìm theo Barcode trước, rồi theo Tên
                    MS2.Models.Entities.Product? existing = null;
                    if (!string.IsNullOrWhiteSpace(row.Barcode))
                        existing = await _unitOfWork.Products.GetByBarcodeAsync(row.Barcode);
                    if (existing == null)
                        existing = (await _unitOfWork.Products.FindAsync(p => p.Name == row.Name)).FirstOrDefault();

                    if (existing != null)
                    {
                        // Cập nhật tồn kho (cộng thêm số lượng từ file)
                        existing.Stock += row.Stock;
                        if (row.Price > 0) existing.Price = row.Price;
                        await _unitOfWork.Products.UpdateAsync(existing);
                        updated++;
                    }
                    else
                    {
                        // Thêm sản phẩm mới
                        var product = new MS2.Models.Entities.Product
                        {
                            Name = row.Name,
                            CategoryId = row.CategoryId > 0 ? row.CategoryId : 1,
                            Price = row.Price,
                            Stock = row.Stock,
                            Barcode = row.Barcode,
                            Description = row.Description,
                            IsActive = true,
                            CreatedAt = DateTime.Now
                        };
                        await _unitOfWork.Products.AddAsync(product);
                        added++;
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                return TcpResponse.CreateSuccess(new { Added = added, Updated = updated },
                    $"Import hoàn tất: +{added} mới, ~{updated} cập nhật", message.RequestId);
            }
            catch (Exception ex)
            {
                return TcpResponse.CreateError($"Batch upsert error: {ex.Message}", message.RequestId);
            }
        }
    }
}