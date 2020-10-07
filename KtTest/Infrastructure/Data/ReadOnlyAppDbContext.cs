using Microsoft.EntityFrameworkCore;

namespace KtTest.Infrastructure.Data
{
    public sealed class ReadOnlyAppDbContext : AppDbContext
    {
        public ReadOnlyAppDbContext(DbContextOptions options) : base(options)
        {
            ChangeTracker.LazyLoadingEnabled = false;
            ChangeTracker.AutoDetectChangesEnabled = false;
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }
    }
}
