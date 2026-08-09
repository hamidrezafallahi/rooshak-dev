using Application.Dtos;
using Common;
using Domain.Enums;
using MediatR;

namespace Application.Commands
{
    public class CreatePaymentCommand : IRequest<ServiceResult<IdDto>>
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public int PaymentMethodId { get; set; }
    }
    public class RequestPaymentCommand : IRequest<ServiceResult<PaymentStartDto>>
    {
        public int OrderId { get; set; }

    }

    public class VerifyPaymentCommand : IRequest<ServiceResult<PaymentVerifyResponseDto>>
    {
        public string Authority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? OrderId { get; set; }
    }

    public class MarkPaymentAsPaidCommand : IRequest<ServiceResult<IdDto>>
    {
        public int PaymentId { get; set; }
        public string TransactionId { get; set; } = string.Empty;
    }

    public class MarkPaymentAsFailedCommand : IRequest<ServiceResult<IdDto>>
    {
        public int PaymentId { get; set; }
        public string TransactionId { get; set; } = string.Empty;
    }

    public class CancelPaymentCommand : IRequest<ServiceResult<IdDto>>
    {
        public int PaymentId { get; set; }
    }
    public class ActivePaymentCommand : ActiveCommand, IRequest<ServiceResult<IdDto>> { }
    public class DeletePaymentCommand : IRequest<ServiceResult<IdDto>>
    {
        public int PaymentId { get; set; }
    }
}
