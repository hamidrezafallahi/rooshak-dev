using OnlineShop.Domain.Entities;

namespace OnlineShop.Domain.Interfaces
{
    public interface IThemeSettingRepository : IRepository<ThemeSetting>
    {
        /// <summary>تم فعال سایت (جدیدترین رکورد فعال)؛ null اگر هیچ تمی فعال نباشد.</summary>
        Task<ThemeSetting?> GetActiveAsync(CancellationToken cancellationToken = default);
    }
}
