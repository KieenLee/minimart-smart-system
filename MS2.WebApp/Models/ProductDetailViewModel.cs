using MS2.Models.Entities;

namespace MS2.WebApp.Models
{
    public class ProductDetailViewModel
    {
        public Product Product { get; set; } = null!;
        public Category? Category { get; set; }
        public List<Product> RelatedProducts { get; set; } = new List<Product>();
    }
}
