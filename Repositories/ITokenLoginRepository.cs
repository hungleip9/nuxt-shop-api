using nuxt_shop.Models;

namespace nuxt_shop.Repositories
{
    public interface ITokenLoginRepository
    {
        IQueryable<TokenLogin> GetAll();
        Task Create(TokenLogin entity);
        Task Update(TokenLogin entity);
        Task<TokenLogin?> FindToken(string RefreshToken);
        Task Delete(TokenLogin entity);
    }
}
