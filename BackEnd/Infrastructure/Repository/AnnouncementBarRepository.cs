using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
using OnlineShop.Infrastructure.Persistence;

namespace OnlineShop.Infrastructure.Repositories
{
    public class AnnouncementBarRepository : Repository<AnnouncementBar>, IAnnouncementBarRepository
    {
        public AnnouncementBarRepository(AppDbContext context) : base(context)
        {
        }

        public Task<AnnouncementBar?> GetCurrentAsync(DateTime utcNow, CancellationToken cancellationToken = default) =>
            Query(b => b.IsActive
                       && (b.StartsAt == null || b.StartsAt <= utcNow)
                       && (b.EndsAt == null || b.EndsAt >= utcNow))
                .OrderBy(b => b.DisplayOrder)
                .ThenByDescending(b => b.Id)
                .FirstOrDefaultAsync(cancellationToken);
    }
}
