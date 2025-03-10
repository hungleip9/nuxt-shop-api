using System;
using System.Collections.Generic;

namespace nuxt_shop.Models;

public partial class UserLog
{
    public string Id { get; set; } = null!;

    public int UserId { get; set; }

    public string Description { get; set; } = null!;

    public string? Detail { get; set; }

    public string? IpAddress { get; set; }

    public DateTime ActionDate { get; set; }
}
