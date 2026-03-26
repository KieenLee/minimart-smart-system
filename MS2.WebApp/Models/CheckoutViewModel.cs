using System.ComponentModel.DataAnnotations;

namespace MS2.WebApp.Models
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên người nhận")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên người nhận phải từ 2-100 ký tự")]
        [Display(Name = "Tên người nhận")]
        public string ReceiverName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Số điện thoại phải bắt đầu bằng 0 và có 10-11 số")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Địa chỉ giao hàng phải từ 10-500 ký tự")]
        [Display(Name = "Địa chỉ giao hàng")]
        public string DeliveryAddress { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Ghi chú không được vượt quá 200 ký tự")]
        [Display(Name = "Ghi chú")]
        public string? Note { get; set; }

        // Thông tin giỏ hàng (hiển thị)
        public List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();
        public decimal TotalAmount { get; set; }

        // Helper method để tạo chuỗi Notes cho Order
        public string GetOrderNotes()
        {
            var notes = $"Người nhận: {ReceiverName}\nSĐT: {PhoneNumber}\nĐịa chỉ: {DeliveryAddress}";
            if (!string.IsNullOrEmpty(Note))
            {
                notes += $"\nGhi chú: {Note}";
            }
            return notes;
        }
    }
}
