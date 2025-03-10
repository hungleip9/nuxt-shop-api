using nuxt_shop.Models;

namespace nuxt_shop.Repositories
{
    public interface IUserLogRepository
    {
        Task AddUserLog(int userId, string description, string detail, DateTime actionDate, string ipAddress);
    }
}
