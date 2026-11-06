using Application.Common;
using Common;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using OnlineShop.Domain.Entities;

public class UserTagCommandHandler(IUserTagRepository _repo, IHttpContextAccessor _accessor)
    : IRequestHandler<CreateUserTagCommand, ServiceResult<IdDto>>,
    IRequestHandler<UpdateUserTagCommand, ServiceResult<IdDto>>,
    IRequestHandler<ActiveUserTagCommand, ServiceResult<IdDto>>,
    IRequestHandler<DeleteUserTagCommand, ServiceResult<IdDto>>
{
   
    public async Task<ServiceResult<IdDto>> Handle(CreateUserTagCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Unauthorized();
        var userTag = UserTag.Create(request.UserId, request.TagId, userId.Value);
        var exist = _repo.Query(pot => pot.TagId == request.TagId && pot.UserId == request.UserId).FirstOrDefault();
        if (exist is not null)
        {
            return ServiceResult<IdDto>.Failed("این برچسب قبلاً به این کاربر اضافه شده است.");
        }
        await _repo.AddAsync(userTag);
        await _repo.SaveChangesAsync(cancellationToken);

        return ServiceResult<IdDto>.Ok(new IdDto { Id = userTag.Id });
    }
    public async Task<ServiceResult<IdDto>> Handle(UpdateUserTagCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Unauthorized();
        var userTag = await _repo.GetByIdAsync(request.Id);
        if (userTag == null) return ServiceResult<IdDto>.Failed("برچسب کاربر موردنظر پیدا نشد.");
        if (userTag.UserId == request.UserId && userTag.TagId == request.TagId)
        {
            return ServiceResult<IdDto>.Failed("این برچسب قبلاً به این کاربر اضافه شده است.");
        }

        userTag.Update(request.UserId, request.TagId, userId.Value);

        await _repo.SaveChangesAsync(cancellationToken);

        return ServiceResult<IdDto>.Ok(new IdDto { Id = userTag.Id });
    }
    public async Task<ServiceResult<IdDto>> Handle(ActiveUserTagCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Unauthorized();
        var userTag = await _repo.GetByIdAsync(request.Id);
        if (userTag == null) return ServiceResult<IdDto>.Failed("برچسب کاربر موردنظر پیدا نشد.");

        userTag.SetActive(request.IsActive, userId.Value);

        await _repo.SaveChangesAsync(cancellationToken);

        return ServiceResult<IdDto>.Ok(new IdDto { Id = userTag.Id });
    }
    public async Task<ServiceResult<IdDto>> Handle(DeleteUserTagCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Unauthorized();
        var userTag = await _repo.GetByIdAsync(request.Id);
        if (userTag == null) return ServiceResult<IdDto>.Failed("برچسب کاربر موردنظر پیدا نشد.");

        userTag.Delete(userId.Value);

        await _repo.SaveChangesAsync(cancellationToken);

        return ServiceResult<IdDto>.Ok(new IdDto { Id = userTag.Id });
    }
}
 