using OnlineShop.Domain.Common;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Domain.Entities
{
    public class ShippingMethod : BaseEntity
    {
        private ShippingMethod() { }

        public string Title { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public decimal Price { get; private set; }

        /// <summary>
        /// مدت زمان تقریبی تحویل (به ساعت یا روز — انتخاب با شما)
        /// </summary>
        public int EstimatedDeliveryTime { get; private set; }

        /// <summary>
        /// آیا فقط برای تهران فعال است؟
        /// </summary>
        public bool IsDefault { get; private set; } = false;

        // ========== Factory Method ==========
        public static ShippingMethod Create(
            string title,
            string description,
            int estimatedDeliveryTime,
            decimal price,
            bool? isDefault,
            int currentUserId
            )
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new DomainException("عنوان روش ارسال نمی‌تواند خالی باشد.");

            if (price < 0)
                throw new DomainException("هزینهٔ ارسال نمی‌تواند منفی باشد.");

            var method = new ShippingMethod
            {
                Title = title,
                Price = price,
                EstimatedDeliveryTime = estimatedDeliveryTime,
                IsDefault = isDefault ?? false,
                Description = description
            };

            method.MarkCreated(currentUserId);
            return method;
        }

        // ======== Update Method =========
        public void Update(
            string? title,
            string? description,
            int? estimatedDeliveryTime,
            decimal? price,
            bool? isDefault,
            int currentUserId)
        {
            if (!string.IsNullOrWhiteSpace(title))
                Title = title;

            if (price.HasValue)
            {
                if (price < 0)
                    throw new DomainException("هزینهٔ ارسال نمی‌تواند منفی باشد.");
                Price = price.Value;
            }

            if (estimatedDeliveryTime.HasValue)
                EstimatedDeliveryTime = estimatedDeliveryTime.Value;

            if (description != null)
                Description = description;
            if (isDefault.HasValue)
            {
                IsDefault = isDefault.Value;
            }

            MarkUpdated(currentUserId);
        }

 
        // ========= setDefault =========

        public void SetDefault(bool isDefault, int currentUserId)
        {
            IsDefault = isDefault;
            MarkUpdated(currentUserId);
        }

        
    }
}
