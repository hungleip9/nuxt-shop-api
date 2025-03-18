using nuxt_shop.Models;
using System.Linq.Expressions;

namespace nuxt_shop.Repositories
{
    public interface IUserRepository
    {
        IQueryable<User> GetAll();
        Task Register(User user);
        Task Update(User user);
        Task<bool> CheckEmail(string Email);
        Task<bool> CheckPhoneNumber(string PhoneNumber);
        Task<bool> CheckUserName(string UserName);
        Task<User?> GetInfoById(int Id);
    }
}
