using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;
using System.Text.Json;

namespace OnlineShop.Infrastructure.Configurations
{
    public class EntityConfigConfiguration : IEntityTypeConfiguration<EntityConfig>
    {
        public void Configure(EntityTypeBuilder<EntityConfig> builder)
        {
            // Primary Key
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedOnAdd();

            // Properties
            builder.Property(e => e.EntityName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.PersianDisplayName)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(e => e.EnglishDisplayName)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(e => e.EndPoint)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.EntityIconBase64)
                .HasColumnType("text");

            builder.Property(e => e.ActionsJson)
                .IsRequired()
                .HasColumnType("text");

            builder.Property(e => e.ColumnsJson)
                .IsRequired()
                .HasColumnType("text");

            builder.Property(e => e.FormFieldsJson)
                .HasColumnType("text")
                .IsRequired(false);

            // BaseEntity common fields
            builder.Property(e => e.CreatedAt).IsRequired();
            builder.Property(e => e.UpdatedAt);
            builder.Property(e => e.DeletedAt);
            builder.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);
            builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
            builder.Property(e => e.CreatedBy);
            builder.Property(e => e.UpdatedBy);
            builder.Property(e => e.DeletedBy);

            // Index
            builder.HasIndex(e => e.EntityName).IsUnique();

