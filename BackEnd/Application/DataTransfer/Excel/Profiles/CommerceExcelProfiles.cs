using Application.Commands;
using Application.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Interfaces;

namespace Application.DataTransfer.Excel.Profiles
{
    public sealed class DiscountsExcelProfile(IMediator mediator, IDiscountRepository discounts) : ExcelEntityProfile(mediator)
    {
        public override string EntityKey => "discounts";
        public override string DisplayName => "تخفیف‌ها";

        protected override IEnumerable<ExcelSheetSchema> DefineSheets()
        {
            yield return Sheet("discounts", "تخفیف‌ها")
                .Id()
                .Text("Title", "عنوان", required: true, width: 25)
                .Decimal("Amount", "مقدار", required: true, description: "درصد یا مبلغ (ریال) بسته به ستون «درصدی».")
                .Boolean("IsPercent", "درصدی", required: true)
                .Date("StartDate", "تاریخ شروع", required: true)
                .Date("EndDate", "تاریخ پایان", required: true)
                .Integer("Priority", "اولویت")
                .Boolean("IsActive", "فعال")
                .Build();
        }

        public override async Task<IReadOnlyDictionary<string, List<ExcelRow>>> ExportAsync(ExcelExportRequest request, CancellationToken ct)
        {
            var query = discounts.Query().AsNoTracking();
            if (request.OnlyActives == true) query = query.Where(d => d.IsActive);
            if (!string.IsNullOrWhiteSpace(request.Q)) query = query.Where(d => d.Title.Contains(request.Q));
            if (request.Ids is { Count: > 0 }) query = query.Where(d => request.Ids.Contains(d.Id));

            var rows = await query.OrderBy(d => d.Id).ToListAsync(ct);
            return new Dictionary<string, List<ExcelRow>>
            {
                ["discounts"] = rows.Select(d => Row(new Dictionary<string, object?>
                {
                    ["Id"] = d.Id,
                    ["Title"] = d.Title,
                    ["Amount"] = d.Amount,
                    ["IsPercent"] = d.IsPercent,
                    ["StartDate"] = d.StartDate,
                    ["EndDate"] = d.EndDate,
                    ["Priority"] = (int)d.Priority,
                    ["IsActive"] = d.IsActive,
                })).ToList(),
            };
        }

        public override Task ImportAsync(ExcelImportContext context)
        {
            var ct = context.CancellationToken;
            return context.ProcessSheetAsync("discounts", async row =>
            {
                var result = await UpsertAsync(row,
                    create: () => SendAsync(new CreateDiscountCommand
                    {
                        Title = row.RequireString("Title"),
                        Amount = row.RequireDecimal("Amount"),
                        IsPercent = row.RequireBool("IsPercent"),
                        StartDate = row.RequireDate("StartDate"),
                        EndDate = row.RequireDate("EndDate"),
                        Priority = row.GetInt("Priority"),
                        IsActive = row.GetBool("IsActive") ?? true,
                    }, ct),
                    update: async id =>
                    {
                        var current = await discounts.GetByIdAsync(id)
                                      ?? throw new ExcelRowException($"تخفیفی با شناسه {id} پیدا نشد.");
                        return await SendAsync(new UpdateDiscountCommand
                        {
                            Id = id,
                            Title = row.GetString("Title") ?? current.Title,
                            Amount = row.GetDecimal("Amount") ?? current.Amount,
                            IsPercent = row.GetBool("IsPercent") ?? current.IsPercent,
                            StartDate = row.GetDate("StartDate") ?? current.StartDate,
                            EndDate = row.GetDate("EndDate") ?? current.EndDate,
                            Priority = row.GetInt("Priority") ?? (int)current.Priority,
                        }, ct);
                    });
                await SetActiveAsync<ActiveDiscountCommand>(row, result.Id!.Value, ct);
                return result;
            });
        }
    }

