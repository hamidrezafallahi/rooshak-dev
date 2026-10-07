using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
using OnlineShop.Infrastructure.Persistence;

namespace OnlineShop.Infrastructure.Repositories
{
    public class ThemeSettingRepository : Repository<ThemeSetting>, IThemeSettingRepository
    {
        public ThemeSettingRepository(AppDbContext context) : base(context)
        {
        }

        public Task<ThemeSetting?> GetActiveAsync(CancellationToken cancellationToken = default) =>
            Query(t => t.IsActive)
                .OrderByDescending(t => t.UpdatedAt ?? t.CreatedAt)
                .ThenByDescending(t => t.Id)
                .FirstOrDefaultAsync(cancellationToken);
    }
}
