using Application.Dtos;
using Application.Queries;
using Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;

public class PaymentQueryHandlers(IPaymentRepository _repo,IEntityConfigRepository _configRepo) :
    IRequestHandler<GetAllPaymentsQuery, ServiceResult<ListDto<PaymentDto?>>>,
        IRequestHandler<GetPaymentByIdQuery, ServiceResult<PaymentDto?>>,
        IRequestHandler<GetPaymentsByOrderIdQuery, ServiceResult<IEnumerable<PaymentDto>>>
{
 
    public async Task<ServiceResult<ListDto<PaymentDto?>>> Handle(GetAllPaymentsQuery request, CancellationToken cancellationToken)
    {
        int pageNumber = request.page ?? 1;
        int pageSize = request.pageSize ?? 10;
        IQueryable<Payment> query;
        if (request.Q is not null && request.Q.Length > 0)
        {
            if (!request.OnlyActives.HasValue || request.OnlyActives == false)
            {
                query = _repo.Query(b => b.TransactionId.Contains(request.Q));
            }
            else
            {
                query = _repo.Query(b => b.IsActive && (b.TransactionId.Contains(request.Q)));
            }
        }
        else
        {
            if (!request.OnlyActives.HasValue || request.OnlyActives == false)
            {

                query = _repo.Query();
            }
            else
            {
                query = _repo.Query(b => b.IsActive);
            }
        }

        int totalCount = await query.CountAsync(cancellationToken);
        var pagedBrands = await query
    .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);
        var paymentsDto = pagedBrands.Select(payment => new PaymentDto
        {
            Id = payment.Id,
            Amount = payment.Amount,
            PaymentDate = payment.PaymentDate,
            TransactionId = payment.TransactionId,
            Status = payment.Status
            
            
        }).ToList();

        dynamic? config = null;

        if (request.ByConfig == true)
        {
            config = await _configRepo.GetByEntityNameAsync("Payments");
        }

        var resultDto = new ListDto<PaymentDto>
        {
            Records = paymentsDto,
            ColumnsJson = config?.ColumnsJson,
            ActionsJson = config?.ActionsJson,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };

         var dto = new PaymentDto
        {
          
        };
        return ServiceResult<ListDto<PaymentDto?>>.Ok(resultDto);
    }
    public async Task<ServiceResult<PaymentDto?>> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await _repo.GetByIdAsync(request.Id);
        if (payment == null) return ServiceResult<PaymentDto?>.Failed("پرداخت موردنظر پیدا نشد.");
        var dto = new PaymentDto
        {
            Amount = payment.Amount,
            Id = payment.Id,
            PaymentDate = payment.PaymentDate,
            TransactionId = payment.TransactionId,
            Status = payment.Status
        };
        return ServiceResult<PaymentDto?>.Ok(dto);
    }

    public async Task<ServiceResult<IEnumerable<PaymentDto>>> Handle(GetPaymentsByOrderIdQuery request, CancellationToken cancellationToken)
    {
        var payments = await _repo.GetByOrderIdAsync(request.OrderId);
        var dtos = payments.Select(p=> new PaymentDto
        {
            Amount = p.Amount,
            Id = p.Id,
            PaymentDate = p.PaymentDate,
            TransactionId = p.TransactionId,
            Status = p.Status
        }).ToList();
        return ServiceResult<IEnumerable<PaymentDto>>.Ok(dtos);
    }
}
