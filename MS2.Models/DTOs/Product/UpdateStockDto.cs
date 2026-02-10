namespace MS2.Models.DTOs.Product;

public class UpdatePriceDto
{
    public int ProductId { get; set; }
    public decimal NewPrice { get; set; }
}