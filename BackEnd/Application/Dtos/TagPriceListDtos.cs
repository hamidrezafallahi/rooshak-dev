namespace Application.Dtos
{
    /// <summary>One entry of the exhibition index: a tag plus how many priced products it holds.</summary>
    public class TagFamilyDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public string? CoverImage { get; set; }
    }

    /// <summary>
    /// Public price list for one tag ("family"), used by the exhibition pages.
    /// A tag is carried by ProductOffers, so prices come from the tagged offers only.
    /// </summary>
    public class TagPriceListDto
    {
        public int TagId { get; set; }
        public string TagName { get; set; } = string.Empty;
        public string TagSlug { get; set; } = string.Empty;
        public string Currency { get; set; } = "IRR";
        public int ItemCount { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<TagPriceListItemDto> Items { get; set; } = new();
    }

    public class TagPriceListItemDto
    {
        public int ProductId { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? MainImage { get; set; }
        public string? CategoryName { get; set; }
        public string? CategorySlug { get; set; }
        public string? BrandName { get; set; }
        public string? BrandSlug { get; set; }

        /// <summary>Catalog code shown on the exhibition sheet, e.g. "RSK-8435".</summary>
        public string Code { get; set; } = string.Empty;

        public decimal? Price { get; set; }
        public decimal? FinalPrice { get; set; }
        public decimal? DiscountAmount { get; set; }
        public bool? DiscountIsPercent { get; set; }
        public bool HasDiscount { get; set; }

        public int Inventory { get; set; }
        public bool InStock { get; set; }

        /// <summary>Vessel specs (قطر / ارتفاع / تعداد پارچه) as stored on ProductSpecification.</summary>
        public string? Diameter { get; set; }
        public string? Height { get; set; }
        public string? PieceCount { get; set; }

        public ProductDimensionsDto? Dimensions { get; set; }
    }
}
