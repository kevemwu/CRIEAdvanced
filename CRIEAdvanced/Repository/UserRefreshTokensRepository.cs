using CRIEAdvanced.Data;
using CRIEAdvanced.Models;
using Microsoft.EntityFrameworkCore;

namespace CRIEAdvanced.Repository
{
    public class UserRefreshTokensRepository : IUserRefreshTokensRepository
    {
        private readonly CRIEAdvanceDbContext _context;

        public UserRefreshTokensRepository(CRIEAdvanceDbContext context)
        {
            this._context = context;
        }

        public async Task<UserRefreshTokens> Create(UserRefreshTokens model)
        {
            _context.UserRefreshTokens.Add(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<UserRefreshTokens> Read(UserRefreshTokens model)
        {
            DateTime currentDate = DateTime.Now;

            var entity = await _context.UserRefreshTokens
                .FirstOrDefaultAsync(x => x.UserIP == model.UserIP && x.UserId == model.UserId && x.RefreshToken == model.RefreshToken
                                    && x.LimitDate >= currentDate && x.ResetDate == null);
            if (entity != null)
                return entity;

            return null!;
        }

        public async Task Update(UserRefreshTokens model)
        {
            DateTime currentDate = DateTime.Now;

            var entity = _context.UserRefreshTokens.
                FirstOrDefault(x => x.UserIP == model.UserIP && x.UserId == model.UserId && x.RefreshToken == model.RefreshToken
                                    && x.LimitDate >= currentDate && x.ResetDate == null);
            if (entity != null)
            {
                entity.ResetDate = DateTime.Now;

                _context.Entry(entity).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
        }
    }
}
