using Application.Common;
using Application.Dtos;
using Application.Queries;
using Common;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class CommentQueryHandler(ICommentRepository _repo,
        IRateRepository _rateRepo,
        IUserRepository _userRepo,
        IEntityConfigRepository _configRepo)
    :
    IRequestHandler<GetAllCommentsQuery, ServiceResult<ListDto<CommentDto>>>,
    IRequestHandler<GetCommentsByTargetQuery, ServiceResult<List<CommentDto>>>,
    IRequestHandler<GetCommentByIdQuery, ServiceResult<CommentDto?>>

{
    public async Task<ServiceResult<ListDto<CommentDto>>> Handle(GetAllCommentsQuery request, CancellationToken cancellationToken)
    {
        int pageNumber = request.page ?? 1;
        int pageSize = request.pageSize ?? 10;
        IQueryable<Comment> query;
        if (request.Q is not null && request.Q.Length > 0)
        {
            if (!request.OnlyActives.HasValue || request.OnlyActives == false)
            {
                if (request.OnlyApproved)
                {
                    query = _repo.Query(b => b.IsApproved && b.Content.Contains(request.Q));
                }
                else
                {
                    query = _repo.Query(b => b.Content.Contains(request.Q));
                }
            }
            else
            {
                if (request.OnlyApproved)
                {
                    query = _repo.Query(b => b.IsActive && b.IsApproved && (b.Content.Contains(request.Q)));

                }
                else
                {
                    query = _repo.Query(b => b.IsActive && (b.Content.Contains(request.Q)));

                }
            }
        }
        else
        {
            if (!request.OnlyActives.HasValue || request.OnlyActives == false)
            {
                if (request.OnlyApproved)
                {
                    query = _repo.Query(c => c.IsApproved);
                }
                else
                { query = _repo.Query(); }

            }
            else
            {
                if (request.OnlyApproved)
                {
                    query = _repo.Query(c => c.IsActive && c.IsApproved);
                }
                else
                { query = _repo.Query(c => c.IsActive); }
            }
        }

        int totalCount = await query.CountAsync(cancellationToken);
        var pagedCartItems = await query
    .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);
        var CommentsDto = pagedCartItems.Select(c => new CommentDto
        {
            Id = c.Id,
            Content = c.Content,
            UserId = c.UserId,
            UserName = c.UserName,
            ParentName = c.ParentName,
            TargetTitle = c.TargetTitle,
            CreatedAt = c.CreatedAt,
            ParentId = c.ParentId,
            IsApproved = c.IsApproved,
            TargetId = c.TargetId,
            TargetType =  c.TargetType.TargetTypeToDisplay(),
            IsActive = c.IsActive

        }).ToList();

        dynamic? config = null;

        if (request.ByConfig == true)
        {
            config = await _configRepo.GetByEntityNameAsync("comments");
        }

        var resultDto = new ListDto<CommentDto>
        {
            Records = CommentsDto,
            ColumnsJson = config?.ColumnsJson,
            ActionsJson = config?.ActionsJson,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };

        return ServiceResult<ListDto<CommentDto>>.Ok(resultDto);


    }

    public async Task<ServiceResult<List<CommentDto>>> Handle(GetCommentsByTargetQuery request, CancellationToken cancellationToken)
    {
        var limit = request.Limit ?? 5;
        IQueryable<Comment> query;
        if (request.Q is not null && request.Q.Length > 0)
        {
            query = _repo.Query(b => b.IsActive && b.TargetId == request.TargetId && b.TargetType == request.TargetType && b.Content.Contains(request.Q))
                .OrderBy(c => c.CreatedAt).Take(limit);

        }
        else
        {
            query = _repo.Query(b => b.IsActive && b.TargetId == request.TargetId && b.TargetType == request.TargetType)
                .OrderBy(c => c.CreatedAt).Take(limit);
        }

        var dto = query.Select(c => new CommentDto
        {
            Id = c.Id,
            Content = c.Content,
            UserId = c.UserId,
            UserName = c.UserName,
            ParentName = c.ParentName,
            TargetTitle = c.TargetTitle,
            CreatedAt = c.CreatedAt,
            ParentId = c.ParentId,
            IsApproved = c.IsApproved,
            TargetId = c.TargetId,
            TargetType = c.TargetType.TargetTypeToDisplay(),
        })
        .OrderBy(c => c.CreatedAt)
        .ToList();
        foreach (var item in dto)
        {
            var rate =  _rateRepo.Query(r => r.IsActive && r.TargetId == request.TargetId && r.TargetType == request.TargetType && r.UserId == item.UserId).FirstOrDefault(); 
            if (rate is not null)
            {
                item.UserRate = rate.Value.Value;

            }
            var user = await _userRepo.GetByIdAsync(item.UserId);
            if (user is not null)
            {
                item.UserFullName = user.FullName;
                item.UserImage = user.Image;
            }
        }
        return ServiceResult<List<CommentDto>>.Ok(dto);
    }
    public async Task<ServiceResult<CommentDto?>> Handle(GetCommentByIdQuery request, CancellationToken cancellationToken)
    {
        var comment = _repo.Query(b => b.Id == request.Id)
                   .FirstOrDefault();

        if (comment == null)
            return ServiceResult<CommentDto?>.Failed("مقالهٔ موردنظر پیدا نشد.");
        var commentDto = new CommentDto
        {
            Id = comment.Id,
            Content = comment.Content,
            UserId = comment.UserId,
            UserName = comment.UserName,
            ParentName = comment.ParentName,
            TargetTitle = comment.TargetTitle,
            CreatedAt = comment.CreatedAt,
            ParentId = comment.ParentId,
            IsApproved = comment.IsApproved,
            TargetId = comment.TargetId,
            TargetType = comment.TargetType.TargetTypeToDisplay(),
        };
        return ServiceResult<CommentDto?>.Ok(commentDto);
    }

}