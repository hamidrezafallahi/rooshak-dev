
using Common;
using MediatR;
public class GetAllProductImagesQuery :BaseListDto, IRequest<ServiceResult<ListDto<GetProductImageDto>>>{}
public class GetMainImageByProductIdQuery : IRequest<ServiceResult<GetProductImageDto?>>
{
    public int ProductId { get; set; }

    public GetMainImageByProductIdQuery(int productId)
    {
        ProductId = productId;
    }
}
public class GetProductImagesByIdQuery : IRequest<ServiceResult<GetProductImageDto>>
{
    public int Id { get; set; }

}

/// <summary>The whole gallery of one product (active and inactive), main image first.</summary>
public class GetProductImagesByProductIdQuery : IRequest<ServiceResult<List<GetProductImageDto>>>
{
    public int ProductId { get; set; }
}
