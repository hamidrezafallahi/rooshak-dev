using OnlineShop.Domain.Common;
using Domain.Entities;
using OnlineShop.Domain.Entities;

public class ProductOffers : BaseEntity
{
    public int ProductId { get; private set; }
    public Product Product { get; private set; }

    public int SupplierId { get; private set; }
    public User Supplier { get; private set; }

    public decimal BasePrice { get; private set; }
    public int Inventory { get; private set; }

    // ==== Variant (تنوع) ====
    // هر پیشنهاد می‌تواند یک تنوع از محصول باشد (مثلاً «بزرگنمایی» = «3.5x») با قیمت و موجودی خودش.
    // سبد خرید و سفارش به ProductOfferId وصل‌اند، پس هر تنوع بدون تغییر در آن‌ها قابل خرید است.
    public const int VariantMaxLength = 50;
    public string? VariantName { get; private set; }
    public string? VariantValue { get; private set; }

    public bool HasVariant => !string.IsNullOrWhiteSpace(VariantValue);

    /// <summary>«نام محصول - بزرگنمایی 3.5x» برای سبد خرید و سفارش؛ بدون تنوع همان نام محصول.</summary>
    public static string BuildDisplayName(string productName, string? variantName, string? variantValue)
    {
        if (string.IsNullOrWhiteSpace(variantValue))
            return productName;
        var label = string.IsNullOrWhiteSpace(variantName) ? variantValue.Trim() : $"{variantName.Trim()} {variantValue.Trim()}";
        return $"{productName} - {label}";
    }

    public ICollection<ProductOfferTag> ProductOfferTags { get; private set; } = new List<ProductOfferTag>();
    public ICollection<ProductOfferDiscount> Discounts { get; private set; } = new List<ProductOfferDiscount>();

    public bool CanSell(int quantity)
        => !IsDeleted && IsActive && Inventory >= quantity;

    public static ProductOffers Create(
      int productId,
      int supplierId,
      decimal basePrice,
      int inventory,
      int currentUserId,
      string? variantName = null,
      string? variantValue = null)
    {
        var offer = new ProductOffers
        {
            ProductId = productId,
            SupplierId = supplierId,
            BasePrice = basePrice,
            Inventory = inventory
        };
        offer.SetVariant(variantName, variantValue);

        offer.MarkCreated(currentUserId);
        return offer;
    }

    public void Update(
        decimal? basePrice,
        int? inventory,
        int currentUserId,
        string? variantName = null,
        string? variantValue = null)
    {
        if (basePrice.HasValue)
            BasePrice = basePrice.Value;

        if (inventory.HasValue)
            Inventory = inventory.Value;

        // null یعنی «تغییر نده»؛ رشتهٔ خالی یعنی «تنوع را پاک کن».
        if (variantName != null || variantValue != null)
            SetVariant(variantName ?? VariantName, variantValue ?? VariantValue);

        MarkUpdated(currentUserId);
    }

    private void SetVariant(string? variantName, string? variantValue)
    {
        var name = string.IsNullOrWhiteSpace(variantName) ? null : variantName.Trim();
        var value = string.IsNullOrWhiteSpace(variantValue) ? null : variantValue.Trim();

        if (name?.Length > VariantMaxLength || value?.Length > VariantMaxLength)
            throw new DomainException($"عنوان و مقدار تنوع حداکثر {VariantMaxLength} کاراکتر است.");
        if (name != null && value == null)
            throw new DomainException("برای تنوع، مقدار (مثلاً 3.5x) را هم وارد کنید.");

        VariantName = name;
        VariantValue = value;
    }

    public decimal GetFinalPrice(DateTime now)
    {
        var discount = Discounts
            .Where(d => d.Discount != null)
            .Select(d => d.Discount)
            .Where(d => !d.IsDeleted && d.StartDate <= now && d.EndDate >= now)
            .OrderByDescending(d => d.Priority)
            .FirstOrDefault();

        if (discount == null)
            return BasePrice;

        var amount = discount.IsPercent
            ? BasePrice * discount.Amount / 100
            : discount.Amount;

        return Math.Max(0, BasePrice - amount);
    }

    public void DecreaseInventory(int quantity, int userId)
    {
        if (!CanSell(quantity))
            throw new DomainException("موجودی این کالا کافی نیست.");

        Inventory -= quantity;
        MarkUpdated(userId);
    }

    public void increaseInventory(int quantity, int userId)
    {
        Inventory += quantity;
        MarkUpdated(userId);
    }

    public void AddTag(Tag tag, int currentUserId)
    {
        if (tag == null)
            throw new ArgumentNullException(nameof(tag));

        if (ProductOfferTags.Any(pt => pt.TagId == tag.Id))
            return;
        var newTag = ProductOfferTag.Create(this.Id, tag.Id, currentUserId);
        ProductOfferTags.Add(newTag);
        MarkUpdated(currentUserId);
    }

    public void RemoveTag(Tag tag, int currentUserId)
    {
        if (tag == null)
            throw new ArgumentNullException(nameof(tag));

        var productTag = ProductOfferTags.FirstOrDefault(pt => pt.TagId == tag.Id);
        if (productTag != null)
        {
            ProductOfferTags.Remove(productTag);
            MarkUpdated(currentUserId);
        }
    }
}
