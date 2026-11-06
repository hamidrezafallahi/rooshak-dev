using OnlineShop.Domain.Common;
using Domain.Enums;

using OnlineShop.Domain.Enums;

namespace OnlineShop.Domain.Entities
{
    public class Order : BaseEntity
    {
        private Order() { }


        public int UserId { get; private set; }
        public User User { get; private set; } = default!;
        public DateTime OrderDate { get; private set; }
        public OrderStatus Status { get; private set; } = OrderStatus.Pending;

        public ICollection<OrderItem> Items { get; private set; } = new HashSet<OrderItem>();
        public ICollection<Payment> Payments { get; private set; } = new HashSet<Payment>();
        public int? DiscountCodeId { get; private set; }
        public DiscountCode? DiscountCode { get; private set; }


        // ===================== جزئیات سفارش =====================
        public int ShippingAddressId { get; private set; }
        public UserAddress ShippingAddress { get; private set; } = default!;
        public int ShippingMethodId { get; private set; }
        public ShippingMethod ShippingMethod { get; private set; } = default!;
        public int PaymentMethodId { get; private set; }
        public PaymentMethod PaymentMethod { get; private set; }
        public decimal TotalPrice { get; private set; }
        public decimal ShippingCost { get; private set; }
        public decimal DiscountAmount { get; private set; }
        public decimal FinalPrice { get; private set; }
        public string? TrackingCode { get; private set; }

        // ===================== Factory =====================
        public static Order Create(
      int currentUserId,
      int ShippingAddressId,
      int shippingMethodId,
      int paymentMethodId,
      decimal? shippingCost,
      decimal? discountAmount,
      int? discountCodeId
  )
        {
            var order = new Order
            {
                UserId = currentUserId,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                ShippingAddressId = ShippingAddressId,
                ShippingMethodId = shippingMethodId,
                PaymentMethodId = paymentMethodId,
                ShippingCost = shippingCost ?? 0m,
                DiscountAmount = discountAmount ?? 0m,
                DiscountCodeId= discountCodeId ?? null
            };

            order.RecalculateFinalPrice();
            order.MarkCreated(currentUserId);
            return order;
        }
        public void Update(
     int? shippingAddressId,
     int? shippingMethodId,
     int? paymentMethodId,
     decimal? shippingCost,
     decimal? discountAmount,
     string? trackingCode,
     int currentUserId)
        {
            if (shippingAddressId.HasValue)
                this.ShippingAddressId = shippingAddressId.Value;

            if (shippingMethodId.HasValue)
                this.ShippingMethodId = shippingMethodId.Value;

            if (paymentMethodId.HasValue)
                this.PaymentMethodId = paymentMethodId.Value;

            if (shippingCost.HasValue)
                ShippingCost = shippingCost.Value;

            if (discountAmount.HasValue)
                DiscountAmount = discountAmount.Value;
            if (!string.IsNullOrEmpty(trackingCode))
                TrackingCode = trackingCode;
            RecalculateFinalPrice();
            MarkUpdated(currentUserId);
        }


        // ===================== Behaviors =====================
        public void AddItem(ProductOffers offer,int quantity, int currentUserId)
        {
            if (Status != OrderStatus.Pending)
                throw new DomainException("بعد از تأیید سفارش نمی‌توان کالایی به آن اضافه کرد.");

            if (quantity <= 0)
                throw new DomainException("تعداد باید بیشتر از صفر باشد.");

            var existing = Items.FirstOrDefault(i => i.OrderId == offer.Id);
            if (existing != null)
            {
                existing.IncreaseQuantity(quantity, currentUserId);
            }
            else
            {
                var item = OrderItem.Create(Id, offer.Id,offer.BasePrice, quantity, currentUserId);
                Items.Add(item);
            }

            RecalculateFinalPrice();
            MarkUpdated(currentUserId);
        }

        public void RemoveItem(int productId, int currentUserId)
        {
            if (Status != OrderStatus.Pending)
                throw new DomainException("بعد از تأیید سفارش نمی‌توان کالایی از آن حذف کرد.");

            var item = Items.FirstOrDefault(i => i.ProductOfferId == productId);
            if (item == null)
                throw new DomainException("این کالا در سفارش پیدا نشد.");

            Items.Remove(item);
            RecalculateFinalPrice();
            MarkUpdated(currentUserId);
        }