    public sealed class DiscountCodesExcelProfile(
        IMediator mediator,
        IDiscountCodeRepository codes,
        IHttpContextAccessor accessor) : ExcelEntityProfile(mediator)
    {
        public override string EntityKey => "discountCodes";
        public override string DisplayName => "کدهای تخفیف";

        protected override IEnumerable<ExcelSheetSchema> DefineSheets()
        {
            yield return Sheet("discountCodes", "کدهای تخفیف")
                .Id()
                .Text("Code", "کد", required: true, description: "به حروف کوچک ذخیره می‌شود.")
                .Decimal("Amount", "مقدار", required: true, description: "درصد یا مبلغ (ریال) بسته به ستون «درصدی».")
                .Boolean("IsPercent", "درصدی", required: true)
                .Date("StartDate", "تاریخ شروع", required: true)
                .Date("EndDate", "تاریخ پایان", required: true)
                .Integer("UsageLimit", "سقف استفاده", required: true)
                .Integer("UserId", "شناسه کاربر", description: "اختیاری؛ اگر کد مخصوص یک کاربر است.")
                .Boolean("IsActive", "فعال")
                .ReadOnly("UsedCount", "تعداد استفاده‌شده", ExcelValueType.Integer)
                .Build();
        }

        public override async Task<IReadOnlyDictionary<string, List<ExcelRow>>> ExportAsync(ExcelExportRequest request, CancellationToken ct)
        {
            var query = codes.Query().AsNoTracking();
            if (request.OnlyActives == true) query = query.Where(d => d.IsActive);
            if (!string.IsNullOrWhiteSpace(request.Q)) query = query.Where(d => d.Code.Contains(request.Q));
            if (request.Ids is { Count: > 0 }) query = query.Where(d => request.Ids.Contains(d.Id));

            var rows = await query.OrderBy(d => d.Id).ToListAsync(ct);
            return new Dictionary<string, List<ExcelRow>>
            {
                ["discountCodes"] = rows.Select(d => Row(new Dictionary<string, object?>
                {
                    ["Id"] = d.Id,
                    ["Code"] = d.Code,
                    ["Amount"] = d.Amount,
                    ["IsPercent"] = d.IsPercent,
                    ["StartDate"] = d.StartDate,
                    ["EndDate"] = d.EndDate,
                    ["UsageLimit"] = d.UsageLimit,
                    ["UserId"] = d.UserId,
                    ["IsActive"] = d.IsActive,
                    ["UsedCount"] = d.UsedCount,
                })).ToList(),
            };
        }

        public override Task ImportAsync(ExcelImportContext context)
        {
            var ct = context.CancellationToken;
            return context.ProcessSheetAsync("discountCodes", async row =>
            {
                var result = await UpsertAsync(row,
                    create: () => SendAsync(new CreateDiscountCodeCommand
                    {
                        Code = row.RequireString("Code"),
                        Amount = row.RequireDecimal("Amount"),
                        IsPercent = row.RequireBool("IsPercent"),
                        StartDate = row.RequireDate("StartDate"),
                        EndDate = row.RequireDate("EndDate"),
                        UsageLimit = row.RequireInt("UsageLimit"),
                        UserId = row.GetInt("UserId"),
                    }, ct),
                    update: async id =>
                    {
                        var current = await codes.GetByIdAsync(id)
                                      ?? throw new ExcelRowException($"کد تخفیفی با شناسه {id} پیدا نشد.");
                        return await SendAsync(new UpdateDiscountCodeCommand
                        {
                            Id = id,
                            Code = row.GetString("Code") ?? current.Code,
                            Amount = row.GetDecimal("Amount") ?? current.Amount,
                            IsPercent = row.GetBool("IsPercent") ?? current.IsPercent,
                            StartDate = row.GetDate("StartDate") ?? current.StartDate,
                            EndDate = row.GetDate("EndDate") ?? current.EndDate,
                            UsageLimit = row.GetInt("UsageLimit") ?? current.UsageLimit ?? 0,
                            UserId = row.GetInt("UserId") ?? current.UserId,
                            CurrentUserId = accessor.HttpContext?.GetUserId() ?? 0,
                        }, ct);
                    });
                await SetActiveAsync<ActiveDiscountCodeCommand>(row, result.Id!.Value, ct);
                return result;
            });
        }
    }

    public sealed class ShippingMethodsExcelProfile(IMediator mediator, IShippingMethodRepository methods) : ExcelEntityProfile(mediator)
    {
        public override string EntityKey => "shippingMethods";
        public override string DisplayName => "روش‌های ارسال";

