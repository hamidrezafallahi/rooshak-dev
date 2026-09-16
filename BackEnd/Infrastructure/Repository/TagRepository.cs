using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.Persistence;

namespace OnlineShop.Infrastructure.Repositories
{
    public class TagRepository : Repository<Tag>, ITagRepository
    {
        public TagRepository(AppDbContext context) : base(context)
        {
        }


        public async Task<bool> ExistsByTagNameAsync(string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName))
                return false;

            tagName = tagName.Trim().ToLower();

            return await Query(b =>
                    b.Name.ToLower() == tagName &&
                    !b.IsDeleted &&
                    b.IsActive)
                .AnyAsync();
        }

        public async Task<bool> ExistsBySlugAsync(string slug, int? excludeTagId = null)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return false;

            var normalized = slug.Trim().ToLower();
            return await Query(t =>
                    !t.IsDeleted &&
                    t.Slug.ToLower() == normalized &&
                    (!excludeTagId.HasValue || t.Id != excludeTagId.Value))
                .AnyAsync();
        }
    }
}