        public void ApplyDiscount(decimal discountAmount, int currentUserId)
        {
            if (discountAmount < 0)
                throw new DomainException("مبلغ تخفیف نمی‌تواند منفی باشد.");

            DiscountAmount = discountAmount;
            RecalculateFinalPrice();
            MarkUpdated(currentUserId);
        }
        public void ApplyDiscountCode(DiscountCode code, int currentUserId)
        {
            if (Status != OrderStatus.Pending)
                throw new DomainException("بعد از تأیید سفارش نمی‌توان کد تخفیف اعمال کرد.");

            if (code == null)
                throw new DomainException("کد تخفیف نامعتبر است.");

            if (!code.IsValid)
                throw new DomainException("کد تخفیف معتبر نیست؛ ممکن است منقضی شده یا ظرفیت آن تمام شده باشد.");

            if (code.UserId != null && code.UserId != UserId)
                throw new DomainException("این کد تخفیف متعلق به حساب شما نیست.");

            DiscountCodeId = code.Id;

            // محاسبه مقدار تخفیف بر اساس قیمت کل + تخفیف محصول
            var discountAmount = code.IsPercent
                ? (TotalPrice * code.Amount / 100)
                : code.Amount;

            if (discountAmount > TotalPrice)
                discountAmount = TotalPrice;

            DiscountAmount = discountAmount;

            RecalculateFinalPrice();
            MarkUpdated(currentUserId);
        }
        public void RemoveDiscountCode(int currentUserId)
        {
            DiscountCodeId = null;
            DiscountAmount = 0;
            RecalculateFinalPrice();
            MarkUpdated(currentUserId);
        }


        public void SetTrackingCode(string trackingCode, int currentUserId)
        {
            TrackingCode = trackingCode;
            MarkUpdated(currentUserId);
        }

        // ===================== Status Changes =====================
        public void Confirm(int currentUserId)
        {
            if (!Items.Any())
                throw new DomainException("سفارش باید حداقل یک کالا داشته باشد.");

            if (Status != OrderStatus.Pending)
                throw new DomainException("فقط سفارش‌های در انتظار تأیید قابل تأیید هستند.");

            Status = OrderStatus.Confirmed;
            MarkUpdated(currentUserId);
        }

        public void Pay(int currentUserId)
        {
            if (Status != OrderStatus.Confirmed)
                throw new DomainException("سفارش باید پیش از پرداخت تأیید شود.");

            Status = OrderStatus.Paid;
            MarkUpdated(currentUserId);
        }

        public void Ship(int currentUserId)
        {
            if (Status != OrderStatus.Paid)
                throw new DomainException("سفارش باید پیش از ارسال پرداخت شود.");

            Status = OrderStatus.Shipped;
            MarkUpdated(currentUserId);
        }

        public void Deliver(int currentUserId)
        {
            if (Status != OrderStatus.Shipped)
                throw new DomainException("سفارش باید پیش از تحویل ارسال شود.");

            Status = OrderStatus.Delivered;
            MarkUpdated(currentUserId);
        }

        public void Cancel(int currentUserId)
        {
            if (Status == OrderStatus.Paid || Status == OrderStatus.Shipped || Status == OrderStatus.Delivered)
                throw new DomainException("سفارشی که پرداخت یا ارسال شده باشد قابل لغو نیست.");

            Status = OrderStatus.Cancelled;
            MarkUpdated(currentUserId);
        }

        public void DeleteOrder(int currentUserId)
        {
            if (IsDeleted) return;
            foreach (var item in Items)
            {
                item.Delete(currentUserId);
            }
            MarkDeleted(currentUserId);
        }

        // ===================== Private Helpers =====================
        private void RecalculateFinalPrice()
        {
            TotalPrice = Items.Sum(i => i.TotalPrice);
            FinalPrice = TotalPrice + ShippingCost - DiscountAmount;
        }
 
    }
}

