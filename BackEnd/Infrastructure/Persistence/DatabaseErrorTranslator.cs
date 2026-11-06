using System.Net.Sockets;
using Application.Common;
using Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql;
using OnlineShop.Domain.Common;

namespace OnlineShop.Infrastructure.Persistence
{
    /// <summary>
    /// Turns what PostgreSQL and EF Core report into an explanation people can act on.
    /// Tables, columns and constraints are resolved through the EF model and shown by their
    /// labels (<see cref="FieldLabels"/>); the raw text only goes to TechnicalDetail.
    /// </summary>
    public static class DatabaseErrorTranslator
    {
        /// <summary>Returns null when the exception did not come from the database.</summary>
        public static DomainException? Translate(Exception exception, IModel? model = null)
        {
            if (exception is DomainException domain)
                return domain;

            for (var current = exception; current is not null; current = current.InnerException)
            {
                switch (current)
                {
                    case DbUpdateConcurrencyException:
                        return new ConflictException(
                            "این رکورد هم‌زمان توسط کاربر دیگری تغییر کرده یا حذف شده است؛ صفحه را تازه کنید و دوباره تلاش کنید.",
                            exception,
                            ErrorCodes.ConcurrencyConflict)
                        { TechnicalDetail = current.Message };

                    case PostgresException postgres:
                        return FromPostgres(postgres, exception, exception as DbUpdateException, model);

                    case NpgsqlException npgsql:
                        return FromConnection(npgsql, exception);
                }
            }

            return null;
        }

