using OnlineShop.Domain.Common;
using OnlineShop.Domain.Entities;

public class OrderItem : BaseEntity
{
    private OrderItem() { }

    public int OrderId { get; private set; }
    public Order Order { get; private set; }
    public int ProductOfferId { get; private set; }
    public ProductOffers ProductOffer { get; private set; }

    public int Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }
    public decimal TotalPrice => UnitPrice * Quantity;

    public static OrderItem Create(
        int orderId,
        int productOfferId,
        decimal unitPrice,
        int quantity,
        int currentUserId)
    {
        var item = new OrderItem
        {
            OrderId = orderId,
            ProductOfferId = productOfferId,
            UnitPrice = unitPrice,
            Quantity = quantity
        };

        item.MarkCreated(currentUserId);
        return item;
    }


    public void IncreaseQuantity(int quantity, int userId)
    {
        if (quantity <= 0)
            throw new DomainException("تعداد باید بیشتر از صفر باشد.");

        Quantity += quantity;
        MarkUpdated(userId);
    }

    public void DecreaseQuantity(int quantity, int userId)
    {
        if (quantity <= 0)
            throw new DomainException("تعداد باید بیشتر از صفر باشد.");

        if (Quantity - quantity <= 0)
        {
            MarkDeleted(userId);
            return;
        }

        Quantity -= quantity;
        MarkUpdated(userId);
    }
    public void UpdateQuantity(int quantity, int userId)
    {
        if (quantity <= 0)
            throw new DomainException("تعداد باید بیشتر از صفر باشد.");

        if (Quantity - quantity <= 0)
        {
            MarkDeleted(userId);
            return;
        }

        Quantity = quantity;
        MarkUpdated(userId);
    }
  
}
