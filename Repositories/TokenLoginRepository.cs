using Azure;
using Microsoft.EntityFrameworkCore;
using nuxt_shop.Models;

namespace nuxt_shop.Repositories
{
    public class TokenLoginRepository : ITokenLoginRepository
    {
        private NuxtShopApiDbContext context;
        public TokenLoginRepository(NuxtShopApiDbContext _context)
        {
            this.context = _context;
        }
        public IQueryable<TokenLogin> GetAll()
        {
            return context.TokenLogins.AsQueryable<TokenLogin>();
        }
        public async Task Create(TokenLogin entity)
        {
            await context.AddAsync(entity);
            await context.SaveChangesAsync();
        }
        public async Task Update(TokenLogin entity)
        {
            context.Update(entity);
            await context.SaveChangesAsync();
        }
        public async Task<TokenLogin?> FindToken(string RefreshToken)
        {
            return await context.TokenLogins.Where(u => u.RefreshToken == RefreshToken).FirstOrDefaultAsync();
        }
        public async Task Delete(TokenLogin entity)
        {
            context.Remove(entity);
        }
    }
}
