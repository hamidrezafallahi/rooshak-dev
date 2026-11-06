using OnlineShop.Domain.Common;

using Domain.Enums;
using Domain.Enums.Domain.Enums;

namespace OnlineShop.Domain.Entities
{
    public class Payment : BaseEntity
    {
        private Payment() { }

        public int OrderId { get; private set; }
        public Order Order { get; private set; } = default!;

        public DateTime PaymentDate { get; private set; }
        public decimal Amount { get; private set; }
        public int PaymentMethodId { get; private set; }
        public PaymentMethod PaymentMethod { get; private set; } = default!;
        public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
        public string? TransactionId { get; private set; }

        // ================= Factory =================
        public static Payment Create(int orderId, decimal amount, int paymentMethodId, int currentUserId)
        {
            if (amount <= 0)
                throw new DomainException("مبلغ پرداخت باید بیشتر از صفر باشد.");

            var payment = new Payment
            {
                OrderId = orderId,
                Amount = amount,
                PaymentMethodId = paymentMethodId,
                PaymentDate = DateTime.UtcNow,
                Status = PaymentStatus.Pending
            };

            payment.MarkCreated(currentUserId);
            return payment;
        }

        // ================= Behaviors =================
        public void MarkAsPaid(string transactionId, int currentUserId)
        {
            Status = PaymentStatus.Success;
            TransactionId = transactionId;
            PaymentDate = DateTime.UtcNow;
            MarkUpdated(currentUserId);
        }

        public void MarkAsFailed(string transactionId, int currentUserId)
        {
            Status = PaymentStatus.Failed;
            TransactionId = transactionId;
            PaymentDate = DateTime.UtcNow;
            MarkUpdated(currentUserId);
        }

        public void Cancel(int currentUserId)
        {
            Status = PaymentStatus.Cancelled;
            MarkUpdated(currentUserId);
        }

      
        public void SetTransactionId(string transactionId, int userId)
        {
            TransactionId = transactionId;
            MarkUpdated(userId);
        }

    }
}
