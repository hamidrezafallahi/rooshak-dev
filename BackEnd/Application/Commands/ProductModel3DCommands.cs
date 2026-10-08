using Application.Dtos;
using Common;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Commands
{
    /// <summary>Creates or updates the 3D model of a product. Only the provided files are replaced.</summary>
    public class UpsertProductModel3DCommand : IRequest<ServiceResult<ProductModel3DAdminDto>>
    {
        public int ProductId { get; set; }
        public IFormFile? Model { get; set; }
        public IFormFile? Usdz { get; set; }
        /// <summary>Remove the iOS USDZ file without touching the GLB.</summary>
        public bool RemoveUsdz { get; set; }
    }

    public class DeleteProductModel3DCommand : IRequest<ServiceResult<IdDto>>
    {
        public int ProductId { get; set; }
    }

    public class AddProductScanSourcesCommand : IRequest<ServiceResult<ProductModel3DAdminDto>>
    {
        public int ProductId { get; set; }
        public List<IFormFile> Files { get; set; } = new();
    }

    public class DeleteProductScanSourceCommand : IRequest<ServiceResult<IdDto>>
    {
        public int Id { get; set; }
    }
}
