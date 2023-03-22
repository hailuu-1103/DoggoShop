using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebRazor.Models
{
    public partial class Product
    {
        public Product()
        {
            OrderDetails = new HashSet<OrderDetail>();
        }

        public int ProductId { get; set; }

        [Required(ErrorMessage = "ProductName is required")]
        public string ProductName { get; set; } = null!;

        [Required(ErrorMessage = "Category is required")]
        public int? CategoryId { get; set; }
        public string? QuantityPerUnit { get; set; }

        [Range(0, Double.MaxValue)]
        public decimal? UnitPrice { get; set; }

        [Required(ErrorMessage = "Units in stock is required")]
        [Range(0, short.MaxValue)]
        public short? UnitsInStock { get; set; }

        [Range(0, short.MaxValue)]
        public short? UnitsOnOrder { get; set; }
        public short? ReorderLevel { get; set; }
        public bool Discontinued { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual Category? Category { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
