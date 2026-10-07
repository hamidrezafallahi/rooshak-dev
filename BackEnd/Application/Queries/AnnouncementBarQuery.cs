using Application.Dtos;
using Common;
using MediatR;

namespace Application.Queries
{
    public class GetAllAnnouncementBarsQuery : BaseListDto, IRequest<ServiceResult<ListDto<AnnouncementBarDto>>>
    {
    }

    public class GetAnnouncementBarByIdQuery : IRequest<ServiceResult<AnnouncementBarDto>>
    {
        public int Id { get; set; }
    }

    /// <summary>نواری که همین الان باید نمایش داده شود (عمومی). Data برابر null یعنی نوار معتبری نیست.</summary>
    public class GetCurrentAnnouncementBarQuery : IRequest<ServiceResult<AnnouncementBarDto>>
    {
    }
}
