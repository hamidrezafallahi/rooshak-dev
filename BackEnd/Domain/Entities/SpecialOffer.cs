using OnlineShop.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineShop.Domain.Entities
{
    public class SpecialOffer : BaseEntity
    {
        private SpecialOffer() { }

        public int ProductOfferId { get; private set; }
        public ProductOffers ProductOffer { get; private set; }

        // اگر بخواهیم پیشنهاد ویژه با تخفیف خاصی نمایش داده شود
        public int? DiscountId { get; private set; }
        public Discount? Discount { get; private set; }

        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }

        public int DisplayOrder { get; private set; }

        public bool IsToday => DateTime.UtcNow.Date >= StartDate.Date && DateTime.UtcNow.Date <= EndDate.Date;


        // ===== Factory =====
        public static SpecialOffer Create(
            int productOfferId,
            DateTime startDate,
            DateTime endDate,
            int currentUserId,
            int? discountId = null,
            int displayOrder = 0)
        {
            if (endDate <= startDate)
                throw new DomainException("تاریخ پایان باید بعد از تاریخ شروع باشد.");

            var offer = new SpecialOffer
            {
                ProductOfferId = productOfferId,
                DiscountId = discountId,
                StartDate = startDate,
                EndDate = endDate,
                DisplayOrder = displayOrder
            };

            offer.MarkCreated(currentUserId);
            return offer;
        }

        // ===== Behavior =====
        public void Update(
            int currentUserId,
            int? productOfferId,
            DateTime? startDate,
            DateTime? endDate,
            int? discountId ,
            int? displayOrder )
        {
            if (startDate.HasValue)
                StartDate = startDate.Value;
            if (endDate.HasValue)
                EndDate = endDate.Value;
            if (discountId.HasValue)
                DiscountId = discountId;
            if (productOfferId.HasValue)
                ProductOfferId = productOfferId.Value;
            if (displayOrder.HasValue)
                DisplayOrder = displayOrder.Value;

            MarkUpdated(currentUserId);
        }


    }
}
