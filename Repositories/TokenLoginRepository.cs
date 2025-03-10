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
        public async Task Create(TokenLogin entity)
        {
            context.Add(entity);
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
