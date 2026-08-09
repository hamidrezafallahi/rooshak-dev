using Application.Commands;
using Application.Common;
using Application.Common.Interfaces;
using Application.Dtos;
using Common;
using Domain.Enums.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Enums;
using OnlineShop.Domain.Interfaces;

namespace Application.Handler.CommandHandler
{
    // ========================= Payment Command Handlers =========================
    public class PaymentCommandHandlers(IPaymentRepository _repo, IHttpContextAccessor _accessor) :
        IRequestHandler<CreatePaymentCommand, ServiceResult<IdDto>>,
        IRequestHandler<MarkPaymentAsPaidCommand, ServiceResult<IdDto>>,
        IRequestHandler<MarkPaymentAsFailedCommand, ServiceResult<IdDto>>,
        IRequestHandler<CancelPaymentCommand, ServiceResult<IdDto>>,
        IRequestHandler<ActivePaymentCommand, ServiceResult<IdDto>>,
        IRequestHandler<DeletePaymentCommand, ServiceResult<IdDto>>
    {
        

        // ================= Create Payment =================
        public async Task<ServiceResult<IdDto>> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Failed("Unauthorized");

            var payment = Payment.Create(request.OrderId, request.Amount, request.PaymentMethodId, userId.Value);
            await _repo.AddAsync(payment);
            await _repo.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = payment.Id });
        }

        // ================= Mark Payment As Paid =================
        public async Task<ServiceResult<IdDto>> Handle(MarkPaymentAsPaidCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null) return ServiceResult<IdDto>.Failed("Unauthorized");

            var payment = await _repo.GetByIdAsync(request.PaymentId);
            if (payment == null) return ServiceResult<IdDto>.Failed("Payment not found");

            payment.MarkAsPaid(request.TransactionId, userId.Value);
            await _repo.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = payment.Id });
        }

        // ================= Mark Payment As Failed =================
        public async Task<ServiceResult<IdDto>> Handle(MarkPaymentAsFailedCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null) return ServiceResult<IdDto>.Failed("Unauthorized");

            var payment = await _repo.GetByIdAsync(request.PaymentId);
            if (payment == null) return ServiceResult<IdDto>.Failed("Payment not found");

            payment.MarkAsFailed(request.TransactionId, userId.Value);
            await _repo.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = payment.Id });
        }

        // ================= Cancel Payment =================
        public async Task<ServiceResult<IdDto>> Handle(CancelPaymentCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null) return ServiceResult<IdDto>.Failed("Unauthorized");

            var payment = await _repo.GetByIdAsync(request.PaymentId);
            if (payment == null) return ServiceResult<IdDto>.Failed("Payment not found");

            payment.Cancel(userId.Value);
            await _repo.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = payment.Id });
        }
        // ================= Active Payment =================
        public async Task<ServiceResult<IdDto>> Handle(ActivePaymentCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null) return ServiceResult<IdDto>.Failed("Unauthorized");

            var payment = await _repo.GetByIdAsync(request.Id);
            if (payment == null) return ServiceResult<IdDto>.Failed("Payment not found");

            payment.SetActive(request.IsActive, userId.Value);
            await _repo.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = payment.Id });
        }

        // ================= Delete Payment =================
        public async Task<ServiceResult<IdDto>> Handle(DeletePaymentCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null) return ServiceResult<IdDto>.Failed("Unauthorized");

            var payment = await _repo.GetByIdAsync(request.PaymentId);
            if (payment == null) return ServiceResult<IdDto>.Failed("Payment not found");

            payment.Delete(userId.Value);
            await _repo.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = payment.Id });
        }
    }

    // ================= Start Payment Handler =================
    public class RequestPaymentCommandHandler(
            IOrderRepository _orderRepo,
            IPaymentRepository _paymentRepo,
            IHttpContextAccessor _accessor,
            IPaymentGateway _gateway,
            IConfiguration _configuration) :
        IRequestHandler<RequestPaymentCommand, ServiceResult<PaymentStartDto>>
    {
        public async Task<ServiceResult<PaymentStartDto>> Handle(RequestPaymentCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<PaymentStartDto>.Failed("Unauthorized");

            var order = await _orderRepo.GetByIdAsync(request.OrderId);
            if (order == null || order.UserId != userId)
                return ServiceResult<PaymentStartDto>.Failed("Order not found");

            if (order.Status != OrderStatus.Pending)
                return ServiceResult<PaymentStartDto>.Failed("Order not payable");

            var payment = Payment.Create(order.Id, order.FinalPrice, order.PaymentMethodId, userId.Value);
            await _paymentRepo.AddAsync(payment);
            await _paymentRepo.SaveChangesAsync(cancellationToken);

            var callbackBase = _configuration["Zarinpal:CallbackUrl"]
                ?? "http://localhost:3000/fa/payment";
            var separator = callbackBase.Contains('?', StringComparison.Ordinal) ? "&" : "?";
            var callbackUrl = $"{callbackBase}{separator}orderId={order.Id}";

            var gatewayResult = await _gateway.RequestPaymentAsync(
                payment.Amount,
                callbackUrl: callbackUrl,
                description: $"Order #{payment.OrderId}"
            );

            if (!gatewayResult.IsSuccess || string.IsNullOrWhiteSpace(gatewayResult.Authority))
            {
                payment.MarkAsFailed(gatewayResult.Authority ?? string.Empty, userId.Value);
                await _paymentRepo.SaveChangesAsync(cancellationToken);
                return ServiceResult<PaymentStartDto>.Failed(
                    gatewayResult.ErrorMessage ?? "Gateway request failed");
            }

            payment.SetTransactionId(gatewayResult.Authority, userId.Value);
            await _paymentRepo.SaveChangesAsync(cancellationToken);

            return ServiceResult<PaymentStartDto>.Ok(new PaymentStartDto
            {
                PaymentUrl = gatewayResult.PaymentUrl
            });
        }
    }

    // ================= Verify Payment Handler =================
    public class VerifyPaymentCommandHandler(
            IOrderRepository _orderRepo,
            IPaymentRepository _paymentRepo,
            ICartRepository _cartRepo,
            IHttpContextAccessor _accessor,
            IPaymentGateway _gateway) :
        IRequestHandler<VerifyPaymentCommand, ServiceResult<PaymentVerifyResponseDto>>
    {
        public async Task<ServiceResult<PaymentVerifyResponseDto>> Handle(
            VerifyPaymentCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<PaymentVerifyResponseDto>.Failed("Unauthorized");

            if (string.IsNullOrWhiteSpace(request.Authority))
                return ServiceResult<PaymentVerifyResponseDto>.Failed("Authority is required");

            var payment = await _paymentRepo.GetByTransactionIdAsync(request.Authority);

            // After a successful verify, TransactionId becomes gateway RefId.
            // Callback refresh still sends Authority — fall back via orderId.
            if (payment == null && request.OrderId.HasValue)
            {
                var orderPayments = await _paymentRepo.GetByOrderIdAsync(request.OrderId.Value);
                payment = orderPayments
                    .OrderByDescending(p => p.PaymentDate)
                    .FirstOrDefault(p =>
                        p.Status == PaymentStatus.Success ||
                        p.TransactionId == request.Authority);
            }

            if (payment == null)
                return ServiceResult<PaymentVerifyResponseDto>.Failed("Payment not found");

            var order = await _orderRepo.GetOrderWithItemsAsync(payment.OrderId);
            if (order == null || order.UserId != userId)
                return ServiceResult<PaymentVerifyResponseDto>.Failed("Order not found");

            // Idempotent: already paid
            if (payment.Status == PaymentStatus.Success && order.Status == OrderStatus.Paid)
            {
                return ServiceResult<PaymentVerifyResponseDto>.Ok(new PaymentVerifyResponseDto
                {
                    IsSuccess = true,
                    OrderId = order.Id,
                    TransactionId = payment.TransactionId ?? request.Authority,
                    Amount = payment.Amount
                });
            }

            if (order.Status != OrderStatus.Pending)
                return ServiceResult<PaymentVerifyResponseDto>.Failed("Order not payable");

            var isOkStatus = string.Equals(request.Status, "OK", StringComparison.OrdinalIgnoreCase);
            if (!isOkStatus)
            {
                payment.MarkAsFailed(request.Authority, userId.Value);
                await _paymentRepo.SaveChangesAsync(cancellationToken);

                return ServiceResult<PaymentVerifyResponseDto>.Ok(new PaymentVerifyResponseDto
                {
                    IsSuccess = false,
                    OrderId = order.Id,
                    TransactionId = request.Authority,
                    Amount = payment.Amount,
                    ErrorMessage = "Payment cancelled or failed"
                });
            }

            var verifyResult = await _gateway.VerifyPaymentAsync(request.Authority, payment.Amount);
            if (!verifyResult.IsSuccess)
            {
                payment.MarkAsFailed(request.Authority, userId.Value);
                await _paymentRepo.SaveChangesAsync(cancellationToken);

                return ServiceResult<PaymentVerifyResponseDto>.Ok(new PaymentVerifyResponseDto
                {
                    IsSuccess = false,
                    OrderId = order.Id,
                    TransactionId = request.Authority,
                    Amount = payment.Amount,
                    ErrorMessage = verifyResult.ErrorMessage ?? "Payment verification failed"
                });
            }

            payment.MarkAsPaid(verifyResult.RefId, userId.Value);
            order.Confirm(userId.Value);
            order.Pay(userId.Value);
            await _cartRepo.ClearCartAsync(userId.Value);

            await _paymentRepo.SaveChangesAsync(cancellationToken);
            await _orderRepo.SaveChangesAsync(cancellationToken);

            return ServiceResult<PaymentVerifyResponseDto>.Ok(new PaymentVerifyResponseDto
            {
                IsSuccess = true,
                OrderId = order.Id,
                TransactionId = verifyResult.RefId,
                Amount = payment.Amount
            });
        }
    }
}
