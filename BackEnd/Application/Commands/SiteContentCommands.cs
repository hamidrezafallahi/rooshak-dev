using Common;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Commands
{
    // ===== اسلایدر میانی صفحه‌ی اصلی (جدا از بنر هیرو) =====
    public class CreateHomeSlideCommand : IRequest<ServiceResult<IdDto>>
    {
        public IFormFile? ImageUrl { get; set; }
        public IFormFile? MobileImageUrl { get; set; }
        public string? LinkUrl { get; set; }
        public string? TitleFa { get; set; }
        public string? TitleEn { get; set; }
        public int? DisplayOrder { get; set; }
    }

    public class UpdateHomeSlideCommand : IRequest<ServiceResult<IdDto>>
    {
        public int Id { get; set; }
        public IFormFile? ImageUrl { get; set; }
        public IFormFile? MobileImageUrl { get; set; }
        public bool RemoveMobileImage { get; set; }
        public string? LinkUrl { get; set; }
        public string? TitleFa { get; set; }
        public string? TitleEn { get; set; }
        public int? DisplayOrder { get; set; }
    }

    public class ActiveHomeSlideCommand : ActiveCommand, IRequest<ServiceResult<IdDto>> { }

    public class DeleteHomeSlideCommand : IRequest<ServiceResult<IdDto>>
    {
        public int Id { get; set; }
    }

    // ===== درباره ما / داستان ما =====
    public class UpdateAboutSectionCommand : IRequest<ServiceResult<IdDto>>
    {
        public int Id { get; set; }
        public int? DisplayOrder { get; set; }
        public string? EyebrowFa { get; set; }
        public string? EyebrowEn { get; set; }
        public string? TitleFa { get; set; }
        public string? TitleEn { get; set; }
        public string? BodyFa { get; set; }
        public string? BodyEn { get; set; }
        public string? LinkTextFa { get; set; }
        public string? LinkTextEn { get; set; }
        public string? LinkUrl { get; set; }
    }

    public class ActiveAboutSectionCommand : ActiveCommand, IRequest<ServiceResult<IdDto>> { }

    // ===== فونت‌های آپلودی =====
    public class CreateCustomFontCommand : IRequest<ServiceResult<IdDto>>
    {
        public IFormFile? FontFile { get; set; }
        public string? NameFa { get; set; }
        public string? NameEn { get; set; }
        /// <summary>general / formal / fancy</summary>
        public string? Category { get; set; }
    }

    public class UpdateCustomFontCommand : IRequest<ServiceResult<IdDto>>
    {
        public int Id { get; set; }
        public IFormFile? FontFile { get; set; }
        public string? NameFa { get; set; }
        public string? NameEn { get; set; }
        public string? Category { get; set; }
    }

    public class ActiveCustomFontCommand : ActiveCommand, IRequest<ServiceResult<IdDto>> { }

    public class DeleteCustomFontCommand : IRequest<ServiceResult<IdDto>>
    {
        public int Id { get; set; }
    }
}
