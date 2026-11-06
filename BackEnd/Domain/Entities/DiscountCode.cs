using OnlineShop.Domain.Common;
using Domain.Enums;
using OnlineShop.Domain.Entities;

public class DiscountCode : BaseEntity
{
    private DiscountCode() { }

    public string Code { get; private set; } = string.Empty;   // مثل: SPRING50
    public decimal Amount { get; private set; }
    public bool IsPercent { get; private set; }

    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }

    public int? UsageLimit { get; private set; }     // مثلا 100 بار
    public int UsedCount { get; private set; } = 0;  // چند بار استفاده شده

    public int? UserId { get; private set; }         // اختیاری → اگر فقط برای یک کاربر باشد

    public bool IsValid =>
        !IsDeleted &&
        DateTime.UtcNow >= StartDate &&
        DateTime.UtcNow <= EndDate &&
        (UsageLimit == null || UsedCount < UsageLimit);

    public static DiscountCode Create(
    string code,
    decimal amount,
    bool isPercent,
    DateTime startDate,
    DateTime endDate,
    int? userId,
    int? usageLimit,
    int currentUserId)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("کد تخفیف الزامی است.");

        var discountCode = new DiscountCode
        {
            Code = code.Trim().ToUpper(),
            Amount = amount,
            IsPercent = isPercent,
            StartDate = startDate,
            EndDate = endDate,
            UserId = userId,
            UsageLimit = usageLimit
        };

        discountCode.MarkCreated(currentUserId);
        return discountCode;
    }
    public void Update(
      string code,
      decimal amount,
      bool isPercent,
      DateTime startDate,
      DateTime endDate,
      int? userId,
      int? usageLimit,
      int currentUserId)
    {
        if (!string.IsNullOrWhiteSpace(code))
            Code = code.Trim().ToUpper();

        Amount = amount;
        IsPercent = isPercent;
        StartDate = startDate;
        EndDate = endDate;
        UserId = userId;
        UsageLimit = usageLimit;
        MarkUpdated(currentUserId);
    }


    public void Use(int currentUserId)
    {
        if (!IsValid)
            throw new DomainException("کد تخفیف معتبر نیست؛ ممکن است منقضی شده یا ظرفیت آن تمام شده باشد.");

        UsedCount++;
        MarkUpdated(currentUserId);
    }
 
}
