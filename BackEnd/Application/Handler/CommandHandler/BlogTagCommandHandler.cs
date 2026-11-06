using Application.Common;
using Common;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using OnlineShop.Domain.Entities;

public class BlogTagCommandHandler(IBlogTagRepository _repo, IHttpContextAccessor _accessor)
    : IRequestHandler<CreateBlogTagCommand, ServiceResult<IdDto>>,
    IRequestHandler<UpdateBlogTagCommand, ServiceResult<IdDto>>,
    IRequestHandler<ActiveBlogTagCommand, ServiceResult<IdDto>>,
    IRequestHandler<DeleteBlogTagCommand, ServiceResult<IdDto>>
{
   
    public async Task<ServiceResult<IdDto>> Handle(CreateBlogTagCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Unauthorized();
        var blogTag = BlogTag.Create(request.BlogId, request.TagId, userId.Value);
        var exist = _repo.Query(pot => pot.TagId == request.TagId && pot.BlogId == request.BlogId).FirstOrDefault();
        if (exist is not null)
        {
            return ServiceResult<IdDto>.Failed("این برچسب قبلاً به این مقاله اضافه شده است.");
        }
        await _repo.AddAsync(blogTag);
        await _repo.SaveChangesAsync(cancellationToken);

        return ServiceResult<IdDto>.Ok(new IdDto { Id = blogTag.Id });
    }
    public async Task<ServiceResult<IdDto>> Handle(UpdateBlogTagCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Unauthorized();
        var blogTag = await _repo.GetByIdAsync(request.Id);
        if (blogTag == null) return ServiceResult<IdDto>.Failed("برچسب مقالهٔ موردنظر پیدا نشد.");
        if (blogTag.BlogId == request.BlogId && blogTag.TagId == request.TagId)
        {
            return ServiceResult<IdDto>.Failed("این برچسب قبلاً به این مقاله اضافه شده است.");
        }

        blogTag.Update(request.BlogId, request.TagId, userId.Value);

        await _repo.SaveChangesAsync(cancellationToken);

        return ServiceResult<IdDto>.Ok(new IdDto { Id = blogTag.Id });
    }
    public async Task<ServiceResult<IdDto>> Handle(ActiveBlogTagCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Unauthorized();
        var blogTag = await _repo.GetByIdAsync(request.Id);
        if (blogTag == null) return ServiceResult<IdDto>.Failed("برچسب مقالهٔ موردنظر پیدا نشد.");

        blogTag.SetActive(request.IsActive, userId.Value);

        await _repo.SaveChangesAsync(cancellationToken);

        return ServiceResult<IdDto>.Ok(new IdDto { Id = blogTag.Id });
    }
    public async Task<ServiceResult<IdDto>> Handle(DeleteBlogTagCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Unauthorized();
        var blogTag = await _repo.GetByIdAsync(request.Id);
        if (blogTag == null) return ServiceResult<IdDto>.Failed("برچسب مقالهٔ موردنظر پیدا نشد.");

        blogTag.Delete(userId.Value);

        await _repo.SaveChangesAsync(cancellationToken);

        return ServiceResult<IdDto>.Ok(new IdDto { Id = blogTag.Id });
    }
}
 