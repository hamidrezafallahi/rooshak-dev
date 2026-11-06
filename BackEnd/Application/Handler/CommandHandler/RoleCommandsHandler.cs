using Application.Commands;
using Application.Common;
using Common;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;

public class RoleCommandHandler(
        IRoleRepository _repo,
        IHttpContextAccessor _accessor) :
     IRequestHandler<ActiveRoleCommand, ServiceResult<IdDto>>,
    IRequestHandler<DeleteRoleCommand, ServiceResult<IdDto>>
{

    public async Task<ServiceResult<IdDto>> Handle(ActiveRoleCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Unauthorized();

        var role = await _repo.GetByIdAsync(request.Id);
        if (role == null)
            return ServiceResult<IdDto>.Failed("نقش کاربری موردنظر پیدا نشد.");

        role.SetActive(request.IsActive, userId.Value);

        await _repo.SaveChangesAsync(cancellationToken);

        return ServiceResult<IdDto>.Ok(new IdDto { Id = role.Id });
    }
    public async Task<ServiceResult<IdDto>> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Unauthorized();

        var role = await _repo.GetByIdAsync(request.Id);
        if (role == null)
            return ServiceResult<IdDto>.Failed("نقش کاربری موردنظر پیدا نشد.");

        role.Delete(userId.Value);

        await _repo.SaveChangesAsync(cancellationToken);

        return ServiceResult<IdDto>.Ok(new IdDto { Id = role.Id });
    }

}
