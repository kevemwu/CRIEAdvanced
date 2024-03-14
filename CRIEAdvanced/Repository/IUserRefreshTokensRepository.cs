using CRIEAdvanced.Models;

namespace CRIEAdvanced.Repository
{
    public interface IUserRefreshTokensRepository
    {
        Task<UserRefreshTokens> Create(UserRefreshTokens model);

        Task<UserRefreshTokens> Read(UserRefreshTokens model);

        Task Update(UserRefreshTokens model);

    }
}
