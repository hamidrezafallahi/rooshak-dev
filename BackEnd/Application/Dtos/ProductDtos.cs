using Domain.Entities;
using Microsoft.AspNetCore.Http;
using OnlineShop.Domain.Entities;

namespace Application.Dtos
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Slug { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string? SeoTitleFa { get; set; }
        public string? SeoTitleEn { get; set; }
        public string? MetaDescriptionFa { get; set; }
        public string? MetaDescriptionEn { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } 

        public int? BrandId { get; set; }
        public string BrandName { get; set; }

        public bool IsActive { get; set; }

        public string? MainImage { get; set; }
        public ProductDimensionsDto? Dimentions { get; set; }

    }


    public class ProductByDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Slug { get; set; } = default!;
        public string Description { get; set; } = default!;
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int? BrandId { get; set; }
        public string? BrandName { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
        public string? MainImage { get; set; }
        public ProductDimensionsDto? Dimensions { get; set; }
        public decimal? Width { get; set; } = 0;
        public decimal? Height { get; set; } = 0;
        public decimal? Depth { get; set; } = 0;
        public decimal? Weight { get; set; } = 0;

        /// <summary>Best (lowest) active offer base price.</summary>
        public decimal? Price { get; set; }
        /// <summary>Best offer price after discounts.</summary>
        public decimal? FinalPrice { get; set; }
        public bool? DiscountIsPercent { get; set; }
        public decimal? DiscountAmount { get; set; }
        public int? BestOfferId { get; set; }
        public int Inventory { get; set; }
        public bool InStock { get; set; }
        public string Currency { get; set; } = "IRR";
        public double AverageRate { get; set; }
        public int RateCount { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? SeoTitleFa { get; set; }
        public string? SeoTitleEn { get; set; }
        public string? MetaDescriptionFa { get; set; }
        public string? MetaDescriptionEn { get; set; }
        public string? BrandSlug { get; set; }
        public string? CategorySlug { get; set; }

        /// <summary>3D model for the storefront viewer / AR; null when the product has none.</summary>
        public ProductModel3DPublicDto? Model3D { get; set; }
    }

    public class ProductCardDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string? MainImage { get; set; }

    }
    public class ProductByDetailForSpecialsDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public int? BrandId { get; set; }
        public int? DiscountId { get; set; }
        public int ProductId { get; set; }
        public int ProductOfferId { get; set; }
        public string? DiscountName { get; set; }

        public bool? DiscountIsPercent { get; set; }
        public decimal? DiscountAmount { get; set; }

        // قیمت محاسبه شده نهایی (با تخفیف)
        public decimal FinalPrice { get; set; }

        public List<string> ImageUrls { get; set; } = new List<string>();
        public List<TagDto> Tags { get; set; } = new List<TagDto>();

        public string? MainImage { get; set; }
        public double AverageRate { get; set; }
        public int RateCount { get; set; }
        public SpecialOffersDto specialOfferData { get; set; }
        public ProductDimensionsDto? Dimensions { get; set; }
        public UserDto SupplierExtended { get; set; }


    }
    public class ProductSpecialsByIdDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        
        public List<ProductSpecificationDto> Specifications { get; set; } = new();
        


    }
    public class IdDto
    {
        public int Id { get; set; }
    }


    public class TheMostProductDto
    {
        public int Id { get; set; }
        public string Slug { get; set; } = default!;
        public int BestOfferId { get; set; }

        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public decimal Price { get; set; }
        public int Inventory { get; set; }
        public string? PersianCategoryName { get; set; }
        public string? EnglishCategoryName { get; set; }
        public string? Brand  { get; set; }
        public bool? DiscountIsPercent { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal FinalPrice { get; set; }
        public string? MainImage { get; set; }
        public double AverageRate { get; set; }
        public int RateCount { get; set; }



    }




    public class ProductImageDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public IFormFile? ProductImageFile { get; set; }
        public bool IsMain { get; set; }
    }

    public class ProductDimensionsDto
    {
        public decimal? Width { get; set; } = 0;
        public decimal? Height { get; set; } = 0;
        public decimal? Depth { get; set; } = 0;
        public decimal? Weight { get; set; } = 0;
    }
    public class ProductSpecificationDto
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;

    }


    public class ProductRelatinalToUserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;

        public decimal Price { get; set; }
        public int? DiscountId { get; set; }
        public bool? DiscountIsPercent { get; set; }

        public decimal? DiscountAmount { get; set; }
        public decimal FinalPrice { get; set; }
        public string? MainImage { get; set; }

        public BrandDto Brand { get; set; } = new();
        public CategoryDto Category { get; set; } = new();

    }

    public class ProductCardBySupplierDto : ProductCardDto
    {

        public List<SupplierDto> suppliers { get; set; }
    }



}
