using Domain.Enums;
using Domain.Enums.Domain.Enums;

namespace Application.Dtos
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethodDto PaymentMethod { get; set; }
        public string? TransactionId { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime PaymentDate { get; set; }

    }
    public class PaymentStartDto
    {
        public string PaymentUrl { get; set; } = string.Empty;
    }

    public class PaymentVerifyResponseDto
    {
        public bool IsSuccess { get; set; }
        public int OrderId { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class PaymentRequestResult
    {
        public bool IsSuccess { get; set; }
        public string Authority { get; set; } = default!;
        public string PaymentUrl { get; set; } = default!;
        public string? ErrorMessage { get; set; }
    }

    public class PaymentVerifyResult
    {
        public bool IsSuccess { get; set; }
        public string RefId { get; set; } = default!;
        public string? ErrorMessage { get; set; }
    }

}