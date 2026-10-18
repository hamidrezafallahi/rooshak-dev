using Application.Common;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Interfaces;

namespace Application.DataTransfer.Excel.Profiles
{
    // Export-only profiles: records created by customers or with rich content/media, where
    // creating them from a spreadsheet is not meaningful. The Excel toolbar shows only "export".

    public sealed class UsersExcelProfile(IMediator mediator, IUserRepository users) : ExcelEntityProfile(mediator)
    {
        public override string EntityKey => "users";
        public override string DisplayName => "کاربران";
        public override bool CanImport => false;

        // Personal data: not for content editors.
        public override IReadOnlyList<string> AllowedRoles => ["SuperAdmin", "Admin"];

        protected override IEnumerable<ExcelSheetSchema> DefineSheets()
        {
            yield return Sheet("users", "کاربران")
                .Id()
                .Text("FullName", "نام و نام خانوادگی", width: 25)
                .Text("PhoneNumber", "شماره موبایل")
                .Text("Email", "ایمیل", width: 25)
                .Text("Role", "نقش")
                .Boolean("IsActive", "فعال")
                .DateTime("CreatedAt", "تاریخ عضویت")
                .Build();
        }

        public override async Task<IReadOnlyDictionary<string, List<ExcelRow>>> ExportAsync(ExcelExportRequest request, CancellationToken ct)
        {
            var query = users.Query().AsNoTracking();
            if (request.OnlyActives == true) query = query.Where(u => u.IsActive);
            if (!string.IsNullOrWhiteSpace(request.Q))
                query = query.Where(u => u.FullName.Contains(request.Q) || u.PhoneNumber.Contains(request.Q) || u.Email.Contains(request.Q));
            if (request.Ids is { Count: > 0 }) query = query.Where(u => request.Ids.Contains(u.Id));
            if (request.FilterInt("roleId") is { } roleId) query = query.Where(u => u.RoleId == roleId);

            var rows = await query.OrderBy(u => u.Id)
                .Select(u => new { u.Id, u.FullName, u.PhoneNumber, u.Email, Role = u.Role.RoleName, u.IsActive, u.CreatedAt })
                .ToListAsync(ct);
            return new Dictionary<string, List<ExcelRow>>
            {
                ["users"] = rows.Select(u => Row(new Dictionary<string, object?>
                {
                    ["Id"] = u.Id,
                    ["FullName"] = u.FullName,
                    ["PhoneNumber"] = u.PhoneNumber,
                    ["Email"] = u.Email,
                    ["Role"] = u.Role,
                    ["IsActive"] = u.IsActive,
                    ["CreatedAt"] = u.CreatedAt,
                })).ToList(),
            };
        }
    }

    public sealed class CommentsExcelProfile(IMediator mediator, ICommentRepository comments) : ExcelEntityProfile(mediator)
    {
        public override string EntityKey => "comments";
        public override string DisplayName => "نظرات";
        public override bool CanImport => false;

        protected override IEnumerable<ExcelSheetSchema> DefineSheets()
        {
            yield return Sheet("comments", "نظرات")
                .Id()
                .Text("UserName", "کاربر")
                .Text("TargetType", "نوع")
                .Text("TargetTitle", "مربوط به", width: 30)
                .Text("Content", "متن نظر", width: 60)
                .Integer("ParentId", "پاسخ به (شناسه)")
                .Boolean("IsApproved", "تأیید شده")
                .DateTime("CreatedAt", "تاریخ ثبت")
                .Build();
        }

        public override async Task<IReadOnlyDictionary<string, List<ExcelRow>>> ExportAsync(ExcelExportRequest request, CancellationToken ct)
        {
            var query = comments.Query().AsNoTracking();
            if (request.OnlyActives == true) query = query.Where(c => c.IsActive);
            if (!string.IsNullOrWhiteSpace(request.Q))
                query = query.Where(c => c.Content.Contains(request.Q) || c.UserName.Contains(request.Q) || c.TargetTitle.Contains(request.Q));
            if (request.Ids is { Count: > 0 }) query = query.Where(c => request.Ids.Contains(c.Id));
            if (request.FilterBool("isApproved") is { } approved) query = query.Where(c => c.IsApproved == approved);

            var rows = await query.OrderByDescending(c => c.Id).ToListAsync(ct);
            return new Dictionary<string, List<ExcelRow>>
            {
                ["comments"] = rows.Select(c => Row(new Dictionary<string, object?>
                {
                    ["Id"] = c.Id,
                    ["UserName"] = c.UserName,
                    ["TargetType"] = c.TargetType.TargetTypeToDisplay(),
                    ["TargetTitle"] = c.TargetTitle,
                    ["Content"] = c.Content,
                    ["ParentId"] = c.ParentId,
                    ["IsApproved"] = c.IsApproved,
                    ["CreatedAt"] = c.CreatedAt,
                })).ToList(),
            };
        }
    }

    public sealed class ContactRequestsExcelProfile(IMediator mediator, IContactRequestRepository requests) : ExcelEntityProfile(mediator)
    {
        public override string EntityKey => "contactRequests";
        public override string DisplayName => "درخواست‌های تماس";
        public override bool CanImport => false;
        public override IReadOnlyList<string> AllowedRoles => ["SuperAdmin", "Admin"];

