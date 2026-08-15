using Application.Dtos;
using Common;
using MediatR;

namespace Application.Commands
{
    public class CreateCatalogItemCommand : ProductDimensionsDto, IRequest<ServiceResult<CatalogItemIdsDto>>
    {
        public string Name { get; set; } = default!;
        public string? Slug { get; set; }
        public string Description { get; set; } = default!;
        public string? SeoTitleFa { get; set; }
        public string? SeoTitleEn { get; set; }
        public string? MetaDescriptionFa { get; set; }
        public string? MetaDescriptionEn { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }

        public decimal BasePrice { get; set; }
        public int Inventory { get; set; }

        public string? ImageUrl { get; set; }
        public bool ImageIsMain { get; set; } = true;
    }
}
