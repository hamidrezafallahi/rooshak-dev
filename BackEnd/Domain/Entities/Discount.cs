using OnlineShop.Domain.Common;
using Domain.Entities;


namespace OnlineShop.Domain.Entities
{
    public class Discount : BaseEntity
    {
        private Discount() { }

        public string Title { get; private set; } = string.Empty;
        public decimal Amount { get; private set; }
        public decimal Priority { get; private set; }

        public bool IsPercent { get; private set; }

        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }

         

        private readonly List<ProductOfferDiscount> _productOfferDiscounts = new();
        public IReadOnlyCollection<ProductOfferDiscount> ProductOfferDiscounts => _productOfferDiscounts.AsReadOnly();

        // ===== Factory Method =====
        public static Discount Create(
            string title,
            decimal amount,
            bool isPercent,
            DateTime startDate,
            DateTime endDate,
            int? priority,
            int currentUserId)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new DomainException("عنوان تخفیف نمی‌تواند خالی باشد.");

            if (amount <= 0)
                throw new DomainException("مبلغ تخفیف باید بیشتر از صفر باشد.");

            if (isPercent && amount > 100)
                throw new DomainException("تخفیف درصدی نمی‌تواند بیشتر از ۱۰۰ باشد.");

            if (endDate <= startDate)
                throw new DomainException("تاریخ پایان باید بعد از تاریخ شروع باشد.");

            var discount = new Discount
            {
                Title = title,
                Amount = amount,
                IsPercent = isPercent,
                StartDate = startDate,
                EndDate = endDate,
                Priority= priority??1
            };

            discount.MarkCreated(currentUserId);
            return discount;
        }

        // ===== Behavior Methods =====
        public void Update(
            int currentUserId,
            string? title = null,
            decimal? amount = null,
            bool? isPercent = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? priority=null)
        {
            if (!string.IsNullOrWhiteSpace(title))
                Title = title;

            if (amount.HasValue)
            {
                if (amount.Value <= 0)
                    throw new DomainException("مبلغ تخفیف باید بیشتر از صفر باشد.");
                if (isPercent == true && amount.Value > 100)
                    throw new DomainException("تخفیف درصدی نمی‌تواند بیشتر از ۱۰۰ باشد.");

                Amount = amount.Value;
            }

            if (isPercent.HasValue)
                IsPercent = isPercent.Value;
            if (priority is not null)
                Priority = priority.Value;

            if (startDate.HasValue && endDate.HasValue)
            {
                if (endDate.Value <= startDate.Value)
                    throw new DomainException("تاریخ پایان باید بعد از تاریخ شروع باشد.");

                StartDate = startDate.Value;
                EndDate = endDate.Value;
            }
            else if (startDate.HasValue || endDate.HasValue)
            {
                throw new DomainException("تاریخ شروع و پایان تخفیف باید با هم وارد شوند.");
            }

            MarkUpdated(currentUserId);
        }

        public void AddProduct(ProductOfferDiscount productOfferDiscount, int currentUserId)
        {
            if (productOfferDiscount is null)
                throw new ArgumentNullException(nameof(productOfferDiscount));

            _productOfferDiscounts.Add(productOfferDiscount);
            MarkUpdated(currentUserId);
        }

        public void RemoveProduct(ProductOfferDiscount productOfferDiscount, int currentUserId)
        {
            if (productOfferDiscount is null)
                throw new ArgumentNullException(nameof(productOfferDiscount));

            _productOfferDiscounts.Remove(productOfferDiscount);
            MarkUpdated(currentUserId);
        }

       
    }
}
