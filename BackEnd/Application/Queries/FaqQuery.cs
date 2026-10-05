using Application.Dtos;
using Common;
using MediatR;

namespace Application.Queries
{
    public class GetAllFaqsQuery : BaseListDto, IRequest<ServiceResult<ListDto<FaqDto>>>
    {
    }

    public class GetFaqByIdQuery : IRequest<ServiceResult<FaqDto>>
    {
        public int Id { get; set; }
    }
}
