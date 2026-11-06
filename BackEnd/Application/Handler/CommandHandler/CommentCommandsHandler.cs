using OnlineShop.Domain.Common;
using Application.Commands;
using Application.Common;
using Common;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Enums;
using OnlineShop.Domain.Interfaces;
namespace Application.Handler.CommandHandler
{
    public class CommentCommandHandler(
        ICommentRepository _repo, IHttpContextAccessor _accessor,IUserRepository _userRepo,IBlogRepository _blogRepo,IProductRepository _productRepo)
        : IRequestHandler<CreateCommentCommand, ServiceResult<IdDto>>,
        IRequestHandler<ReplyCommentCommand, ServiceResult<IdDto>>,
        IRequestHandler<ApproveCommentCommand, ServiceResult<IdDto>>,
        IRequestHandler<EditCommentCommand, ServiceResult<IdDto>>,
        IRequestHandler<DeleteCommentCommand, ServiceResult<IdDto>>

    {
        public async Task<ServiceResult<IdDto>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();
            var user = _userRepo.Query(u => u.Id == userId).FirstOrDefault();
            var parentName = _repo.Query(u => u.Id == request.ParentId).Select(p => p.UserName).FirstOrDefault();
            string title = request.TargetType switch
            {
                Domain.Enums.EnumTargetType.Blog =>
                    await _blogRepo.Query(p => p.Id == request.TargetId)
                                  .Select(p => p.TitleFa)
                                  .FirstOrDefaultAsync(),

                Domain.Enums.EnumTargetType.Supplier =>
                    await _userRepo.Query(p => p.Id == request.TargetId)
                                  .Select(p => p.FullName)
                                  .FirstOrDefaultAsync(),

                Domain.Enums.EnumTargetType.Product =>
                    await _productRepo.Query(p => p.Id == request.TargetId)
                                     .Select(p => p.Name)
                                     .FirstOrDefaultAsync(),

                _ => throw new DomainException($"نوع «{request.TargetType}» برای این عملیات پشتیبانی نمی‌شود.")
            };


            var comment = Comment.Create(
                userId: userId.Value,
                userName: user.FullName,
                targetId: request.TargetId,
                targetTitle: title,
                targetType: request.TargetType,
                content: request.Content,
                currentUserId: userId.Value,
                parentId: request.ParentId,
                parentName: parentName ?? ""
            );

            await _repo.AddAsync(comment);
            await _repo.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = comment.Id });
        }
        public async Task<ServiceResult<IdDto>> Handle(ReplyCommentCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();
            var user = _userRepo.Query(u => u.Id == userId).FirstOrDefault();
            var parentName = _repo.Query(u => u.Id == request.ParentId).Select(p => p.UserName).FirstOrDefault();
            string title = request.TargetType switch
            {
                Domain.Enums.EnumTargetType.Blog =>
                    await _blogRepo.Query(p => p.Id == request.TargetId)
                                  .Select(p => p.TitleFa)
                                  .FirstOrDefaultAsync(),

                Domain.Enums.EnumTargetType.Supplier =>
                    await _userRepo.Query(p => p.Id == request.TargetId)
                                  .Select(p => p.FullName)
                                  .FirstOrDefaultAsync(),

                Domain.Enums.EnumTargetType.Product =>
                    await _productRepo.Query(p => p.Id == request.TargetId)
                                     .Select(p => p.Name)
                                     .FirstOrDefaultAsync(),

                _ => throw new DomainException($"نوع «{request.TargetType}» برای این عملیات پشتیبانی نمی‌شود.")
            };
            var comment = Comment.Create(
                userId: userId.Value,
                userName: user.FullName,
                targetId: request.TargetId,
                targetTitle: title,
                targetType: request.TargetType,
                content: request.Content,
                currentUserId: userId.Value,
                parentId: request.ParentId,
                parentName: parentName ?? ""
            );

            await _repo.AddAsync(comment);
            await _repo.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = comment.Id });
        }
        public async Task<ServiceResult<IdDto>> Handle(ApproveCommentCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();

            var comment = await _repo.GetByIdAsync(request.Id);
            if (comment == null)
                return ServiceResult<IdDto>.Failed("نظر موردنظر پیدا نشد.");

            comment.Approve(request.IsApprove, userId.Value);
            await _repo.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = comment.Id });
        }
        public async Task<ServiceResult<IdDto>> Handle(ActiveCommentCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();

            var comment = await _repo.GetByIdAsync(request.Id);
            if (comment == null)
                return ServiceResult<IdDto>.Failed("نظر موردنظر پیدا نشد.");
            comment.SetActive(request.IsActive, userId.Value);
            await _repo.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = comment.Id });
        }

        public async Task<ServiceResult<IdDto>> Handle(EditCommentCommand request, CancellationToken cancellationToken)
        {
            var comment = await _repo.GetByIdAsync(request.CommentId);
            if (comment == null)
                return ServiceResult<IdDto>.Failed("نظر موردنظر پیدا نشد.");
            if (comment.UserId != request.UserId)
                return ServiceResult<IdDto>.Failed("فقط نویسندهٔ نظر می‌تواند آن را ویرایش کند.");
            comment.Edit(request.Content, request.UserId);
            await _repo.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = comment.Id });
        }
        public async Task<ServiceResult<IdDto>> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();
            var comment = await _repo.GetByIdAsync(request.Id);
            if (comment == null)
                return ServiceResult<IdDto>.Failed("نظر موردنظر پیدا نشد.");
            comment.Delete(userId.Value);

            await _repo.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = comment.Id });
        }

    }
    
}
