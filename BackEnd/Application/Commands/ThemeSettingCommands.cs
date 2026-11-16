using Common;
using MediatR;

namespace Application.Commands
{
    public class CreateThemeSettingCommand : IRequest<ServiceResult<IdDto>>
    {
        public string Name { get; set; } = string.Empty;
        public string? PrimaryColor { get; set; }
        public string? SecondaryColor { get; set; }
        public string? HighlightColor { get; set; }
        public string? NeutralColor { get; set; }
        public string? SuccessColor { get; set; }
        public string? ErrorColor { get; set; }
        public string? WarningColor { get; set; }
        public string? InfoColor { get; set; }
        public string? SurfaceColor { get; set; }
        public string? SurfaceMutedColor { get; set; }
        public string? BorderColor { get; set; }
        public string? TextColor { get; set; }
        public string? TextMutedColor { get; set; }
        /// <summary>کلید فونت اصلی (مثلاً vazirmatn)؛ خالی یعنی بدون تغییر.</summary>
        public string? FontFamily { get; set; }
        /// <summary>کلید فونت تیترها؛ خالی یعنی بدون تغییر.</summary>
        public string? HeadingFontFamily { get; set; }
        /// <summary>small / medium / large / custom؛ خالی یعنی بدون تغییر.</summary>
        public string? BodyTextSize { get; set; }
        public int? BodyTextSizePx { get; set; }
        public string? HeadingTextSize { get; set; }
        public int? HeadingTextSizePx { get; set; }
        /// <summary>100..900 (multiple of 100); null = unchanged.</summary>
        public int? BodyFontWeight { get; set; }
        public int? HeadingFontWeight { get; set; }
    }

    public class UpdateThemeSettingCommand : IRequest<ServiceResult<IdDto>>
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? PrimaryColor { get; set; }
        public string? SecondaryColor { get; set; }
        public string? HighlightColor { get; set; }
        public string? NeutralColor { get; set; }
        public string? SuccessColor { get; set; }
        public string? ErrorColor { get; set; }
        public string? WarningColor { get; set; }
        public string? InfoColor { get; set; }
        public string? SurfaceColor { get; set; }
        public string? SurfaceMutedColor { get; set; }
        public string? BorderColor { get; set; }
        public string? TextColor { get; set; }
        public string? TextMutedColor { get; set; }
        /// <summary>کلید فونت اصلی (مثلاً vazirmatn)؛ خالی یعنی بدون تغییر.</summary>
        public string? FontFamily { get; set; }
        /// <summary>کلید فونت تیترها؛ خالی یعنی بدون تغییر.</summary>
        public string? HeadingFontFamily { get; set; }
        /// <summary>small / medium / large / custom؛ خالی یعنی بدون تغییر.</summary>
        public string? BodyTextSize { get; set; }
        public int? BodyTextSizePx { get; set; }
        public string? HeadingTextSize { get; set; }
        public int? HeadingTextSizePx { get; set; }
        /// <summary>100..900 (multiple of 100); null = unchanged.</summary>
        public int? BodyFontWeight { get; set; }
        public int? HeadingFontWeight { get; set; }
    }

    /// <summary>فعال‌کردن یک تم، بقیه‌ی تم‌ها را غیرفعال می‌کند (فقط یک تم فعال).</summary>
    public class ActiveThemeSettingCommand : ActiveCommand, IRequest<ServiceResult<IdDto>> { }

    public class DeleteThemeSettingCommand : IRequest<ServiceResult<IdDto>>
    {
        public int Id { get; set; }
    }
}
