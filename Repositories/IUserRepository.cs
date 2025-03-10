using nuxt_shop.Models;
using System.Linq.Expressions;

namespace nuxt_shop.Repositories
{
    public interface IUserRepository
    {
        Task Register(User user);
        Task<User?> Authenticate(string PhoneNumber, string password);
        Task<bool> CheckEmail(string Email);
        Task<bool> CheckPhoneNumber(string PhoneNumber);
        Task<bool> CheckUserName(string UserName);
        Task<User?> GetInfoById(int Id);
    }
}
