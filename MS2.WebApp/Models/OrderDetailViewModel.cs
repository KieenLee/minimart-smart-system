using MS2.Models.Entities;

namespace MS2.WebApp.Models
{
    public class OrderDetailViewModel
    {
        public Order Order { get; set; } = null!;
        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public decimal TotalAmount { get; set; }
    }
}
