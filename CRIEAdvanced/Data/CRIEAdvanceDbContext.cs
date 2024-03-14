using CRIEAdvanced.Models;
using Microsoft.EntityFrameworkCore;

namespace CRIEAdvanced.Data
{
    public class CRIEAdvanceDbContext : DbContext
    {
        public CRIEAdvanceDbContext(DbContextOptions<CRIEAdvanceDbContext> options) : base(options)
        {
        }
        public virtual DbSet<Users> Users { get; set; } = null!;
        public virtual DbSet<UserRefreshTokens> UserRefreshTokens { get; set; } = null!;
    }
}
