# پرامپت انتقال تغییرات rooshak-dev به luca-dev

> این فایل را کامل برای Claude داخل ریپوی `C:\fallahi\luca-dev` بفرستید.
> مرجع کد: `C:\fallahi\rooshak-dev` (فقط بخوان، تغییر نده). بازه‌ی commitها: `15776c0..HEAD` (شروع از `04a1a7c`).
> قبل از شروع در rooshak-dev آخرین تغییر `FrontEnd/components/organisms/heroMedia.tsx` (fallback ویدیو) را commit کنید تا در diff باشد.

---

## نقش و روش کار

تو داخل پروژه‌ی **luca-dev** هستی. همین استک (Next 15 + next-intl + Tailwind 3 + redux-persist، بک‌اند ASP.NET Clean/DDD + MediatR + EF Core/Postgres) را در **rooshak-dev** هم داریم و آنجا یک بازطراحی کامل انجام شده. همان کارها را اینجا پیاده کن.

۱. اول `C:\fallahi\rooshak-dev` را بخوان: `git -C C:\fallahi\rooshak-dev log --oneline 15776c0..HEAD` و `git -C C:\fallahi\rooshak-dev diff 15776c0..HEAD --stat`. کپی کورکورانه نکن؛ هر فایل را با نسخه‌ی luca مقایسه کن و با ساختار/نام‌های luca هماهنگ بنویس.
۲. هرجا نام برند «روشاک/Rooshak»، متن‌های i18n، آدرس سایت، شبکه‌های اجتماعی، لوگو، عکس‌های `public/images/landingPage` و دسته‌بندی‌ها آمده، با معادل **luca** جایگزین کن یا اگر نداری از من بپرس.
۳. **صفحه‌های exhibition** (اگر در luca وجود دارد) را بازطراحی نکن و ظاهرشان را حفظ کن.
۴. قبل از هر مرحله‌ی بزرگ توضیح کوتاه بده؛ بعد از هر مرحله `npx tsc --noEmit` (FrontEnd) و `dotnet build` + `dotnet test` (BackEnd) را اجرا کن.
۵. هیچ چیز را commit/push نکن مگر بخواهم. migration را روی دیتابیس اجرا نکن، فقط بساز.
۶. پایان: گزارش فارسی با لیست فایل‌ها، چیزهایی که تطبیق دادی و چیزهایی که پیاده نشد.

### نکات فنی (از تجربه‌ی rooshak)
- فایل‌ها CRLF هستند؛ ابزار ویرایش را CRLF-safe بنویس (نرمال‌سازی `\r\n` قبل از replace و برگرداندن بعد از آن).
- در heredocهای bash کاراکترهای خاص/فارسی گاهی خراب می‌شوند؛ اسکریپت‌های بزرگ را با ابزار Write در فایل بنویس.
- اگر برای migration از `dotnet ef` استفاده می‌کنی: `dotnet ef migrations add <Name> --project ../Infrastructure --startup-project .` از پوشه‌ی `BackEnd/Api`. migration تولیدی ممکن است `UpdateData` اضافی برای آیکون‌های EntityConfig (فقط تفاوت خط جدید) داشته باشد؛ آنها را حذف کن و فقط تغییرات واقعی بماند.
- بک‌اند لوکال نداری؛ برای دیدن UI یک mock ساده‌ی API (node) با `INTERNAL_SERVER_SIDE_API_URL=http://localhost:8099` بساز و بعد از کار پاکش کن (در ریپو نگذار). کش مرورگر pane برای `/_next/static` را در dev با header `no-store` غیرفعال کن.
- از Tailwind استفاده کن؛ CSS سفارشی فقط برای توکن‌ها.

---

## بخش A — طراحی فرانت‌اند به سبک Swarovski (به‌جز exhibition)

