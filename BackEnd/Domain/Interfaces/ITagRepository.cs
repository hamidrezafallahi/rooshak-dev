using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;

namespace Domain.Interfaces
{
    public interface ITagRepository : IRepository<Tag>
    {
        Task<bool> ExistsByTagNameAsync(string tagName);
        Task<bool> ExistsBySlugAsync(string slug, int? excludeTagId = null);
    }
}
