using OnlineShop.Domain.Common;
namespace OnlineShop.Domain.Entities
{

    public class CartItem : BaseEntity
    {
        private CartItem() { }

        public int CartId { get; private set; }
        public Cart Cart { get; private set; }
        public int ProductId { get; private set; }
        public Product  Product  { get; private set; }

        public int ProductOfferId { get; private set; }
        public ProductOffers ProductOffer { get; private set; }

        public int Quantity { get; private set; }

        public decimal UnitPrice { get; private set; }
        public decimal TotalPrice => UnitPrice * Quantity;

        // ===== Factory =====
        public static CartItem Create(
            int cartId,
            int productId,
            int productOfferId,
            decimal unitPrice,
            int quantity,
            int currentUserId)
        {
            if (quantity <= 0)
                throw new DomainException("تعداد باید بیشتر از صفر باشد.");

            var item = new CartItem
            {
                CartId = cartId,
                ProductId = productId,
                ProductOfferId = productOfferId,
                UnitPrice = unitPrice,
                Quantity = quantity
            };
            item.MarkCreated(currentUserId);
            return item;
        }

        public void Update(
      int productId,
      int productOfferId,
      int quantity,
      int currentUserId)
        {
            if (quantity < 0)
                throw new DomainException("تعداد باید بیشتر از صفر باشد.");
            ProductId = productId;
            ProductOfferId = productOfferId;
            Quantity = quantity;
            MarkUpdated(currentUserId);
        }
        // ===== Behavior =====
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

           

            Quantity = quantity;
            MarkUpdated(userId);
        }
       
    }
}
