using Application.Dtos;
using Common;
using MediatR;

namespace Application.Queries
{
    // ===== اسلایدر میانی =====
    public class GetAllHomeSlidesQuery : BaseListDto, IRequest<ServiceResult<ListDto<HomeSlideDto>>> { }

    public class GetHomeSlideByIdQuery : IRequest<ServiceResult<HomeSlideDto>>
    {
        public int Id { get; set; }
    }

    /// <summary>اسلایدهای فعال به ترتیب نمایش (عمومی).</summary>
    public class GetPublicHomeSlidesQuery : IRequest<ServiceResult<List<HomeSlideDto>>> { }

    // ===== درباره ما =====
    public class GetAllAboutSectionsQuery : BaseListDto, IRequest<ServiceResult<ListDto<AboutSectionDto>>> { }

    public class GetAboutSectionByIdQuery : IRequest<ServiceResult<AboutSectionDto>>
    {
        public int Id { get; set; }
    }

    /// <summary>بخش‌های فعال صفحه‌ی درباره ما (عمومی).</summary>
    public class GetPublicAboutSectionsQuery : IRequest<ServiceResult<List<AboutSectionDto>>> { }

    // ===== فونت‌ها =====
    public class GetAllCustomFontsQuery : BaseListDto, IRequest<ServiceResult<ListDto<CustomFontDto>>> { }

    public class GetCustomFontByIdQuery : IRequest<ServiceResult<CustomFontDto>>
    {
        public int Id { get; set; }
    }

    /// <summary>فونت‌های فعال برای ساخت @font-face در فرانت (عمومی).</summary>
    public class GetPublicCustomFontsQuery : IRequest<ServiceResult<List<CustomFontDto>>> { }
}