        private static DomainException FromPostgres(
            PostgresException pg,
            Exception original,
            DbUpdateException? update,
            IModel? model)
        {
            var detail = $"PostgreSQL {pg.SqlState}: {pg.MessageText}"
                + (pg.ConstraintName is null ? string.Empty : $" (constraint {pg.ConstraintName})")
                + (pg.TableName is null ? string.Empty : $" (table {pg.TableName})")
                + (pg.ColumnName is null ? string.Empty : $" (column {pg.ColumnName})");

            switch (pg.SqlState)
            {
                case PostgresErrorCodes.UniqueViolation:
                    return Duplicate(pg, original, update, model, detail);
                case PostgresErrorCodes.ForeignKeyViolation:
                    return ForeignKey(pg, original, update, model, detail);
                case PostgresErrorCodes.NotNullViolation:
                    return NotNull(pg, original, detail);
                case PostgresErrorCodes.CheckViolation:
                    return Check(pg, original, detail);
                case PostgresErrorCodes.ExclusionViolation:
                    return new ConflictException(
                        $"این مقدار با یک رکورد دیگر در «{FieldLabels.Entity(pg.TableName)}» تداخل دارد.",
                        original,
                        ErrorCodes.DuplicateValue)
                    { TechnicalDetail = detail };

                case PostgresErrorCodes.StringDataRightTruncation:
                    return Invalid(
                        "متن یکی از فیلدها از طول مجاز پایگاه داده بیشتر است؛ متن کوتاه‌تری وارد کنید.",
                        original, ErrorCodes.FieldTooLong, detail);
                case PostgresErrorCodes.NumericValueOutOfRange:
                    return Invalid(
                        "یکی از عددهای واردشده بیش از حد بزرگ است و در پایگاه داده جا نمی‌شود؛ عدد کوچک‌تری وارد کنید.",
                        original, ErrorCodes.InvalidData, detail);
                case PostgresErrorCodes.DivisionByZero:
                    return Invalid(
                        "محاسبهٔ این درخواست به تقسیم بر صفر رسید؛ مقادیر عددی را بررسی کنید.",
                        original, ErrorCodes.InvalidData, detail);
                case PostgresErrorCodes.InvalidDatetimeFormat:
                case PostgresErrorCodes.DatetimeFieldOverflow:
                    return Invalid(
                        "تاریخ یا زمان واردشده معتبر نیست.",
                        original, ErrorCodes.InvalidData, detail);
                case PostgresErrorCodes.InvalidTextRepresentation:
                    return Invalid(
                        "قالب یکی از مقادیر ارسال‌شده با نوع داده در پایگاه داده سازگار نیست (مثلاً متن به‌جای عدد).",
                        original, ErrorCodes.InvalidData, detail);
                case PostgresErrorCodes.CharacterNotInRepertoire:
                case PostgresErrorCodes.UntranslatableCharacter:
                    return Invalid(
                        "متن ارسال‌شده نویسهٔ نامعتبری دارد که در پایگاه داده قابل ذخیره نیست.",
                        original, ErrorCodes.InvalidData, detail);

                case PostgresErrorCodes.SerializationFailure:
                case PostgresErrorCodes.DeadlockDetected:
                    return new ConflictException(
                        "این عملیات با یک عملیات هم‌زمان دیگر روی همین داده تداخل پیدا کرد؛ چند لحظه بعد دوباره تلاش کنید.",
                        original,
                        ErrorCodes.DatabaseBusy)
                    { TechnicalDetail = detail };
                case PostgresErrorCodes.LockNotAvailable:
                    return new ConflictException(
                        "این رکورد الان توسط عملیات دیگری قفل شده است؛ چند لحظه بعد دوباره تلاش کنید.",
                        original,
                        ErrorCodes.DatabaseBusy)
                    { TechnicalDetail = detail };
                case PostgresErrorCodes.QueryCanceled:
                    return Dependency(
                        "پاسخ پایگاه داده بیش از حد طول کشید و درخواست لغو شد؛ چند لحظه بعد دوباره تلاش کنید.",
                        original, ErrorCodes.DatabaseTimeout, detail);

                case PostgresErrorCodes.TooManyConnections:
                    return Dependency(
                        "تعداد اتصال‌های هم‌زمان به پایگاه داده به سقف رسیده است؛ چند لحظه بعد دوباره تلاش کنید.",
                        original, ErrorCodes.DatabaseBusy, detail);
                case PostgresErrorCodes.DiskFull:
                    return Dependency(
                        "فضای دیسک سرور پایگاه داده پر شده است و اطلاعات جدید ذخیره نمی‌شود.",
                        original, ErrorCodes.DatabaseResources, detail);
                case PostgresErrorCodes.OutOfMemory:
                case PostgresErrorCodes.InsufficientResources:
                    return Dependency(
                        "حافظهٔ سرور پایگاه داده برای انجام این درخواست کافی نیست.",
                        original, ErrorCodes.DatabaseResources, detail);

                case PostgresErrorCodes.UndefinedTable:
                case PostgresErrorCodes.UndefinedColumn:
                case PostgresErrorCodes.UndefinedFunction:
                    return Dependency(
                        "ساختار پایگاه داده با نسخهٔ فعلی برنامه هم‌خوان نیست (به‌روزرسانی پایگاه داده اجرا نشده است).",
                        original, ErrorCodes.DatabaseSchemaMismatch, detail);
                case PostgresErrorCodes.InvalidCatalogName:
                    return Dependency(
                        "پایگاه دادهٔ تعریف‌شده برای برنامه روی سرور وجود ندارد.",
                        original, ErrorCodes.DatabaseUnavailable, detail);
                case PostgresErrorCodes.InvalidPassword:
                case PostgresErrorCodes.InvalidAuthorizationSpecification:
                    return Dependency(
                        "برنامه نتوانست با نام کاربری و رمز تنظیم‌شده به پایگاه داده وصل شود.",
                        original, ErrorCodes.DatabaseUnavailable, detail);
                case PostgresErrorCodes.InsufficientPrivilege:
                    return Dependency(
                        "کاربر پایگاه داده اجازهٔ انجام این عملیات را ندارد.",
                        original, ErrorCodes.DatabaseError, detail);
                case PostgresErrorCodes.ReadOnlySqlTransaction:
                    return Dependency(
                        "پایگاه داده الان فقط‌خواندنی است و اطلاعات جدید ذخیره نمی‌شود.",
                        original, ErrorCodes.DatabaseUnavailable, detail);
            }

            // Everything else, by SQLSTATE class.
            var errorClass = pg.SqlState.Length >= 2 ? pg.SqlState[..2] : string.Empty;
            return errorClass switch
            {
                "08" or "57" => Dependency(
                    "ارتباط با پایگاه داده قطع شد؛ چند لحظه بعد دوباره تلاش کنید.",
                    original, ErrorCodes.DatabaseUnavailable, detail),
                "53" => Dependency(
                    "منابع سرور پایگاه داده برای انجام این درخواست کافی نیست.",
                    original, ErrorCodes.DatabaseResources, detail),
                "22" => Invalid(
                    "یکی از مقادیر ارسال‌شده در قالب درستی نیست و در پایگاه داده ذخیره نشد.",
                    original, ErrorCodes.InvalidData, detail),
                "23" => Invalid(
                    "این تغییر با قوانین یکپارچگی داده‌ها سازگار نیست.",
                    original, ErrorCodes.InvalidData, detail),
                "42" => Dependency(
                    "ساختار پایگاه داده با نسخهٔ فعلی برنامه هم‌خوان نیست.",
                    original, ErrorCodes.DatabaseSchemaMismatch, detail),
                _ => Dependency(
                    "پایگاه داده این درخواست را انجام نداد؛ جزئیات خطا در گزارش سرور ثبت شد.",
                    original, ErrorCodes.DatabaseError, detail),
            };
        }

