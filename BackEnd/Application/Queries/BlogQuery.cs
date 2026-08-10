using Application.Dtos;
using Common;
using MediatR;

namespace Application.Queries
{
    public class GetBlogsQuery : BaseListDto, IRequest<ServiceResult<ListDto<BlogDto>>> { }
    public class GetBlogs4selectOptionQuery : BaseListDto, IRequest<ServiceResult<ListDto<SelectOptionDto>>> { }


    public class GetBlogByIdQuery : IRequest<ServiceResult<BlogDto?>>
    {
        public int Id { get; set; }
    }
    public class GetBlogBySlugQuery : IRequest<ServiceResult<BlogDto?>>
    {
        public string Slug { get; set; } = null!;
    }
    public class GetAllBlogsSlugsQuery : IRequest<ServiceResult<IEnumerable<SlugDto?>>>
    {
        /// <summary>
        /// When true, include inactive drafts (needed by n8n slug de-duplication).
        /// </summary>
        public bool IncludeInactive { get; set; }
    }
}