        protected override IEnumerable<ExcelSheetSchema> DefineSheets()
        {
            yield return Sheet("shippingMethods", "روش‌های ارسال")
                .Id()
                .Text("Title", "عنوان", required: true, width: 25)
                .Text("Description", "توضیحات", width: 40)
                .Decimal("Price", "هزینه (ریال)", required: true)
                .Integer("EstimatedDeliveryTime", "زمان تقریبی تحویل (روز)", required: true)
                .Boolean("IsDefault", "پیش‌فرض", description: "فقط یک روش می‌تواند پیش‌فرض باشد.")
                .Boolean("IsActive", "فعال")
                .Build();
        }

        public override async Task<IReadOnlyDictionary<string, List<ExcelRow>>> ExportAsync(ExcelExportRequest request, CancellationToken ct)
        {
            var query = methods.Query().AsNoTracking();
            if (request.OnlyActives == true) query = query.Where(m => m.IsActive);
            if (!string.IsNullOrWhiteSpace(request.Q)) query = query.Where(m => m.Title.Contains(request.Q));
            if (request.Ids is { Count: > 0 }) query = query.Where(m => request.Ids.Contains(m.Id));

            var rows = await query.OrderBy(m => m.Id).ToListAsync(ct);
            return new Dictionary<string, List<ExcelRow>>
            {
                ["shippingMethods"] = rows.Select(m => Row(new Dictionary<string, object?>
                {
                    ["Id"] = m.Id,
                    ["Title"] = m.Title,
                    ["Description"] = m.Description,
                    ["Price"] = m.Price,
                    ["EstimatedDeliveryTime"] = m.EstimatedDeliveryTime,
                    ["IsDefault"] = m.IsDefault,
                    ["IsActive"] = m.IsActive,
                })).ToList(),
            };
        }

        public override Task ImportAsync(ExcelImportContext context)
        {
            var ct = context.CancellationToken;
            return context.ProcessSheetAsync("shippingMethods", async row =>
            {
                var result = await UpsertAsync(row,
                    create: () => SendAsync(new CreateShippingMethodCommand
                    {
                        Title = row.RequireString("Title"),
                        Description = row.GetString("Description"),
                        Price = row.RequireDecimal("Price"),
                        EstimatedDeliveryTime = row.RequireInt("EstimatedDeliveryTime"),
                        IsDefault = row.GetBool("IsDefault") ?? false,
                    }, ct),
                    update: async id =>
                    {
                        var current = await methods.GetByIdAsync(id)
                                      ?? throw new ExcelRowException($"روش ارسالی با شناسه {id} پیدا نشد.");
                        return await SendAsync(new UpdateShippingMethodCommand
                        {
                            Id = id,
                            Title = row.GetString("Title") ?? current.Title,
                            Description = row.GetString("Description") ?? current.Description,
                            Price = row.GetDecimal("Price") ?? current.Price,
                            EstimatedDeliveryTime = row.GetInt("EstimatedDeliveryTime") ?? current.EstimatedDeliveryTime,
                            // The handler treats a missing value as "not default", so keep the current flag.
                            IsDefault = row.GetBool("IsDefault") ?? current.IsDefault,
                        }, ct);
                    });
                await SetActiveAsync<ActiveShippingMethodCommand>(row, result.Id!.Value, ct);
                return result;
            });
        }
    }

    public sealed class PaymentMethodsExcelProfile(IMediator mediator, IPaymentMethodRepository methods) : ExcelEntityProfile(mediator)
    {
        public override string EntityKey => "paymentMethods";
        public override string DisplayName => "روش‌های پرداخت";

        protected override IEnumerable<ExcelSheetSchema> DefineSheets()
        {
            yield return Sheet("paymentMethods", "روش‌های پرداخت")
                .Id()
                .Text("Title", "عنوان", required: true, width: 25)
                .Text("Code", "کد", required: true)
                .Boolean("IsOnline", "آنلاین")
                .Integer("DisplayOrder", "ترتیب نمایش")
                .Text("ConfigJson", "تنظیمات (JSON)", width: 40)
                .Boolean("IsActive", "فعال")
                .Build();
        }

