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

    public ICollection<ProductOfferTag> ProductOfferTags { get; private set; } = new List<ProductOfferTag>();
    public ICollection<ProductOfferDiscount> Discounts { get; private set; } = new List<ProductOfferDiscount>();

    public bool CanSell(int quantity)
        => !IsDeleted && IsActive && Inventory >= quantity;

    public static ProductOffers Create(
      int productId,
      int supplierId,
      decimal basePrice,
      int inventory,
      int currentUserId)
    {
        var offer = new ProductOffers
        {
            ProductId = productId,
            SupplierId = supplierId,
            BasePrice = basePrice,
            Inventory = inventory
        };

        offer.MarkCreated(currentUserId);
        return offer;
    }

    public void Update(
        decimal? basePrice,
        int? inventory,
        int currentUserId)
    {
        if (basePrice.HasValue)
            BasePrice = basePrice.Value;

        if (inventory.HasValue)
            Inventory = inventory.Value;

        MarkUpdated(currentUserId);
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
