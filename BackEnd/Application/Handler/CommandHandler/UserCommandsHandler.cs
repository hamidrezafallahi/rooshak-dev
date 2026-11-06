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
    public class UserCommandsHandler(IUserRepository _userRepo,
            IProductOfferRepository _productOfferRepo,
            IUploaderService _uploaderService,
            IUserAddressRepository _addressRepo,
            IHttpContextAccessor _accessor) :
        IRequestHandler<UpdateUserProfileCommand, ServiceResult<IdDto>>,
        IRequestHandler<AddUserAddressCommand, ServiceResult<IdDto>>,
        IRequestHandler<ActiveUserAddressCommand, ServiceResult<IdDto>>,
        IRequestHandler<DeleteUserAddressCommand, ServiceResult<IdDto>>,
        IRequestHandler<SetDefaultUserAddressCommand, ServiceResult<IdDto>>,
        IRequestHandler<UpdateUserAddressCommand, ServiceResult<IdDto>>
    {

        public async Task<ServiceResult<IdDto>> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Failed("Unauthorized");
            var user = await _userRepo.GetByIdAsync(request.UserId);
            if (user == null)
                return ServiceResult<IdDto>.Failed("Unauthorized");
            if (request.UserImageFile != null)
            {
                var oldImage = user.Image;
                user.UpdateProfile(request.FullName, request.PhoneNumber, request.UserDescription, user.Id);
                var uploadDto = new UploadDTO
                {
                    File = request.UserImageFile,
                    Path = UploadPaths.Users(user.Id)
                };
                var imageUrl = await _uploaderService.UploadAsWebp(uploadDto);
                if (!UploadPaths.IsStoredPath(imageUrl))
                    return ServiceResult<IdDto>.Failed("آپلود تصویر پروفایل ناموفق بود");

                user.SetImage(imageUrl!, user.Id);
                await _userRepo.SaveChangesAsync(cancellationToken);

                // Only now: a failed upload or save must leave the current picture in place.
                await _uploaderService.DeleteStoredFile(oldImage, UploadPaths.Users(user.Id));
            }
            await _userRepo.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = user.Id });
        }
        public async Task<ServiceResult<IdDto>> Handle(AddUserAddressCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Failed("Unauthorized");

            var user = await _userRepo.GetByIdAsync(userId.Value);
            if (user == null)
                return ServiceResult<IdDto>.Failed("Unauthorized");
            var address = UserAddress.Create(
                userId: request.UserId ?? userId.Value,
                name: request.Name,
                phoneNumber: request.PhoneNumber,
                city: request.City,
                state: request.State,
                postalCode: request.PostalCode,
                fullAddress: request.FullAddress,
                currentUserId: userId.Value
            );

            user.AddAddress(address, userId.Value);
            await _userRepo.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = address.Id });
        }
        public async Task<ServiceResult<IdDto>> Handle(ActiveUserAddressCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Failed("Unauthorized");

            var address = await _addressRepo.GetByIdAsync(request.Id);
            if (address == null)
                return ServiceResult<IdDto>.Failed("آدرس پیدا نشد");

            address.SetActive(request.IsActive, userId.Value);
            await _addressRepo.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = address.Id });
        }
        public async Task<ServiceResult<IdDto>> Handle(DeleteUserAddressCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Failed("Unauthorized");

            var address = await _addressRepo.GetByIdAsync(request.Id);
            if (address == null)
                return ServiceResult<IdDto>.Failed("آدرس پیدا نشد");

            address.Delete(userId.Value);
            await _addressRepo.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = address.Id });
        }
        public async Task<ServiceResult<IdDto>> Handle(UpdateUserAddressCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Failed("Unauthorized");

            var user = await _userRepo.GetByIdAsync(userId.Value);
            if (user == null)
                return ServiceResult<IdDto>.Failed("Unauthorized");

            var address = await _addressRepo.GetByIdAsync(request.Id);
            if (address == null || address.UserId != userId.Value)
                return ServiceResult<IdDto>.Failed("آدرس پیدا نشد");

            address.Update(
                userId.Value,
                request.Name,
                request.PhoneNumber,
                request.City,
                request.State,
                request.PostalCode,
                request.FullAddress
            );
            await _addressRepo.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = user.Id });
        }
        public async Task<ServiceResult<IdDto>> Handle(SetDefaultUserAddressCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Failed("Unauthorized");

            var addresses = await _addressRepo.GetAddressesByUserIdAsync(userId.Value);
            if (!addresses.Any())
                return ServiceResult<IdDto>.Failed("آدرسی یافت نشد");

            // همه رو غیرفعال می‌کنیم
            foreach (var addr in addresses)
            {
                addr.SetDefault(addr.Id == request.Id, userId.Value);
            }

            await _addressRepo.SaveChangesAsync(cancellationToken);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = request.Id });
        }
    }
}