### A1. توکن‌ها و Tailwind
- `style/globals.css`: تم پیش‌فرض مونوکروم (primary `#000`، secondary `#f2f2f2`، highlight `#a38a52`، neutral `#f7f7f7`، error `#c8102e`...)، متغیرهای `--store-*` (surface, surface-muted, border, border-strong, text, text-muted, shadow, `--store-announce-h`, `--store-nav-h`, `--store-header-h = announce+nav`)، گوشه‌های تیز (override کلاس‌های `rounded-xl/2xl/3xl` به صفر و `rounded-lg/md` به 2px، ادمین با `[data-surface="admin"]` مستثنی)، پالت قدیمی ادمین فقط برای `[data-surface="admin"]`.
- `tailwind.config.ts`: رنگ‌های `store.{surface,muted,border,strong,text,subtle}` و `primary-foreground` (= `var(--store-surface-solid)`).
- `app/layout.tsx`: body با `bg-store-surface text-store-text`، `viewport` با `themeColor`، `icons`، ورودی فارسی/انگلیسی و SEO حفظ شود. `AdminShell` ریشه‌اش `data-surface="admin"` بگیرد.
- حرف‌چینی (letter-spacing) هرگز روی متن فارسی نیاید؛ فقط با `ltr:tracking-[...]`.
- بعد از بازطراحی، CSS قدیمی بی‌استفاده `store-nav/store-hero/store-footer/...` را حذف کن (`.store-btn`, `.store-panel`, `.store-card`, `.store-empty*`, `.store-page*` که هنوز استفاده می‌شوند بمانند).

### A2. هدر، فوتر، منو
- **هدر** (`layout/header`): fixed، روی هیرو شفاف با گرادیان تیره و بعد از اسکرول سفید (`data-floating`)، لوگو وسط، لینک‌های ناوبری چپ/راست با خط زیرین، آیکون‌های کاربر/سبد، `LangSwitcher`. هدر به دو فایل تقسیم شود: `index.tsx` (server wrapper) و `headerClient.tsx` (client). دکمه‌ی تغییر تم **وجود ندارد** (نه در هدر، نه منوی موبایل، نه ادمین).
- **MobileMenu**: پنل کشویی از کنار (RTL/LTR درست)، لیست لینک‌ها با خط جداکننده، دکمه‌ی ورود و `LangSwitcher`.
- **فوتر**: ستون‌های لینک، برند، فقط شبکه‌های اجتماعی واقعی، خط زیرین hover با `after:` و transition **۳۰۰ms** خطی (از ابتدا تا انتها و برگشت همان سرعت؛ در RTL از سمت راست شروع شود: `after:origin-left rtl:after:origin-right`).
- layoutهای صفحه‌ها padding بالا را از `pt-[var(--store-header-h)]` بگیرند.

### A3. لندینگ و کامپوننت‌ها
- `LandingHero` تمام‌عرض (`h-svh`) با h1، دو CTA؛ `LandingSlider` به کاشی‌های editorial (بدون JS کروسل) تبدیل شود؛ برندها، محصولات (تب‌ها + کروسل scroll-snap)، دسته‌ها (کاشی‌ها)، پیشنهاد ویژه (بخش مشکی)، اعتماد/USP، بلاگ، نظرات، FAQ همه با Tailwind و توکن‌های `store`.
- `components/molecules/storefront/SectionHeading.tsx` برای تیتر بخش‌ها.
- کارت‌ها: `SimpleProductCard`, `BrandCard`, `CategoryCard`, supplier/blog cards به سبک Swarovski (تصویر نسبت 4/5، متن ساده، خط زیرین).
- صفحه محصول: گالری مربعی، اطلاعات sticky، قیمت بدون جعبه، تب‌ها با underline، کارت تأمین‌کننده.
- storefront helpers: breadcrumbs، PageHeader، FaqAccordion، FaqSection، SeoHighlight، RelatedSeoLinks مطابق نسخه‌ی rooshak.
- صفحه‌های سبد/چک‌اوت/سفارش/پرداخت که پس‌زمینه‌ی تیره داشتند روشن و با توکن‌ها شوند (اسکریپت remap کلاس‌های `bg-zinc-*`, `text-white`, `text-gray-*`, `border-gray-*` → `bg-store-muted`, `text-store-text`, `text-store-subtle`, `border-store-border`؛ دکمه‌های سفید روی تیره → `bg-primary text-primary-foreground`؛ متن سفید روی overlay تصویر را نگه دار).
- `app/not-found.tsx` دوزبانه، `error.tsx`, `global-error.tsx` روشن.
- **FAQ**: ردیف‌های بدون پرسش/پاسخ فیلتر شوند (`lib/faq.ts`)، حالت خالی درست، صفحه‌ی faq با `max-w-6xl` و آکاردئون دو ستونه در `lg`.

---

## بخش B — هیرو: ویدیو، موبایل/دسکتاپ، پوستر

