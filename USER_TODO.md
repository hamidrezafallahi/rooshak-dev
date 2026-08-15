# کارهای شما (User TODO)

این فهرست کارهایی است که **شما** باید دستی انجام دهید؛ من در کد/سرور تا جایی که ممکن بوده انجام داده‌ام.
ترتیب را از بالا به پایین پیش بروید. بعد از هر مورد، چک‌باکس را علامت بزنید.

آخرین به‌روزرسانی توسط Agent: ۱۴۰۵/۰۵/۲۳ (2026-08-14)

نقشهٔ فازها: `ROADMAP.md` · وضعیت اجرا: `EXECUTION_REPORT.md`

---

## فوری — سرور فعلی (Hetzner) و اسرار لو‌رفته در چت 2026-08-14

VPS فعلی: `65.109.212.237` (`ubuntu-4gb-hel1-2`، `/opt/shop`).  
موارد ۱–۵ پایین‌تر مربوط به سرور/دامنهٔ قدیمی‌اند؛ اول این بخش را ببند.

### A) تعویض رمز SSH root — Deferred در توسعه (قبل از prod اجباری)

- **چرا:** رمز root در چت Cursor فرستاده شد. باید عوض شود.
- **کار شما:**
  1. `ssh root@65.109.212.237`
  2. `passwd` و رمز قوی جدید
  3. ورود با **SSH Key**؛ بعداً PasswordAuthentication را محدود کنید
- **هرگز** رمز را دوباره در چت نگذارید. اگر Agent لازم داشت، بگویید از کلید SSH استفاده کند.

### B) چرخش داشبورد و API هرمس

- **چرا:** یوزر/رمز داشبورد و کلید API در چت فرستاده شد.
- **کار شما:** در `/opt/shop/.env` این‌ها را عوض کنید (مقادیر را اینجا نمی‌نویسم):
  - `HERMES_DASHBOARD_BASIC_AUTH_USERNAME` / `HERMES_DASHBOARD_BASIC_AUTH_PASSWORD`
  - `API_SERVER_KEY`
- بعد: `cd /opt/shop && ./deploy.sh automation recreate`
- تا P003، داشبورد را از اینترنت باز نکنید. UFW الان 9119/8642 را نباید از WAN باز کند؛ بعد از P003 روی `127.0.0.1` bind می‌شود.

### C) کلید OpenRouter (بلاک تلگرام)

- **چرا:** لاگ Hermes: `HTTP 401 User not found` روی `openrouter/free`. بدون این، مالک جواب نمی‌گیرد.
- **کار شما:** کلید معتبر بسازید، در `/opt/shop/.env` و در صورت نیاز `/root/.hermes/.env` بگذارید، کانتینر Hermes را recreate کنید.
- بعد به Agent بگویید P005 را روی سرور چک کند.

### D) قانون چت از این به بعد

رمز، توکن تلگرام، JWT، کلید OpenRouter، کلید Hermes را در چت نفرستید.

---

## فوری — امنیت (سرور/ریپوی قبلی — ممکن است کهنه باشد)

### 1) تعویض رمز root سرور (IP قدیمی در این بند)

- **نوع:** امنیت سرور
- **محل:** VPS `188.240.196.165` (SSH)
- **چرا:** رمز root در چت Cursor لو رفته است.
- **کار شما:**
  1. `ssh root@188.240.196.165`
  2. `passwd` و رمز قوی جدید بگذارید
  3. ترجیحاً ورود با رمز را محدود و **SSH Key** بسازید

### 2) Revoke کردن GitHub Personal Access Token لو‌رفته

