using System.ComponentModel.DataAnnotations;

namespace MS2.WebApp.Models
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên người nhận")]
        [StringLength(100, ErrorMessage = "Tên người nhận không được vượt quá 100 ký tự")]
        public string ReceiverName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Số điện thoại phải bắt đầu bằng 0 và có 10-11 số")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        [StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự")]
        public string DeliveryAddress { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Ghi chú không được vượt quá 200 ký tự")]
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