        private static DomainException FromConnection(NpgsqlException npgsql, Exception original)
        {
            var detail = $"{npgsql.GetType().Name}: {npgsql.Message}";

            for (var inner = npgsql.InnerException; inner is not null; inner = inner.InnerException)
            {
                if (inner is TimeoutException)
                {
                    return Dependency(
                        "پایگاه داده در زمان مجاز پاسخ نداد؛ چند لحظه بعد دوباره تلاش کنید.",
                        original, ErrorCodes.DatabaseTimeout, detail);
                }

                if (inner is SocketException)
                    break;
            }

            if (npgsql.Message.Contains("pool", StringComparison.OrdinalIgnoreCase))
            {
                return Dependency(
                    "همهٔ اتصال‌های برنامه به پایگاه داده مشغول هستند؛ چند لحظه بعد دوباره تلاش کنید.",
                    original, ErrorCodes.DatabaseBusy, detail);
            }

            return Dependency(
                "اتصال به پایگاه داده برقرار نشد؛ سرور پایگاه داده خاموش است یا در دسترس نیست.",
                original, ErrorCodes.DatabaseUnavailable, detail);
        }

        private static DomainException Duplicate(
            PostgresException pg,
            Exception original,
            DbUpdateException? update,
            IModel? model,
            string detail)
        {
            var entity = FindEntity(model, pg.TableName);
            var properties = FindConstraintProperties(entity, pg.ConstraintName);
            var entityLabel = FieldLabels.Entity(entity?.ClrType.Name ?? pg.TableName);

            // A column the user typed ("نامک تکراری است") reads better than a pair of ids.
            var named = properties.Where(p => !p.IsForeignKey()).ToList();
            string message;
            IReadOnlyList<DomainFieldError>? fieldErrors = null;
            if (named.Count == 0)
            {
                message = $"این «{entityLabel}» قبلاً ثبت شده است و نمی‌توان آن را دوباره ثبت کرد.";
            }
            else
            {
                var labels = string.Join("، ", named.Select(p => $"«{FieldLabels.Field(p.Name)}»"));
                var value = named.Count == 1 ? FindValue(update, entity, named[0]) : null;
                message = value is null
                    ? $"{entityLabel} دیگری با همین {labels} وجود دارد؛ مقدار دیگری وارد کنید."
                    : $"{entityLabel} دیگری با {labels} برابر «{value}» وجود دارد؛ مقدار دیگری وارد کنید.";
                fieldErrors = named
                    .Select(p => new DomainFieldError(
                        p.Name,
                        $"این {FieldLabels.Field(p.Name)} قبلاً ثبت شده است."))
                    .ToList();
            }

            return new ConflictException(message, original, ErrorCodes.DuplicateValue)
            {
                TechnicalDetail = detail,
                FieldErrors = fieldErrors,
            };
        }

