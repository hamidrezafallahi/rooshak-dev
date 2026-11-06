using OnlineShop.Domain.Common;
namespace OnlineShop.Domain.ValueObjects
{
    public class ProductDimensions
    {
        public decimal? Width { get; }
        public decimal? Height { get; }
        public decimal? Depth { get; }
        public decimal? Weight { get; }

        private ProductDimensions() { }

        public ProductDimensions(decimal? width, decimal? height, decimal? depth, decimal? weight)
        {
            if (width < 0 || height < 0 || depth < 0)
                throw new DomainException("ابعاد باید بیشتر از صفر باشند.");

            if (weight < 0)
                throw new DomainException("وزن نمی‌تواند منفی باشد.");

            Width = width;
            Height = height;
            Depth = depth;
            Weight = weight;
        }
    }
}
