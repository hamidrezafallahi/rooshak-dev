namespace Application.Dtos
{
    /// <summary>Public part of the 3D model, embedded in the product detail response.</summary>
    public class ProductModel3DPublicDto
    {
        /// <summary>GLB / glTF for the web viewer and Android AR.</summary>
        public string ModelUrl { get; set; } = string.Empty;
        /// <summary>USDZ for iOS AR Quick Look (optional).</summary>
        public string? UsdzUrl { get; set; }
    }

    public class ProductScanSourceDto
    {
        public int Id { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public string Kind { get; set; } = "image";
        public long SizeBytes { get; set; }
    }

    /// <summary>Admin view: model + the raw phone captures collected for the product.</summary>
    public class ProductModel3DAdminDto
    {
        public int ProductId { get; set; }
        public int? ModelId { get; set; }
        public string? ModelUrl { get; set; }
        public long? ModelSizeBytes { get; set; }
        public string? UsdzUrl { get; set; }
        public long? UsdzSizeBytes { get; set; }
        public List<ProductScanSourceDto> ScanSources { get; set; } = new();
    }
}
