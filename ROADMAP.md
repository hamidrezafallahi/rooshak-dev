# ROADMAP.md — Rooshak Owner Catalog Automation

> **وضعیت سند:** برنامهٔ اجرایی فازبندی‌شده  
> **هم‌تراز با:** کد `BackEnd/` + اتوماسیون `rooshak-prod` (`/opt/shop` روی VPS)  
> **ممیزی سرور:** 2026-08-14 — `ubuntu-4gb-hel1-2`  
> **فرض تیم:** ۱ مهندس + مالک فروشگاه ظروف کریستال/دکوری  
> **قانون معماری:** Telegram → Hermes محدود → n8n فقط برای write چندمرحله‌ای → ASP.NET موجود → PostgreSQL  
> **خارج از محدوده:** MCP / UCP / event bus / API موازی `/api/automation` / Agent خریدار / شل و SQL

کپی عملیاتی همین سند در ریپوی infra: `C:\falahi\rooshak-prod\ROADMAP.md`

---

## 0) قانون اجرا

1. **فاز یعنی خروجی قابل دمو**، نه «۵۰٪ کار شده».
2. اولویت: `P0` بلاک‌کننده / `P1` ارزش روزانهٔ مالک / `P2` کیفیت / `P3` بعداً.
3. تا **Gate A** سبز نشده، محصول واقعی از تلگرام نساز (کلید LLM خراب است).
4. Agent هرگز به PostgreSQL، پنل ادمین، یا شل هاست وصل نمی‌شود.
5. محصول تلگرامی اول **پیش‌نویس / غیرفعال** است؛ انتشار جدا و با تأیید.
6. پنل ادمین سر جایش می‌ماند. تلگرام رابط دوم است، جایگزین نیست.
7. هر فاز را جدا در چت بگو: `فاز: P00X را طبق ROADMAP شروع کن.`

### نمادها
- **Pri:** P0 / P1 / P2 / P3  
- **Dep:** وابستگی  
- **DoD:** تعریف اتمام

---

## 1) نمای موج‌ها

| موج | فازها | هدف |
|-----|-------|------|
| W0 Harden | P001–P008 | لولهٔ فعلی را امن و روشن کن؛ تلگرام باید جواب بدهد |
| W1 Catalog write | P009–P015 | یک ثبت کاتالوگ درست (Product+عکس+Offer) به‌صورت پیش‌نویس |
| W2 Daily ops | P016–P021 | قیمت، موجودی، فعال/غیرفعال، ابعاد ظروف، سفارش |
| W3 Automate | P022–P026 | موجودی کم، لاگ، یک هفته کار واقعی مالک |

---

## 2) وضعیت فعلی (خلاصهٔ ممیزی 2026-08-14)

| قطعه | واقعیت روی VPS |
|------|----------------|
| فروشگاه | `shop-frontend-prod` + `shop-backend-prod` + Postgres — healthy |
| n8n | Docker، فقط `127.0.0.1:5678` — ورک‌فلوهای shop فعال‌اند |
| Hermes | Docker `shop-hermes-prod`؛ bind `127.0.0.1`؛ systemd `hermes-gateway` masked |
| تلگرام | allowlist یک کاربر؛ بدون terminal؛ persona Rooshak فارسی؛ **Gate A = 5/5** |
| بک‌اند از داخل Hermes | HTTP 200 روی `/api/Products` |
| LLM | **P005:** کلید معتبر؛ Hermes chat فارسی 200 |
| کاتالوگ | `CreateCatalogItem` روی prod. آپدیت offer (P016) و انتشار (P017) با `X-Api-Key`. ابعاد ظروف Specification (P019). قانون دو پیام قبل از write (P020). **P021 Gate C = In Progress**. **P022** موجودی کم. **P024** taxonomy match. **P025:** audit JSONL ساخت از تلگرام (`shop-catalog-audit.jsonl`) |
| سفارش مالک | Hermes `list-orders.sh` → n8n `shop-orders-summary` (P018، on-demand). **P023:** دایجست روزانه `shop-orders-digest` cron `0 8 * * *` timezone `Asia/Tehran` → تلگرام مالک (۲۴ ساعت اخیر؛ فقط GET) |
| اسکریپت‌ها | فقط `skills/shop/shop-owner` (P013). از Hermes با DNS داکر `n8n` / `backend` می‌رسند (P014). لوپ‌بک در env بازنویسی می‌شود |

---

## 3) فازهای اجرایی

### W0 — Harden (P001–P008)

| ID | Pri | عنوان | Dep | DoD |
|----|-----|--------|-----|-----|
| P001 | P0 | قفل اسناد ROADMAP / EXECUTION_REPORT / USER_TODO | — | سه فایل در `rooshak-dev` و اشاره در `rooshak-prod` |
| P002 | P0 | چرخش اسرار لو‌رفته در چت (SSH / داشبورد Hermes / API Hermes) | P001 | **در توسعه: Deferred** — قبل از تولید اجباری |
| P003 | P0 | bind داشبورد و API هرمس روی `127.0.0.1` (الان `0.0.0.0`) | P001 | پورت‌ها روی localhost؛ دسترسی با SSH tunnel یا VPN |
| P004 | P0 | mask کردن `hermes-gateway.service` باقی‌مانده | P001 | `systemctl is-enabled` = masked؛ فقط کانتینر بالا است |
| P005 | P0 | کلید OpenRouter معتبر در Hermes (الان 401) | P002 | یک پیام تلگرام از مالک جواب فارسی می‌گیرد |
| P006 | P0 | Telegram toolset: حذف terminal آزاد؛ فقط اسکریپت‌های shop-owner | P005 | از تلگرام `rm` / `docker` / `psql` ممکن نباشد |
| P007 | P1 | STT فارسی (`stt.language: fa`) برای ویس مالک | P005 | یک ویس تست به متن فارسی نزدیک تبدیل می‌شود |
| P008 | P0 | **Gate A:** تلگرام زنده + allowlist + بدون شل هاست | P003–P007 | چک‌لیست Gate A در EXECUTION_REPORT امضا |

