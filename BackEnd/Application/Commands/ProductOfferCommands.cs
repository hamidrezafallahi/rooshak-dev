using Common;
using MediatR;

namespace Application.Commands
{
    public class CreateProductOfferCommand : IRequest<ServiceResult<IdDto>>
    {
        public int ProductId { get; set; }
        public decimal BasePrice { get; set; }
        public int Inventory { get; set; }
        /// <summary>عنوان تنوع، مثلاً «بزرگنمایی». اختیاری.</summary>
        public string? VariantName { get; set; }
        /// <summary>مقدار تنوع، مثلاً «3.5x». هر مقدار قیمت و موجودی خودش را دارد.</summary>
        public string? VariantValue { get; set; }
    }

    public class UpdateProductOfferCommand : IRequest<ServiceResult<IdDto>>
    {
        public int Id { get; set; }
        public decimal? BasePrice { get; set; }
        public int? Inventory { get; set; }
        public bool? IsActive { get; set; }
        public string? VariantName { get; set; }
        public string? VariantValue { get; set; }
    }

    public class DeleteProductOfferCommand : IRequest<ServiceResult<IdDto>>
    {
        public int Id { get; set; }
    }

    public class ActiveProductOfferCommand : ActiveCommand, IRequest<ServiceResult<IdDto>> { }
}
