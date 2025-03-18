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
        public IQueryable<User> GetAll()
        {
            return context.Users.AsQueryable<User>();
        }
        public async Task Register(User user)
        {
            await context.AddAsync(user);
            await context.SaveChangesAsync();
        }
        public async Task Update(User user)
        {
            context.Update(user);
            await context.SaveChangesAsync();
        }
        public async Task<bool> CheckEmail(string Email)
        {
            return await context.Users.AnyAsync(user => user.Email == Email);
        }
        public async Task<bool> CheckPhoneNumber(string PhoneNumber)
        {
            return await context.Users.AnyAsync(user => user.PhoneNumber == PhoneNumber);
        }
        public async Task<bool> CheckUserName(string UserName)
        {
            return await context.Users.AnyAsync(user => user.UserName == UserName);
        }
        public async Task<User?> GetInfoById(int Id)
        {
            return await context.Users.Where(user => user.Id == Id).FirstOrDefaultAsync();
        }
    }
}
