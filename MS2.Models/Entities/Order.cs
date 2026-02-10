using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MS2.Models.Entities;

public partial class Order
{
    [Key]
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public int? EmployeeId { get; set; }

    public DateTime OrderDate { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalAmount { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = null!;

    [StringLength(20)]
    public string OrderType { get; set; } = null!;

    [StringLength(500)]
    public string? Notes { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("OrderCustomers")]
    public virtual User Customer { get; set; } = null!;

    [ForeignKey("EmployeeId")]
    [InverseProperty("OrderEmployees")]
    public virtual User? Employee { get; set; }

    [InverseProperty("Order")]
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
