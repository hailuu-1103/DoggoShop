using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebRazor.Models
{
    public partial class Customer
    {
        public Customer()
        {
            Accounts = new HashSet<Account>();
            Orders = new HashSet<Order>();
        }

        [ValidateNever]
        public string CustomerId { get; set; } = null!;
        [Required(ErrorMessage = "CompanyName is required")]
        public string CompanyName { get; set; } = null!;
        [Required(ErrorMessage = "ContactName is required")]
        public string? ContactName { get; set; }
        [Required(ErrorMessage = "ContactTitle is required")]
        public string? ContactTitle { get; set; }
        [Required(ErrorMessage = "Address is required")]
        public string? Address { get; set; }

        public DateTime? CreatedAt { get; set; }
        public bool? Active { get; set; }

        public virtual ICollection<Account> Accounts { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}
