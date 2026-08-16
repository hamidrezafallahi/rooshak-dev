namespace Application.Dtos
{
    public class CatalogItemIdsDto
    {
        public int ProductId { get; set; }
        public int OfferId { get; set; }
        public int? ImageId { get; set; }
    }

    public class CatalogItemActiveDto
    {
        public int ProductId { get; set; }
        public bool IsActive { get; set; }
        public int OfferCount { get; set; }
        public int ImageCount { get; set; }
    }
}
