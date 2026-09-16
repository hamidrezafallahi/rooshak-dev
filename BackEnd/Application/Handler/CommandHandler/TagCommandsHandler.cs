using Application.Commands;
using Application.Common;
using Common;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;



namespace Application.Handler.CommandHandler
{
    public class TagCommandHandler(ITagRepository _repo, IHttpContextAccessor _accessor) : 
        IRequestHandler<CreateTagCommand, ServiceResult<IdDto>>,
        IRequestHandler<UpdateTagCommand, ServiceResult<IdDto>>,
        IRequestHandler<ActiveTagCommand, ServiceResult<IdDto>>,
        IRequestHandler<DeleteTagCommand, ServiceResult<IdDto>>

    {
        public async Task<ServiceResult<IdDto>> Handle(CreateTagCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Failed("Unauthorized");

            var existsTag = await _repo.ExistsByTagNameAsync(request.Name);
            if (existsTag)
                return ServiceResult<IdDto>.Failed("تگی با همین متن قبلاً ثبت شده است.");

            var tag = Tag.Create(request.Name, userId.Value, request.Slug);

            await _repo.AddAsync(tag);
            await _repo.SaveChangesAsync(cancellationToken);

            // Keep clean exhibition URLs (e.g. /exhibition/sunshine).
            // Only append the tag Id when that slug is already taken.
            if (string.IsNullOrWhiteSpace(tag.Slug)
                || await _repo.ExistsBySlugAsync(tag.Slug, tag.Id))
            {
                tag.EnsureSlug(tag.Id);
                await _repo.SaveChangesAsync(cancellationToken);
            }

            return ServiceResult<IdDto>.Ok(new IdDto { Id = tag.Id });
        }
        public async Task<ServiceResult<IdDto>> Handle(UpdateTagCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Failed("Unauthorized");
            var tag = await _repo
                      .Query(t => t.Id == request.Id)
                      .FirstOrDefaultAsync(cancellationToken);
            if (tag == null)
                return ServiceResult<IdDto>.Failed("مورد پیدا نشد");

            tag.Update(request.Name, userId.Value, request.Slug);
            if (string.IsNullOrWhiteSpace(tag.Slug)
                || await _repo.ExistsBySlugAsync(tag.Slug, tag.Id))
            {
                tag.EnsureSlug(tag.Id);
            }
            await _repo.SaveChangesAsync(cancellationToken);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = tag.Id });


        }
        public async Task<ServiceResult<IdDto>> Handle(ActiveTagCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Failed("Unauthorized");
            var tag = await _repo.GetByIdAsync(request.Id);
            if (tag == null)
                return ServiceResult<IdDto>.Failed("این کتگوری موجود نیست");
            tag.SetActive(request.IsActive, userId.Value);
            await _repo.SaveChangesAsync(cancellationToken);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = tag.Id });

        }
        public async Task<ServiceResult<IdDto>> Handle(DeleteTagCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Failed("Unauthorized");
            var tag = await _repo.GetByIdAsync(request.Id);
            if (tag == null)
                return ServiceResult<IdDto>.Failed("این کتگوری موجود نیست");
            tag.Delete(userId.Value);
            await _repo.SaveChangesAsync(cancellationToken);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = tag.Id });

        }

    }
}