        public override async Task<IReadOnlyDictionary<string, List<ExcelRow>>> ExportAsync(ExcelExportRequest request, CancellationToken ct)
        {
            var query = methods.Query().AsNoTracking();
            if (request.OnlyActives == true) query = query.Where(m => m.IsActive);
            if (!string.IsNullOrWhiteSpace(request.Q)) query = query.Where(m => m.Title.Contains(request.Q) || m.Code.Contains(request.Q));
            if (request.Ids is { Count: > 0 }) query = query.Where(m => request.Ids.Contains(m.Id));

            var rows = await query.OrderBy(m => m.DisplayOrder).ThenBy(m => m.Id).ToListAsync(ct);
            return new Dictionary<string, List<ExcelRow>>
            {
                ["paymentMethods"] = rows.Select(m => Row(new Dictionary<string, object?>
                {
                    ["Id"] = m.Id,
                    ["Title"] = m.Title,
                    ["Code"] = m.Code,
                    ["IsOnline"] = m.IsOnline,
                    ["DisplayOrder"] = m.DisplayOrder,
                    ["ConfigJson"] = m.ConfigJson,
                    ["IsActive"] = m.IsActive,
                })).ToList(),
            };
        }

        public override Task ImportAsync(ExcelImportContext context)
        {
            var ct = context.CancellationToken;
            return context.ProcessSheetAsync("paymentMethods", row => UpsertAsync(row,
                create: async () =>
                {
                    var id = await SendAsync(new CreatePaymentMethodCommand
                    {
                        Title = row.RequireString("Title"),
                        Code = row.RequireString("Code"),
                        IsOnline = row.GetBool("IsOnline") ?? false,
                        DisplayOrder = row.GetInt("DisplayOrder") ?? 0,
                        ConfigJson = row.GetString("ConfigJson"),
                    }, ct);
                    await SetActiveAsync<ActivePaymentMethodCommand>(row, id, ct);
                    return id;
                },
                update: id => SendAsync(new UpdatePaymentMethodCommand
                {
                    Id = id,
                    Title = row.GetString("Title"),
                    Code = row.GetString("Code"),
                    IsOnline = row.GetBool("IsOnline"),
                    DisplayOrder = row.GetInt("DisplayOrder"),
                    ConfigJson = row.GetString("ConfigJson"),
                    IsActive = row.GetBool("IsActive"),
                }, ct)));
        }
    }

    public sealed class SpecialOffersExcelProfile(
        IMediator mediator,
        ISpecialOfferRepository specialOffers,
        IProductOfferRepository offers,
        IDiscountRepository discounts) : ExcelEntityProfile(mediator)
    {
        public override string EntityKey => "specialOffers";
        public override string DisplayName => "پیشنهادهای ویژه";

        protected override IEnumerable<ExcelSheetSchema> DefineSheets()
        {
            yield return Sheet("specialOffers", "پیشنهادهای ویژه")
                .Id()
                .Lookup("ProductOfferId", "محصول (پیشنهاد فروش)", ExcelLookupKeys.ProductOffers, required: true)
                .Lookup("DiscountId", "تخفیف", ExcelLookupKeys.Discounts)
                .Date("StartDate", "تاریخ شروع", required: true)
                .Date("EndDate", "تاریخ پایان", required: true)
                .Integer("DisplayOrder", "ترتیب نمایش")
                .Boolean("IsActive", "فعال")
                .Build();
        }

        public override async Task<IReadOnlyList<ExcelLookup>> GetLookupsAsync(CancellationToken ct) =>
        [
            await ExcelLookups.ProductOffersAsync(offers, ct),
            await ExcelLookups.DiscountsAsync(discounts, ct),
        ];

        public override async Task<IReadOnlyDictionary<string, List<ExcelRow>>> ExportAsync(ExcelExportRequest request, CancellationToken ct)
        {
            var query = specialOffers.Query().AsNoTracking();
            if (request.OnlyActives == true) query = query.Where(s => s.IsActive);
            if (!string.IsNullOrWhiteSpace(request.Q)) query = query.Where(s => s.ProductOffer.Product.Name.Contains(request.Q));
            if (request.Ids is { Count: > 0 }) query = query.Where(s => request.Ids.Contains(s.Id));

            var rows = await query.OrderBy(s => s.DisplayOrder).ThenBy(s => s.Id).ToListAsync(ct);
            return new Dictionary<string, List<ExcelRow>>
            {
                ["specialOffers"] = rows.Select(s => Row(new Dictionary<string, object?>
                {
                    ["Id"] = s.Id,
                    ["ProductOfferId"] = s.ProductOfferId,
                    ["DiscountId"] = s.DiscountId,
                    ["StartDate"] = s.StartDate,
                    ["EndDate"] = s.EndDate,
                    ["DisplayOrder"] = s.DisplayOrder,
                    ["IsActive"] = s.IsActive,
                })).ToList(),
            };
        }

        public override Task ImportAsync(ExcelImportContext context)
        {
            var ct = context.CancellationToken;
            return context.ProcessSheetAsync("specialOffers", async row =>
            {
                var productOfferId = context.ResolveId(MainSheet, row, "ProductOfferId");
                var discountId = context.ResolveId(MainSheet, row, "DiscountId");

                var result = await UpsertAsync(row,
                    create: () => SendAsync(new CreateSpecialOfferCommand
                    {
                        ProductOfferId = productOfferId!.Value,
                        DiscountId = discountId,
                        StartDate = row.RequireDate("StartDate"),
                        EndDate = row.RequireDate("EndDate"),
                        DisplayOrder = row.GetInt("DisplayOrder") ?? 0,
                    }, ct),
                    update: async id =>
                    {
                        var current = await specialOffers.GetByIdAsync(id)
                                      ?? throw new ExcelRowException($"پیشنهاد ویژه‌ای با شناسه {id} پیدا نشد.");
                        return await SendAsync(new UpdateSpecialOfferCommand
                        {
                            Id = id,
                            ProductOfferId = productOfferId ?? current.ProductOfferId,
                            DiscountId = discountId ?? current.DiscountId,
                            StartDate = row.GetDate("StartDate") ?? current.StartDate,
                            EndDate = row.GetDate("EndDate") ?? current.EndDate,
                            DisplayOrder = row.GetInt("DisplayOrder") ?? current.DisplayOrder,
                        }, ct);
                    });
                await SetActiveAsync<ActiveSpecialOfferCommand>(row, result.Id!.Value, ct);
                return result;
            });
        }
    }

