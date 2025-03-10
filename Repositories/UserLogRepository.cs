using nuxt_shop.Models;

namespace nuxt_shop.Repositories
{
    public class UserLogRepository : IUserLogRepository
    {
        private NuxtShopApiDbContext context;
        public UserLogRepository(NuxtShopApiDbContext _context)
        {
            this.context = _context;
        }
        public async Task AddUserLog(int userId, string description, string detail, DateTime actionDate, string ipAddress)
        {
            var userLog = new UserLog
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Description = description,
                Detail = detail,
                ActionDate = actionDate,
                IpAddress = ipAddress,
            };
            context.Add(userLog);
            await context.SaveChangesAsync();
        }
    }
}
