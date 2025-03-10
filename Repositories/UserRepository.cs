using Microsoft.EntityFrameworkCore;
using nuxt_shop.Models;
using System.Data;
using System.Linq.Expressions;

namespace nuxt_shop.Repositories
{
    public class UserRepository : IUserRepository
    {
        private NuxtShopApiDbContext context;
        public UserRepository(NuxtShopApiDbContext _context)
        {
            this.context = _context;
        }
        public async Task Register(User user)
        {
            context.Add(user);
            await context.SaveChangesAsync();
        }

        public async Task<User?> Authenticate(string PhoneNumber, string password)
        {
            var user = await context.Users.Where(u => u.PhoneNumber == PhoneNumber).FirstOrDefaultAsync();
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password)) return null;
            return user;
        }
        public async Task<bool> CheckEmail(string Email)
        {
            return await context.Users.AnyAsync(user => user.Email == Email);
        }
        public async Task<bool> CheckPhoneNumber(string PhoneNumber)
        {
            return await context.Users.AnyAsync(user => user.Email == PhoneNumber);
        }
        public async Task<bool> CheckUserName(string UserName)
        {
            return await context.Users.AnyAsync(user => user.Email == UserName);
        }
        public async Task<User?> GetInfoById(int Id)
        {
            return await context.Users.Where(user => user.Id == Id).FirstOrDefaultAsync();
        }
    }
}
