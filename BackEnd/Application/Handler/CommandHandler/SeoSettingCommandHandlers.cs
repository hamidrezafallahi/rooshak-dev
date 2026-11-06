using Application.Commands;
using Application.Common;
using Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using OnlineShop.Domain.Interfaces;

public class SeoSettingCommandHandlers(ISeoSettingRepository _repo, IHttpContextAccessor _accessor) :
    IRequestHandler<CreateSeoSettingCommand, ServiceResult<IdDto>>,
    IRequestHandler<UpdateSeoSettingCommand, ServiceResult<IdDto>>,
    IRequestHandler<ActiveSeoSettingCommand, ServiceResult<IdDto>>,
    IRequestHandler<DeleteSeoSettingCommand, ServiceResult<IdDto>>
{
    public async Task<ServiceResult<IdDto>> Handle(CreateSeoSettingCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null) return ServiceResult<IdDto>.Unauthorized();

        var entity = OnlineShop.Domain.Entities.SeoSetting.Create(
            request.RoutePath,
            request.MatchType,
            request.TitleFa,
            request.TitleEn,
            request.DescriptionFa,
            request.DescriptionEn,
            request.KeywordsFa,
            request.KeywordsEn,
            request.CanonicalPath,
            request.OgImageUrl,
            request.RobotsIndex,
            request.RobotsFollow,
            request.Priority,
            request.Notes,
            userId.Value);

        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync(cancellationToken);

        return ServiceResult<IdDto>.Ok(new IdDto { Id = entity.Id });
    }

    public async Task<ServiceResult<IdDto>> Handle(UpdateSeoSettingCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null) return ServiceResult<IdDto>.Unauthorized();

        var entity = await _repo.GetByIdAsync(request.Id);
        if (entity == null) return ServiceResult<IdDto>.Failed("تنظیمات سئوی موردنظر پیدا نشد.");

        entity.Update(
            userId.Value,
            request.RoutePath,
            request.MatchType,
            request.TitleFa,
            request.TitleEn,
            request.DescriptionFa,
            request.DescriptionEn,
            request.KeywordsFa,
            request.KeywordsEn,
            request.CanonicalPath,
            request.OgImageUrl,
            request.RobotsIndex,
            request.RobotsFollow,
            request.Priority,
            request.Notes);

        if (request.IsActive.HasValue)
        {
            entity.SetActive(request.IsActive.Value, userId.Value);
        }

        await _repo.SaveChangesAsync(cancellationToken);
        return ServiceResult<IdDto>.Ok(new IdDto { Id = entity.Id });
    }

    public async Task<ServiceResult<IdDto>> Handle(ActiveSeoSettingCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null) return ServiceResult<IdDto>.Unauthorized();

        var entity = await _repo.GetByIdAsync(request.Id);
        if (entity == null) return ServiceResult<IdDto>.Failed("تنظیمات سئوی موردنظر پیدا نشد.");

        entity.SetActive(request.IsActive, userId.Value);
        await _repo.SaveChangesAsync(cancellationToken);

        return ServiceResult<IdDto>.Ok(new IdDto { Id = entity.Id });
    }

    public async Task<ServiceResult<IdDto>> Handle(DeleteSeoSettingCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null) return ServiceResult<IdDto>.Unauthorized();

        var entity = await _repo.GetByIdAsync(request.Id);
        if (entity == null) return ServiceResult<IdDto>.Failed("تنظیمات سئوی موردنظر پیدا نشد.");

        entity.Delete(userId.Value);
        await _repo.SaveChangesAsync(cancellationToken);

        return ServiceResult<IdDto>.Ok(new IdDto { Id = entity.Id });
    }
}
