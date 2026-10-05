using Common;
using MediatR;

namespace Application.Commands
{
    /// <summary>
    /// ثبت درخواست توسط بازدیدکننده (بدون نیاز به ورود).
    /// </summary>
    public class CreateContactRequestCommand : IRequest<ServiceResult<IdDto>>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? PreferredContactTime { get; set; }
        public string? Message { get; set; }

        /// <summary>
        /// Honeypot: این فیلد در فرم مخفی است و کاربر واقعی آن را پر نمی کند.
        /// </summary>
        public string? Website { get; set; }
    }

    public class UpdateContactRequestCommand : IRequest<ServiceResult<IdDto>>
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? PreferredContactTime { get; set; }
        public string? Message { get; set; }
        public bool? IsReviewed { get; set; }
        public string? AdminNote { get; set; }
    }

    public class DeleteContactRequestCommand : IRequest<ServiceResult<IdDto>>
    {
        public int Id { get; set; }
    }
}
