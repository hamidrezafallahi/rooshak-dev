using Application.Commands;
using Application.Common;
using Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
    public class EntityConfigCommandHandler(IEntityConfigRepository _repo, IHttpContextAccessor _accessor) : 
        IRequestHandler<CreateEntityConfigCommand, ServiceResult<IdDto>>,
        IRequestHandler<UpdateEntityConfigCommand, ServiceResult<IdDto>>,
        IRequestHandler<DeleteEntityConfigCommand, ServiceResult<IdDto>>,
        IRequestHandler<ActiveEntityConfigCommand, ServiceResult<IdDto>>
    {
        public async Task<ServiceResult<IdDto>> Handle(CreateEntityConfigCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();

            var entityConfig = EntityConfig.Create(
                 userId.Value,
                 request.EntityName,
                 request.PersianDisplayName,
                 request.EnglishDisplayName,
                 request.EndPoint,
                 request.EntityIconBase64,
                 request.Actions,
                 request.Columns,
                request.FormFields
                );
            await _repo.AddAsync(entityConfig);
            await _repo.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = entityConfig.Id });
        }
        public async Task<ServiceResult<IdDto>> Handle(UpdateEntityConfigCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();

            var config = await _repo.GetByIdAsync(request.Id);
            if (config == null)
                return ServiceResult<IdDto>.Failed("پیکربندی موردنظر پیدا نشد.");
            //var title = string.IsNullOrWhiteSpace(request.Title) ? null : request.Title;
            //var content = string.IsNullOrWhiteSpace(request.Content) ? null : request.Content;
            //var slug = string.IsNullOrWhiteSpace(request.Slug) ? null : request.Slug;

            config.Update(
                request.Id,
                request.EntityName,
                request.PersianDisplayName,
                request.EnglishDisplayName,
                request.EndPoint,
                request.EntityIconBase64,
                request.Actions,
                request.Columns,
                request.FormFields
                );
            await _repo.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = config.Id });
        }
        public async Task<ServiceResult<IdDto>> Handle(DeleteEntityConfigCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();

            var config = await _repo.GetByIdAsync(request.Id);
            if (config == null)
                return ServiceResult<IdDto>.Failed("پیکربندی موردنظر پیدا نشد.");

            config.Delete(userId.Value);
            await _repo.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = config.Id });
        }
        public async Task<ServiceResult<IdDto>> Handle(ActiveEntityConfigCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();

            var config = await _repo.GetByIdAsync(request.Id);
            if (config == null)
                return ServiceResult<IdDto>.Failed("پیکربندی موردنظر پیدا نشد.");

            config.SetActive(request.IsActive, userId.Value);
            await _repo.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = config.Id });
        }
    }

