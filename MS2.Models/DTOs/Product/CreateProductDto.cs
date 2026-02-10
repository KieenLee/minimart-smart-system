namespace MS2.Models.DTOs.Product;

public class CreateProductDto
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? Barcode { get; set; }
    public string? ImageUrl { get; set; }
}