        protected override IEnumerable<ExcelSheetSchema> DefineSheets()
        {
            yield return Sheet("contactRequests", "درخواست‌های تماس")
                .Id()
                .Text("FirstName", "نام")
                .Text("LastName", "نام خانوادگی")
                .Text("PhoneNumber", "شماره تماس")
                .Text("Email", "ایمیل")
                .Text("Address", "آدرس", width: 40)
                .Text("PreferredContactTime", "زمان مناسب تماس")
                .Text("Message", "پیام", width: 60)
                .Boolean("IsReviewed", "بررسی شده")
                .Text("AdminNote", "یادداشت ادمین", width: 40)
                .DateTime("CreatedAt", "تاریخ ثبت")
                .Build();
        }

        public override async Task<IReadOnlyDictionary<string, List<ExcelRow>>> ExportAsync(ExcelExportRequest request, CancellationToken ct)
        {
            var query = requests.Query().AsNoTracking();
            if (!string.IsNullOrWhiteSpace(request.Q))
                query = query.Where(r => r.FirstName.Contains(request.Q) || r.LastName.Contains(request.Q) || r.PhoneNumber.Contains(request.Q));
            if (request.Ids is { Count: > 0 }) query = query.Where(r => request.Ids.Contains(r.Id));
            if (request.FilterBool("isReviewed") is { } reviewed) query = query.Where(r => r.IsReviewed == reviewed);

            var rows = await query.OrderByDescending(r => r.Id).ToListAsync(ct);
            return new Dictionary<string, List<ExcelRow>>
            {
                ["contactRequests"] = rows.Select(r => Row(new Dictionary<string, object?>
                {
                    ["Id"] = r.Id,
                    ["FirstName"] = r.FirstName,
                    ["LastName"] = r.LastName,
                    ["PhoneNumber"] = r.PhoneNumber,
                    ["Email"] = r.Email,
                    ["Address"] = r.Address,
                    ["PreferredContactTime"] = r.PreferredContactTime,
                    ["Message"] = r.Message,
                    ["IsReviewed"] = r.IsReviewed,
                    ["AdminNote"] = r.AdminNote,
                    ["CreatedAt"] = r.CreatedAt,
                })).ToList(),
            };
        }
    }

    /// <summary>Blog posts: export of metadata for SEO review (content and images stay in the editor).</summary>
    public sealed class BlogsExcelProfile(IMediator mediator, IBlogRepository blogs) : ExcelEntityProfile(mediator)
    {
        public override string EntityKey => "blogs";
        public override string DisplayName => "مقالات";
        public override bool CanImport => false;

        protected override IEnumerable<ExcelSheetSchema> DefineSheets()
        {
            yield return Sheet("blogs", "مقالات")
                .Id()
                .Text("TitleFa", "عنوان فارسی", width: 35)
                .Text("TitleEn", "عنوان انگلیسی", width: 35)
                .Text("Slug", "نامک (Slug)", width: 30)
                .Text("ExcerptFa", "خلاصه فارسی", width: 50)
                .Text("MetaDescriptionFa", "توضیحات متا (فارسی)", width: 50)
                .Text("MetaKeywordsFa", "کلمات کلیدی (فارسی)", width: 30)
                .Text("Author", "نویسنده")
                .Boolean("IsActive", "فعال")
                .DateTime("CreatedAt", "تاریخ ایجاد")
                .DateTime("UpdatedAt", "آخرین ویرایش")
                .Build();
        }

        public override async Task<IReadOnlyDictionary<string, List<ExcelRow>>> ExportAsync(ExcelExportRequest request, CancellationToken ct)
        {
            var query = blogs.Query().AsNoTracking();
            if (request.OnlyActives == true) query = query.Where(b => b.IsActive);
            if (!string.IsNullOrWhiteSpace(request.Q))
                query = query.Where(b => b.TitleFa.Contains(request.Q) || b.TitleEn.Contains(request.Q) || b.Slug.Contains(request.Q));
            if (request.Ids is { Count: > 0 }) query = query.Where(b => request.Ids.Contains(b.Id));

            var rows = await query.OrderByDescending(b => b.Id)
                .Select(b => new
                {
                    b.Id, b.TitleFa, b.TitleEn, b.Slug, b.ExcerptFa, b.MetaDescriptionFa, b.MetaKeywordsFa,
                    Author = b.Author.FullName, b.IsActive, b.CreatedAt, b.UpdatedAt,
                })
                .ToListAsync(ct);
            return new Dictionary<string, List<ExcelRow>>
            {
                ["blogs"] = rows.Select(b => Row(new Dictionary<string, object?>
                {
                    ["Id"] = b.Id,
                    ["TitleFa"] = b.TitleFa,
                    ["TitleEn"] = b.TitleEn,
                    ["Slug"] = b.Slug,
                    ["ExcerptFa"] = b.ExcerptFa,
                    ["MetaDescriptionFa"] = b.MetaDescriptionFa,
                    ["MetaKeywordsFa"] = b.MetaKeywordsFa,
                    ["Author"] = b.Author,
                    ["IsActive"] = b.IsActive,
                    ["CreatedAt"] = b.CreatedAt,
                    ["UpdatedAt"] = b.UpdatedAt,
                })).ToList(),
            };
        }
    }
}
