using Application.Dtos;
using Common;
using MediatR;

namespace Application.Commands
{
    public class SetCatalogItemActiveCommand : IRequest<ServiceResult<CatalogItemActiveDto>>
    {
        public int Id { get; set; }

        public int ProductId
        {
            get => Id;
            set
            {
                if (value != 0)
                    Id = value;
            }
        }

        public bool IsActive { get; set; }
    }
}
