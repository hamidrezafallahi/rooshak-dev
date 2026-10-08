using Application.Dtos;
using Common;
using MediatR;

namespace Application.Queries
{
    public class GetProductModel3DAdminQuery : IRequest<ServiceResult<ProductModel3DAdminDto>>
    {
        public int ProductId { get; set; }
    }
}
