using CRIEAdvanced.Data;
using CRIEAdvanced.Models;
using CRIEAdvanced.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CRIEAdvanced.Repository
{
    public class UsersRepository
    {
        private readonly CRIEAdvanceDbContext _context;
        public UsersRepository(CRIEAdvanceDbContext context)
        {
            this._context = context;
            this._context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public async Task<Users?> ValidateUser(ValidateUser model)
        {
            try
            {
                FormattableString? sqlStr = null;

                sqlStr = $@"SELECT [Id], [UserID], [UserPassword]
                            FROM [Users]
                            WHERE [UserID] = {model.Account} AND [UserPassword] = {model.Password}";

                var result = await _context.Users
                    .FromSqlInterpolated(sqlStr)
                    .FirstOrDefaultAsync();

                if (result != null)
                    return result;

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
