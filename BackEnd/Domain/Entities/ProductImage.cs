using OnlineShop.Domain.Common;


namespace OnlineShop.Domain.Entities
{
    public class ProductImage : BaseEntity
    {
        private ProductImage() { } 

        public int ProductId { get; private set; }
        public Product Product { get; private set; } = default!;

        public string ImageUrl { get; private set; } = string.Empty;
        public bool IsMain { get; private set; }

        // ===== Factory Method =====
        public static ProductImage Create(int productId, string imageUrl, bool isMain, int currentUserId)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                throw new DomainException("تصویر نمی‌تواند خالی باشد.");

            var image = new ProductImage
            {
                ProductId = productId,
                ImageUrl = imageUrl,
                IsMain = isMain
            };

            image.MarkCreated(currentUserId);
            return image;
        }

        // ===== Behavior Methods =====
        public void SetMain(bool isMain, int currentUserId)
        {
            IsMain = isMain;
            MarkUpdated(currentUserId);
        }

        public void UpdateUrl(string newUrl, int currentUserId)
        {
            if (string.IsNullOrWhiteSpace(newUrl))
                throw new DomainException("تصویر نمی‌تواند خالی باشد.");

            ImageUrl = newUrl;
            MarkUpdated(currentUserId);
        }

    }
}
