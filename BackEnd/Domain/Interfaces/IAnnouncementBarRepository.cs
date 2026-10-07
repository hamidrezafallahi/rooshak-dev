using OnlineShop.Domain.Entities;

namespace OnlineShop.Domain.Interfaces
{
    public interface IAnnouncementBarRepository : IRepository<AnnouncementBar>
    {
        /// <summary>
        /// نوار اعلانی که همین حالا باید دیده شود (فعال و داخل بازه‌ی زمانی، کمترین DisplayOrder)؛
        /// null اگر نوار معتبری نباشد.
        /// </summary>
        Task<AnnouncementBar?> GetCurrentAsync(DateTime utcNow, CancellationToken cancellationToken = default);
    }
}
