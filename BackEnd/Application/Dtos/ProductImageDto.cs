using Microsoft.AspNetCore.Http;
public class ProductImageDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public IFormFile? ProductImageFile { get; set; }  
    public bool IsMain { get; set; }
}
public class GetProductImageDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; }

    public string ProductImageUrl { get; set; }

    /// <summary>Full-quality original file; null when only the compressed copy exists.</summary>
    public string? OriginalImageUrl { get; set; }
    public bool IsMain { get; set; }
    public bool IsActive { get; set; }

}