### B1. بک‌اند (Slide)
- `Slide`: `VideoUrl`, `MobileBannerUrl`, `MobileVideoUrl` (+ `SetMobileMedia`, `ClearVideo`, `ClearMobileBanner`, `ClearMobileVideo`). `BannerUrl` همان پوستر/عکس دسکتاپ است.
- `IUploaderService.UploadVideo` (فقط mp4/webm، ≤ ۳۰MB، بدون تبدیل، ذخیره `{guid}_hero.{ext}`)؛ fake تست‌ها را هم به‌روز کن.
- `CreateSlideCommand`/`UpdateSlideCommand`: فیلدهای `VideoUrl`, `MobileBannerUrl`, `MobileVideoUrl` (IFormFile?) و در Update فلگ‌های `RemoveVideo`, `RemoveMobileBanner`, `RemoveMobileVideo`. Handler: آپلود، حذف فایل قبلی، پاک‌کردن همه‌ی فایل‌ها با حذف اسلاید.
- DTO و Query handlerها: `VideoUrl`, `MobileBannerUrl`, `MobileVideoUrl`.
- EntityConfig اسلاید: فیلدهای ادمین با type `video` برای ویدیوها و `file` برای پوسترها + چک‌باکس‌های حذف؛ ستون‌های لیست.
- Migration و EF config (`HasMaxLength(300)`).

### B2. فرانت
- `lib/landing.ts`: نوع `LandingSlide` (با فیلدهای موبایل)، `getSlides()` با `?OnlyActives=true`، `splitHeroSlide()` (اسلاید `isHero`، وگرنه اولین دارای ویدیو، وگرنه اولین).
- `components/organisms/heroMedia.tsx` (client): پوستر به‌صورت `<picture>` با art direction (`getImageProps`، preload با `media`)، ویدیو فقط روی کلاینت و فقط اگر reduced-motion/saveData/2G نباشد، انتخاب فایل با `matchMedia('(max-width:767px)')`، دکمه‌ی توقف/پخش، **fallback دوطرفه**: اگر فقط یک ویدیو گذاشته شد برای هر دو نمایش استفاده شود؛ پوستر هر نمایش مستقل (پوستر موبایل نبود ← دسکتاپ).
- `LandingHero` (server) از `HeroMedia` استفاده کند؛ عکس پیش‌فرض وقتی اسلایدی نیست. `next.config.ts` هدر cache برای `/video/*`؛ middleware matcher پسوندهای `mp4|webm|avif|woff2?` را exclude کند.
- ادمین: `components/atoms/defaultElements/videoUploader` و case `video` در `formFieldRenderer`؛ در `formGenerator` شرط `hasFile` شامل `video` هم باشد؛ کلیدهای i18n `uploader.videoType/videoSize/videoPlaceholder`.

---

## بخش C — SSR و سئو

- **باگ بحرانی**: `store/provider` با `PersistGate` باعث می‌شد HTML همه صفحات فقط اسپینر باشد. اصلاح:
  - `store/index.ts`: `persistStore(store, { manualPersist: true } as ...)`.
  - `store/provider/index.tsx` (`'use client'`): `ReduxProvider` فقط `Provider` + `PersistBoot` (یک‌بار `persistor.persist()` در useEffect، با فلگ ماژول برای StrictMode)؛ و export `ClientOnlyPersistGate` (PersistGate با اسپینر).
  - route layoutهای `checkout/order/payment/register/shoppingCart` و `admin/layout.tsx` را با `ClientOnlyPersistGate` بپیچ (این مسیرها noindex‌اند).
- `utils/core.tsx` `getCookie`: روی سرور `''` برگرداند.
- `CountdownDisplayClient`: state اولیه `null`، محاسبه بعد از mount (جلوگیری از hydration mismatch).
- `lib/seo.ts`: `DEFAULT_OG_IMAGE = '/og-image.jpg'` و استفاده وقتی تصویری نیست؛ فایل `public/og-image.jpg` (۱۲۰۰×۶۳۰ از عکس هیرو).
- JSON-LD صفحه اصلی: `logo`/`image` با `siteBaseUrl` و فقط اگر فایل لوگو وجود داشته باشد؛ `manifest.json` فقط آیکون‌های موجود.
- بعد از تغییرات `next build` بزن و HTML تولیدی صفحه اصلی را چک کن (`<h1>`، تیترها، JSON-LD، preload تصویر هیرو) و در مرورگر خطای hydration نباشد.