- **نوع:** امنیت GitHub
- **محل:** [https://github.com/settings/tokens](https://github.com/settings/tokens)
- **چرا:** روی VPS در `/opt/shop` آدرس `git remote` قبلاً شامل PAT بود (`ghp_...`). من URL را به حالت بدون توکن عوض کردم، ولی خود توکن هنوز باید باطل شود.
- **کار شما:**
  1. همه PATهای مشکوک/قدیمی را Revoke کنید
  2. اگر نیاز به `git pull` روی سرور دارید، یکی از این‌ها را جایگزین کنید:
    - **Deploy Key** فقط‌خواندنی برای ریپوی `candyRose`، یا
    - یک Fine-grained PAT جدید با دسترسی محدود فقط به `candyRose` و ذخیره در credential helper سرور (نه داخل URL)



### 3) بررسی GitHub Secrets ریپوی توسعه

- **نوع:** تنظیمات CI/CD
- **محل:** [https://github.com/hamidrezafallahi/OnlineShop-Compose/settings/secrets/actions](https://github.com/hamidrezafallahi/OnlineShop-Compose/settings/secrets/actions)
- **اسرار مورد انتظار (نام‌ها):**
  - `DOCKER_USERNAME`
  - `DOCKERHUB_TOKEN`
  - `VPS_HOST` (= `188.240.196.165`)
  - `VPS_USER` (= `root` یا کاربر deploy)
  - `VPS_PORT` (= `22`)
  - `SSH_PRIVATE_KEY` (کلید خصوصی متناظر با سرور؛ **نه** پسورد)
- **کار شما:** مطمئن شوید مقادیر واقعی‌اند و با توکن/رمزهای جدید هم‌خوان هستند. فایل محلی `secrets.txt` دیگر در git نیست؛ فقط روی ماشین خودتان نگه دارید یا از `secrets.example.txt` الگو بگیرید.



### 4) چرخش سایر اسرار در صورت نیاز

- **نوع:** امنیت اپ
- **محل VPS:** `/opt/shop/.env`
- **وضعیت فعلی:**
  - `Jwt__Key` را من روی سرور چرخاندم (کاربران باید دوباره لاگین کنند)
  - `POSTGRES_PASSWORD` را من عوض نکردم (ریسک قطع سرویس)
- **کار شما (پیشنهادی ولی مهم):**
  1. اگر `POSTGRES_PASSWORD` قبلاً در git لو رفته بود، با برنامه downtime آن را عوض کنید
  2. پس از عوض کردن: آپدیت `/opt/shop/.env` → `docker compose -f docker-compose.prod.yml up -d` برای postgres/backend
  3. `SEED_ADMIN_PASSWORD` / `PGADMIN_PASSWORD` را اگر ضعیف‌اند عوض کنید



### 5) پاک‌سازی تاریخچه git (اختیاری ولی توصیه‌شده)

- **نوع:** امنیت ریپو
- **محل:** ریپوی `OnlineShop-Compose` روی GitHub
- **چرا:** حذف فایل از commit جدید، تاریخچهٔ قدیمی (`.env` / `secrets.txt`) را پاک نمی‌کند
- **کار شما:** با `git filter-repo` یا BFG تاریخچه را پاک کنید، سپس force-push کنترل‌شده روی `master` (فقط اگر تیم/CI آماده است). اگر انجام نمی‌دهید، حداقل همهٔ مقادیر لو‌رفته را Revoke/Rotate کنید (موارد ۱–۴).

---



## دامنه و SEO



### 6) DNS و دامنهٔ اصلی فروشگاه

- **نوع:** DNS / دامنه
- **وضعیت یافت‌شده:**
  - فروشگاه زنده روی VPS: `shooshkitchen.ir` → `188.240.196.165`
  - `www.hamidrezafalahi.ir` به **Vercel** می‌رود (سایت قدیمی/دیگر)
- **محل تنظیمات کد/سرور که من هم‌تراز کردم:**
  - `FrontEnd/Dockerfile.prod` → پیش‌فرض `NEXT_PUBLIC_SITE_URL=https://shooshkitchen.ir`
  - `.github/workflows/main.yml` → build-arg همان دامنه
  - `/opt/shop/.env` روی VPS → `NEXT_PUBLIC_SITE_URL` و `DOMAIN_NAME=shooshkitchen.ir`
- **کار شما:**
  1. تصمیم بگیرید دامنهٔ برند نهایی چیست (Candy Rose روی کدام دامنه؟)
  2. اگر دامنهٔ نهایی چیز دیگری است، بگویید تا در کد/CI عوض شود
  3. در پنل DNS، `A`/`AAAA` را فقط به VPS بدهید؛ از تداخل با Vercel جلوگیری کنید
  4. SSL برای دامنهٔ نهایی را با certbot روی VPS چک کنید



### 7) فایل‌های محلی env روی سیستم خودتان

- **نوع:** محیط توسعه محلی
- **محل:**
  - `C:\falahi\OnlineShop-Compose\.env` (دیگر در git track نمی‌شود)
  - `C:\falahi\OnlineShop-Compose\FrontEnd\.env` (از روی `FrontEnd/.env.example`)
  - `C:\falahi\candyRose\.env` فقط روی VPS در `/opt/shop/.env`
- **کار شما:** اگر کلون تازه می‌گیرید، از `.env.example` کپی کنید و مقادیر واقعی را پر کنید. هرگز دوباره `.env` یا `secrets.txt` را commit نکنید.

---



## محصول / عملیات فروشگاه



### 8) تست لاگین بعد از httpOnly

- **نوع:** QA دستی
- **محل سایت:** [https://shooshkitchen.ir/fa/register](https://shooshkitchen.ir/fa/register)
- **تغییر فنی من:**
  - کوکی‌های `candyAccess` / `candyRefresh` دیگر برای JS قابل خواندن نیستند
  - فلگ UI: `candySession`
  - API مرورگر از `/auth/bff/...`
- **کار شما:**
  1. یک‌بار لاگین ادمین و مشتری
  2. افزودن به سبد، sync سبد، ورود به ادمین
  3. در DevTools → Application → Cookies چک کنید `candyAccess` باید **HttpOnly** باشد
  4. اگر جایی شکست، اسکرین/لاگ بدهید



### 9) اعمال Seed کاتالوگ (در صورت خالی بودن محصولات)

- **نوع:** داده
- **محل:** `.\database\seed.ps1 -Clean` (نه API جدا)
- **چرا:** کاتالوگ نمونه برای تست فروشگاه native
- **کار شما:** seed را اجرا کنید و یک محصول عمومی (بدون مدل عطر) تست کنید



### 10) ~~تعریف واریانت‌های عطر~~ (حذف شد)

- مدل `ProductVariant` / `PerfumeConcentration` / `SizeMl` از دامنه حذف شد.
- فروشگاه product-agnostic است؛ مشخصات خاص هر محصول با `ProductSpecification` (key/value) تعریف می‌شود.
- Offer فقط به `ProductId` + قیمت + موجودی وصل است.



### 11) بکاپ آف‌سایت (هنوز با من کامل نشده)

- **نوع:** Ops / Disaster Recovery
- **محل فعلی:** volume داکر `shop-backups-prod` روی همان VPS (`/app/backups`)
- **کار شما:**
  1. یک مقصد خارج از سرور انتخاب کنید (Object Storage / هارد دیگر)
  2. cron یا اسکریپت کپی دوره‌ای از volume بکاپ + در صورت تمایل از `uploads`
  3. ماهانه یک‌بار restore تست کنید



### 12) Deploy Key برای candyRose روی VPS

- **نوع:** DevOps
- **محل سرور:** `/opt/shop` + GitHub → Settings → Deploy keys ریپوی `candyRose`
- **کار شما:** بعد از Revoke توکن، Deploy Key بسازید تا `git pull` روی VPS بدون قرار دادن secret در URL کار کند



### 13) بررسی slug محصول بعد از دیپلوی اولویت ۵

- **نوع:** QA / SEO
- **محل:** [https://shooshkitchen.ir/fa/products](https://shooshkitchen.ir/fa/products) و صفحه جزئیات محصول
- **تغییر فنی من:**
  - ستون `Products.Slug` + migration `AddProductSlug`
  - API: `GET /api/Products/{idOrSlug}` و `GET /api/Products/getslugs`
  - URL عددی قدیمی → redirect دائمی به slug
  - لینک کارت‌های محصول از `slug` استفاده می‌کنند
- **کار شما بعد از دیپلوی:**
  1. یک محصول بسازید/seed کنید
  2. URL باید شبیه `/fa/products/product-12` یا slug خوانا باشد
  3. باز کردن `/fa/products/12` باید به slug ریدایرکت شود
  4. در ادمین هنگام ویرایش، در صورت نیاز `Slug` را دستی تنظیم کنید



### 15) بررسی CI شکست‌خوردهٔ قبلی (اولویت ۵)

- **نوع:** اطلاع / نیازی به کار نیست اگر دیپلوی بعدی سبز شد
- **محل:** [https://github.com/hamidrezafallahi/OnlineShop-Compose/actions/runs/30547649781](https://github.com/hamidrezafallahi/OnlineShop-Compose/actions/runs/30547649781)
- **علت:** تایپ `product.slug` در کارت محصول در بیلد Next نبود
- **وضعیت:** در commit بعدی رفع و دوباره پوش شد؛ شما فقط بعد از سبز شدن Actions، تست slug (مورد ۱۳) را انجام دهید

---



## کارهای بعدی Agent (وضعیت)


| اولویت | موضوع                               | وضعیت                    |
| ------ | ----------------------------------- | ------------------------ |
| 1      | حذف secrets از git + چرخش JWT       | انجام شد                 |
| 2      | httpOnly + BFF                      | انجام شد                 |
| 3      | volume بکاپ + dhparams              | انجام شد                 |
| 4      | مدل Variant عطر                     | انجام شد                 |
| 5      | Slug محصول + redirect از id         | انجام شد (در حال دیپلوی) |
| …      | ادامه نقشه راه (فیلتر، wishlist، …) | صف                       |


هر وقت اولویت جدید نیاز به کار دستی شما داشته باشد، **به همین فایل اضافه می‌شود**.

---



## چک‌لیست سریع روزانه

- [ ] رمز root عوض شد
- [ ] PAT گیت‌هاب Revoke شد
- [ ] GitHub Actions Secrets درست است
- [ ] دامنه نهایی و DNS مشخص است
- [ ] لاگین/سبد/ادمین روی shooshkitchen.ir تست شد
- [ ] Seed/محصولات و واریانت‌ها بررسی شد
- [ ] بکاپ آف‌سایت برنامه‌ریزی شد
- [ ] Deploy Key برای candyRose ساخته شد
- [ ] slug محصول و redirect از id تست شد
- [ ] فیلد Slug در فرم ادمین در صورت نیاز اضافه شد