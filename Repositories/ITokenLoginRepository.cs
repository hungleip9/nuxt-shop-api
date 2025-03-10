using nuxt_shop.Models;

namespace nuxt_shop.Repositories
{
    public interface ITokenLoginRepository
    {
        Task Create(TokenLogin entity);
        Task<TokenLogin?> FindToken(string RefreshToken);
        Task Delete(TokenLogin entity);
    }
}