### W1 — Catalog write (P009–P015)

| ID | Pri | عنوان | Dep | DoD |
|----|-----|--------|-----|-----|
| P009 | P0 | Use-case `CreateCatalogItem` در Application (یک تراکنش: Product + Image? + Offer) | P008 | تست: نیمه‌کاره Offer نماند |
| P010 | P0 | ساخت از این مسیر همیشه `IsActive=false` تا تأیید انتشار | P009 | محصول در سایت عمومی دیده نشود |
| P011 | P0 | n8n create به همان یک endpoint؛ نه ۳ HTTP + لاگین رمز هر بار | P009 | ورک‌فلو کوتاه‌تر؛ شکست جزئی کمتر |
| P012 | P0 | سرویس‌اکانت با API key / JWT scoped به‌جای `Identity/login` با پسورد | P011 | پسورد Content Bot از نود n8n حذف |
| P013 | P1 | یک اسکیل `shop-owner`؛ حذف مسیر تکراری قدیمی | P006 | فقط `skills/shop/shop-owner` |
| P014 | P1 | اسکریپت‌ها فقط DNS داکر (`n8n` / `backend`) نه `127.0.0.1` | P013 | create از داخل کانتینر Hermes سبز |
| P015 | P0 | **Gate B:** عکس+کپشن → خلاصه → تأیید → پیش‌نویس با Offer و عکس | P010–P014 | یک کالای تست روی prod با `IsActive=false` |

### W2 — Daily ops (P016–P021)

| ID | Pri | عنوان | Dep | DoD |
|----|-----|--------|-----|-----|
| P016 | P0 | قیمت/موجودی روی `ProductOffer` با staff bypass ContentEditor | P015 | آپدیت offer دیگر 403 ندهد |
| P017 | P1 | فعال/غیرفعال محصول از تلگرام (انتشار پیش‌نویس) | P010 | «بگذار روی سایت» فقط بعد از تأیید |
| P018 | P1 | سفارش‌ها read-only (ورک‌فلو موجود را پایدار نگه دار) | P008 | «سفارش‌های امروز» بدون تأیید |
| P019 | P1 | فیلد ظروف: ابعاد / تعداد پارچه به‌صورت Specification نه حجم عطر | P015 | گلدان با قطر/ارتفاع ذخیره می‌شود |
| P020 | P0 | قانون سخت: هیچ write در همان پیام خلاصه؛ صبر برای تأیید | P015 | تست دوپیام |
| P021 | P0 | **Gate C:** مالک یک محصول واقعی کریستال از تلگرام تا پیش‌نویس | P016–P020 | **In Progress** — لوله سبز؛ DoD = یک کالای واقعی از تلگرام، بدون پنل |

### W3 — Automate (P022–P026)

| ID | Pri | عنوان | Dep | DoD |
|----|-----|--------|-----|-----|
| P022 | P1 | هشدار موجودی کم به تلگرام مالک (webhook از بک‌اند یا n8n) | P016 | **Done** — فوری روی offer-update + اسکن ساعتی `shop-low-stock-alert` |
| P023 | P1 | دایجست روزانه سفارش (الان هست) را نگه دار و مستند کن | P018 | **Done** — `shop-orders-digest` فعال؛ `0 8 * * *` / `Asia/Tehran` |
| P024 | P1 | برند/دسته خودکار ساخته نشود؛ از لیست زنده match | P015 | **Done** — «گلدون» → «گلدان کریستالی»؛ شمارش دسته ثابت؛ create id جعلی را رد می‌کند |
| P025 | P2 | Audit ساده: چه کسی از تلگرام چه محصولی ساخت | P012 | **Done** — JSONL قابل tail + `list-catalog-audit.sh` |
| P026 | P0 | **Gate D:** یک هفتهٔ مالک بدون پنل برای ورود کالای روزمره | P021–P024 | **Blocked** — منتظر امضای Gate C؛ سپس هفتهٔ مالک |

---

## 4) صریحاً نمی‌سازیم (مگر تصمیم جدید)

- MCP server برای فروشگاه
- UCP / Agent خریدار داخل سایت (جدا است؛ Hermes هرگز چت مشتری نشود)
- `/api/automation/*` موازی با Products
- `execute_sql` / `docker.sock` روی Hermes
- ساخت دسته/برند توسط Agent
- انتشار بدون تأیید

---

## 5) جملهٔ آماده برای فاز بعد

الان Gate D شروع نمی‌شود تا Gate C بسته شود:

```
Gate C ساخته شد؛ productId=N را verify کن.
```

بعد از امضای Gate C و طی شدن یک هفته کار واقعی:

```
فاز: P026 را ادامه بده — هفته Gate D تمام شد؛ لاگ زیر را verify و امضا کن.
روز1: productId=…
روز2: …
…
خارج: persona، MCP.
```

P026 **Blocked**: Dep = P021–P024؛ P021 هنوز In Progress. لوله W3 (P022–P025) سبز است.
