using OnlineShop.Domain.Common;
using Application.Commands;
using Application.Common;
using Application.Common.Interfaces;
using Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
using Services.Services.Uploader.DTO;

namespace Application.Handler.CommandHandler
{
    public class AnnouncementBarCommandHandler(
        IAnnouncementBarRepository _repo,
        IHttpContextAccessor _accessor,
        IUploaderService _uploader,
        IUnitOfWork _unitOfWork) :
        IRequestHandler<CreateAnnouncementBarCommand, ServiceResult<IdDto>>,
        IRequestHandler<UpdateAnnouncementBarCommand, ServiceResult<IdDto>>,
        IRequestHandler<ActiveAnnouncementBarCommand, ServiceResult<IdDto>>,
        IRequestHandler<DeleteAnnouncementBarCommand, ServiceResult<IdDto>>
    {
        public async Task<ServiceResult<IdDto>> Handle(CreateAnnouncementBarCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();

            AnnouncementBar bar;
            try
            {
                bar = AnnouncementBar.Create(
                    request.MessageFa, request.MessageEn, request.LinkUrl,
                    request.BackgroundColor, request.TextColor, request.HeightPx,
                    request.StartsAt, request.EndsAt, request.DisplayOrder, userId.Value);
            }
            catch (DomainException ex)
            {
                return ServiceResult<IdDto>.Failed(ex.Message);
            }

            // The id names the upload folder, so the row is saved before the file; the
            // transaction takes it back if the file cannot be stored.
            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            await _repo.AddAsync(bar);
            await _repo.SaveChangesAsync(cancellationToken);

            if (request.BackgroundImageUrl is { Length: > 0 })
            {
                var path = await UploadBackground(request.BackgroundImageUrl, bar.Id);
                if (path is null)
                    return ServiceResult<IdDto>.Failed("آپلود تصویر پس‌زمینه ناموفق بود");

                bar.SetBackgroundImage(path, userId.Value);
                await _repo.SaveChangesAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = bar.Id });
        }

        public async Task<ServiceResult<IdDto>> Handle(UpdateAnnouncementBarCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();

            var bar = await _repo.GetByIdAsync(request.Id);
            if (bar == null)
                return ServiceResult<IdDto>.Failed("نوار اعلان یافت نشد");

            try
            {
                bar.Update(
                    request.MessageFa, request.MessageEn, request.LinkUrl,
                    request.BackgroundColor, request.TextColor, request.HeightPx,
                    request.StartsAt, request.EndsAt, request.DisplayOrder, userId.Value);
            }
            catch (DomainException ex)
            {
                return ServiceResult<IdDto>.Failed(ex.Message);
            }

            var folder = UploadPaths.AnnouncementBars(bar.Id);
            var oldImage = bar.BackgroundImageUrl;

            if (request.RemoveBackgroundImage && !string.IsNullOrWhiteSpace(bar.BackgroundImageUrl))
                bar.ClearBackgroundImage(userId.Value);

            if (request.BackgroundImageUrl is { Length: > 0 })
            {
                var path = await UploadBackground(request.BackgroundImageUrl, bar.Id);
                if (path is null)
                    return ServiceResult<IdDto>.Failed("آپلود تصویر پس‌زمینه ناموفق بود");

                bar.SetBackgroundImage(path, userId.Value);
            }

            await _repo.SaveChangesAsync(cancellationToken);

            // Only now: a failed upload or save must leave the current image in place.
            if (!string.IsNullOrWhiteSpace(oldImage) && oldImage != bar.BackgroundImageUrl)
                await _uploader.DeleteStoredFile(oldImage, folder);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = bar.Id });
        }

        public async Task<ServiceResult<IdDto>> Handle(ActiveAnnouncementBarCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();

            var bar = await _repo.GetByIdAsync(request.Id);
            if (bar == null)
                return ServiceResult<IdDto>.Failed("نوار اعلان یافت نشد");

            bar.SetActive(request.IsActive, userId.Value);
            await _repo.SaveChangesAsync(cancellationToken);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = bar.Id });
        }

        public async Task<ServiceResult<IdDto>> Handle(DeleteAnnouncementBarCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();

            var bar = await _repo.GetByIdAsync(request.Id);
            if (bar == null)
                return ServiceResult<IdDto>.Failed("نوار اعلان یافت نشد");

            if (!string.IsNullOrWhiteSpace(bar.BackgroundImageUrl))
                await _uploader.DeleteStoredFile(bar.BackgroundImageUrl, UploadPaths.AnnouncementBars(bar.Id));

            bar.Delete(userId.Value);
            await _repo.SaveChangesAsync(cancellationToken);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = bar.Id });
        }

        private async Task<string?> UploadBackground(IFormFile file, int barId)
        {
            var stored = await _uploader.UploadAsWebp(new UploadDTO
            {
                File = file,
                Path = UploadPaths.AnnouncementBars(barId),
            });
            return UploadPaths.IsStoredPath(stored) ? stored : null;
        }
    }
}
