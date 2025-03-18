using System;
using System.Collections.Generic;

namespace nuxt_shop.Models;

public partial class TokenLogin
{
    public int UserId { get; set; }

    public string AccessToken { get; set; } = null!;

    public string RefreshToken { get; set; } = null!;
}
