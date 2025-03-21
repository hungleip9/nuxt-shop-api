using System.Security.Claims;
namespace nuxt_shop.Extensions
{
    public static class CommonExtensions
    {
        public static int GetCustomerId(this ClaimsPrincipal user)
        {
            var claim = user.Claims.FirstOrDefault(item => item.Type == ClaimTypes.NameIdentifier);
            return int.TryParse(claim?.Value, out var id) ? id : 0;
        }
        public static DateTime GetCustomerExpireDate(this ClaimsPrincipal user)
        {
            var claim = user.Claims.FirstOrDefault(item => item.Type == ClaimTypes.Expiration);
            return DateTime.TryParse(claim?.Value, out var date) ? date : DateTime.MinValue;
        }
        public static string GetIpAddress(this HttpRequest request)
        {
            var ip = request.Headers["X-Forwarded-For"].ToString() ?? request.HttpContext.Connection.RemoteIpAddress?.ToString();
            return string.IsNullOrEmpty(ip) ? "Khong lay duoc ip" : ip;
        }
    }
}