        private static DomainException ForeignKey(
            PostgresException pg,
            Exception original,
            DbUpdateException? update,
            IModel? model,
            string detail)
        {
            // PostgreSQL reports the referencing table in both directions.
            var dependent = FindEntity(model, pg.TableName);
            var foreignKey = dependent?.GetForeignKeys()
                .FirstOrDefault(fk => fk.GetConstraintName() == pg.ConstraintName);
            var dependentLabel = FieldLabels.Entity(dependent?.ClrType.Name ?? pg.TableName);
            var principalLabel = FieldLabels.Entity(foreignKey?.PrincipalEntityType.ClrType.Name);

            var parentChanged = update?.Entries.Any(e => e.State == EntityState.Deleted) == true
                || pg.MessageText.StartsWith("update or delete", StringComparison.OrdinalIgnoreCase);

            if (parentChanged)
            {
                var what = foreignKey is null ? "این رکورد" : $"این {principalLabel}";
                return new ConflictException(
                    $"حذف {what} ممکن نیست چون هنوز «{dependentLabel}» به آن وابسته است؛ ابتدا آن‌ها را حذف کنید یا به مورد دیگری منتقل کنید.",
                    original,
                    ErrorCodes.ReferenceInUse)
                { TechnicalDetail = detail };
            }

            var property = foreignKey?.Properties.FirstOrDefault();
            var fieldLabel = FieldLabels.Field(property?.Name);
            return new DomainException(
                foreignKey is null
                    ? $"یکی از موارد انتخاب‌شده برای «{dependentLabel}» وجود ندارد یا حذف شده است."
                    : $"«{fieldLabel}» انتخاب‌شده وجود ندارد یا حذف شده است؛ مورد دیگری انتخاب کنید.",
                original,
                ErrorCodes.ReferenceMissing)
            {
                TechnicalDetail = detail,
                FieldErrors = property is null
                    ? null
                    : [new DomainFieldError(property.Name, $"«{fieldLabel}» انتخاب‌شده وجود ندارد یا حذف شده است.")],
            };
        }

        private static DomainException NotNull(PostgresException pg, Exception original, string detail)
        {
            var fieldLabel = FieldLabels.Field(pg.ColumnName);
            var entityLabel = FieldLabels.Entity(pg.TableName);
            return new DomainException(
                $"«{fieldLabel}» برای ثبت «{entityLabel}» الزامی است و خالی فرستاده شده است.",
                original,
                ErrorCodes.RequiredValueMissing)
            {
                TechnicalDetail = detail,
                FieldErrors = pg.ColumnName is null
                    ? null
                    : [new DomainFieldError(pg.ColumnName, $"«{fieldLabel}» الزامی است.")],
            };
        }

        private static DomainException Check(PostgresException pg, Exception original, string detail) =>
            Invalid(
                $"مقدار واردشده برای «{FieldLabels.Entity(pg.TableName)}» با قوانین مجاز پایگاه داده سازگار نیست.",
                original,
                ErrorCodes.InvalidData,
                detail);

        private static DomainException Invalid(string message, Exception original, string code, string detail) =>
            new(message, original, code) { TechnicalDetail = detail };

        private static DomainException Dependency(string message, Exception original, string code, string detail) =>
            new DependencyFailureException(message, original, code) { TechnicalDetail = detail };

        private static IEntityType? FindEntity(IModel? model, string? table)
        {
            if (model is null || table is null)
                return null;

            return model.GetEntityTypes().FirstOrDefault(e => e.GetTableName() == table);
        }

        private static IReadOnlyList<IProperty> FindConstraintProperties(IEntityType? entity, string? constraint)
        {
            if (entity is null || constraint is null)
                return [];

            var index = entity.GetIndexes().FirstOrDefault(i => i.GetDatabaseName() == constraint);
            if (index is not null)
                return index.Properties;

            var key = entity.GetKeys().FirstOrDefault(k => k.GetName() == constraint);
            return key is not null ? key.Properties : [];
        }

        /// <summary>The value that clashed, when the failed save still holds it. Long values are left out.</summary>
        private static string? FindValue(DbUpdateException? update, IEntityType? entity, IProperty property)
        {
            if (update is null || entity is null)
                return null;

            foreach (EntityEntry entry in update.Entries)
            {
                if (entry.Metadata != entity || entry.State is not (EntityState.Added or EntityState.Modified))
                    continue;

                var value = entry.CurrentValues[property]?.ToString();
                if (!string.IsNullOrWhiteSpace(value) && value.Length <= 100)
                    return value;
            }

            return null;
        }
    }
}
