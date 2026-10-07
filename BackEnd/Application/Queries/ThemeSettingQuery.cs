using Application.Dtos;
using Common;
using MediatR;

namespace Application.Queries
{
    public class GetAllThemeSettingsQuery : BaseListDto, IRequest<ServiceResult<ListDto<ThemeSettingDto>>>
    {
    }

    public class GetThemeSettingByIdQuery : IRequest<ServiceResult<ThemeSettingDto>>
    {
        public int Id { get; set; }
    }

    /// <summary>تم فعال سایت برای فرانت (عمومی).</summary>
    public class GetActiveThemeSettingQuery : IRequest<ServiceResult<ThemeSettingDto>>
    {
    }
}
