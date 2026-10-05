namespace OnlineShop.Domain.Entities
{
    /// <summary>
    /// سوالات متداول سایت (پرسش و پاسخ) که در فرانت به صورت لیست نمایش داده می شود.
    /// </summary>
    public class Faq : BaseEntity
    {
        private Faq() { }

        public string Question { get; private set; } = string.Empty;
        public string Answer { get; private set; } = string.Empty;

        /// <summary>
        /// ترتیب نمایش؛ عدد کوچک تر بالاتر نمایش داده می شود.
        /// </summary>
        public int DisplayOrder { get; private set; }

        // ===== Factory Method =====
        public static Faq Create(
            string question,
            string answer,
            int displayOrder,
            int currentUserId)
        {
            if (string.IsNullOrWhiteSpace(question))
                throw new ArgumentException("Question cannot be empty.", nameof(question));

            if (string.IsNullOrWhiteSpace(answer))
                throw new ArgumentException("Answer cannot be empty.", nameof(answer));

            var faq = new Faq
            {
                Question = question.Trim(),
                Answer = answer.Trim(),
                DisplayOrder = displayOrder
            };

            faq.MarkCreated(currentUserId);
            return faq;
        }

        // ===== Behavior Methods =====
        public void Update(
            string? question,
            string? answer,
            int? displayOrder,
            int currentUserId)
        {
            if (!string.IsNullOrWhiteSpace(question))
                Question = question.Trim();

            if (!string.IsNullOrWhiteSpace(answer))
                Answer = answer.Trim();

            if (displayOrder.HasValue)
                DisplayOrder = displayOrder.Value;

            MarkUpdated(currentUserId);
        }
    }
}
