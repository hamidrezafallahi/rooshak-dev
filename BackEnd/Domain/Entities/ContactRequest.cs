namespace OnlineShop.Domain.Entities
{
    /// <summary>
    /// درخواست ثبت شده توسط بازدیدکننده سایت.
    /// مفهوم آن به سایت بستگی دارد: در یک پروژه «همکاری با ما» و در پروژه ای دیگر «مشاوره خرید» است.
    /// </summary>
    public class ContactRequest : BaseEntity
    {
        private ContactRequest() { }

        public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
        public string PhoneNumber { get; private set; } = string.Empty;
        public string? Email { get; private set; }
        public string? Address { get; private set; }

        /// <summary>
        /// زمان مناسب برای تماس (متن آزاد، مثلا «شنبه تا چهارشنبه ۹ تا ۱۳»)
        /// </summary>
        public string? PreferredContactTime { get; private set; }
        public string? Message { get; private set; }

        /// <summary>
        /// آیا ادمین این درخواست را بررسی / پیگیری کرده است؟
        /// </summary>
        public bool IsReviewed { get; private set; }
        public string? AdminNote { get; private set; }

        // ===== Factory Method =====
        public static ContactRequest Create(
            string firstName,
            string lastName,
            string phoneNumber,
            string? email,
            string? address,
            string? preferredContactTime,
            string? message,
            int currentUserId)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("FirstName cannot be empty.", nameof(firstName));

            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("LastName cannot be empty.", nameof(lastName));

            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("PhoneNumber cannot be empty.", nameof(phoneNumber));

            var request = new ContactRequest
            {
                FirstName = firstName.Trim(),
                LastName = lastName.Trim(),
                PhoneNumber = phoneNumber.Trim(),
                Email = NullIfWhiteSpace(email),
                Address = NullIfWhiteSpace(address),
                PreferredContactTime = NullIfWhiteSpace(preferredContactTime),
                Message = NullIfWhiteSpace(message),
                IsReviewed = false
            };

            request.MarkCreated(currentUserId);
            return request;
        }

        // ===== Behavior Methods =====
        public void Update(
            string? firstName,
            string? lastName,
            string? phoneNumber,
            string? email,
            string? address,
            string? preferredContactTime,
            string? message,
            int currentUserId)
        {
            if (!string.IsNullOrWhiteSpace(firstName))
                FirstName = firstName.Trim();

            if (!string.IsNullOrWhiteSpace(lastName))
                LastName = lastName.Trim();

            if (!string.IsNullOrWhiteSpace(phoneNumber))
                PhoneNumber = phoneNumber.Trim();

            if (email != null) Email = NullIfWhiteSpace(email);
            if (address != null) Address = NullIfWhiteSpace(address);
            if (preferredContactTime != null) PreferredContactTime = NullIfWhiteSpace(preferredContactTime);
            if (message != null) Message = NullIfWhiteSpace(message);

            MarkUpdated(currentUserId);
        }

        public void SetReview(bool isReviewed, string? adminNote, int currentUserId)
        {
            IsReviewed = isReviewed;
            if (adminNote != null)
                AdminNote = NullIfWhiteSpace(adminNote);

            MarkUpdated(currentUserId);
        }

        private static string? NullIfWhiteSpace(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
