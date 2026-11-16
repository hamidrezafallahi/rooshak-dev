using OnlineShop.Domain.Entities;

namespace OnlineShop.Domain.Interfaces
{
    /// <summary>اسلایدر میانی صفحه‌ی اصلی (جدا از بنر هیرو).</summary>
    public interface IHomeSlideRepository : IRepository<HomeSlide>
    {
    }

    /// <summary>بخش‌های دوزبانه‌ی صفحه‌ی درباره ما / داستان ما.</summary>
    public interface IAboutSectionRepository : IRepository<AboutSection>
    {
    }

    /// <summary>فونت‌های آپلودشده توسط ادمین.</summary>
    public interface ICustomFontRepository : IRepository<CustomFont>
    {
        /// <summary>آیا فونت فعالی با این کلید (custom-{Id}) وجود دارد؟</summary>
        Task<bool> IsActiveKeyAsync(string? key, CancellationToken cancellationToken = default);
    }
}