    public sealed class FaqsExcelProfile(IMediator mediator, IFaqRepository faqs) : ExcelEntityProfile(mediator)
    {
        public override string EntityKey => "faqs";
        public override string DisplayName => "سوالات متداول";

        protected override IEnumerable<ExcelSheetSchema> DefineSheets()
        {
            yield return Sheet("faqs", "سوالات متداول")
                .Id()
                .Text("Question", "سوال", required: true, width: 40)
                .Text("Answer", "پاسخ", required: true, width: 60)
                .Integer("DisplayOrder", "ترتیب نمایش")
                .Boolean("IsActive", "فعال")
                .Build();
        }

        public override async Task<IReadOnlyDictionary<string, List<ExcelRow>>> ExportAsync(ExcelExportRequest request, CancellationToken ct)
        {
            var query = faqs.Query().AsNoTracking();
            if (request.OnlyActives == true) query = query.Where(f => f.IsActive);
            if (!string.IsNullOrWhiteSpace(request.Q)) query = query.Where(f => f.Question.Contains(request.Q) || f.Answer.Contains(request.Q));
            if (request.Ids is { Count: > 0 }) query = query.Where(f => request.Ids.Contains(f.Id));

            var rows = await query.OrderBy(f => f.DisplayOrder).ThenBy(f => f.Id).ToListAsync(ct);
            return new Dictionary<string, List<ExcelRow>>
            {
                ["faqs"] = rows.Select(f => Row(new Dictionary<string, object?>
                {
                    ["Id"] = f.Id,
                    ["Question"] = f.Question,
                    ["Answer"] = f.Answer,
                    ["DisplayOrder"] = f.DisplayOrder,
                    ["IsActive"] = f.IsActive,
                })).ToList(),
            };
        }

        public override Task ImportAsync(ExcelImportContext context)
        {
            var ct = context.CancellationToken;
            return context.ProcessSheetAsync("faqs", async row =>
            {
                var result = await UpsertAsync(row,
                    create: () => SendAsync(new CreateFaqCommand
                    {
                        Question = row.RequireString("Question"),
                        Answer = row.RequireString("Answer"),
                        DisplayOrder = row.GetInt("DisplayOrder"),
                    }, ct),
                    update: id => SendAsync(new UpdateFaqCommand
                    {
                        Id = id,
                        Question = row.GetString("Question"),
                        Answer = row.GetString("Answer"),
                        DisplayOrder = row.GetInt("DisplayOrder"),
                    }, ct));
                await SetActiveAsync<ActiveFaqCommand>(row, result.Id!.Value, ct);
                return result;
            });
        }
    }
}
