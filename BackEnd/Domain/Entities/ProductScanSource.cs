namespace OnlineShop.Domain.Entities
{
    /// <summary>
    /// Raw photo / video captured with a phone for a product. These are the input for a
    /// photogrammetry tool (RealityScan, Polycam, KIRI Engine, ...) that produces the GLB.
    /// </summary>
    public class ProductScanSource : BaseEntity
    {
        private ProductScanSource() { }

        public int ProductId { get; private set; }
        public Product Product { get; private set; } = default!;

        public string FileUrl { get; private set; } = string.Empty;

        /// <summary>"image" or "video".</summary>
        public string Kind { get; private set; } = "image";
        public long SizeBytes { get; private set; }

        public static ProductScanSource Create(int productId, string fileUrl, string kind, long sizeBytes, int currentUserId)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
                throw new ArgumentException("FileUrl cannot be empty.", nameof(fileUrl));

            var source = new ProductScanSource
            {
                ProductId = productId,
                FileUrl = fileUrl,
                Kind = kind == "video" ? "video" : "image",
                SizeBytes = sizeBytes
            };
            source.MarkCreated(currentUserId);
            return source;
        }
    }
}
