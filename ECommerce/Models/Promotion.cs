﻿using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ECommerce.Models
{
    public class Promotion
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        [ValidateNever]
        public Product Product { get; set; }

        //public string ApplicationUserId { get; set; }
        //public ApplicationUser ApplicationUser { get; set; }

        public DateTime PublishAt { get; set; } = DateTime.UtcNow;
        public DateTime ValidTo { get; set; }
        public bool IsValid { get; set; } = true;

        public string Code { get; set; }
        public decimal Discount { get; set; }
    }
}