---

## بخش D — صفحه‌ی ورود/ثبت‌نام (`/register`)

- بدون هدر/فوتر (فقط `Register` در page). دسکتاپ (`lg+`): دو نیمه‌ی ۵۰٪ با `absolute`؛ نیمه‌ی **عکس/ویدیوی هیرو** (`HeroMedia` با `noScrim` اختیاری) و نیمه‌ی فرم.
- با رفتن به ثبت‌نام جای دو نیمه با `transition-transform duration-[800ms] ease-[cubic-bezier(0.65,0,0.35,1)]` عوض شود؛ جهت‌ها `ltr:`/`rtl:`؛ **پنل مدیا `z-10` و بالاتر از فرم** باشد تا فرم از پشت آن جابه‌جا شود؛ محتوای فرم با انیمیشن `formIn` (۶۰۰ms، keyframes در tailwind.config) وارد شود.
- موبایل: مدیا مخفی، فقط نام برند بالای فرم.
- `register/authStyles.ts`: ثابت‌های مشترک (زیرعنوان، لیبل، ورودی، خطا، دکمه، فوتر، لینک فوتر) با توکن‌های تم؛ هر دو فرم از آن استفاده کنند. فرم ثبت‌نام دقیقاً ساختار ورود را دارد: زیرعنوان (`register.startSignUp`)، فیلدها، دکمه‌ی اصلی و خط پایین «من اکانت دارم؟ ورود» برای بازگشت. بدون `dark:` و بدون رنگ سخت‌کد.
- کلید i18n `register.backToLogin` (در نسخه‌ی نهایی rooshak استفاده نمی‌شود).

---

## بخش E — اکسپو (فقط اگر luca دارد)

- `EXHIBITION_INTRO_PHOTO` در `lib/exhibitionCatalogs.ts` و گذاشتن اولین عکس (`public/exhibition/<intro>.webp`، تبدیل به webp) اول همه‌ی صفحه‌های لیست قیمت. عکس‌ها/لیست‌های قیمت luca را از من بگیر.
- CSS محلی `.exhibit-root .store-btn*` تا ظاهر اکسپو با بازطراحی عوض نشود.

---

## بخش F — موجودیت «تم سایت» (`ThemeSetting`) — Clean/DDD کامل

**دامنه**
- `Domain/Entities/ThemeSetting.cs`: `Name` + ۱۳ رنگ (`Primary, Secondary, Highlight, Neutral, Success, Error, Warning, Info, Surface, SurfaceMuted, Border, Text, TextMuted`)، اعتبارسنجی `#RRGGBB`، `Create/Update(ThemePalette)`؛ record `ThemePalette`.
- `Domain/Interfaces/IThemeSettingRepository` (+ `GetActiveAsync`).

**Application**
- DTO، Queries (`GetAll`, `GetById`, `GetActive`)، Commands (`Create`, `Update`, `Active`, `Delete`)، QueryHandler، CommandHandler: ایجاد → غیرفعال؛ فعال‌سازی → بقیه غیرفعال؛ غیرفعال‌کردن/حذف تم فعال ممنوع (پیام فارسی).
**Infrastructure/Api**
- Repository، `ThemeSettingConfiguration` (جدول `ThemeSettings`، `HasMaxLength(7)`، **seed** تم پیش‌فرض با رنگ‌های فعلی سایت)، DbSet، DI.
- `ThemeSettingsController`: `GET active` عمومی؛ **`GET` لیست عمومی** (صفحه‌ی لیست ادمین بدون توکن fetch می‌کند و با Authorize خطای «بارگذاری لیست ناموفق بود» می‌دهد)؛ `GET {id}`, POST, PUT, `PUT active`, DELETE فقط `SuperAdmin,Admin,ContentEditor`.
- EntityConfig (Id بعدیِ آزاد؛ در rooshak `32`): `EntityName/EndPoint = "themeSettings"`، actions `active/edit/delete/new`، ستون‌ها، فیلدها: `name` + ۱۳ فیلد type **`color`** با `PlaceHolder` برابر رنگ پیش‌فرض.
**فرانت**
- `components/atoms/defaultElements/colorField` (input type=color + hex + نمونه‌ی رنگ؛ مقدار جدید را با placeholder پر می‌کند) و case `color` در `formFieldRenderer`.
- `lib/theme.ts` (server-only): `getActiveTheme()` (`/ThemeSettings/active`، revalidate 60، tag `themeSettings`، timeout) و `themeToCss()` که `:root:root{--primary-color:...;--store-surface:...;...}` می‌سازد و هر مقدار را دوباره با regex هگز اعتبارسنجی می‌کند. `app/layout.tsx` (async) این را در `<style id="site-theme">` داخل head بگذارد. رنگ‌ها از دیتابیس می‌آیند؛ CSS فقط fallback است.
- دکمه‌ی تغییر تم و کامپوننت `ThemeSwitcher` و اسکریپت bootstrap کوکی تم **حذف** شوند (ادمین هم).

