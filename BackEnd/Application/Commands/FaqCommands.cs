using Common;
using MediatR;

namespace Application.Commands
{
    public class CreateFaqCommand : IRequest<ServiceResult<IdDto>>
    {
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public int? DisplayOrder { get; set; }
    }

    public class UpdateFaqCommand : IRequest<ServiceResult<IdDto>>
    {
        public int Id { get; set; }
        public string? Question { get; set; }
        public string? Answer { get; set; }
        public int? DisplayOrder { get; set; }
    }

    public class ActiveFaqCommand : ActiveCommand, IRequest<ServiceResult<IdDto>> { }

    public class DeleteFaqCommand : IRequest<ServiceResult<IdDto>>
    {
        public int Id { get; set; }
    }
}
