using System;
using System.Collections.Generic;

namespace nuxt_shop.Models;

public partial class User
{
    public int Id { get; set; }

    public string UserName { get; set; } = null!;

    public string? FullName { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string? Address { get; set; }

    public string? Status { get; set; }

    public string? Photo { get; set; }

    public bool IsLocked { get; set; }

    public string? IdNumber { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Otp { get; set; }

    public DateTime? OtpExpired { get; set; }

    public DateTime ExpirationDate { get; set; }
}