---

## بخش G — موجودیت «نوار اعلان» (`AnnouncementBar`) — Clean/DDD کامل

**دامنه**: `Domain/Entities/AnnouncementBar.cs`: `MessageFa/En`, `LinkUrl`, `BackgroundImageUrl`, `BackgroundColor`, `TextColor`, `HeightPx` (۲۴ تا ۱۶۰)، `StartsAt/EndsAt` (UTC، پایان قبل از شروع ممنوع)، `DisplayOrder`؛ `IsActive` یعنی نمایش/عدم نمایش؛ متد `IsVisibleAt(utcNow)`؛ `SetBackgroundImage/ClearBackgroundImage`. `IAnnouncementBarRepository.GetCurrentAsync(utcNow)`.
**Application**: DTO، Queries (`GetAll`, `GetById`, `GetCurrent`)، Commands (`Create`/`Update` با `[FromForm]` و IFormFile برای تصویر + `RemoveBackgroundImage`، `Active`, `Delete`)، Handlerها (آپلود webp در `UploadPaths.AnnouncementBars(id)`، حذف فایل). `GetCurrent` موفق با `Data = null` وقتی نواری نیست.
**Infrastructure/Api**: Repository، Configuration (جدول `AnnouncementBars`، ایندکس `(IsActive, StartsAt, EndsAt)`، seed نوار پیش‌فرض با متن فعلی سایت)، DbSet، DI، `AnnouncementBarsController` (`GET current` عمومی، لیست عمومی، بقیه فقط ادمین).
**EntityConfig** (در rooshak `33`، `announcementBars`): فیلدها `messageFa, messageEn, linkUrl, backgroundImageUrl(file), removeBackgroundImage(checkbox), backgroundColor(color), textColor(color), heightPx(number), startsAt(date), endsAt(date), displayOrder(number)`.
**فرانت**
- `lib/announcement.ts`: `getCurrentAnnouncement()` (revalidate 60، tag `announcementBars`) و `resolveAnnouncementHref(locale, raw)` (نسبی با پیشوند locale، فقط http(s)/mailto/tel، رد `javascript:` و scheme‌های ناشناخته).
- Header server wrapper: متن بر اساس locale (با fallback به زبان دیگر)، رنگ‌ها فقط با regex هگز معتبر، تصویر پس‌زمینه، و `<style>:root:root{--store-announce-h:{px}px}</style>` (۰ وقتی نواری نیست) تا padding صفحه‌ها با ارتفاع نوار هماهنگ شود؛ `AnnouncementStrip` در `headerClient` (لینک داخلی `Link`، خارجی `<a target=_blank rel=noopener noreferrer>`).
- متن `header.announcement` از فایل‌های i18n حذف شود.

---

## تست‌ها و تحویل

- BackEnd: تست‌های دامنه برای `ThemeSetting` (hex، نام الزامی، Update با ورودی خالی) و `AnnouncementBar` (بازه‌ی نمایش، ارتفاع، رنگ‌ها، پایان قبل از شروع). همه‌ی تست‌های قبلی سبز بمانند.
- FrontEnd: `tsc` تمیز، ESLint روی فایل‌های جدید بدون خطا، `next build` موفق، بررسی دستی دسکتاپ/موبایل (۳۹۰px بدون اسکرول افقی)، حالت RTL/LTR.
- در گزارش پایانی بنویس: چه چیزی تطبیق داده شد، چه چیزهایی به‌خاطر تفاوت پروژه پیاده نشد، و یادآوری اجرای migration قبل از دیپلوی.