            //Seed Data
            builder.HasData(
                CreateBlogsEntity(),
                CreateBrandsEntity(),
                CreateCategoriesEntity(),
                CreateDiscountsEntity(),
                CreateProductsEntity(),
                CreateTagsEntity(),
                CreateUsersEntity(),
                CreateUserAddressesEntity(),
                CreateCartItemsEntity(),
                CreateOrderItemsEntity(),
                CreatePaymentsEntity(),
                CreateRolesEntity(),
                CreateProductOfferDiscountsEntity(),
                CreateProductImagesEntity(),
                CreateProductOfferTagsEntity(),
                CreateBlogTagsEntity(),
                CreateUserTagsEntity(),
                CreateCommentEntity(),
                CreateProductOffersEntity(),
                CreateDiscountCodesEntity(),
                CreatePaymentMethodesEntity(),
                CreateEntityConfigsEntity(),
                CreateShippingMethodsEntity(),
                CreateSpecialOffersEntity(),
                CreateLandingSlidesEntity(),
                CreateRatesEntity(),
                CreateProductSpecificationsEntity(),
                CreateSeoSettingsEntity(),
                CreateSeoDashboardEntity(),
                CreateFaqsEntity(),
                CreateContactRequestsEntity(),
                CreateThemeSettingsEntity(),
                CreateAnnouncementBarsEntity()
            //CreateCartsEntity(),
            //CreateOrdersEntity(),

            );
        }

        private static object CreateBlogsEntity()
        {
            return new
            {
                Id = 1,
                EntityName = "blogs",
                PersianDisplayName = "بلاگ‌ها",
                EnglishDisplayName = "Blog",
                EndPoint = "blogs",
                EntityIconBase64 = "<svg xmlns=\"http://www.w3.org/2000/svg\" fill=\"none\" viewBox=\"0 0 24 24\" stroke-width=\"1.5\" stroke=\"currentColor\" class=\"size-6\">\r\n  <path stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M12 7.5h1.5m-1.5 3h1.5m-7.5 3h7.5m-7.5 3h7.5m3-9h3.375c.621 0 1.125.504 1.125 1.125V18a2.25 2.25 0 0 1-2.25 2.25M16.5 7.5V18a2.25 2.25 0 0 0 2.25 2.25M16.5 7.5V4.875c0-.621-.504-1.125-1.125-1.125H4.125C3.504 3.75 3 4.254 3 4.875V18a2.25 2.25 0 0 0 2.25 2.25h13.5M6 7.5h3v3H6v-3Z\" />\r\n</svg>",
                ActionsJson = JsonSerializer.Serialize(new List<string> { "active", "edit", "delete", "new" }),
                ColumnsJson = JsonSerializer.Serialize(new List<JsonDefinition>
                {
                    new JsonDefinition { Header = "شناسه", Accessor = "id", Type = "number", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "Slug", Accessor = "slug", Type = "text", Sortable = true, Filterable = true },
                    new JsonDefinition { Header = "عنوان فارسی", Accessor = "titleFa", Type = "text", Sortable = true, Filterable = true },
                    //new JsonDefinition { Header = "نویسنده", Accessor = "authorName", Type = "text", Sortable = true, Filterable = true },
                    //new JsonDefinition { Header = "تاریخ انتشار", Accessor = "publishedDate", Type = "date", Sortable = true, Filterable = false },
                    new JsonDefinition { Header = "تصویر شاخص", Accessor = "thumbnailFile", Type = "image", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "وضعیت فعال", Accessor = "isActive", Type = "bool", Sortable = false, Filterable = false },
                    //new JsonDefinition { Header = "توضیحات متا فارسی", Accessor = "metaDescriptionFa", Type = "text", Sortable = false, Filterable = false },
                    //new JsonDefinition { Header = "کلمات کلیدی متا فارسی", Accessor = "metaKeywordsFa", Type = "text", Sortable = false, Filterable = false },
                    //new JsonDefinition { Header = "عنوان انگلیسی", Accessor = "titleEn", Type = "text", Sortable = true, Filterable = true },
                    //new JsonDefinition { Header = "خلاصه انگلیسی", Accessor = "excerptEn", Type = "textarea", Sortable = false, Filterable = false },
                    //new JsonDefinition { Header = "توضیحات متا انگلیسی", Accessor = "metaDescriptionEn", Type = "text", Sortable = false, Filterable = false },
                    //new JsonDefinition { Header = "کلمات کلیدی متا انگلیسی", Accessor = "metaKeywordsEn", Type = "text", Sortable = false, Filterable = false }
                }),
                FormFieldsJson = JsonSerializer.Serialize(new List<FormFieldDefinition>
                {
                    // ===== فارسی =====
                    new FormFieldDefinition { Name = "titleFa", Caption = "عنوان فارسی", Type = "text", PlaceHolder = "مثلاً: معرفی محصول جدید", Help = "عنوان بلاگ را وارد کنید", Rules = new List<ValidationRule> { new ValidationRule { Rule = "required", Condition = "true", Message = "عنوان فارسی الزامی است" } } },
                    new FormFieldDefinition { Name = "introFa", Caption = "مقدمه فارسی", Type = "textarea", PlaceHolder = "مقدمه بلاگ را وارد کنید (HTML مجاز است)", Rules = new List<ValidationRule> { new ValidationRule { Rule = "required", Condition = "true", Message = "مقدمه فارسی الزامی است" } } },
                    new FormFieldDefinition { Name = "contentFa", Caption = "محتوا فارسی", Type = "textarea", PlaceHolder = "متن اصلی با H2/H3، جدول و FAQ (HTML)" },
                    new FormFieldDefinition { Name = "conclusionFa", Caption = "جمع بندی فارسی", Type = "textarea", PlaceHolder = "جمع بندی بلاگ را وارد کنید (HTML مجاز است)" },
                    new FormFieldDefinition { Name = "excerptFa", Caption = "خلاصه فارسی", Type = "textarea", PlaceHolder = "حداکثر ۱۶۰ کاراکتر — برای کارت بلاگ و فال‌بک متا" },
                    new FormFieldDefinition { Name = "metaDescriptionFa", Caption = "توضیحات متا فارسی", Type = "text", PlaceHolder = "حداکثر ۱۶۰ کاراکتر — اختصاصی برای SERP" },
                    new FormFieldDefinition { Name = "metaKeywordsFa", Caption = "کلمات کلیدی متا فارسی", Type = "text", PlaceHolder = "کلمات کلیدی با کاما جدا شوند" },

                    // ===== انگلیسی =====  
                    new FormFieldDefinition { Name = "titleEn", Caption = "عنوان انگلیسی", Type = "text", PlaceHolder = "New Product Introduction" },
                    new FormFieldDefinition { Name = "introEn", Caption = "مقدمه انگلیسی", Type = "textarea", PlaceHolder = "Blog introduction (HTML allowed)" },
                    new FormFieldDefinition { Name = "contentEn", Caption = "محتوا انگلیسی", Type = "textarea", PlaceHolder = "Main body with H2/H3, table and FAQ (HTML)" },
                    new FormFieldDefinition { Name = "conclusionEn", Caption = "جمع بندی انگلیسی", Type = "textarea", PlaceHolder = "Blog conclusion (HTML allowed)" },
                    new FormFieldDefinition { Name = "excerptEn", Caption = "خلاصه انگلیسی", Type = "textarea", PlaceHolder = "Short blog summary" },
                    new FormFieldDefinition { Name = "metaDescriptionEn", Caption = "Meta Description", Type = "text", PlaceHolder = "Max 160 characters" },
                    new FormFieldDefinition { Name = "metaKeywordsEn", Caption = "Meta Keywords", Type = "text", PlaceHolder = "keyword1, keyword2" },

                    // ===== سایر =====
                    new FormFieldDefinition { Name = "slug", Caption = "Slug", Type = "text", PlaceHolder = "new-product-introduction", Help = "برای آدرس URL بلاگ", Rules = new List<ValidationRule> { new ValidationRule { Rule = "required", Condition = "true", Message = "Slug الزامی است" } } },
                    new FormFieldDefinition { Name = "thumbnailFile", Caption = "تصویر شاخص", Type = "file", PlaceHolder = "تصویر انتخاب کنید", Help = "jpg, png (حداکثر 2MB)" },
                    new FormFieldDefinition { Name = "authorId", Caption = "نویسنده", Type = "dynamicSelect", FetchConfig = new FetchConfig { api = "Users/selectOption", fetchFilters = new List<object>() }, Help = "نویسنده بلاگ" }
                }),
                IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1),
                CreatedBy = 1,
                IsDeleted = false
            };
        }


        private static object CreateBrandsEntity()
        {
            return new
            {
                Id = 2,
                EntityName = "brands",
                PersianDisplayName = "برندها",
                EnglishDisplayName = "Brand",
                EndPoint = "brands",
                EntityIconBase64 = "<svg xmlns=\"http://www.w3.org/2000/svg\" fill=\"none\" viewBox=\"0 0 24 24\" stroke-width=\"1.5\" stroke=\"currentColor\" class=\"size-6\">\r\n  <path stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M12 9v3.75m0-10.036A11.959 11.959 0 0 1 3.598 6 11.99 11.99 0 0 0 3 9.75c0 5.592 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.31-.21-2.57-.598-3.75h-.152c-3.196 0-6.1-1.25-8.25-3.286Zm0 13.036h.008v.008H12v-.008Z\" />\r\n</svg>",
                ActionsJson = JsonSerializer.Serialize(new List<string> { "active", "edit", "delete", "new" }),
                ColumnsJson = JsonSerializer.Serialize(new List<JsonDefinition>
                {
                    new JsonDefinition { Header = "شناسه", Accessor = "id", Type = "number", Sortable = false, Filterable = false},
                    new JsonDefinition { Header = "نام برند", Accessor = "name", Type = "text", Sortable = false, Filterable = false},
                    new JsonDefinition { Header = "لوگو", Accessor = "logoFile", Type = "image", Sortable = false, Filterable = false},
                    new JsonDefinition { Header = "توضیحات", Accessor = "description", Type = "textarea", Sortable = false}
                }),
                FormFieldsJson = JsonSerializer.Serialize(new List<FormFieldDefinition>
                {
                    new FormFieldDefinition
                    {
                        Name = "name",
                        Caption = "نام برند",
                        Type = "text",
                        PlaceHolder = "مثلاً: سامسونگ",
                        Help = "نام برند را وارد کنید",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "نام برند الزامی است" },
                            new ValidationRule { Rule = "minLength", Condition = "2", Message = "حداقل دو کاراکتر وارد کنید" }
                        }
                    },
                    new FormFieldDefinition { Name = "logoFile", Caption = "لوگو", Type = "file", PlaceHolder = "لوگوی برند را انتخاب کنید", Help = "فرمت‌های مجاز: jpg, png, svg" },
                    new FormFieldDefinition { Name = "description", Caption = "توضیحات", Type = "textarea", PlaceHolder = "توضیح کوتاهی درباره برند بنویسید..." },
                    new FormFieldDefinition { Name = "slug", Caption = "Slug", Type = "text", PlaceHolder = "brand-slug", Help = "آدرس یکتای برند در URL" },
                    new FormFieldDefinition { Name = "seoTitleFa", Caption = "عنوان سئو فارسی", Type = "text", PlaceHolder = "عنوان نمایش در گوگل" },
                    new FormFieldDefinition { Name = "seoTitleEn", Caption = "SEO Title EN", Type = "text", PlaceHolder = "Google title" },
                    new FormFieldDefinition { Name = "metaDescriptionFa", Caption = "توضیح متا فارسی", Type = "textarea", PlaceHolder = "حداکثر ۱۶۰ کاراکتر" },
                    new FormFieldDefinition { Name = "metaDescriptionEn", Caption = "Meta Description EN", Type = "textarea", PlaceHolder = "Max 160 chars" },
                    //new FormFieldDefinition { Name = "isActive", Caption = "وضعیت", Type = "checkbox", Help = "در صورت غیرفعال بودن برند در لیست نمایش داده نمی‌شود" }
                }),
                IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1),
                CreatedBy = 1,
                IsDeleted = false
            };
        }
        private static object CreateCategoriesEntity() => new
        {
            Id = 3,
            EntityName = "categories",
            PersianDisplayName = "دسته‌بندی‌ها",
            EnglishDisplayName = "Category",
            EndPoint = "categories",
            EntityIconBase64 = "<svg xmlns=\"http://www.w3.org/2000/svg\" fill=\"none\" viewBox=\"0 0 24 24\" stroke-width=\"1.5\" stroke=\"currentColor\" class=\"size-6\">\r\n  <path stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"m7.875 14.25 1.214 1.942a2.25 2.25 0 0 0 1.908 1.058h2.006c.776 0 1.497-.4 1.908-1.058l1.214-1.942M2.41 9h4.636a2.25 2.25 0 0 1 1.872 1.002l.164.246a2.25 2.25 0 0 0 1.872 1.002h2.092a2.25 2.25 0 0 0 1.872-1.002l.164-.246A2.25 2.25 0 0 1 16.954 9h4.636M2.41 9a2.25 2.25 0 0 0-.16.832V12a2.25 2.25 0 0 0 2.25 2.25h15A2.25 2.25 0 0 0 21.75 12V9.832c0-.287-.055-.57-.16-.832M2.41 9a2.25 2.25 0 0 1 .382-.632l3.285-3.832a2.25 2.25 0 0 1 1.708-.786h8.43c.657 0 1.281.287 1.709.786l3.284 3.832c.163.19.291.404.382.632M4.5 20.25h15A2.25 2.25 0 0 0 21.75 18v-2.625c0-.621-.504-1.125-1.125-1.125H3.375c-.621 0-1.125.504-1.125 1.125V18a2.25 2.25 0 0 0 2.25 2.25Z\" />\r\n</svg>",
            ActionsJson = JsonSerializer.Serialize(new List<string> { "active", "edit", "delete", "new" }),
            ColumnsJson = JsonSerializer.Serialize(new List<JsonDefinition>
                {
                    new JsonDefinition { Header = "شناسه", Accessor = "id", Type = "number", Sortable = false, Filterable = false, },
                    new JsonDefinition { Header = "نام دسته‌بندی", Accessor = "name", Type = "text", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "دسته والد", Accessor = "parentCategoryId", Type = "number", Sortable = false, Filterable = false},
                    new JsonDefinition { Header = "تاریخ ایجاد", Accessor = "createdAt", Type = "date", Sortable = false, Filterable = false}
                }),
            FormFieldsJson = JsonSerializer.Serialize(new List<FormFieldDefinition>
                {
                    new FormFieldDefinition
                    {
                        Name = "persianName",
                        Caption = "نام دسته‌بندی فارسی",
                        Type = "text",
                        PlaceHolder = "مثلاً: تلویزیون",
                        Help = "نام دسته را وارد کنید",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "نام دسته‌بندی الزامی است" },
                            new ValidationRule { Rule = "minLength", Condition = "2", Message = "حداقل دو کاراکتر وارد کنید" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "englishName",
                        Caption = "English category name",
                        Type = "text",
                        PlaceHolder = "example: cars",
                        Help = "insert english category name",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "english name is required" },
                            new ValidationRule { Rule = "minLength", Condition = "2", Message = "at least insert two characters" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "categoryCover",
                        Caption = "تصویر دسته بندی",
                        Type = "file",
                        PlaceHolder = "example: car.jpg",
                        Help = "add an image",
                        Rules = new List<ValidationRule>
                        {}
                    },
                    new FormFieldDefinition
                    {
                        Name = "categoryPersianDesc",
                        Caption = "توضیحات این دسته بندی",
                        Type = "text",
                        PlaceHolder = "مثال:این دسته بندی برای...",
                        Help = "توضیحاتی در خصوص این دسته بندی بنویسید",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "توضیحات دسته بندی الزامی میباشد." },
                            new ValidationRule { Rule = "minLength", Condition = "2", Message = "حداقل دو کاراکتر بنویسید" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "categoryEnglishDesc",
                        Caption = "description about this category",
                        Type = "text",
                        PlaceHolder = "example : this is category of ...",
                        Help = "add some info about this category",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "category description is required" },
                            new ValidationRule { Rule = "minLength", Condition = "2", Message = "at least insert two characters" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "parentCategoryId",
                        Caption = "دسته والد",
                        Type = "dynamicSelect",
                        FetchConfig =new FetchConfig{api="Categories/selectOption",fetchFilters=["id"]},
                        PlaceHolder = "در صورت وجود، دسته والد را انتخاب کنید",
                        Help = "می‌توانید این دسته را به عنوان زیرمجموعه یک دسته دیگر تعیین کنید"
                    },
                    new FormFieldDefinition
                    {
                        Name = "isShowInLanding",
                        Caption = "نمایش در صفحه اصلی",
                        Type = "checkbox",
                        Help = "در صورت غیرفعال بودن، دسته‌بندی در صفحه اصلی نمایش داده نمی‌شود"
                    },
                    new FormFieldDefinition { Name = "slug", Caption = "Slug", Type = "text", PlaceHolder = "category-slug", Help = "آدرس یکتای دسته در URL" },
                    new FormFieldDefinition { Name = "seoTitleFa", Caption = "عنوان سئو فارسی", Type = "text", PlaceHolder = "عنوان نمایش در گوگل" },
                    new FormFieldDefinition { Name = "seoTitleEn", Caption = "SEO Title EN", Type = "text", PlaceHolder = "Google title" },
                    new FormFieldDefinition { Name = "metaDescriptionFa", Caption = "توضیح متا فارسی", Type = "textarea", PlaceHolder = "حداکثر ۱۶۰ کاراکتر" },
                    new FormFieldDefinition { Name = "metaDescriptionEn", Caption = "Meta Description EN", Type = "textarea", PlaceHolder = "Max 160 chars" },
                    new FormFieldDefinition { Name = "faqJson", Caption = "FAQ JSON", Type = "textarea", PlaceHolder = "[{\"question\":\"...\",\"answer\":\"...\"}]", Help = "سوالات متداول دسته برای schema" },
                    //  new FormFieldDefinition
                    //{
                    //    Name = "isActive",
                    //    Caption = "وضعیت",
                    //    Type = "checkbox",
                    //    Help = "در صورت غیرفعال بودن، دسته‌بندی در لیست نمایش داده نمی‌شود"
                    //}
                }),
            IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1),
            CreatedBy = 1,
            IsDeleted = false
        };
        private static object CreateDiscountsEntity()
        {
            return new
            {
                Id = 4,
                EntityName = "discounts",
                PersianDisplayName = "تخفیف‌ها",
                EnglishDisplayName = "Discount",
                EndPoint = "discounts",
                EntityIconBase64 = @"
<svg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 24 24' stroke-width='1.5' stroke='currentColor' class='size-6'>
  <path stroke-linecap='round' stroke-linejoin='round' d='m8.99 14.993 6-6m6 3.001c0 1.268-.63 2.39-1.593 3.069a3.746 3.746 0 0 1-1.043 3.296 3.745 3.745 0 0 1-3.296 1.043 3.745 3.745 0 0 1-3.068 1.593c-1.268 0-2.39-.63-3.068-1.593a3.745 3.745 0 0 1-3.296-1.043 3.746 3.746 0 0 1-1.043-3.297 3.746 3.746 0 0 1-1.593-3.068c0-1.268.63-2.39 1.593-3.068a3.746 3.746 0 0 1 1.043-3.297 3.745 3.745 0 0 1 3.296-1.042 3.745 3.745 0 0 1 3.068-1.594c1.268 0 2.39.63 3.068 1.593a3.745 3.745 0 0 1 3.296 1.043 3.746 3.746 0 0 1 1.043 3.297 3.746 3.746 0 0 1 1.593 3.068ZM9.74 9.743h.008v.007H9.74v-.007Zm.375 0a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0Zm4.125 4.5h.008v.008h-.008v-.008Zm.375 0a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0Z' />
</svg>",
                ActionsJson = JsonSerializer.Serialize(new List<string> { "edit", "delete", "new" }),
                ColumnsJson = JsonSerializer.Serialize(new List<JsonDefinition>
                {
                    new JsonDefinition { Header = "شناسه", Accessor = "id", Type = "number", Sortable = false, Filterable = false},
                    new JsonDefinition { Header = "عنوان تخفیف", Accessor = "title", Type = "text", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "مقدار تخفیف", Accessor = "amount", Type = "number", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "درصدی؟", Accessor = "isPercent", Type = "bool", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "تاریخ شروع", Accessor = "startDate", Type = "date", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "تاریخ پایان", Accessor = "endDate", Type = "date", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "وضعیت فعال", Accessor = "isActive", Type = "bool", Sortable = false, Filterable = false }
                }),
                FormFieldsJson = JsonSerializer.Serialize(new List<FormFieldDefinition>
                {
                    new FormFieldDefinition
                    {
                        Name = "title",
                        Caption = "عنوان تخفیف",
                        Type = "text",
                        PlaceHolder = "مثلاً: تخفیف تابستانی",
                        Help = "نام نمایشی برای تخفیف",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "عنوان تخفیف الزامی است" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "amount",
                        Caption = "مقدار تخفیف",
                        Type = "number",
                        PlaceHolder = "مثلاً: 15",
                        Help = "عدد تخفیف به تومان یا درصد",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "مقدار تخفیف الزامی است" },
                            new ValidationRule { Rule = "min", Condition = "1", Message = "مقدار باید بزرگ‌تر از صفر باشد" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "isPercent",
                        Caption = "آیا تخفیف درصدی است؟",
                        Type = "checkbox",
                        Help = "در صورت فعال بودن، مقدار تخفیف برحسب درصد است",
                        Rules = new List<ValidationRule>()
                    },
                    new FormFieldDefinition
                    {
                        Name = "startDate",
                        Caption = "تاریخ شروع تخفیف",
                        Type = "date",
                        Help = "تاریخی که تخفیف از آن فعال می‌شود",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "تاریخ شروع الزامی است" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "endDate",
                        Caption = "تاریخ پایان تخفیف",
                        Type = "date",
                        Help = "تاریخی که تخفیف تا آن معتبر است",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "تاریخ پایان الزامی است" }
                        }
                    }
                    //new FormFieldDefinition
                    //{
                    //    Name = "isActive",
                    //    Caption = "فعال باشد؟",
                    //    Type = "checkbox",
                    //    Help = "برای فعال/غیرفعال کردن وضعیت تخفیف",
                    //    Rules = new List<ValidationRule>()
                    //}
                }),
                IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1),
                CreatedBy = 1,
                IsDeleted = false
            };
        }
        private static object CreateProductsEntity()
        {
            return new
            {
                Id = 5,
                EntityName = "products",
                PersianDisplayName = "محصولات",
                EnglishDisplayName = "Products",
                EndPoint = "products",
                EntityIconBase64 = @"<svg xmlns=""http://www.w3.org/2000/svg"" fill=""none"" viewBox=""0 0 24 24"" stroke-width=""1.5"" stroke=""currentColor"" class=""size-6"">
  <path stroke-linecap=""round"" stroke-linejoin=""round"" d=""m20.25 7.5-.625 10.632a2.25 2.25 0 0 1-2.247 2.118H6.622a2.25 2.25 0 0 1-2.247-2.118L3.75 7.5M10 11.25h4M3.375 7.5h17.25c.621 0 1.125-.504 1.125-1.125v-1.5c0-.621-.504-1.125-1.125-1.125H3.375c-.621 0-1.125.504-1.125 1.125v1.5c0 .621.504 1.125 1.125 1.125Z"" />
</svg>
",
                ActionsJson = JsonSerializer.Serialize(new List<string> { "active", "edit", "delete", "new" }),
                ColumnsJson = JsonSerializer.Serialize(new List<JsonDefinition>
                {
                    new JsonDefinition { Header = "شناسه", Accessor = "id", Type = "number", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "نام محصول", Accessor = "name", Type = "text", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "دسته‌بندی", Accessor = "categoryName", Type = "text", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "برند", Accessor = "brandName", Type = "text", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "توضیحات", Accessor = "description", Type = "textarea", Sortable = false, Filterable = false }
                }),
                FormFieldsJson = JsonSerializer.Serialize(new List<FormFieldDefinition>
                {
                    new FormFieldDefinition
                    {
                        Name = "name",
                        Caption = "نام محصول",
                        Type = "text",
                        PlaceHolder = "نام محصول را وارد کنید",
                        Help = "مثلاً: تلویزیون سامسونگ 55 اینچ",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "نام محصول الزامی است" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "slug",
                        Caption = "Slug",
                        Type = "text",
                        PlaceHolder = "product-slug",
                        Help = "آدرس یکتای محصول در URL"
                    },
                    new FormFieldDefinition
                    {
                        Name = "seoTitleFa",
                        Caption = "عنوان سئو فارسی",
                        Type = "text",
                        PlaceHolder = "عنوان نمایش در گوگل"
                    },
                    new FormFieldDefinition
                    {
                        Name = "seoTitleEn",
                        Caption = "SEO Title EN",
                        Type = "text",
                        PlaceHolder = "Google title"
                    },
                    new FormFieldDefinition
                    {
                        Name = "metaDescriptionFa",
                        Caption = "توضیح متا فارسی",
                        Type = "textarea",
                        PlaceHolder = "حداکثر ۱۶۰ کاراکتر"
                    },
                    new FormFieldDefinition
                    {
                        Name = "metaDescriptionEn",
                        Caption = "Meta Description EN",
                        Type = "textarea",
                        PlaceHolder = "Max 160 chars"
                    },
                    new FormFieldDefinition
                    {
                        Name = "description",
                        Caption = "توضیحات",
                        Type = "textarea",
                        PlaceHolder = "توضیحات محصول را وارد کنید",
                        Help = "می‌توانید ویژگی‌ها و مشخصات محصول را بنویسید",
                        Rules = new List<ValidationRule>()
                    },
                    new FormFieldDefinition
                    {
                        Name = "categoryId",
                        Caption = "دسته‌بندی",
                        Type = "dynamicSelect",
                        FetchConfig =new FetchConfig{api="Categories/selectOption",fetchFilters=[]},
                        PlaceHolder = "دسته‌بندی محصول را انتخاب کنید",
                        Help = "محصول به کدام دسته تعلق دارد",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "دسته‌بندی الزامی است" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "brandId",
                        Caption = "برند",
                        Type = "dynamicSelect",
                        FetchConfig =new FetchConfig{api="Brands/selectOption",fetchFilters=[]},
                        PlaceHolder = "برند محصول را انتخاب کنید",
                        Help = "محصول به کدام برند تعلق دارد",
                        Rules = new List<ValidationRule>()
                    },
                    new FormFieldDefinition
                    {
                        Name = "width",
                        Caption = "عرض",
                        Type = "number",
                        PlaceHolder = "عرض محصول را وارد کنید",
                        Help = "می‌تواند خالی باشد",
                        Rules = new List<ValidationRule>()
                    },
                    new FormFieldDefinition
                    {
                        Name = "height",
                        Caption = "طول یا ارتفاع",
                        Type = "number",
                        PlaceHolder = "طول یا ارتفاع را وارد کنید",
                        Help = "می‌تواند خالی باشد",
                        Rules = new List<ValidationRule>()
                    }, new FormFieldDefinition
                    {
                        Name = "depth",
                        Caption = "عمق",
                        Type = "number",
                        PlaceHolder = "عمق را وارد کنید",
                        Help = "می‌تواند خالی باشد",
                        Rules = new List<ValidationRule>()
                    }, new FormFieldDefinition
                    {
                        Name = "weight",
                        Caption = "وزن",
                        Type = "number",
                        PlaceHolder = "وزن را وارد کنید",
                        Help = "می‌تواند خالی باشد",
                        Rules = new List<ValidationRule>()
                    }
                }),
                IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1),
                CreatedBy = 1,
                IsDeleted = false
            };
        }
        private static object CreateTagsEntity()
        {
            return new
            {
                Id = 6,
                EntityName = "tags",
                PersianDisplayName = "تگ‌ها",
                EnglishDisplayName = "Tags",
                EndPoint = "tags",
                EntityIconBase64 = @"<svg xmlns=""http://www.w3.org/2000/svg"" fill=""none"" viewBox=""0 0 24 24"" stroke-width=""1.5"" stroke=""currentColor"" class=""size-6"">
  <path stroke-linecap=""round"" stroke-linejoin=""round"" d=""M9.568 3H5.25A2.25 2.25 0 0 0 3 5.25v4.318c0 .597.237 1.17.659 1.591l9.581 9.581c.699.699 1.78.872 2.607.33a18.095 18.095 0 0 0 5.223-5.223c.542-.827.369-1.908-.33-2.607L11.16 3.66A2.25 2.25 0 0 0 9.568 3Z"" />
  <path stroke-linecap=""round"" stroke-linejoin=""round"" d=""M6 6h.008v.008H6V6Z"" /></svg>",
                ActionsJson = JsonSerializer.Serialize(new List<string> { "active", "edit", "delete", "new" }),
                ColumnsJson = JsonSerializer.Serialize(new List<JsonDefinition>
                {
                    new JsonDefinition { Header = "شناسه", Accessor = "id", Type = "number", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "نام تگ", Accessor = "name", Type = "text", Sortable = false, Filterable = false }
                }),
                FormFieldsJson = JsonSerializer.Serialize(new List<FormFieldDefinition>
                {
                    new FormFieldDefinition
                    {
                        Name = "name",
                        Caption = "نام تگ",
                        Type = "text",
                        PlaceHolder = "نام تگ را وارد کنید",
                        Help = "مثلاً: تلویزیون، گوشی، لپ‌تاپ",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "نام تگ الزامی است" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "slug",
                        Caption = "Slug",
                        Type = "text",
                        PlaceHolder = "tag-slug",
                        Help = "آدرس یکتای تگ در URL"
                    }
                }),
                IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1),
                CreatedBy = 1,
                IsDeleted = false
            };
        }
        private static object CreateUsersEntity()
        {
            return new
            {
                Id = 7,
                EntityName = "users",
                PersianDisplayName = "کاربران",
                EnglishDisplayName = "Users",
                EndPoint = "users",
                EntityIconBase64 = @"<svg xmlns=""http://www.w3.org/2000/svg"" fill=""none"" viewBox=""0 0 24 24"" stroke-width=""1.5"" stroke=""currentColor"" class=""size-6"">
  <path stroke-linecap=""round"" stroke-linejoin=""round"" d=""M15 19.128a9.38 9.38 0 0 0 2.625.372 9.337 9.337 0 0 0 4.121-.952 4.125 4.125 0 0 0-7.533-2.493M15 19.128v-.003c0-1.113-.285-2.16-.786-3.07M15 19.128v.106A12.318 12.318 0 0 1 8.624 21c-2.331 0-4.512-.645-6.374-1.766l-.001-.109a6.375 6.375 0 0 1 11.964-3.07M12 6.375a3.375 3.375 0 1 1-6.75 0 3.375 3.375 0 0 1 6.75 0Zm8.25 2.25a2.625 2.625 0 1 1-5.25 0 2.625 2.625 0 0 1 5.25 0Z"" />
</svg>",
                ActionsJson = JsonSerializer.Serialize(new List<string> { "active", "edit", "delete", "new" }),
                ColumnsJson = JsonSerializer.Serialize(new List<JsonDefinition>
                {
                    new JsonDefinition { Header = "شناسه", Accessor = "id", Type = "number", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "نام کامل", Accessor = "fullName", Type = "text", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "ایمیل", Accessor = "email", Type = "text", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "شماره تماس", Accessor = "phoneNumber", Type = "text", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "نقش", Accessor = "roleId", Type = "number", Sortable = false, Filterable = false }
                }),
                FormFieldsJson = JsonSerializer.Serialize(new List<FormFieldDefinition>
                {
                    new FormFieldDefinition
                    {
                        Name = "fullName",
                        Caption = "نام کامل",
                        Type = "text",
                        PlaceHolder = "نام و نام خانوادگی را وارد کنید",
                        Help = "مثلاً: محمد رضایی",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "نام کامل الزامی است" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "email",
                        Caption = "ایمیل",
                        Type = "text",
                        PlaceHolder = "example@gmail.com",
                        Help = "آدرس ایمیل معتبر وارد کنید",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "ایمیل الزامی است" },
                            new ValidationRule { Rule = "email", Condition = "true", Message = "ایمیل نامعتبر است" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "phoneNumber",
                        Caption = "شماره تماس",
                        Type = "text",
                        PlaceHolder = "09123456789",
                        Help = "شماره موبایل را بدون فاصله وارد کنید",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "شماره تماس الزامی است" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "password",
                        Caption = "رمز عبور",
                        Type = "password",
                        PlaceHolder = "رمز عبور را وارد کنید",
                        Help = "حداقل ۸ کاراکتر",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "رمز عبور الزامی است" },
                            new ValidationRule { Rule = "minLength", Condition = "8", Message = "رمز عبور باید حداقل ۸ کاراکتر باشد" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "roleId",
                        Caption = "نقش",
                        Type = "select",
                        PlaceHolder = "نقش کاربر را انتخاب کنید",
                        Help = "مثلاً: Admin, Customer",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "نقش الزامی است" }
                        }
                    }
                }),
                IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1),
                CreatedBy = 1,
                IsDeleted = false
            };
        }
        private static object CreateUserAddressesEntity()
        {
            return new
            {
                Id = 8,
                EntityName = "address",
                PersianDisplayName = "آدرس کاربران",
                EnglishDisplayName = "User Addresses",
                EndPoint = "address",
                EntityIconBase64 = @"<svg width=""24"" height=""24"" viewBox=""0 0 24 24"" fill=""none"" xmlns=""http://www.w3.org/2000/svg"" stroke=""#000"" className=""stroke-primary""><path strokeLinecap=""round"" strokeLinejoin=""round"" d=""M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7z""/></svg>",
                ActionsJson = JsonSerializer.Serialize(new List<string> { "active", "edit", "delete", "new", "default" }),
                ColumnsJson = JsonSerializer.Serialize(new List<JsonDefinition>
                {
                    new JsonDefinition { Header = "شناسه", Accessor = "id", Type = "number", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "نام کاربر", Accessor = "userName", Type = "text", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "شهر", Accessor = "city", Type = "text", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "استان", Accessor = "state", Type = "text", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "کد پستی", Accessor = "postalCode", Type = "text", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "آدرس کامل", Accessor = "fullAddress", Type = "textarea", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "پیش‌فرض", Accessor = "isDefault", Type = "bool", Sortable = false, Filterable = false }
                }),
                FormFieldsJson = JsonSerializer.Serialize(new List<FormFieldDefinition>
                {
                    new FormFieldDefinition
                    {
                        Name = "userId",
                        Caption = "شناسه کاربر",
                        Type = "dynamicSelect",
                        FetchConfig =new FetchConfig{api="Users/selectOption",fetchFilters=[] },
                        PlaceHolder = "شناسه کاربر را وارد کنید",
                        Help = "در صورتی که انتخاب نکنید برای خودتان این آدرس ثبت میشود",
                        Rules = new List<ValidationRule>
                        {

                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "name",
                        Caption = "نام آدرس",
                        Type = "text",
                        PlaceHolder = "نام آدرس را وارد کنید",
                        Help = "برای سازماندهی بهتر چند آدرس به کار میرود",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "نام آدرس الزامی است" }

                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "phoneNumber",
                        Caption = "شماره تماس",
                        Type = "phoneNumber",
                        PlaceHolder = "شماره تماس را وارد کنید",
                        Help = "09121234567",
                        Rules = new List<ValidationRule>
                        {
                           new ValidationRule { Rule = "required", Condition = "true", Message = "شماره تماس الزامی است" }

                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "city",
                        Caption = "شهر",
                        Type = "text",
                        PlaceHolder = "نام شهر",
                        Help = "مثلاً: تهران",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "نام شهر الزامی است" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "state",
                        Caption = "استان",
                        Type = "text",
                        PlaceHolder = "نام استان",
                        Help = "مثلاً: تهران",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "نام استان الزامی است" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "postalCode",
                        Caption = "کد پستی",
                        Type = "text",
                        PlaceHolder = "کد پستی را وارد کنید",
                        Help = "مثلاً: 1234567890",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "کد پستی الزامی است" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "fullAddress",
                        Caption = "آدرس کامل",
                        Type = "textarea",
                        PlaceHolder = "آدرس کامل را وارد کنید",
                        Help = "مثلاً: خیابان آزادی، پلاک 12",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "آدرس کامل الزامی است" }
                        }
                    }

                }),
                IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1),
                CreatedBy = 1,
                IsDeleted = false
            };
        }
        private static object CreatePaymentsEntity()
        {
            return new
            {
                Id = 9,
                EntityName = "payments",
                PersianDisplayName = "پرداخت‌ها",
                EnglishDisplayName = "Payment",
                EndPoint = "payments",
                EntityIconBase64 = @"<svg xmlns=""http://www.w3.org/2000/svg"" fill=""none"" viewBox=""0 0 24 24"" stroke-width=""1.5"" stroke=""currentColor"" class=""size-6"">
  <path stroke-linecap=""round"" stroke-linejoin=""round"" d=""M12 21v-8.25M15.75 21v-8.25M8.25 21v-8.25M3 9l9-6 9 6m-1.5 12V10.332A48.36 48.36 0 0 0 12 9.75c-2.551 0-5.056.2-7.5.582V21M3 21h18M12 6.75h.008v.008H12V6.75Z"" /></svg>",
                ActionsJson = JsonSerializer.Serialize(new List<string> { "active", "edit", "delete", "new" }),
                ColumnsJson = JsonSerializer.Serialize(new List<JsonDefinition>
                {
                    new JsonDefinition { Header = "شناسه", Accessor = "id", Type = "number", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "سفارش", Accessor = "orderId", Type = "number", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "تاریخ پرداخت", Accessor = "paymentDate", Type = "datetime", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "مبلغ", Accessor = "amount", Type = "decimal", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "روش پرداخت", Accessor = "paymentMethod", Type = "enum", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "وضعیت", Accessor = "status", Type = "enum", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "شناسه تراکنش", Accessor = "transactionId", Type = "text", Sortable = false, Filterable = false }
                }),
                FormFieldsJson = JsonSerializer.Serialize(new List<FormFieldDefinition>
                {
                    new FormFieldDefinition
                    {
                        Name = "orderId",
                        Caption = "شناسه سفارش",
                        Type = "text",
                        PlaceHolder = "شناسه سفارش را وارد کنید",
                        Help = "این پرداخت مربوط به کدام سفارش است",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "شناسه سفارش الزامی است" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "amount",
                        Caption = "مبلغ",
                        Type = "decimal",
                        PlaceHolder = "مبلغ پرداخت را وارد کنید",
                        Help = "مبلغ پرداخت شده توسط مشتری",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "مبلغ الزامی است" },
                            new ValidationRule { Rule = "min", Condition = "0.01", Message = "مبلغ باید بزرگتر از صفر باشد" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "paymentMethod",
                        Caption = "روش پرداخت",
                        Type = "enum",
                        PlaceHolder = "روش پرداخت را انتخاب کنید",
                        Help = "روش پرداخت: آنلاین یا نقدی",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "روش پرداخت الزامی است" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "status",
                        Caption = "وضعیت",
                        Type = "enum",
                        PlaceHolder = "وضعیت پرداخت",
                        Help = "وضعیت پرداخت: Pending, Success, Failed, Cancelled"
                    },
                    new FormFieldDefinition
                    {
                        Name = "transactionId",
                        Caption = "شناسه تراکنش",
                        Type = "text",
                        PlaceHolder = "شناسه تراکنش را وارد کنید",
                        Help = "شناسه تراکنش پرداخت آنلاین"
                    },
                    new FormFieldDefinition
                    {
                        Name = "paymentDate",
                        Caption = "تاریخ پرداخت",
                        Type = "datetime",
                        PlaceHolder = "تاریخ و زمان پرداخت",
                        Help = "تاریخ انجام پرداخت"
                    }
                }),
                IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1),
                CreatedBy = 1,
                IsDeleted = false
            };
        }
        private static object CreateProductOfferDiscountsEntity()
        {
            return new
            {
                Id = 10,
                EntityName = "productOfferDiscounts",
                PersianDisplayName = "تخفیف سفارشات",
                EnglishDisplayName = "ProductOfferDiscounts",
                EndPoint = "productOfferDiscounts",
                EntityIconBase64 = @"<svg width=""24"" height=""24"" viewBox=""0 0 24 24"" fill=""none"" xmlns=""http://www.w3.org/2000/svg"" stroke=""#000"" className=""stroke-primary""><path strokeLinecap=""round"" strokeLinejoin=""round"" d=""M12 2l3 7H9l3-7zM12 22a10 10 0 100-20 10 10 0 000 20z""/></svg>",
                ActionsJson = JsonSerializer.Serialize(new List<string> { "active", "edit", "delete", "new" }),
                ColumnsJson = JsonSerializer.Serialize(new List<JsonDefinition>
                {
                    new JsonDefinition { Header = "شناسه", Accessor = "id", Type = "number", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "نام محصول", Accessor = "productName", Type = "text", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "عکس محصول", Accessor = "productImage", Type = "image", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "تامین کننده", Accessor = "supplier", Type = "text", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "نام تخفیف", Accessor = "discountTitle", Type = "text", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "مقدار تخفیف", Accessor = "discountAmount", Type = "number", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "درصدی", Accessor = "discountIsPercent", Type = "bool", Sortable = false, Filterable = false }
                }),
                FormFieldsJson = JsonSerializer.Serialize(new List<FormFieldDefinition>
                {
                    new FormFieldDefinition
                    {
                        Name = "productId",
                        Caption = "انتخاب محصول",
                        Type = "dynamicSelect",
                        FetchConfig = new FetchConfig { api = "Products/selectOption", fetchFilters = [] },
                        PlaceHolder = "محصول را انتخاب کنید",
                        Help = "انتخاب محصول برای تخفیف",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "انتخاب محصول الزامی است" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "productOfferId",
                        Caption = "انتخاب سفارش",
                        Type = "dynamicSelect",
                        FetchConfig = new FetchConfig { api = "productOffers/selectOption", fetchFilters = ["productId"] },
                        PlaceHolder = " سفارش را انتخاب کنید",
                        Help = " سفارش را انتخاب کنید",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "انتخاب سفارش الزامی است" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "discountId",
                        Caption = "تخفیف",
                        Type = "dynamicSelect",
                        FetchConfig = new FetchConfig { api = "discounts/selectOption", fetchFilters = [] },
                        PlaceHolder = "تخفیف را انتخاب کنید",
                        Help = "انتخاب تخفیف برای محصول",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "انتخاب تخفیف الزامی است" }
                        }
                    }
                }),
                IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1),
                CreatedBy = 1,
                IsDeleted = false
            };
        }
        private static object CreateProductImagesEntity()
        {
            return new
            {
                Id = 11,
                EntityName = "productImages",
                PersianDisplayName = "تصاویر محصول",
                EnglishDisplayName = "ProductImage",
                EndPoint = "productImages",
                EntityIconBase64 = @"<svg width=""24"" height=""24"" viewBox=""0 0 24 24"" fill=""none"" xmlns=""http://www.w3.org/2000/svg"" stroke=""#000"" className=""stroke-primary""><path strokeLinecap=""round"" strokeLinejoin=""round"" d=""M21 19V5a2 2 0 00-2-2H5a2 2 0 00-2 2v14a2 2 0 002 2h14a2 2 0 002-2zM3 9l4 4 3-3 5 5 4-4""/></svg>",
                ActionsJson = JsonSerializer.Serialize(new List<string> { "active", "delete", "new" }),
                ColumnsJson = JsonSerializer.Serialize(new List<JsonDefinition>
                {
                    new JsonDefinition { Header = "شناسه", Accessor = "id", Type = "number", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "نام محصول", Accessor = "productName", Type = "text", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "تصویر محصول", Accessor = "productImageUrl", Type = "image", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "عکس اصلی", Accessor = "isMain", Type = "bool", Sortable = false, Filterable = false }
                }),
                FormFieldsJson = JsonSerializer.Serialize(new List<FormFieldDefinition>
                {
                    new FormFieldDefinition
                    {
                        Name = "productId",
                        Caption = "انتخاب محصول",
                        Type = "dynamicSelect",
                        FetchConfig =new FetchConfig{api="Products/selectOption",fetchFilters=[]},
                        PlaceHolder = "محصول را انتخاب کنید",
                        Help = "تصویر برای کدام محصول است",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "انتخاب محصول الزامی است" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "Images",
                        Caption = "تصویر",
                        Type = "fileArray",
                        PlaceHolder = "آدرس تصویر را وارد کنید",
                        Help = "تصویر محصول را آپلود کنید",
                        Rules = new List<ValidationRule>
                        {
                            //new ValidationRule { Rule = "required", Condition = "true", Message = "وارد کردن تصویر الزامی است" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "IsMainImages",
                        Caption = "",
                        Type = "hidden",
                        PlaceHolder = "",
                        Help = "",

                    }

                }),
                IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1),
                CreatedBy = 1,
                IsDeleted = false
            };
        }
        private static object CreateProductOfferTagsEntity()
        {
            return new
            {
                Id = 12,
                EntityName = "productOfferTags",
                PersianDisplayName = "تگ‌های سفارش",
                EnglishDisplayName = "Product offer tag",
                EndPoint = "productOfferTags",
                EntityIconBase64 = @"<svg xmlns=""http://www.w3.org/2000/svg"" fill=""none"" viewBox=""0 0 24 24"" stroke-width=""2"" stroke=""currentColor"" class=""w-6 h-6""><path stroke-linecap=""round"" stroke-linejoin=""round"" d=""M7 7h10M7 12h10M7 17h10""/></svg>",
                ActionsJson = JsonSerializer.Serialize(new List<string> { "active", "edit", "delete", "new" }),
                ColumnsJson = JsonSerializer.Serialize(new List<JsonDefinition>
                {
                    new JsonDefinition {Header = "شناسه",Accessor="id",Type="number",Sortable=false,Filterable=false,Options=null},
                    new JsonDefinition {Header = "نام محصول",Accessor="productName",Type="text",Sortable=false,Filterable=false,Options=null},
                    new JsonDefinition {Header = "عکس محصول",Accessor="productImage",Type="image",Sortable=false,Filterable=false,Options=null},
                    new JsonDefinition {Header = "نام تامین کننده",Accessor="supplierName",Type="text",Sortable=false,Filterable=false,Options=null},
                    new JsonDefinition {Header = "عکس تامین کننده",Accessor="supplierImage",Type="image",Sortable=false,Filterable=false,Options=null},
                    new JsonDefinition {Header = "متن تگ",Accessor="tagName",Type="text",Sortable=false,Filterable=false,Options=null}

                }),
                FormFieldsJson = JsonSerializer.Serialize(new List<FormFieldDefinition>
                {
                    new FormFieldDefinition
                    {
                        Name = "productId",
                        Caption = "محصول",
                        Type = "dynamicSelect",
                        FetchConfig =new FetchConfig{api="Products/selectOption",fetchFilters=[] },
                        PlaceHolder = " محصول را انتخاب کنید",
                        Help = " محصول را انتخاب کنید",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "انتخاب محصول الزامی است" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "productOfferId",
                        Caption = "انتخاب سفارش",
                        Type = "dynamicSelect",
                        FetchConfig =new FetchConfig{api="productOffers/selectOption",fetchFilters=["productId"] },
                        PlaceHolder = " سفارش را انتخاب کنید",
                        Help ="تگ مربوط به کدام سفارش است",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "انتخاب سفارش الزامی است" }
                        }
                    },
                     new FormFieldDefinition
                    {
                        Name = "tagId",
                        Caption = "تگ",
                        Type = "dynamicSelect",
                        FetchConfig =new FetchConfig{api="tags/selectOption",fetchFilters=[] },
                        PlaceHolder = "تگ را انتخاب کنید",
                        Help = "کدام تگ را به محصول اضافه می‌کنید",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "انتخاب تگ الزامی است" }
                        }
                    }
                }),
                IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1),
                CreatedBy = 1,
                IsDeleted = false
            };
        }
        private static object CreateBlogTagsEntity()
        {
            return new
            {
                Id = 13,
                EntityName = "blogTags",
                PersianDisplayName = "تگ‌های بلاگ",
                EnglishDisplayName = "Blog tag",
                EndPoint = "blogTags",
                EntityIconBase64 = @"<svg xmlns=""http://www.w3.org/2000/svg"" fill=""none"" viewBox=""0 0 24 24"" stroke-width=""2"" stroke=""currentColor"" class=""w-6 h-6""><path stroke-linecap=""round"" stroke-linejoin=""round"" d=""M7 7h10M7 12h10M7 17h10""/></svg>",
                ActionsJson = JsonSerializer.Serialize(new List<string> { "active", "edit", "delete", "new" }),
                ColumnsJson = JsonSerializer.Serialize(new List<JsonDefinition>
                {
                    new JsonDefinition {Header = "شناسه",Accessor="id",Type="number",Sortable=false,Filterable=false,Options=null},
                    new JsonDefinition {Header = "نام تگ",Accessor="tagName",Type="text",Sortable=false,Filterable=false,Options=null},
                    new JsonDefinition {Header = "عنوان فارسی مقاله",Accessor="blogTitleFa",Type="text",Sortable=false,Filterable=false,Options=null},
                    new JsonDefinition {Header = "عنوان انگلیسی مقاله",Accessor="blogTitleEn",Type="text",Sortable=false,Filterable=false,Options=null}
                }),
                FormFieldsJson = JsonSerializer.Serialize(new List<FormFieldDefinition>
                {
                    new FormFieldDefinition
                    {
                        Name = "blogId",
                        Caption = "شناسه مقاله",
                        Type = "dynamicSelect",
                        FetchConfig =new FetchConfig{api="Blogs/selectOption",fetchFilters=[] },
                        PlaceHolder = " مقاله را انتخاب کنید",
                        Help = "به چه مقاله ای تگ وارد شود ؟",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "انتخاب مقاله الزامی است" }
                        }
                    },
                     new FormFieldDefinition
                    {
                        Name = "tagId",
                        Caption = "تگ",
                        Type = "dynamicSelect",
                        FetchConfig =new FetchConfig{api="tags/selectOption",fetchFilters=[] },
                        PlaceHolder = "تگ را انتخاب کنید",
                        Help = "کدام تگ را به مقاله اضافه می‌کنید",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "انتخاب تگ الزامی است" }
                        }
                    }
                }),
                IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1),
                CreatedBy = 1,
                IsDeleted = false
            };
        }
        private static object CreateUserTagsEntity()
        {
            return new
            {
                Id = 14,
                EntityName = "userTags",
                PersianDisplayName = "تگ‌های اشخاص",
                EnglishDisplayName = "User tags",
                EndPoint = "userTags",
                EntityIconBase64 = @"<svg xmlns=""http://www.w3.org/2000/svg"" fill=""none"" viewBox=""0 0 24 24"" stroke-width=""2"" stroke=""currentColor"" class=""w-6 h-6""><path stroke-linecap=""round"" stroke-linejoin=""round"" d=""M7 7h10M7 12h10M7 17h10""/></svg>",
                ActionsJson = JsonSerializer.Serialize(new List<string> { "active", "edit", "delete", "new" }),
                ColumnsJson = JsonSerializer.Serialize(new List<JsonDefinition>
                {
                    new JsonDefinition {Header = "شناسه",Accessor="id",Type="number",Sortable=false,Filterable=false,Options=null},
                    new JsonDefinition {Header = "نام تگ",Accessor="tagName",Type="text",Sortable=false,Filterable=false,Options=null},
                    new JsonDefinition {Header = "نام شخص",Accessor="userName",Type="text",Sortable=false,Filterable=false,Options=null}

                }),
                FormFieldsJson = JsonSerializer.Serialize(new List<FormFieldDefinition>
                {
                    new FormFieldDefinition
                    {
                        Name = "userId",
                        Caption = "شناسه کاربر",
                        Type = "dynamicSelect",
                        FetchConfig =new FetchConfig{api="users/selectOption",fetchFilters=[] },
                        PlaceHolder = " کاربر را انتخاب کنید",
                        Help = "به چه کاربری تگ وارد شود ؟",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "انتخاب کاربر الزامی است" }
                        }
                    },
                     new FormFieldDefinition
                    {
                        Name = "tagId",
                        Caption = "تگ",
                        Type = "dynamicSelect",
                        FetchConfig =new FetchConfig{api="tags/selectOption",fetchFilters=[] },
                        PlaceHolder = "تگ را انتخاب کنید",
                        Help = "کدام تگ را به کاربر اضافه می‌کنید",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "انتخاب تگ الزامی است" }
                        }
                    }
                }),
                IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1),
                CreatedBy = 1,
                IsDeleted = false
            };
        }
        private static object CreateCommentEntity()
        {
            return new
            {
                Id = 15,
                EntityName = "comments",
                PersianDisplayName = "نظرات",
                EnglishDisplayName = "comment",
                EndPoint = "comments",
                EntityIconBase64 = @"<svg width=""13"" height=""13"" viewBox=""0 0 13 13"" fill=""none"" xmlns=""http://www.w3.org/2000/svg""><path d=""M6.49967 4.33337C5.30306 4.33337 4.33301 5.30342 4.33301 6.50004C4.33301 7.69666 5.30306 8.66671 6.49967 8.66671C7.69629 8.66671 8.66634 7.69666 8.66634 6.50004C8.66634 5.30342 7.69629 4.33337 6.49967 4.33337Z"" stroke=""#B1B1B1"" stroke-linecap=""round"" stroke-linejoin=""round""/></svg>",
                ActionsJson = JsonSerializer.Serialize(new List<string> { "active", "edit", "delete", "approve", "new" }),
                ColumnsJson = JsonSerializer.Serialize(new List<JsonDefinition>
                {
                    new JsonDefinition { Header= "شناسه", Accessor= "id", Type= "number", Sortable = false, Filterable = false },
                    new JsonDefinition { Header= "کاربر", Accessor= "userName", Type= "text", Sortable = false, Filterable = false },
                    new JsonDefinition { Header= " مورد نظر", Accessor= "targetTitle", Type= "text", Sortable = false, Filterable = false },
                    new JsonDefinition { Header= "نظر به", Accessor= "targetType", Type= "text", Sortable = false, Filterable = false },
                    new JsonDefinition { Header= "محتوا", Accessor= "content", Type= "textarea", Sortable = false, Filterable = false },
                    new JsonDefinition { Header= "قابل نمایش", Accessor= "isApproved", Type= "bool", Sortable = false, Filterable = false },
                    new JsonDefinition { Header= "اشاره به نظر", Accessor= "parentName", Type= "text", Sortable = false, Filterable = false }
                }),
                FormFieldsJson = JsonSerializer.Serialize(new List<FormFieldDefinition>
                {
                    new FormFieldDefinition
                    {
                        Name = "userId",
                        Caption = "کاربر",
                        Type = "dynamicSelect",
                        FetchConfig =new FetchConfig{api="Users/selectOption",fetchFilters=["id"] },
                        PlaceHolder = "کاربر مد نظر",
                        Help = "مثلاً:علی این نظر را داده",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "کاربر الزامی است" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "targetId",
                        Caption = "آیدی مورد نظر",
                        Type = "select",
                        Options= [new Option{Label="Blog",Value=1 }, new Option { Label="Product",Value=2 }, new Option { Label="Supplier",Value=3 }],
                        PlaceHolder = "مثلاً: از تایپ x منظور آیدی y هست",
                        Help = "با ترکیب آیدی و تایپ مشخص میکنیم از کدوم دسته بندی به کدوم آیدی داریم اشاره می کنیم ",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "آیدی الزامی است" }
                        }
                    }, new FormFieldDefinition
                    {
                        Name = "targetType",
                        Caption = "دسته‌بندی مورد نظر",
                        Type = "select",
                        PlaceHolder = "دسته را انتخاب کنید",
                        Options= [new Option{Label="Blog",Value=1 }, new Option { Label="Product",Value=2 }, new Option { Label="Supplier",Value=3 }],
                        Help = "دسته را انتخاب کنید",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "انتخاب دسته بندی الزامی است" }
                        }
                    }, new FormFieldDefinition
                    {
                        Name = "content",
                        Caption = "محتوا",
                        Type = "textarea",
                        PlaceHolder = "نظر را بنویسید...",
                        Help = "نظر را بنویسید...",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "50", Message = "محتوا باید حداقل ۵۰ کاراکتر باشد" }
                        }
                    }, new FormFieldDefinition
                    {
                        Name = "isApproved",
                        Caption = "قابل نمایش",
                        Type = "bool",
                        PlaceHolder = "قابل نمایش",
                        Help = "قابل نمایش",
                        Rules = []
                    }, new FormFieldDefinition
                    {
                        Name = "parentId",
                        Caption = "ریپلای به",
                        Type = "dynamicSelect",
                        FetchConfig =new FetchConfig{api="Users/selectOption",fetchFilters=[]  },
                        PlaceHolder = "تگ را انتخاب کنید",
                        Help = "کدام تگ را به محصول اضافه می‌کنید",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "انتخاب تگ الزامی است" }
                        }
                    }
                }),
                IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1),
                CreatedBy = 1,
                IsDeleted = false
            };
        }
        private static object CreateProductOffersEntity()
        {
            return new
            {
                Id = 16,
                EntityName = "productOffers",
                PersianDisplayName = "سفارش فروش",
                EnglishDisplayName = "Product offers",
                EndPoint = "productOffers",
                EntityIconBase64 = @"<svg xmlns=""http://www.w3.org/2000/svg"" fill=""none"" viewBox=""0 0 24 24"" stroke-width=""1.5"" stroke=""currentColor"" class=""size-6"">
  <path stroke-linecap=""round"" stroke-linejoin=""round"" d=""m20.25 7.5-.625 10.632a2.25 2.25 0 0 1-2.247 2.118H6.622a2.25 2.25 0 0 1-2.247-2.118L3.75 7.5M10 11.25h4M3.375 7.5h17.25c.621 0 1.125-.504 1.125-1.125v-1.5c0-.621-.504-1.125-1.125-1.125H3.375c-.621 0-1.125.504-1.125 1.125v1.5c0 .621.504 1.125 1.125 1.125Z"" />
</svg>
",
                ActionsJson = JsonSerializer.Serialize(new List<string> { "active", "edit", "delete", "new" }),
                ColumnsJson = JsonSerializer.Serialize(new List<JsonDefinition>
                {
                    new JsonDefinition { Header = "شناسه", Accessor = "id", Type = "number", Sortable = false, Filterable = false,  },
                    new JsonDefinition { Header = "نام محصول", Accessor = "productName", Type = "text", Sortable = false, Filterable = false,  },
                    new JsonDefinition { Header = "نام تامین کننده", Accessor = "supplierName", Type = "text", Sortable = false, Filterable = false,  },
                    new JsonDefinition { Header = "تصویر تامین کننده", Accessor = "supplierImage", Type = "image", Sortable = false, Filterable = false},
                    new JsonDefinition { Header = "قیمت پایه", Accessor = "basePrice", Type = "price", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "قیمت نهایی", Accessor = "finalPrice", Type = "price", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "موجودی", Accessor = "inventory", Type = "number", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "تاریخ ایجاد", Accessor = "createdAt", Type = "date", Sortable = false, Filterable = false },
                }),
                FormFieldsJson = JsonSerializer.Serialize(new List<FormFieldDefinition>
                {
                    new FormFieldDefinition
                    {
                        Name = "productId",
                        Caption = "انتخاب محصول",
                        Type = "dynamicSelect",
                        FetchConfig =new FetchConfig{api="Products/selectOption",fetchFilters=[]},
                        PlaceHolder = " محصول را انتخاب کنید",
                        Help = "مثلاً: تلویزیون سامسونگ 55 اینچ",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = " محصول الزامی است" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "basePrice",
                        Caption = "قیمت پایه",
                        Type = "price",
                        PlaceHolder = "قیمت پایه محصول را وارد کنید",
                        Help = "قیمت محصول را بنویسید",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "قیمت پایه الزامی است" },
                        }
                    },

                    new FormFieldDefinition
                    {
                        Name = "inventory",
                        Caption = "موجودی",
                        Type = "number",
                        PlaceHolder = "تعداد موجودی را وارد کنید",
                        Help = "تعداد محصول در انبار",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "موجودی الزامی است" },
                            new ValidationRule { Rule = "min", Condition = "0", Message = "موجودی نمی‌تواند منفی باشد" }
                        }
                    },

                }),
                IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1),
                CreatedBy = 1,
                IsDeleted = false
            };
        }
        private static object CreateDiscountCodesEntity()
        {
            return new
            {
                Id = 17,
                EntityName = "discountCodes",
                PersianDisplayName = "کد تخفیف",
                EnglishDisplayName = "Promotions",
                EndPoint = "discountCodes",
                EntityIconBase64 = @"<svg xmlns=""http://www.w3.org/2000/svg"" fill=""none"" viewBox=""0 0 24 24"" stroke-width=""1.5"" stroke=""currentColor"" class=""size-6"">
  <path stroke-linecap=""round"" stroke-linejoin=""round"" d=""m20.25 7.5-.625 10.632a2.25 2.25 0 0 1-2.247 2.118H6.622a2.25 2.25 0 0 1-2.247-2.118L3.75 7.5M10 11.25h4M3.375 7.5h17.25c.621 0 1.125-.504 1.125-1.125v-1.5c0-.621-.504-1.125-1.125-1.125H3.375c-.621 0-1.125.504-1.125 1.125v1.5c0 .621.504 1.125 1.125 1.125Z"" />
</svg>
",
                ActionsJson = JsonSerializer.Serialize(new List<string> { "active", "edit", "delete", "new" }),
                ColumnsJson = JsonSerializer.Serialize(new List<JsonDefinition>
                {
                    new JsonDefinition { Header = "شناسه", Accessor = "id", Type = "number", Sortable = false, Filterable = false},
                    new JsonDefinition {Header="کد تخفیف",Accessor="code",Type="text",Sortable=false,Filterable=false,Options=null},
                    new JsonDefinition {Header="مقدار تخفیف",Accessor="amount",Type="number",Sortable=false,Filterable=false,Options=null},
                    new JsonDefinition {Header="درصدی",Accessor="isPercent",Type="bool",Sortable=false,Filterable=false,Options=null},
                    new JsonDefinition {Header="تاریخ شروع",Accessor="startDate",Type="date",Sortable=false,Filterable=false,Options=null},
                    new JsonDefinition {Header="تاریخ پایان",Accessor="endDate",Type="date",Sortable=false,Filterable=false,Options=null},
                    new JsonDefinition {Header="آیدی مالک",Accessor="userId",Type="number",Sortable=false,Filterable=false,Options=null},
                    new JsonDefinition {Header="محدودیت استفاده",Accessor="usageLimit",Type="number",Sortable=false,Filterable=false,Options=null},
                    new JsonDefinition {Header="تعداد استفاده",Accessor="usedCount",Type="number",Sortable=false,Filterable=false,Options=null},
                    new JsonDefinition {Header="دارای اعتبار",Accessor="isValid",Type="bool",Sortable=false,Filterable=false,Options=null}
                }),
                FormFieldsJson = JsonSerializer.Serialize(new List<FormFieldDefinition>
                {
                    new FormFieldDefinition
                    {
                        Name = "code",
                        Caption = "متن کد",
                        Type = "text",
                        PlaceHolder = " متن کد تخفیف را وارد کنید",
                        Help = " هر متن دلخواهی می توان وارد کرد",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = " متن کد تخفیف الزامی است" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "amount",
                        Caption = "مقدار تخفیف",
                        Type = "number",
                        PlaceHolder = " مقدار را وارد کنید",
                        Help = "",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = " مقدار تخفیف الزامی است" },
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "isPercent",
                        Caption = " نوع تخفیف درصدی؟",
                        Type = "checkbox",
                        PlaceHolder = "عدد وارد شده به درصد از مبلغ کم شود یا خیر",
                        Help = "عدد وارد شده به درصد از مبلغ کم شود یا خیر",
                        Rules = new List<ValidationRule>{}
                    },
                                        new FormFieldDefinition
                    {
                        Name = "startDate",
                        Caption = "تاریخ شروع ",
                        Type = "date",
                        PlaceHolder = "تاریخ شروع اعتبار کد تخفیف",
                        Help = "تاریخ شروع اعتبار کد تخفیف",
                         Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message =" تاریخ شروع الزامی است"},
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "endDate",
                        Caption = "تاریخ پایان",
                        Type = "date",
                        PlaceHolder = "تاریخ پایان اعتبار کد تخفیف",
                        Help = "تاریخ پایان اعتبار کد تخفیف",
                         Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = " تاریخ پایان الزامی است"},
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "usageLimit",
                        Caption = "محدودیت ",
                        Type = "number",
                        PlaceHolder ="محدودیت تعداد استفاده از کد",
                        Help ="محدودیت تعداد استفاده از کد",
                        Rules = new List<ValidationRule>{}
                    },
                    new FormFieldDefinition
                    {
                        Name ="userId",
                        Caption ="آیدی کاربر",
                        Type = "number",
                        PlaceHolder ="آیدی کاربری که می تواند از این کد استفاده کند",
                        Help = "آیدی کاربری که می تواند از این کد استفاده کند",
                        Rules = new List<ValidationRule>{}
                    },

                }),
                IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1),
                CreatedBy = 1,
                IsDeleted = false
            };
        }
        private static object CreatePaymentMethodesEntity()
        {
            return new
            {
                Id = 18,
                EntityName = "paymentMethods",
                PersianDisplayName = "نوع پرداخت",
                EnglishDisplayName = "Payment method",
                EndPoint = "paymentMethods",
                EntityIconBase64 = @"<svg xmlns=""http://www.w3.org/2000/svg"" fill=""none"" viewBox=""0 0 24 24"" stroke-width=""1.5"" stroke=""currentColor"" class=""size-6"">
  <path stroke-linecap=""round"" stroke-linejoin=""round"" d=""m20.25 7.5-.625 10.632a2.25 2.25 0 0 1-2.247 2.118H6.622a2.25 2.25 0 0 1-2.247-2.118L3.75 7.5M10 11.25h4M3.375 7.5h17.25c.621 0 1.125-.504 1.125-1.125v-1.5c0-.621-.504-1.125-1.125-1.125H3.375c-.621 0-1.125.504-1.125 1.125v1.5c0 .621.504 1.125 1.125 1.125Z"" />
</svg>
",
                ActionsJson = JsonSerializer.Serialize(new List<string> { "active", "edit", "delete", "new" }),
                ColumnsJson = JsonSerializer.Serialize(new List<JsonDefinition>
                {
                    new JsonDefinition {Header = "شناسه", Accessor = "id", Type = "number", Sortable = false, Filterable = false},
                    new JsonDefinition {Header="درگاه",Accessor="title",Type="text",Sortable = false, Filterable = false},
                    new JsonDefinition {Header="آنلاین",Accessor="isOnline",Type="bool",Sortable = false, Filterable = false},
                    new JsonDefinition {Header="کانفیگ",Accessor="configJson",Type="text",Sortable = false, Filterable = false},
                    new JsonDefinition {Header="ترتیب نمایش",Accessor="displayOrder",Type="number",Sortable = false, Filterable = false}
                }),
                FormFieldsJson = JsonSerializer.Serialize(new List<FormFieldDefinition>
                {
                    new FormFieldDefinition
                    {
                        Name = "title",
                        Caption = "نام درگاه",
                        Type = "text",
                        PlaceHolder = " نام درگاه را وارد کنید",
                        Help = " نام درگاه را وارد کنید",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = " نام درگاه  الزامی است" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "code",
                        Caption = "کد درگاه",
                        Type = "text",
                        PlaceHolder = "کد درگاه را وارد کنید",
                        Help = "کد درگاه الزامی است",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "کد درگاه الزامی است" },
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "isOnline",
                        Caption = "فعال ",
                        Type = "checkbox",
                        PlaceHolder = "آیا این درگاه کار میکند؟",
                        Help = "آیا این درگاه کار میکند؟",
                         Rules = new List<ValidationRule>
                        {}
                    },
                    new FormFieldDefinition
                    {
                        Name = "configJson",
                        Caption = "کانفیگ ",
                        Type = "text",
                        PlaceHolder = "کد جیسون مربوط به کانکشن درگاه",
                        Help = "کد جیسون مربوط به کانکشن درگاه",
                         Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message =" کد جیسون مربوط به کانکشن درگاه الزامی است"},
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "displayOrder",
                        Caption = "ترتیب",
                        Type = "number",
                        PlaceHolder = "ترتیب نمایش را مشخص کنید",
                        Help = "اولویت درگاه را تغییر دهید",
                         Rules = new List<ValidationRule>
                        {

                        }
                    }
                }),
                IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1),
                CreatedBy = 1,
                IsDeleted = false
            };
        }
        private static object CreateSpecialOffersEntity()
        {
            return new
            {
                Id = 19,
                EntityName = "specialOffers",
                PersianDisplayName = "پیشنهاد ویژه",
                EnglishDisplayName = "Special offers",
                EndPoint = "specialOffers",
                EntityIconBase64 = @"<svg xmlns=""http://www.w3.org/2000/svg"" fill=""none"" viewBox=""0 0 24 24"" stroke-width=""1.5"" stroke=""currentColor"" class=""size-6"">
                  <path stroke-linecap=""round"" stroke-linejoin=""round"" d=""m20.25 7.5-.625 10.632a2.25 2.25 0 0 1-2.247 2.118H6.622a2.25 2.25 0 0 1-2.247-2.118L3.75 7.5M10 11.25h4M3.375 7.5h17.25c.621 0 1.125-.504 1.125-1.125v-1.5c0-.621-.504-1.125-1.125-1.125H3.375c-.621 0-1.125.504-1.125 1.125v1.5c0 .621.504 1.125 1.125 1.125Z"" />
                </svg>
                ",
                ActionsJson = JsonSerializer.Serialize(new List<string> { "active", "edit", "delete", "new" }),
                ColumnsJson = JsonSerializer.Serialize(new List<JsonDefinition>
                {
                    new JsonDefinition {Header="شناسه",Accessor="id",Type ="number",Sortable =false,Filterable =false,Options =null},
                    new JsonDefinition {Header="نام محصول",Accessor="productName",Type ="text",Sortable =false,Filterable =false,Options =null},
                    new JsonDefinition {Header="عکس محصول",Accessor="productImage",Type ="image",Sortable =false,Filterable =false,Options =null},
                    new JsonDefinition {Header="نام تامین کننده",Accessor="supplierName",Type ="text",Sortable =false,Filterable =false,Options =null},
                    new JsonDefinition {Header="تاریخ شروع",Accessor="startDate",Type ="date",Sortable =false,Filterable =false,Options =null},
                    new JsonDefinition {Header="تاریخ پایان",Accessor="endDate",Type ="date",Sortable =false,Filterable =false,Options =null},
                    new JsonDefinition {Header="ترتیب نمایش",Accessor="displayOrder",Type ="number",Sortable =false,Filterable =false,Options =null}
                }),
                FormFieldsJson = JsonSerializer.Serialize(new List<FormFieldDefinition>
                {
                    new FormFieldDefinition
                    {
                        Name ="productId",
                        Caption = "انتخاب محصول",
                        Type = "dynamicSelect",
                        FetchConfig =new FetchConfig{api="Products/selectOption",fetchFilters=[] },
                        PlaceHolder = " محصول را انتخاب کنید",
                        Help = "مثلاً: تلویزیون سامسونگ 55 اینچ",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = " محصول الزامی است" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "productOfferId",
                        Caption ="انتخاب سفارش",
                        Type = "dynamicSelect",
                        FetchConfig =new FetchConfig{api="productOffers/selectOption",fetchFilters=["productId"] },
                        PlaceHolder = " سفارش را انتخاب کنید",
                        Help = " سفارش را انتخاب کنید",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = " پیشنهاد محصول الزامی است" }

                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "discountId",
                        Caption = "انتخاب تخفیف ",
                        Type = "dynamicSelect",
                        FetchConfig =new FetchConfig{api="discounts/selectOption",fetchFilters=[] },
                        PlaceHolder = "مثلا :یلدا",
                        Help = "تخفیف خاصی که می خواهید بر روی سفارش شما اعمال بشه رو انتخاب کنید",
                         Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = " تخفیف الزامی است" },
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "startDate",
                        Caption = "زمان شروع",
                        Type = "date",
                        PlaceHolder ="انتخاب تاریخ",
                        Help = "زمان شروع پیشنهاد ویژه",
                         Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message ="زمان شروع الزامی است"},
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name ="endDate",
                        Caption = "زمان پایان",
                        Type = "date",
                        PlaceHolder ="انتخاب تاریخ",
                        Help = "زمان پایان پیشنهاد ویژه",
                          Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message ="زمان پایان الزامی است"},
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name ="displayOrder",
                        Caption = "ترتیب نمایش",
                        Type = "number",
                        PlaceHolder ="اولویت نمایش",
                        Help = "در صفحه اصلی به ترتیب این اولویت نمایش داده میشود",
                          Rules = new List<ValidationRule>
                        {}
                    }
                }),
                IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1),
                CreatedBy = 1,
                IsDeleted = false
            };
        }
        private static object CreateLandingSlidesEntity()
        {
            return new
            {
                Id = 20,
                EntityName = "landing",
                PersianDisplayName = "اسلاید صفحه اصلی",
                EnglishDisplayName = "landing slide",
                EndPoint = "landing",
                EntityIconBase64 = @"<svg xmlns=""http://www.w3.org/2000/svg"" fill=""none"" viewBox=""0 0 24 24"" stroke-width=""1.5"" stroke=""currentColor"" class=""size-6"">
                  <path stroke-linecap=""round"" stroke-linejoin=""round"" d=""m20.25 7.5-.625 10.632a2.25 2.25 0 0 1-2.247 2.118H6.622a2.25 2.25 0 0 1-2.247-2.118L3.75 7.5M10 11.25h4M3.375 7.5h17.25c.621 0 1.125-.504 1.125-1.125v-1.5c0-.621-.504-1.125-1.125-1.125H3.375c-.621 0-1.125.504-1.125 1.125v1.5c0 .621.504 1.125 1.125 1.125Z"" />
                </svg>
                ",
                ActionsJson = JsonSerializer.Serialize(new List<string> { "active", "edit", "delete", "new", "default" }),
                ColumnsJson = JsonSerializer.Serialize(new List<JsonDefinition>
                {
                    new JsonDefinition {Header="شناسه",Accessor="id",Type ="number",Sortable =false,Filterable =false,Options =null},
                    new JsonDefinition {Header="عکس",Accessor="banner",Type ="image",Sortable =false,Filterable =false,Options =null},
                    new JsonDefinition {Header="عنوان بنر",Accessor="bannerTitle",Type ="text",Sortable =false,Filterable =false,Options =null},
                    new JsonDefinition {Header="توضیح بنر",Accessor="bannerDescription",Type ="text",Sortable =false,Filterable =false,Options =null},
                    new JsonDefinition {Header="آدرس اول",Accessor="firstUrl",Type ="text",Sortable =false,Filterable =false,Options =null},
                    new JsonDefinition {Header="آدرس دوم",Accessor="secondUrl",Type ="text",Sortable =false,Filterable =false,Options =null},
                    new JsonDefinition {Header="ویدیو دسکتاپ",Accessor="videoUrl",Type ="text",Sortable =false,Filterable =false,Options =null},
                    new JsonDefinition {Header="ویدیو موبایل",Accessor="mobileVideoUrl",Type ="text",Sortable =false,Filterable =false,Options =null},
                    new JsonDefinition {Header="نمایش بنر",Accessor="isHero",Type ="bool",Sortable =false,Filterable =false,Options =null},

                }),
                FormFieldsJson = JsonSerializer.Serialize(new List<FormFieldDefinition>
                {
                    new FormFieldDefinition
                    {
                        Name ="bannerUrl",
                        Caption = "پوستر / عکس دسکتاپ",
                        Type ="file",
                        PlaceHolder = "انتخاب بنر",
                        Help = "عکس نسخه‌ی دسکتاپ؛ پوستر ویدیو و جایگزین آن هم هست",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "انتخاب عکس الزامی است" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name ="bannerTitle",
                        Caption ="عنوان بنر",
                        Type = "text",
                        PlaceHolder ="عنوان بنر",
                        Help = "در بنر چه عنوانی نمایش داده شود ؟ ؟",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "عنوان بنر الزامی است" }

                        }
                    },
                    new FormFieldDefinition
                    {
                        Name ="bannerDescription",
                        Caption ="توضیح بنر",
                        Type = "text",
                        PlaceHolder ="توضیحات بنر",
                        Help = "در بنر چه توضیحاتی نمایش داده شود ؟",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "توضیحات بنر الزامی است" }

                        }
                    },
                    new FormFieldDefinition
                    {
                        Name ="firstUrl",
                        Caption ="آدرس اول",
                        Type = "text",
                        PlaceHolder ="آدرس اول",
                        Help = "با کلیک بر روی عکس به چه آدرسی برود ؟",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "آدرس   الزامی است" }

                        }
                    },
                    new FormFieldDefinition
                    {
                        Name ="secondUrl",
                        Caption ="آدرس دوم",
                        Type = "text",
                        PlaceHolder ="آدرس صفحه دوم",
                        Help = "با کلیک بر روی دکمه دوم به چه آدرسی برود ؟ (اختیاری)",
                        Rules = new List<ValidationRule>()
                    },
                    new FormFieldDefinition
                    {
                        Name ="videoUrl",
                        Caption ="ویدیوی دسکتاپ (اختیاری)",
                        Type = "video",
                        PlaceHolder ="انتخاب ویدیو (mp4 یا webm)",
                        Help = "ویدیوی کوتاه بی‌صدا، حداکثر ۳۰ مگابایت، ترجیحاً ۱۶:۹ و حدود ۱۵ ثانیه. عکس بنر به‌عنوان پوستر و جایگزین نمایش داده می‌شود.",
                        Rules = new List<ValidationRule>()
                    },
                    new FormFieldDefinition
                    {
                        Name ="mobileBannerUrl",
                        Caption ="پوستر موبایل (اختیاری)",
                        Type = "file",
                        PlaceHolder ="انتخاب پوستر موبایل",
                        Help = "عکس عمودی مخصوص صفحه‌های کوچک؛ اگر خالی باشد از عکس دسکتاپ استفاده می‌شود",
                        Rules = new List<ValidationRule>()
                    },
                    new FormFieldDefinition
                    {
                        Name ="mobileVideoUrl",
                        Caption ="ویدیوی موبایل (اختیاری)",
                        Type = "video",
                        PlaceHolder ="انتخاب ویدیو (mp4 یا webm)",
                        Help = "ویدیوی سبک و عمودی برای موبایل، حداکثر ۳۰ مگابایت؛ اگر خالی باشد از ویدیوی دسکتاپ استفاده می‌شود",
                        Rules = new List<ValidationRule>()
                    },
                    new FormFieldDefinition
                    {
                        Name ="removeMobileBanner",
                        Caption ="حذف پوستر موبایل فعلی",
                        Type = "checkbox",
                        PlaceHolder ="",
                        Help = "در ویرایش: برای حذف پوستر موبایل ثبت‌شده فعال کنید.",
                        Rules = new List<ValidationRule>()
                    },
                    new FormFieldDefinition
                    {
                        Name ="removeMobileVideo",
                        Caption ="حذف ویدیوی موبایل فعلی",
                        Type = "checkbox",
                        PlaceHolder ="",
                        Help = "در ویرایش: برای حذف ویدیوی موبایل ثبت‌شده فعال کنید.",
                        Rules = new List<ValidationRule>()
                    },
                    new FormFieldDefinition
                    {
                        Name ="removeVideo",
                        Caption ="حذف ویدیو فعلی",
                        Type = "checkbox",
                        PlaceHolder ="",
                        Help = "در ویرایش: برای حذف ویدیوی ثبت‌شده فعال کنید.",
                        Rules = new List<ValidationRule>()
                    }
                   
                }),
                IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1),
                CreatedBy = 1,
                IsDeleted = false
            };
        }
        private static object CreateRatesEntity()
        {
            return new
            {
                Id = 21,
                EntityName = "rates",
                PersianDisplayName = "امتیاز دهی",
                EnglishDisplayName = "Rates",
                EndPoint = "rates",
                EntityIconBase64 = @"<svg xmlns=""http://www.w3.org/2000/svg"" fill=""none"" viewBox=""0 0 24 24"" stroke-width=""1.5"" stroke=""currentColor"" class=""size-6"">
                  <path stroke-linecap=""round"" stroke-linejoin=""round"" d=""m20.25 7.5-.625 10.632a2.25 2.25 0 0 1-2.247 2.118H6.622a2.25 2.25 0 0 1-2.247-2.118L3.75 7.5M10 11.25h4M3.375 7.5h17.25c.621 0 1.125-.504 1.125-1.125v-1.5c0-.621-.504-1.125-1.125-1.125H3.375c-.621 0-1.125.504-1.125 1.125v1.5c0 .621.504 1.125 1.125 1.125Z"" />
                </svg>
                ",
                ActionsJson = JsonSerializer.Serialize(new List<string> { "active", "edit", "delete", "new" }),
                ColumnsJson = JsonSerializer.Serialize(new List<JsonDefinition>
                {
                    new JsonDefinition {Header="شناسه",Accessor="id",Type ="number",Sortable =false,Filterable =false,Options =null},
                    new JsonDefinition {Header="کاربر", Accessor = "userName", Type = "text",Sortable =false,Filterable =false,Options =null},
                    new JsonDefinition {Header="نوع", Accessor = "targetType", Type = "text",Sortable =false,Filterable =false,Options =null},
                    new JsonDefinition {Header="شناسه هدف", Accessor = "targetId", Type = "number",Sortable =false,Filterable =false,Options =null},
                    new JsonDefinition {Header="امتیاز", Accessor = "rateValue", Type = "rate",Sortable =false,Filterable =false,Options =null},
                }),
                FormFieldsJson = JsonSerializer.Serialize(new List<FormFieldDefinition>
                {
                    new FormFieldDefinition
                    {
                        Name ="userId",
                        Caption = "کاربر",
                        Type ="dynamicSelect",
                        FetchConfig =new FetchConfig{api="Users/selectOption",fetchFilters=[] },
                        PlaceHolder ="کاربر مد نظر",
                        Help ="مثلاً:علی این نظر را داده",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message ="کاربر الزامی است" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name ="targetType",
                        Caption ="دسته‌بندی مورد نظر",
                        Type ="select",
                        Options= [new Option{Label="Blog",Value=1 }, new Option { Label="Product",Value=2 }, new Option { Label="Supplier",Value=3 }],
                        PlaceHolder ="دسته را انتخاب کنید",
                        Help = "دسته را انتخاب کنید",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "انتخاب دسته الزامی است" }

                        }
                    },
                    new FormFieldDefinition
                    {
                        Name ="targetId",
                        Caption ="شناسه هدف",
                        Type ="number",
                        PlaceHolder ="انتخاب شناسه هدف",
                        Help ="شناسه محصول یا مقاله یا تامین کننده را وارد کنید",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "شناسه هدف الزامی است" }

                        }
                    },
                    new FormFieldDefinition
                    {
                        Name ="value",
                        Caption ="امتیاز",
                        Type ="number",
                        PlaceHolder ="از یک تا پنج امتیاز بدید",
                        Help = "از یک که پایینترین امتیاز است تا 5 بالا ترین امتیاز",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "امتیاز الزامی است" }

                        }
                    }
                }),
                IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1),
                CreatedBy = 1,
                IsDeleted = false
            };
        }
        private static object CreateShippingMethodsEntity()
        {
            return new
            {
                Id = 22,
                EntityName = "shippingMethods",
                PersianDisplayName = "نحوه ارسال",
                EnglishDisplayName = "Shipping methods",
                EndPoint = "shippingMethods",
                EntityIconBase64 = @"<svg xmlns=""http://www.w3.org/2000/svg"" fill=""none"" viewBox=""0 0 24 24"" stroke-width=""1.5"" stroke=""currentColor"" class=""size-6"">
                  <path stroke-linecap=""round"" stroke-linejoin=""round"" d=""m20.25 7.5-.625 10.632a2.25 2.25 0 0 1-2.247 2.118H6.622a2.25 2.25 0 0 1-2.247-2.118L3.75 7.5M10 11.25h4M3.375 7.5h17.25c.621 0 1.125-.504 1.125-1.125v-1.5c0-.621-.504-1.125-1.125-1.125H3.375c-.621 0-1.125.504-1.125 1.125v1.5c0 .621.504 1.125 1.125 1.125Z"" />
                </svg>
                ",
                ActionsJson = JsonSerializer.Serialize(new List<string> { "active", "edit", "delete", "new" }),
                ColumnsJson = JsonSerializer.Serialize(new List<JsonDefinition>
                {
                    new JsonDefinition {Header = "شناسه", Accessor = "id", Type = "text", Sortable = false, Filterable = false},
                    new JsonDefinition {Header="نام",Accessor="title",Type="text",Sortable = false, Filterable = false},
                    new JsonDefinition {Header="توضیحات",Accessor="description",Type="text",Sortable = false, Filterable = false},
                    new JsonDefinition {Header="قیمت",Accessor="price",Type="price",Sortable = false, Filterable = false},
                    new JsonDefinition {Header="زمان تحویل/ساعت",Accessor="estimatedDeliveryTime",Type="number",Sortable = false, Filterable = false},
                    new JsonDefinition {Header="پیش فرض",Accessor="isDefault",Type="bool",Sortable = false, Filterable = false}
                }),
                FormFieldsJson = JsonSerializer.Serialize(new List<FormFieldDefinition>
                {
                    new FormFieldDefinition
                    {
                        Name ="title",
                        Caption = "نام نحوه ارسال",
                        Type = "text",
                        PlaceHolder = "مثلا :پیک",
                        Help = "نحوه ی ارسال مرسوله را مشخص می کند",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "نام نحوه ارسال الزامی است" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "description",
                        Caption ="توضیحات ",
                        Type = "text",
                        PlaceHolder = "مثلا :استفاده از باربری تهران",
                        Help = "توضیح کوتاهی در مورد نحوه ارسال بدهید",
                        Rules = new List<ValidationRule>
                        {}
                    },
                    new FormFieldDefinition
                    {
                        Name = "price",
                        Caption = "هزینه ارسال",
                        Type = "price",
                        PlaceHolder = "200,000 تومان",
                        Help = "هزینه ارسال مستقیم به قیمت پایانی فاکتور اضافه می گردد",
                         Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "هزینه ارسال الزامی است" },
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "estimatedDeliveryTime",
                        Caption = "تخمین فاصله زمانی",
                        Type = "number",
                        PlaceHolder ="مثلا:4",
                        Help = "مفهوم می تواند به روز یا ساعت باشد",
                         Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message ="زمان ارسال الزامی است"},
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name ="isDefault",
                        Caption = "پیش فرض",
                        Type = "checkbox",
                        PlaceHolder ="مثلا:4",
                        Help = "اگر کاربر نحوه ارسال را انتخاب نکند این نوع ارسال پیش فرض قرار گیرد",
                         Rules = new List<ValidationRule>
                        {}
                    }
                }),
                IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1),
                CreatedBy = 1,
                IsDeleted = false
            };
        }
        private static object CreateCartItemsEntity()
        {
            return new
            {
                Id = 23,
                EntityName = "cartItems",
                PersianDisplayName = "آیتم‌های سبد خرید",
                EnglishDisplayName = "CartItem",
                EndPoint = "cartItems",
                EntityIconBase64 = @"<svg width=""24"" height=""24"" viewBox=""0 0 24 24"" fill=""none"" xmlns=""http://www.w3.org/2000/svg"" stroke=""#000"" className=""stroke-primary""><path strokeLinecap=""round"" strokeLinejoin=""round"" d=""M3 3h18v18H3V3zM7 3v18""/></svg>",
                ActionsJson = JsonSerializer.Serialize(new List<string> { "active", "edit", "delete" }),
                ColumnsJson = JsonSerializer.Serialize(new List<JsonDefinition>
                {
                    new JsonDefinition { Header = "شناسه", Accessor = "id", Type = "number", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "شناسه سبد خرید", Accessor = "cartId", Type = "number", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "محصول", Accessor = "productName", Type = "text", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "عکس محصول", Accessor = "productImage", Type = "image", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "کاربر", Accessor = "userName", Type = "text", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "قیمت پایه", Accessor = "basePrice", Type = "number", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "قیمت کل", Accessor = "finalPrice", Type = "number", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "تعداد", Accessor = "quantity", Type = "number", Sortable = false, Filterable = false }
                }),
                FormFieldsJson = JsonSerializer.Serialize(new List<FormFieldDefinition>
                {
                    new FormFieldDefinition
                    {
                        Name = "productId",
                        Caption = "محصول",
                        Type = "dynamicSelect",
                        FetchConfig =new FetchConfig{api="Products/selectOption",fetchFilters=[]},
                        PlaceHolder = "شناسه محصول را وارد کنید",
                        Help = "محصولی که به سبد اضافه شده است",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "شناسه محصول الزامی است" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "productOfferId",
                        Caption = "سفارش محصول",
                        Type = "dynamicSelect",
                        FetchConfig =new FetchConfig{api="productOffers/selectOption",fetchFilters=["productId"]},
                        PlaceHolder = "سفارش فروش محصول را وارد کنید",
                        Help = "انتخاب سفارشی که از این محصول گذاشته شده",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "سفارش محصول الزامی است" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "quantity",
                        Caption = "تعداد",
                        Type = "number",
                        PlaceHolder = "تعداد محصول را وارد کنید",
                        Help = "تعداد واحدهای این محصول در سبد",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "تعداد الزامی است" },
                            new ValidationRule { Rule = "min", Condition = "1", Message = "تعداد باید حداقل 1 باشد" }
                        }
                    }
                }),
                IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1),
                CreatedBy = 1,
                IsDeleted = false
            };
        }
        private static object CreateOrderItemsEntity()
        {
            return new
            {
                Id = 24,
                EntityName = "orderItems",
                PersianDisplayName = "آیتم‌های سفارش",
                EnglishDisplayName = "OrderItem",
                EndPoint = "orderitems",
                EntityIconBase64 = @"<svg xmlns=""http://www.w3.org/2000/svg"" fill=""none"" viewBox=""0 0 24 24"" stroke-width=""1.5"" stroke=""currentColor"" class=""size-6"">
  <path stroke-linecap=""round"" stroke-linejoin=""round"" d=""m21 7.5-9-5.25L3 7.5m18 0-9 5.25m9-5.25v9l-9 5.25M3 7.5l9 5.25M3 7.5v9l9 5.25m0-9v9"" /></svg>",
                ActionsJson = JsonSerializer.Serialize(new List<string> { }),
                ColumnsJson = JsonSerializer.Serialize(new List<JsonDefinition>
                {
                    new JsonDefinition { Header = "شناسه", Accessor = "id", Type = "number", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "نام محصول", Accessor = "productName", Type = "text", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "عکس محصول", Accessor = "productImage", Type = "image", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "تامین کننده", Accessor = "productOfferUser", Type = "image", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "تعداد", Accessor = "quantity", Type = "number", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "قیمت واحد", Accessor = "unitPrice", Type = "number", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "قیمت نهایی", Accessor = "totalPrice", Type = "number", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "مالک سفارش", Accessor = "user", Type = "text", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "وضعیت", Accessor = "orderStatus", Type = "text", Sortable = false, Filterable = false }
                }),
                FormFieldsJson = JsonSerializer.Serialize(new List<FormFieldDefinition>
                { }),
                IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1),
                CreatedBy = 1,
                IsDeleted = false
            };
        }
        private static object CreateRolesEntity()
        {
            return new
            {
                Id = 25,
                EntityName = "roles",
                PersianDisplayName = "نقش‌ها",
                EnglishDisplayName = "Role",
                EndPoint = "roles",
                EntityIconBase64 = @"<svg xmlns=""http://www.w3.org/2000/svg"" fill=""none"" viewBox=""0 0 24 24"" stroke-width=""1.5"" stroke=""currentColor"" class=""size-6"">
  <path stroke-linecap=""round"" stroke-linejoin=""round"" d=""M15.75 6a3.75 3.75 0 1 1-7.5 0 3.75 3.75 0 0 1 7.5 0ZM4.501 20.118a7.5 7.5 0 0 1 14.998 0A17.933 17.933 0 0 1 12 21.75c-2.676 0-5.216-.584-7.499-1.632Z"" /></svg>",
                ActionsJson = JsonSerializer.Serialize(new List<string> { }),
                ColumnsJson = JsonSerializer.Serialize(new List<JsonDefinition>
                {
                    new JsonDefinition { Header = "شناسه", Accessor = "id", Type = "number", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "نام نقش", Accessor = "name", Type = "text", Sortable = false, Filterable = false }
                }),
                FormFieldsJson = JsonSerializer.Serialize(new List<FormFieldDefinition> { }),
                IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1),
                CreatedBy = 1,
                IsDeleted = false
            };
        }
        private static object CreateEntityConfigsEntity()
        {
            return new
            {
                Id = 26,
                EntityName = "entityConfigs",
                PersianDisplayName = "کانفیگ صفحات",
                EnglishDisplayName = "Page configs",
                EndPoint = "entityConfigs",
                EntityIconBase64 = @"<svg xmlns=""http://www.w3.org/2000/svg"" fill=""none"" viewBox=""0 0 24 24"" stroke-width=""1.5"" stroke=""currentColor"" class=""size-6"">
  <path stroke-linecap=""round"" stroke-linejoin=""round"" d=""m20.25 7.5-.625 10.632a2.25 2.25 0 0 1-2.247 2.118H6.622a2.25 2.25 0 0 1-2.247-2.118L3.75 7.5M10 11.25h4M3.375 7.5h17.25c.621 0 1.125-.504 1.125-1.125v-1.5c0-.621-.504-1.125-1.125-1.125H3.375c-.621 0-1.125.504-1.125 1.125v1.5c0 .621.504 1.125 1.125 1.125Z"" />
</svg>
",
                ActionsJson = JsonSerializer.Serialize(new List<string> { "active", "edit", "delete", "new" }),
                ColumnsJson = JsonSerializer.Serialize(new List<JsonDefinition>
                {
                    new JsonDefinition {Header = "شناسه", Accessor = "id", Type = "text", Sortable = false, Filterable = false},
                    new JsonDefinition {Header="نام موجودیت",Accessor="entityName",Type="text",Sortable = false, Filterable = false},
                    new JsonDefinition {Header="نام فارسی",Accessor="persianDisplayName",Type="text",Sortable = false, Filterable = false},
                    new JsonDefinition {Header="نام انگلیسی",Accessor="englishDisplayName",Type="text",Sortable = false, Filterable = false},
                    new JsonDefinition {Header="پایانه/Api",Accessor="endPoint",Type="text",Sortable = false, Filterable = false}
                    
                    //new JsonDefinition {Header="عملیاتهای صفحه",Accessor="actionsJson",Type="json",Sortable = false, Filterable = false}
                    //new JsonDefinition {Header="جدول",Accessor="columnsJson",Type="json",Sortable = false, Filterable = false}
                    //new JsonDefinition {Header="فرم",Accessor="formFieldsJson",Type="json",Sortable = false, Filterable = false}
                    //new JsonDefinition {Header="آیکون",Accessor="entityIconBase64",Type="Base64",Sortable = false, Filterable = false}
                }),
                FormFieldsJson = JsonSerializer.Serialize(new List<FormFieldDefinition>
                {
                    new FormFieldDefinition
                    {
                        Name ="entityName",
                        Caption = "نام موجودیت صفحه",
                        Type = "text",
                        PlaceHolder = "مثلا :comments",
                        Help = "برای واکشی کانفیگ صفحه به کار می رود",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "نام کانفیگ صفحه الزامی است" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "persianDisplayName",
                        Caption = "نام صفحه",
                        Type = "text",
                        PlaceHolder = "مثلا :نظرات",
                        Help = "برای نمایش نام فارسی صفحه در بالای صفحه به کار می رود",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "نام صفحه به فارسی الزامی است" },
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "englishDisplayName",
                        Caption = "نام صفحه به انگلیسی",
                        Type = "text",
                        PlaceHolder = "برای نمایش در بالای صفحه",
                        Help = "برای نمایش در بالای صفحه",
                         Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "نام صفحه به انگلیسی الزامی است" },
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "endPoint",
                        Caption = "پایانه ارتباطی/Api",
                        Type = "text",
                        PlaceHolder = "مثلا:blogs",
                        Help = "نام واحد ارتباطی در api ها",
                         Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message ="نام پایانه/Api الزامی است"},
                        }
                    }
                    //,
                    //new FormFieldDefinition
                    //{
                    //    Name = "entityIconBase64",
                    //    Caption = "آیکون/SVG",
                    //    Type = "textarea",
                    //    PlaceHolder = "متن svg Base64: ",
                    //    Help = "<svg xmlns=",
                    //     Rules = new List<ValidationRule>
                    //    {

                    //    }
                    //},
                    //new FormFieldDefinition
                    //{
                    //    Name ="columns",
                    //    Caption = "ستون های جدول",
                    //    Type = "textarea",
                    //    PlaceHolder = "ستون های جدول نمایش را مشخص کنید",
                    //    Help = "ستون های جدول نمایش را مشخص کنید",
                    //     Rules = new List<ValidationRule>
                    //    {

                    //    }
                    //},
                    //new FormFieldDefinition
                    //{
                    //    Name = "actions",
                    //    Caption = "عملیات صفحه",
                    //    Type = "json",
                    //    PlaceHolder = "['active','edit','delete','new']",
                    //    Help = "عملیاتی که میتوانند انجام بدهند را مشخص کنید",
                    //     Rules = new List<ValidationRule>
                    //    {

                    //    }
                    //},
                    //new FormFieldDefinition
                    //{
                    //    Name = "formFields",
                    //    Caption = "فیلد های فرم",
                    //    Type = "json",
                    //    PlaceHolder ="فیلد های مورد نیاز فرم را مشخص کنید",
                    //    Help ="فیلد های مورد نیاز فرم را مشخص کنید",
                    //     Rules = new List<ValidationRule>
                    //    {

                    //    }
                    //}
                }),
                IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1),
                CreatedBy = 1,
                IsDeleted = false
            };
        }
        private static object CreateProductSpecificationsEntity()
        {
            return new
            {
                Id = 27,
                EntityName = "specifications",
                PersianDisplayName = "مشخصات کالا",
                EnglishDisplayName = "Product specifications",
                EndPoint = "specifications",
                EntityIconBase64 = @"<svg xmlns=""http://www.w3.org/2000/svg"" fill=""none"" viewBox=""0 0 24 24"" stroke-width=""1.5"" stroke=""currentColor"" class=""size-6"">
  <path stroke-linecap=""round"" stroke-linejoin=""round"" d=""m21 7.5-9-5.25L3 7.5m18 0-9 5.25m9-5.25v9l-9 5.25M3 7.5l9 5.25M3 7.5v9l9 5.25m0-9v9"" /></svg>",
                ActionsJson = JsonSerializer.Serialize(new List<string> { "active", "edit", "delete", "new" }),
                ColumnsJson = JsonSerializer.Serialize(new List<JsonDefinition>
                {
                    new JsonDefinition { Header = "شناسه", Accessor = "id", Type = "number", Sortable = false, Filterable = false },
                    new JsonDefinition {Header="شناسه",Accessor ="id",Type ="number",Sortable =false,Filterable =false},
                    new JsonDefinition {Header="نام کالا",Accessor ="productName",Type ="text",Sortable =false,Filterable =false},
                    new JsonDefinition {Header="مشخصه",Accessor ="key",Type ="text",Sortable =false,Filterable =false},
                    new JsonDefinition {Header="مقدار",Accessor ="value",Type ="text",Sortable =false,Filterable =false}

                }),
                FormFieldsJson = JsonSerializer.Serialize(new List<FormFieldDefinition>
                {  new FormFieldDefinition
                    {
                        Name ="productId",
                        Caption ="شناسه کالا",
                        Type = "dynamicSelect",
                        FetchConfig =new FetchConfig{api="Products/selectOption",fetchFilters=[]},
                        PlaceHolder = " محصول را انتخاب کنید",
                        Help = "مثلاً: تلویزیون سامسونگ 55 اینچ",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "انتخاب کالا الزامی است" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name ="key",
                        Caption = "مشخصه یا صفت محصول",
                        Type = "text",
                        PlaceHolder = "مشخصه را وارد کنید",
                        Help = "صفت یا کلمه کلیدی مربوطه",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "نام مشخصه الزامی است" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name ="value",
                        Caption = "مقدار صفت",
                        Type = "text",
                        PlaceHolder = "مقدار مشخصه را وارد کنید",
                        Help = "مقدار صفت این محصول",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "مقدار مشخصه الزامی است" }
                        }
                    } }),
                IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1),
                CreatedBy = 1,
                IsDeleted = false
            };
        }

        private static object CreateSeoSettingsEntity()
        {
            return new
            {
                Id = 28,
                EntityName = "seoSettings",
                PersianDisplayName = "قوانین سئو",
                EnglishDisplayName = "SEO Rules",
                EndPoint = "seoSettings",
                EntityIconBase64 = @"<svg xmlns=""http://www.w3.org/2000/svg"" fill=""none"" viewBox=""0 0 24 24"" stroke-width=""1.5"" stroke=""currentColor"" class=""size-6""><path stroke-linecap=""round"" stroke-linejoin=""round"" d=""M9 12.75 11.25 15 15 9.75m6 2.25a9 9 0 1 1-18 0 9 9 0 0 1 18 0Z"" /></svg>",
                ActionsJson = JsonSerializer.Serialize(new List<string> { "active", "edit", "delete", "new" }),
                ColumnsJson = JsonSerializer.Serialize(new List<JsonDefinition>
                {
                    new JsonDefinition { Header = "شناسه", Accessor = "id", Type = "number", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "مسیر", Accessor = "routePath", Type = "text", Sortable = true, Filterable = true },
                    new JsonDefinition { Header = "نوع تطبیق", Accessor = "matchType", Type = "text", Sortable = true, Filterable = true },
                    new JsonDefinition { Header = "عنوان فارسی", Accessor = "titleFa", Type = "text", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "Canonical", Accessor = "canonicalPath", Type = "text", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "Priority", Accessor = "priority", Type = "number", Sortable = true, Filterable = false }
                }),
                FormFieldsJson = JsonSerializer.Serialize(new List<FormFieldDefinition>
                {
                    new FormFieldDefinition
                    {
                        Name = "routePath",
                        Caption = "مسیر صفحه",
                        Type = "text",
                        PlaceHolder = "مثلاً: products یا products/samsung-tv",
                        Help = "مسیر را بدون locale وارد کنید. برای صفحه اصلی خالی بگذارید.",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "مسیر صفحه الزامی است" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "matchType",
                        Caption = "نوع تطبیق",
                        Type = "text",
                        PlaceHolder = "exact یا prefix",
                        Help = "برای همان مسیر exact و برای تمام زیرمسیرها prefix را وارد کنید"
                    },
                    new FormFieldDefinition { Name = "titleFa", Caption = "SEO Title فارسی", Type = "text", PlaceHolder = "عنوان سئوی فارسی" },
                    new FormFieldDefinition { Name = "titleEn", Caption = "SEO Title انگلیسی", Type = "text", PlaceHolder = "English SEO title" },
                    new FormFieldDefinition { Name = "descriptionFa", Caption = "Meta Description فارسی", Type = "textarea", PlaceHolder = "حداکثر حدود 160 کاراکتر" },
                    new FormFieldDefinition { Name = "descriptionEn", Caption = "Meta Description انگلیسی", Type = "textarea", PlaceHolder = "About 160 characters" },
                    new FormFieldDefinition { Name = "keywordsFa", Caption = "Keywords فارسی", Type = "text", PlaceHolder = "کلمات کلیدی با کاما" },
                    new FormFieldDefinition { Name = "keywordsEn", Caption = "Keywords انگلیسی", Type = "text", PlaceHolder = "keyword1, keyword2" },
                    new FormFieldDefinition { Name = "canonicalPath", Caption = "Canonical Path", Type = "text", PlaceHolder = "مثلاً: products/samsung-tv" },
                    new FormFieldDefinition { Name = "ogImageUrl", Caption = "Open Graph Image", Type = "text", PlaceHolder = "/images/og/default.jpg" },
                    new FormFieldDefinition { Name = "priority", Caption = "Priority", Type = "number", PlaceHolder = "اعداد بزرگ‌تر اولویت بالاتر دارند" },
                    new FormFieldDefinition { Name = "robotsIndex", Caption = "Index شود", Type = "checkbox", Help = "اگر خاموش باشد، noindex اعمال می‌شود" },
                    new FormFieldDefinition { Name = "robotsFollow", Caption = "Follow شود", Type = "checkbox", Help = "اگر خاموش باشد، nofollow اعمال می‌شود" },
                    new FormFieldDefinition { Name = "notes", Caption = "یادداشت", Type = "textarea", PlaceHolder = "یادداشت داخلی برای تیم محتوا/سئو" }
                }),
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1),
                CreatedBy = 1,
                IsDeleted = false
            };
        }

        private static object CreateSeoDashboardEntity()
        {
            return new
            {
                Id = 29,
                EntityName = "seo",
                PersianDisplayName = "داشبورد سئو",
                EnglishDisplayName = "SEO Dashboard",
                EndPoint = "seo",
                EntityIconBase64 = @"<svg xmlns=""http://www.w3.org/2000/svg"" fill=""none"" viewBox=""0 0 24 24"" stroke-width=""1.5"" stroke=""currentColor"" class=""size-6""><path stroke-linecap=""round"" stroke-linejoin=""round"" d=""M3.75 3v11.25A2.25 2.25 0 0 0 6 16.5h12m-9 4.5 3.75-3.75M15 21l3.75-3.75M7.5 12h.008v.008H7.5V12Zm4.5-3h.008v.008H12V9Zm4.5-3h.008v.008H16.5V6Z"" /></svg>",
                ActionsJson = JsonSerializer.Serialize(new List<string>()),
                ColumnsJson = JsonSerializer.Serialize(new List<JsonDefinition>()),
                FormFieldsJson = JsonSerializer.Serialize(new List<FormFieldDefinition>()),
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1),
                CreatedBy = 1,
                IsDeleted = false
            };
        }

        private static object CreateFaqsEntity()
        {
            return new
            {
                Id = 30,
                EntityName = "faqs",
                PersianDisplayName = "سوالات متداول",
                EnglishDisplayName = "FAQs",
                EndPoint = "faqs",
                EntityIconBase64 = @"<svg xmlns=""http://www.w3.org/2000/svg"" fill=""none"" viewBox=""0 0 24 24"" stroke-width=""1.5"" stroke=""currentColor"" class=""size-6""><path stroke-linecap=""round"" stroke-linejoin=""round"" d=""M9.879 7.519c1.171-1.025 3.071-1.025 4.242 0 1.172 1.025 1.172 2.687 0 3.712-.203.179-.43.326-.67.442-.745.361-1.45.999-1.45 1.827v.75M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0Zm-9 5.25h.008v.008H12v-.008Z"" /></svg>",
                ActionsJson = JsonSerializer.Serialize(new List<string> { "active", "edit", "delete", "new" }),
                ColumnsJson = JsonSerializer.Serialize(new List<JsonDefinition>
                {
                    new JsonDefinition { Header = "شناسه", Accessor = "id", Type = "number", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "سوال", Accessor = "question", Type = "text", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "پاسخ", Accessor = "answer", Type = "text", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "ترتیب نمایش", Accessor = "displayOrder", Type = "number", Sortable = false, Filterable = false }
                }),
                FormFieldsJson = JsonSerializer.Serialize(new List<FormFieldDefinition>
                {
                    new FormFieldDefinition
                    {
                        Name = "question",
                        Caption = "سوال",
                        Type = "text",
                        PlaceHolder = "مثلا: ارسال سفارش چقدر زمان می برد؟",
                        Help = "متن سوال همان طور که در سایت نمایش داده می شود",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "متن سوال الزامی است" }
                        }
                    },
                    new FormFieldDefinition { Name = "displayOrder", Caption = "ترتیب نمایش", Type = "number", PlaceHolder = "مثلا: 1", Help = "عدد کوچک تر بالاتر نمایش داده می شود" },
                    new FormFieldDefinition
                    {
                        Name = "answer",
                        Caption = "پاسخ",
                        Type = "textarea",
                        PlaceHolder = "پاسخ کامل سوال",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "متن پاسخ الزامی است" }
                        }
                    }
                }),
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1),
                CreatedBy = 1,
                IsDeleted = false
            };
        }

        private static object CreateThemeSettingsEntity()
        {
            return new
            {
                Id = 32,
                EntityName = "themeSettings",
                PersianDisplayName = "تم سایت",
                EnglishDisplayName = "Site theme",
                EndPoint = "themeSettings",
                EntityIconBase64 = @"<svg xmlns=""http://www.w3.org/2000/svg"" fill=""none"" viewBox=""0 0 24 24"" stroke-width=""1.5"" stroke=""currentColor"" class=""size-6""><path stroke-linecap=""round"" stroke-linejoin=""round"" d=""M9.53 16.122a3 3 0 0 0-5.78 1.128 2.25 2.25 0 0 1-2.4 2.245 4.5 4.5 0 0 0 8.4-2.245c0-.399-.078-.78-.22-1.128Zm0 0a15.998 15.998 0 0 0 3.388-1.62m-5.043-.025a15.994 15.994 0 0 1 1.622-3.395m3.42 3.42a15.995 15.995 0 0 0 4.764-4.648l3.876-5.814a1.151 1.151 0 0 0-1.597-1.597L14.146 6.32a15.996 15.996 0 0 0-4.649 4.763m3.42 3.42a6.776 6.776 0 0 0-3.42-3.42"" /></svg>",
                ActionsJson = JsonSerializer.Serialize(new List<string> { "active", "edit", "delete", "new" }),
                ColumnsJson = JsonSerializer.Serialize(new List<JsonDefinition>
                {
                    new JsonDefinition { Header = "شناسه", Accessor = "id", Type = "number", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "نام تم", Accessor = "name", Type = "text", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "رنگ اصلی", Accessor = "primaryColor", Type = "text", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "پس‌زمینه", Accessor = "surfaceColor", Type = "text", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "متن", Accessor = "textColor", Type = "text", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "فعال (نمایش در سایت)", Accessor = "isActive", Type = "bool", Sortable = false, Filterable = false }
                }),
                FormFieldsJson = JsonSerializer.Serialize(new List<FormFieldDefinition>
                {
                    new FormFieldDefinition
                    {
                        Name = "name",
                        Caption = "نام تم",
                        Type = "text",
                        PlaceHolder = "مثلا: پاییز کریستال",
                        Help = "فقط برای شناسایی در پنل",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "نام تم الزامی است" }
                        }
                    },
                    new FormFieldDefinition { Name = "primaryColor", Caption = "رنگ اصلی", Type = "color", PlaceHolder = "#000000", Help = "دکمه‌ها، لینک‌های مهم و تأکیدها", Rules = new List<ValidationRule> { new ValidationRule { Rule = "required", Condition = "true", Message = "رنگ اصلی الزامی است" } } },
                    new FormFieldDefinition { Name = "secondaryColor", Caption = "رنگ ثانویه", Type = "color", PlaceHolder = "#f2f2f2", Help = "پس‌زمینه‌های ملایم و جداکننده‌ها", Rules = new List<ValidationRule> { new ValidationRule { Rule = "required", Condition = "true", Message = "رنگ ثانویه الزامی است" } } },
                    new FormFieldDefinition { Name = "highlightColor", Caption = "رنگ تأکید (طلایی)", Type = "color", PlaceHolder = "#a38a52", Help = "نشان‌ها و جزئیات تزئینی", Rules = new List<ValidationRule> { new ValidationRule { Rule = "required", Condition = "true", Message = "رنگ تأکید (طلایی) الزامی است" } } },
                    new FormFieldDefinition { Name = "neutralColor", Caption = "رنگ خنثی", Type = "color", PlaceHolder = "#f7f7f7", Help = "پس‌زمینه‌ی بخش‌های کم‌اهمیت", Rules = new List<ValidationRule> { new ValidationRule { Rule = "required", Condition = "true", Message = "رنگ خنثی الزامی است" } } },
                    new FormFieldDefinition { Name = "successColor", Caption = "رنگ موفقیت", Type = "color", PlaceHolder = "#0d9488", Help = "پیام‌های موفق", Rules = new List<ValidationRule> { new ValidationRule { Rule = "required", Condition = "true", Message = "رنگ موفقیت الزامی است" } } },
                    new FormFieldDefinition { Name = "errorColor", Caption = "رنگ خطا", Type = "color", PlaceHolder = "#c8102e", Help = "خطاها و تخفیف‌ها", Rules = new List<ValidationRule> { new ValidationRule { Rule = "required", Condition = "true", Message = "رنگ خطا الزامی است" } } },
                    new FormFieldDefinition { Name = "warningColor", Caption = "رنگ هشدار", Type = "color", PlaceHolder = "#b45309", Help = "هشدارها", Rules = new List<ValidationRule> { new ValidationRule { Rule = "required", Condition = "true", Message = "رنگ هشدار الزامی است" } } },
                    new FormFieldDefinition { Name = "infoColor", Caption = "رنگ اطلاع‌رسانی", Type = "color", PlaceHolder = "#3b82f6", Help = "پیام‌های اطلاعاتی", Rules = new List<ValidationRule> { new ValidationRule { Rule = "required", Condition = "true", Message = "رنگ اطلاع‌رسانی الزامی است" } } },
                    new FormFieldDefinition { Name = "surfaceColor", Caption = "رنگ پس‌زمینه‌ی صفحه", Type = "color", PlaceHolder = "#ffffff", Help = "پس‌زمینه‌ی اصلی سایت و کارت‌ها", Rules = new List<ValidationRule> { new ValidationRule { Rule = "required", Condition = "true", Message = "رنگ پس‌زمینه‌ی صفحه الزامی است" } } },
                    new FormFieldDefinition { Name = "surfaceMutedColor", Caption = "رنگ پس‌زمینه‌ی ملایم", Type = "color", PlaceHolder = "#f7f7f7", Help = "قاب عکس محصولات و بخش‌های خاکستری", Rules = new List<ValidationRule> { new ValidationRule { Rule = "required", Condition = "true", Message = "رنگ پس‌زمینه‌ی ملایم الزامی است" } } },
                    new FormFieldDefinition { Name = "borderColor", Caption = "رنگ خط و حاشیه", Type = "color", PlaceHolder = "#e3e3e3", Help = "خط‌های جداکننده و حاشیه‌ی کارت‌ها", Rules = new List<ValidationRule> { new ValidationRule { Rule = "required", Condition = "true", Message = "رنگ خط و حاشیه الزامی است" } } },
                    new FormFieldDefinition { Name = "textColor", Caption = "رنگ متن", Type = "color", PlaceHolder = "#000000", Help = "متن اصلی و تیترها", Rules = new List<ValidationRule> { new ValidationRule { Rule = "required", Condition = "true", Message = "رنگ متن الزامی است" } } },
                    new FormFieldDefinition { Name = "textMutedColor", Caption = "رنگ متن کم‌رنگ", Type = "color", PlaceHolder = "#6b6b6b", Help = "توضیحات و متن‌های فرعی", Rules = new List<ValidationRule> { new ValidationRule { Rule = "required", Condition = "true", Message = "رنگ متن کم‌رنگ الزامی است" } } }
                }),
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1),
                CreatedBy = 1,
                IsDeleted = false
            };
        }

        private static object CreateAnnouncementBarsEntity()
        {
            return new
            {
                Id = 33,
                EntityName = "announcementBars",
                PersianDisplayName = "نوار اعلان",
                EnglishDisplayName = "Announcement bar",
                EndPoint = "announcementBars",
                EntityIconBase64 = @"<svg xmlns=""http://www.w3.org/2000/svg"" fill=""none"" viewBox=""0 0 24 24"" stroke-width=""1.5"" stroke=""currentColor"" class=""size-6""><path stroke-linecap=""round"" stroke-linejoin=""round"" d=""M10.34 15.84c-.688-.06-1.386-.09-2.09-.09H7.5a4.5 4.5 0 1 1 0-9h.75c.704 0 1.402-.03 2.09-.09m0 9.18c.253.962.584 1.892.985 2.783.247.55.06 1.21-.463 1.511l-.657.38c-.551.318-1.26.117-1.527-.461a20.845 20.845 0 0 1-1.44-4.282m3.102.069a18.03 18.03 0 0 1-.59-4.59c0-1.586.205-3.124.59-4.59m0 9.18a23.848 23.848 0 0 1 8.835 2.535M10.34 6.66a23.847 23.847 0 0 0 8.835-2.535m0 0A23.74 23.74 0 0 0 18.795 3m.38 1.125a23.91 23.91 0 0 1 1.014 5.395m-1.014 8.855c-.118.38-.245.754-.38 1.125m.38-1.125a23.91 23.91 0 0 0 1.014-5.395m0-3.46c.495.413.811 1.035.811 1.73 0 .695-.316 1.317-.811 1.73m0-3.46a24.347 24.347 0 0 1 0 3.46"" /></svg>",
                ActionsJson = JsonSerializer.Serialize(new List<string> { "active", "edit", "delete", "new" }),
                ColumnsJson = JsonSerializer.Serialize(new List<JsonDefinition>
                {
                    new JsonDefinition { Header = "شناسه", Accessor = "id", Type = "number", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "متن فارسی", Accessor = "messageFa", Type = "text", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "آدرس لینک", Accessor = "linkUrl", Type = "text", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "ارتفاع (px)", Accessor = "heightPx", Type = "number", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "شروع نمایش", Accessor = "startsAt", Type = "date", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "پایان نمایش", Accessor = "endsAt", Type = "date", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "ترتیب", Accessor = "displayOrder", Type = "number", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "نمایش داده شود", Accessor = "isActive", Type = "bool", Sortable = false, Filterable = false }
                }),
                FormFieldsJson = JsonSerializer.Serialize(new List<FormFieldDefinition>
                {
                    new FormFieldDefinition
                    {
                        Name = "messageFa",
                        Caption = "متن فارسی",
                        Type = "text",
                        PlaceHolder = "مثلا: ارسال رایگان برای خرید بالای ...",
                        Help = "متنی که در صفحه‌های فارسی روی نوار نمایش داده می‌شود",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "متن فارسی الزامی است" }
                        }
                    },
                    new FormFieldDefinition { Name = "messageEn", Caption = "متن انگلیسی", Type = "text", PlaceHolder = "English message", Help = "برای صفحه‌های انگلیسی؛ اگر خالی باشد متن فارسی نمایش داده می‌شود" },
                    new FormFieldDefinition { Name = "linkUrl", Caption = "آدرس لینک", Type = "text", PlaceHolder = "مثلا: discounts یا https://example.com", Help = "با کلیک روی نوار به این آدرس می‌رود؛ خالی = بدون لینک" },
                    new FormFieldDefinition { Name = "backgroundImageUrl", Caption = "تصویر پس‌زمینه (اختیاری)", Type = "file", PlaceHolder = "انتخاب تصویر", Help = "روی رنگ پس‌زمینه قرار می‌گیرد؛ تصویر افقی و کم‌حجم پیشنهاد می‌شود" },
                    new FormFieldDefinition { Name = "removeBackgroundImage", Caption = "حذف تصویر پس‌زمینه فعلی", Type = "checkbox", PlaceHolder = "", Help = "در ویرایش: برای حذف تصویر ثبت‌شده فعال کنید." },
                    new FormFieldDefinition { Name = "backgroundColor", Caption = "رنگ پس‌زمینه", Type = "color", PlaceHolder = "#000000", Help = "اگر تصویر پس‌زمینه ندارید این رنگ دیده می‌شود" },
                    new FormFieldDefinition { Name = "textColor", Caption = "رنگ متن", Type = "color", PlaceHolder = "#ffffff", Help = "رنگ متن روی نوار" },
                    new FormFieldDefinition { Name = "heightPx", Caption = "ارتفاع نوار (پیکسل)", Type = "number", PlaceHolder = "36", Help = "بین ۲۴ تا ۱۶۰ پیکسل" },
                    new FormFieldDefinition { Name = "startsAt", Caption = "شروع نمایش", Type = "date", PlaceHolder = "", Help = "خالی = از همین حالا" },
                    new FormFieldDefinition { Name = "endsAt", Caption = "پایان نمایش", Type = "date", PlaceHolder = "", Help = "خالی = بدون پایان" },
                    new FormFieldDefinition { Name = "displayOrder", Caption = "ترتیب", Type = "number", PlaceHolder = "0", Help = "اگر چند نوار هم‌زمان معتبر باشند، عدد کوچک‌تر نمایش داده می‌شود" }
                }),
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1),
                CreatedBy = 1,
                IsDeleted = false
            };
        }

        private static object CreateContactRequestsEntity()
        {
            return new
            {
                Id = 31,
                EntityName = "contactRequests",
                PersianDisplayName = "همکاری با ما",
                EnglishDisplayName = "Contact requests",
                EndPoint = "contactRequests",
                EntityIconBase64 = @"<svg xmlns=""http://www.w3.org/2000/svg"" fill=""none"" viewBox=""0 0 24 24"" stroke-width=""1.5"" stroke=""currentColor"" class=""size-6""><path stroke-linecap=""round"" stroke-linejoin=""round"" d=""M21.75 6.75v10.5a2.25 2.25 0 0 1-2.25 2.25h-15a2.25 2.25 0 0 1-2.25-2.25V6.75m19.5 0A2.25 2.25 0 0 0 19.5 4.5h-15a2.25 2.25 0 0 0-2.25 2.25m19.5 0v.243a2.25 2.25 0 0 1-1.07 1.916l-7.5 4.615a2.25 2.25 0 0 1-2.36 0L3.32 8.91a2.25 2.25 0 0 1-1.07-1.916V6.75"" /></svg>",
                ActionsJson = JsonSerializer.Serialize(new List<string> { "edit", "delete", "new" }),
                ColumnsJson = JsonSerializer.Serialize(new List<JsonDefinition>
                {
                    new JsonDefinition { Header = "شناسه", Accessor = "id", Type = "number", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "نام", Accessor = "firstName", Type = "text", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "نام خانوادگی", Accessor = "lastName", Type = "text", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "شماره تماس", Accessor = "phoneNumber", Type = "text", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "آدرس", Accessor = "address", Type = "text", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "زمان مناسب تماس", Accessor = "preferredContactTime", Type = "text", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "تاریخ ثبت", Accessor = "createdAt", Type = "date", Sortable = false, Filterable = false },
                    new JsonDefinition { Header = "بررسی شده", Accessor = "isReviewed", Type = "bool", Sortable = false, Filterable = false }
                }),
                FormFieldsJson = JsonSerializer.Serialize(new List<FormFieldDefinition>
                {
                    new FormFieldDefinition
                    {
                        Name = "firstName",
                        Caption = "نام",
                        Type = "text",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "نام الزامی است" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "lastName",
                        Caption = "نام خانوادگی",
                        Type = "text",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "نام خانوادگی الزامی است" }
                        }
                    },
                    new FormFieldDefinition
                    {
                        Name = "phoneNumber",
                        Caption = "شماره تماس",
                        Type = "text",
                        PlaceHolder = "09123456789",
                        Rules = new List<ValidationRule>
                        {
                            new ValidationRule { Rule = "required", Condition = "true", Message = "شماره تماس الزامی است" }
                        }
                    },
                    new FormFieldDefinition { Name = "email", Caption = "ایمیل", Type = "text", PlaceHolder = "name@example.com" },
                    new FormFieldDefinition { Name = "preferredContactTime", Caption = "زمان مناسب تماس", Type = "text", PlaceHolder = "مثلا: شنبه تا چهارشنبه ۹ تا ۱۳" },
                    new FormFieldDefinition { Name = "isReviewed", Caption = "بررسی شده", Type = "checkbox", Help = "بعد از تماس یا پیگیری درخواست، این گزینه را فعال کنید" },
                    new FormFieldDefinition { Name = "address", Caption = "آدرس", Type = "textarea" },
                    new FormFieldDefinition { Name = "message", Caption = "متن پیام", Type = "textarea" },
                    new FormFieldDefinition { Name = "adminNote", Caption = "یادداشت ادمین", Type = "textarea", PlaceHolder = "نتیجه تماس یا پیگیری" }
                }),
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1),
                CreatedBy = 1,
                IsDeleted = false
            };
        }


    }
}
