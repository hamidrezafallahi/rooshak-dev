using Application.Commands;
using Application.Common;
using Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
using System.Text;
using System.Text.RegularExpressions;

namespace Application.Handler.CommandHandler
{
    public class ContactRequestCommandHandler(IContactRequestRepository _repo, IHttpContextAccessor _accessor) :
        IRequestHandler<CreateContactRequestCommand, ServiceResult<IdDto>>,
        IRequestHandler<UpdateContactRequestCommand, ServiceResult<IdDto>>,
        IRequestHandler<DeleteContactRequestCommand, ServiceResult<IdDto>>
    {
        private static readonly Regex PhoneRegex = new(@"^(\+98|0098|0)?9\d{9}$", RegexOptions.Compiled);
        private static readonly Regex EmailRegex = new(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.Compiled);

        /// <summary>
        /// ثبت درخواست توسط بازدیدکننده. ورود کاربر الزامی نیست.
        /// </summary>
        public async Task<ServiceResult<IdDto>> Handle(CreateContactRequestCommand request, CancellationToken cancellationToken)
        {
            // Honeypot: ربات ها فیلد مخفی را پر می کنند؛ بدون ذخیره، پاسخ موفق برمی گردانیم.
            if (!string.IsNullOrWhiteSpace(request.Website))
                return ServiceResult<IdDto>.Ok(new IdDto { Id = 0 });

            var firstName = request.FirstName?.Trim() ?? string.Empty;
            var lastName = request.LastName?.Trim() ?? string.Empty;
            var phoneNumber = NormalizePhone(request.PhoneNumber);
            var email = request.Email?.Trim();

            if (firstName.Length < 2 || firstName.Length > 100)
                return ServiceResult<IdDto>.Failed("نام را به درستی وارد کنید");

            if (lastName.Length < 2 || lastName.Length > 100)
                return ServiceResult<IdDto>.Failed("نام خانوادگی را به درستی وارد کنید");

            if (!PhoneRegex.IsMatch(phoneNumber))
                return ServiceResult<IdDto>.Failed("شماره تماس معتبر نیست");

            if (!string.IsNullOrEmpty(email) && (email.Length > 200 || !EmailRegex.IsMatch(email)))
                return ServiceResult<IdDto>.Failed("ایمیل معتبر نیست");

            if ((request.Address?.Length ?? 0) > 500)
                return ServiceResult<IdDto>.Failed("آدرس نباید بیشتر از ۵۰۰ کاراکتر باشد");

            if ((request.PreferredContactTime?.Length ?? 0) > 200)
                return ServiceResult<IdDto>.Failed("زمان مناسب تماس نباید بیشتر از ۲۰۰ کاراکتر باشد");

            if ((request.Message?.Length ?? 0) > 2000)
                return ServiceResult<IdDto>.Failed("متن پیام نباید بیشتر از ۲۰۰۰ کاراکتر باشد");

            // کاربر مهمان => 0
            var userId = _accessor.HttpContext?.GetUserId() ?? 0;

            var entity = ContactRequest.Create(
                firstName,
                lastName,
                phoneNumber,
                email,
                request.Address,
                request.PreferredContactTime,
                request.Message,
                userId
            );

            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = entity.Id });
        }

        public async Task<ServiceResult<IdDto>> Handle(UpdateContactRequestCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Failed("Unauthorized");

            var entity = await _repo.GetByIdAsync(request.Id);
            if (entity == null)
                return ServiceResult<IdDto>.Failed("درخواست یافت نشد");

            string? phoneNumber = null;
            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                phoneNumber = NormalizePhone(request.PhoneNumber);
                if (!PhoneRegex.IsMatch(phoneNumber))
                    return ServiceResult<IdDto>.Failed("شماره تماس معتبر نیست");
            }

            entity.Update(
                request.FirstName,
                request.LastName,
                phoneNumber,
                request.Email,
                request.Address,
                request.PreferredContactTime,
                request.Message,
                userId.Value
            );

            if (request.IsReviewed.HasValue || request.AdminNote != null)
                entity.SetReview(request.IsReviewed ?? entity.IsReviewed, request.AdminNote, userId.Value);

            await _repo.SaveChangesAsync(cancellationToken);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = entity.Id });
        }

        public async Task<ServiceResult<IdDto>> Handle(DeleteContactRequestCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Failed("Unauthorized");

            var entity = await _repo.GetByIdAsync(request.Id);
            if (entity == null)
                return ServiceResult<IdDto>.Failed("درخواست یافت نشد");

            entity.Delete(userId.Value);

            await _repo.SaveChangesAsync(cancellationToken);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = entity.Id });
        }

        /// <summary>
        /// ارقام فارسی/عربی را لاتین می کند و فاصله و خط تیره را حذف می کند.
        /// </summary>
        private static string NormalizePhone(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var sb = new StringBuilder(value.Length);
            foreach (var ch in value.Trim())
            {
                if (ch >= '۰' && ch <= '۹')
                    sb.Append((char)('0' + (ch - '۰')));
                else if (ch >= '٠' && ch <= '٩')
                    sb.Append((char)('0' + (ch - '٠')));
                else if (ch == ' ' || ch == '-')
                    continue;
                else
                    sb.Append(ch);
            }
            return sb.ToString();
        }
    }
}
