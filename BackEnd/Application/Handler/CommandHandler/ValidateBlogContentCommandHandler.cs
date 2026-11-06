using Application.Commands;
using Application.Common;
using Application.Common.Interfaces;
using Application.Dtos;
using Common;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Handler.CommandHandler;

public class ValidateBlogContentCommandHandler(
    IBlogContentQualityService qualityService,
    IHttpContextAccessor accessor)
    : IRequestHandler<ValidateBlogContentCommand, ServiceResult<BlogContentQualityResultDto>>
{
    public async Task<ServiceResult<BlogContentQualityResultDto>> Handle(
        ValidateBlogContentCommand request,
        CancellationToken cancellationToken)
    {
        var userId = accessor.HttpContext.GetUserId();
        if (userId is null)
            return ServiceResult<BlogContentQualityResultDto>.Unauthorized();

        var result = await qualityService.ValidateAsync(request.Payload, cancellationToken);
        return ServiceResult<BlogContentQualityResultDto>.Ok(result);
    }
}
