using Application.Dtos;
using Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

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

        /// <summary>Vessel diameter in cm → ProductSpecification «قطر». Not perfume volume.</summary>
        public decimal? Diameter { get; set; }

        /// <summary>Set piece count for multi-piece crystal sets → ProductSpecification «تعداد پارچه».</summary>
        public int? PieceCount { get; set; }

        public string? ImageUrl { get; set; }
        public bool ImageIsMain { get; set; } = true;

        [JsonIgnore]
        public IFormFile? ImageFile { get; set; }
    }
}
