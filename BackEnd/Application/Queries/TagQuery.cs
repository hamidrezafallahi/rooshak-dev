using Application.Dtos;
using Common;
using MediatR;

namespace Application.Queries
{

    public class GetTagsQuery :BaseListDto, IRequest<ServiceResult<ListDto<TagDto>>> { }
    public class GetTags4selectOptionQuery : BaseListDto,IRequest<ServiceResult<ListDto<SelectOptionDto>>> {}
    public class GetTagBySlugQuery : IRequest<ServiceResult<TagDto>>
    {
        public string Slug { get; set; } = string.Empty;



    }
    public class GetAllTagIdsQuery : IRequest<ServiceResult<List<IdDto>>>
    {
        
    }
    public class GetAllTagsSlugsQuery : IRequest<ServiceResult<IEnumerable<SlugDto>>>
    {
    }


    public class GetTagsByProductOfferIdQuery : IRequest<ServiceResult<IEnumerable<TagDto>>>
    {
        public int ProductId { get; set; }

       
    }
    public class GetProductSByTagIdQuery : IRequest<ServiceResult<IEnumerable<ProductDto>>>
    {
        public int TagId { get; set; }
 
    };

    /// <summary>Public price list of one tag ("family") for the exhibition pages.</summary>
    public class GetTagPriceListQuery : IRequest<ServiceResult<TagPriceListDto>>
    {
        public string IdOrSlug { get; set; } = string.Empty;
    }

    /// <summary>Tags that currently have priced products, for the exhibition index.</summary>
    public class GetTagFamiliesQuery : IRequest<ServiceResult<IEnumerable<TagFamilyDto>>>
    {
    }

   
    }
