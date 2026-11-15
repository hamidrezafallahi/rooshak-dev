using OnlineShop.Domain.Entities;

namespace Application.Dtos
{
    public class CreateProductOfferDto
    {
        public int ProductId { get; set; }
        public decimal BasePrice { get; set; }
        public int Inventory { get; set; }
        public List<int>? DiscountIds { get; set; }
    }

    public class UpdateProductOfferDto
    {
        public decimal? BasePrice { get; set; }
        public int? Inventory { get; set; }
        public bool? IsActive { get; set; }
        public List<int>? DiscountIds { get; set; }
    }

    public class ProductOfferDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductSlug { get; set; }
        public string ProductName { get; set; } = default!;
        public string ProductImage { get; set; } = default!;
        public string ProductDescription { get; set; } = default!;

        public int SupplierId { get; set; }
        public string? SupplierSlug { get; set; }
        public string SupplierName { get; set; } = default!;
        public string SupplierImage { get; set; } = default!;
        public string SupplierDesc { get; set; } = default!;

        public decimal BasePrice { get; set; }
        public decimal FinalPrice { get; set; }
        public int Inventory { get; set; }
        public bool IsActive { get; set; }
        public string? VariantName { get; set; }
        public string? VariantValue { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<DiscountDto> ActiveDiscounts { get; set; } = new();
        public List<TagDto> Tags { get; set; } = new();
    }

    public class ProductOfferDetailDto : ProductOfferDto
    {
        public List<DiscountDto> AllDiscounts { get; set; } = new();
        public ProductDto? ProductDetails { get; set; }
        public UserDto? SupplierDetails { get; set; }
    }


    public class SupplierDto
    {
        public int Id { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string FullName { get;  set; } = string.Empty;
        public string Email { get;  set; } = string.Empty;
        public string PhoneNumber { get;  set; } = string.Empty;
        public string Image { get;  set; }
        public string? UserDescription { get;  set; }

        public ICollection<UserAddress> Addresses = new HashSet<UserAddress>();

    }
    public class SupplierListDto
    {
        public List<SupplierDto> Records { get; set; } = new List<SupplierDto>();
        public string? ColumnsJson { get; set; }
        public string? ActionsJson { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    public class ProductOfferListDto
    {
        public List<ProductOfferDto> ProductOffers { get; set; } = new List<ProductOfferDto>();
    }
}
