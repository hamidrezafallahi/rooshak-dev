using Application.Dtos;
using Common;
using MediatR;

namespace Application.Queries
{
    public class GetAllContactRequestsQuery : BaseListDto, IRequest<ServiceResult<ListDto<ContactRequestDto>>>
    {
        /// <summary>
        /// اگر مقدار داشته باشد فقط بررسی شده ها / بررسی نشده ها برگردانده می شود.
        /// </summary>
        public bool? IsReviewed { get; set; }
    }

    public class GetContactRequestByIdQuery : IRequest<ServiceResult<ContactRequestDto>>
    {
        public int Id { get; set; }
    }
}
