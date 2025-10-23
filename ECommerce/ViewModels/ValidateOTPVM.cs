﻿using System.ComponentModel.DataAnnotations;

namespace ECommerce.ViewModels
{
    public class ValidateOTPVM
    {
        public int Id { get; set; }

        [Required]
        public string OTP { get; set; } = string.Empty;

        public string ApplicationUserId { get; set; } = string.Empty;
    }
}
