using OnlineShop.Domain.Common;

using System;

namespace OnlineShop.Domain.Entities
{
    public class Cart : BaseEntity
    {
        private Cart() { }

        public int UserId { get; private set; }
        public User User { get; private set; } = default!;

        public CartStatus Status { get; private set; } = CartStatus.Active;

        public ICollection<CartItem> Items { get; private set; } = new HashSet<CartItem>();

        public decimal TotalPrice => Items.Sum(i => i.TotalPrice);
        // ===== Factory Method =====
        public static Cart Create(int userId, List<CartItem> items, int currentUserId)
        {
            var cart = new Cart
            {
                UserId = userId,
                Status = CartStatus.Active
            };
            cart.MarkCreated(currentUserId);

            foreach (var item in items)
            {
                cart.AddItem(item.ProductId,item.ProductOfferId, item.Quantity,item.UnitPrice, currentUserId);
            }

            return cart;
        }

        // ===== Behavior Methods =====
        public void AddItem(int productId,int productOfferId, int quantity, decimal unitPrice, int currentUserId)
        {
            if (Status != CartStatus.Active)
                throw new DomainException("این سبد خرید دیگر فعال نیست و نمی‌توان کالایی به آن اضافه کرد.");

            var existingItem = Items.FirstOrDefault(i => i.ProductOfferId == productOfferId);
            if (existingItem is null)
            {
                Items.Add(CartItem.Create(
                    cartId: this.Id,
                    productId: productId,
                    productOfferId: productOfferId,
                    unitPrice: unitPrice,
                    quantity: quantity,
                    currentUserId: currentUserId
                ));
            }
            else
            {
                existingItem.IncreaseQuantity(quantity, currentUserId);
            }

            MarkUpdated(currentUserId);
        }

        public void UpdateItems(List<CartItem> items, int currentUserId)
        {
            var itemsToRemove = Items
                .Where(i => !items.Any(newItem => newItem.ProductOfferId == i.ProductOfferId))
                .ToList();

            foreach (var item in itemsToRemove)
            {
                item.Delete(currentUserId);
                Items.Remove(item);
            }

            foreach (var newItem in items)
            {
                var existingItem = Items.FirstOrDefault(i => i.ProductOfferId == newItem.ProductOfferId);

                if (existingItem == null)
                {
                    var createdItem = CartItem.Create(
                        cartId: newItem.CartId,
                        productId: newItem.ProductId,
                        productOfferId: newItem.ProductOfferId,
                        unitPrice: newItem.UnitPrice,
                        quantity: newItem.Quantity,
                        currentUserId: currentUserId
                    );
                    Items.Add(createdItem);
                }
                else
                {
                    existingItem.UpdateQuantity(newItem.Quantity, currentUserId);
                }
            }

            MarkUpdated(currentUserId);
        }

        public void RemoveItem(int productOfferId, int currentUserId)
        {
            var item = Items.FirstOrDefault(i => i.ProductOfferId == productOfferId);
            if (item is null)
                throw new DomainException("این کالا در سبد خرید پیدا نشد.");

            Items.Remove(item);
            MarkUpdated(currentUserId);
        }

        public void Checkout(int currentUserId)
        {
            if (!Items.Any(i => !i.IsDeleted))
                throw new DomainException("سبد خرید شما خالی است و نمی‌توان سفارش ثبت کرد.");

            Status = CartStatus.Checkout;
            MarkUpdated(currentUserId);
        }

        public void MarkAsPaid(int currentUserId)
        {
            if (Status != CartStatus.Checkout)
                throw new DomainException("فقط سبدی که در مرحلهٔ تسویه است می‌تواند پرداخت‌شده ثبت شود.");

            Status = CartStatus.Paid;
            MarkUpdated(currentUserId);
        }

        public void Cancel(int currentUserId)
        {
            if (Status == CartStatus.Paid)
                throw new DomainException("سبد خریدی که پرداخت شده قابل لغو نیست.");

            Status = CartStatus.Cancelled;
            MarkUpdated(currentUserId);
        }

        
        
    }

    public enum CartStatus
    {
        Active = 0,
        Checkout = 1,
        Paid = 2,
        Cancelled = 3
    }
}
