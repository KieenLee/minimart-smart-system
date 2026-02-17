using MS2.Models.Entities;

namespace MS2.WebApp.Models
{
    public class OrderHistoryViewModel
    {
        public List<Order> Orders { get; set; } = new List<Order>();

        // Pagination
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 10;
    }
}
