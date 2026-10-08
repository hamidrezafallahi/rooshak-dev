namespace OnlineShop.Domain.Entities
{
    /// <summary>
    /// 3D model of a product used by the storefront viewer / AR ("view on your table").
    /// One row per product. <see cref="ModelUrl"/> is a GLB (or self-contained glTF) for the
    /// web viewer and Android AR; <see cref="UsdzUrl"/> is the optional iOS AR Quick Look file.
    /// Kept separate from <see cref="ProductImage"/> because image upload replaces all images
    /// and re-encodes them as WebP.
    /// </summary>
    public class ProductModel3D : BaseEntity
    {
        private ProductModel3D() { }

        public int ProductId { get; private set; }
        public Product Product { get; private set; } = default!;

        public string ModelUrl { get; private set; } = string.Empty;
        public long ModelSizeBytes { get; private set; }

        public string? UsdzUrl { get; private set; }
        public long? UsdzSizeBytes { get; private set; }

        public static ProductModel3D Create(int productId, string modelUrl, long modelSizeBytes, int currentUserId)
        {
            if (string.IsNullOrWhiteSpace(modelUrl))
                throw new ArgumentException("ModelUrl cannot be empty.", nameof(modelUrl));

            var model = new ProductModel3D
            {
                ProductId = productId,
                ModelUrl = modelUrl,
                ModelSizeBytes = modelSizeBytes
            };
            model.MarkCreated(currentUserId);
            return model;
        }

        public void SetModel(string modelUrl, long sizeBytes, int currentUserId)
        {
            if (string.IsNullOrWhiteSpace(modelUrl))
                throw new ArgumentException("ModelUrl cannot be empty.", nameof(modelUrl));

            ModelUrl = modelUrl;
            ModelSizeBytes = sizeBytes;
            MarkUpdated(currentUserId);
        }

        public void SetUsdz(string? usdzUrl, long? sizeBytes, int currentUserId)
        {
            UsdzUrl = string.IsNullOrWhiteSpace(usdzUrl) ? null : usdzUrl;
            UsdzSizeBytes = UsdzUrl == null ? null : sizeBytes;
            MarkUpdated(currentUserId);
        }
    }
}
