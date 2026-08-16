# EXECUTION_REPORT.md — گزارش اجرایی Agent مالک فروشگاه Rooshak

> **وضعیت:** سند راهبری + **ممیزی واقعیت VPS و کد** (نه ادعای اتمام)  
> **تاریخ ممیزی:** 2026-08-14  
> **مبتنی بر:** `ROADMAP.md` + `rooshak-dev` (ASP.NET) + `rooshak-prod` (`/opt/shop`)  
> **قانون:** `ROADMAP.md` = ترتیب اجرا · این فایل = Done / Blocked / Not started  
> **خارج از محدوده:** MCP، UCP، Agent مشتری، SQL مستقیم، شل هاست

---

## چطور هر فاز را از من بخواهی

یک فاز در هر پیام. همین قالب را کپی کن و شماره را عوض کن:

```
Gate C ساخته شد؛ productId=N را verify کن.
```

| بگو | یعنی |
|-----|------|
| `فاز: P00X را طبق ROADMAP شروع کن.` | فقط همان فاز |
| `فاز: P00X را طبق ROADMAP شروع کن. فقط همین فاز؛ بدون scope creep.` | همان + جلوگیری از کار اضافه |
| `P002 را در توسعه skip کن` | چرخش رمز را عقب بینداز (الان انجام شد) |

نیازی نیست کل ROADMAP را دوباره بفرستی.

---

## TODO — صف ادامه کار (همین را به Agent بگو)

> به‌روز شده: **2026-08-16 — P026 Blocked on Gate C.**  
> **الان کجا هستیم:** P001–P020 Done · P002 Deferred · **P022–P025 Done** · **P021 Gate C In Progress** · **P026 Blocked** (تا Gate C + یک هفته مالک).

**جملهٔ آماده برای چت بعدی:**

```
Gate C ساخته شد؛ productId=N را verify کن.
```

| ترتیب | فاز | وضعیت | Dep |
|-------|-----|--------|-----|
| W0-1 | P001 اسناد ROADMAP/REPORT | **Done** | — |
| W0-2 | P002 چرخش اسرار | **Deferred (توسعه)** — قبل از prod اجباری | P001 ✓ |
| W0-3 | P003 bind Hermes روی 127.0.0.1 | **Done** (2026-08-14) | P001 ✓ |
| W0-4 | P004 mask systemd hermes-gateway | **Done** (2026-08-15) | P001 ✓ |
| W0-5 | P005 کلید OpenRouter | **Done** (2026-08-15) | P002 deferred؛ کلید فعلی روی سرور 200 شد |
| W0-6 | P006 حذف terminal از تلگرام | **Done** (2026-08-15) | P005 |
| W0-7 | P007 STT فارسی | **Done** (2026-08-15) | P005 |
| W0-8 | **P008 Gate A** | **Done 5/5** (2026-08-15 شب؛ A1 با تأیید مالک) | P003–P007 |
| W1-1 | **P009 CreateCatalogItem** | **Done** (2026-08-15) | P008 |
| W1-2 | **P010 پیش‌نویس IsActive=false** | **Done** (2026-08-15) | P009 ✓ |
| W1-3 | **P011 n8n create یک endpoint** | **Done در ریپو** (2026-08-16) | P009 ✓ |
| W1-4 | **P012 API key به‌جای لاگین رمز** | **Done در ریپو** (2026-08-16) | P011 ✓ |
| W1-5 | **P013 یک اسکیل shop-owner** | **Done** (2026-08-16) | P006 ✓ |
| W1-6 | **P014 DNS داکر** | **Done** (2026-08-16) | P013 ✓ |
| W1-7 | **P015 Gate B** | **Done** (2026-08-16) | P010–P014 ✓ |
| W2-1 | **P016 آپدیت Offer** | **Done** (2026-08-16) | P015 ✓ |
| W2-2 | **P017 فعال/غیرفعال** | **Done** (2026-08-16) | P010 ✓ |
| W2-3 | **P018 سفارش read-only** | **Done** (2026-08-16) | P008 ✓ |
| W2-4 | **P019 ابعاد ظروف Spec** | **Done** (2026-08-16) | P015 ✓ |
| W2-5 | **P020 قانون دو پیام** | **Done** (2026-08-16) | P015 ✓ |
| W2-6 | **P021 Gate C** | **In Progress** — منتظر کالای واقعی تلگرام | P016–P020 ✓ |
| W3-1 | **P022 هشدار موجودی کم** | **Done** (2026-08-16) | P016 ✓ |
| W3-2 | **P023 دایجست سفارش ۸ صبح** | **Done** (2026-08-16) | P018 ✓ |
| W3-3 | **P024 taxonomy match فقط** | **Done** (2026-08-16) | P015 ✓ |
| W3-4 | **P025 audit ساخت تلگرام** | **Done** (2026-08-16) | P012 ✓ |
| W3-5 | **P026 Gate D** | **Blocked** — تا امضای P021 + هفتهٔ مالک | P021–P024 |

---

## تحویل — 2026-08-16 (P026 Gate D — شروع / Blocked)

یک فاز شروع شد ولی Done نیست.

### حکم امروز

Gate D را نمی‌توان امضا کرد: Dep **P021 Gate C** هنوز باز است. اسکن prod: فقط کاتالوگ seed (۱–۵) + تست‌ها (۶–۱۰)؛ هیچ کالای واقعی جدید از تلگرام برای Gate C نیست (`P026_BLOCKED_ON_GATE_C`). لولهٔ W3 (Hermes/n8n/backend + P022–P025) سبز است و برای هفتهٔ Gate D آماده‌اند، اما ساعت هفته بعد از امضای Gate C شروع می‌شود.

| موج | Done | باقی |
|-----|------|------|
| W0 Harden P001–P008 | P001, P003–P008 | فقط P002 Deferred |
| W1 Catalog P009–P015 | P009–P015 | Gate B امضا |
| W2 Daily ops | P016–P020 Done · **P021 In Progress** | بستن Gate C |
| W3 Automate | **P022–P025 Done** · **P026 Blocked** | هفتهٔ مالک پس از Gate C |

### بعدی Agent چه کار کند

1. با پیام مالک `Gate C ساخته شد؛ productId=N را verify کن.` همان را امضا کند.
2. تاریخ شروع هفتهٔ Gate D را در REPORT ثبت کند.
3. بعد از ۷ روز + لاگ مالک، چک‌لیست Gate D را امضا کند.

### مالک چه کار کند

1. **اول Gate C:** از تلگرام یک ظرف واقعی (نه ۷–۱۰) بساز → اینجا بگو `Gate C ساخته شد؛ productId=N را verify کن.`
2. **بعد هفته Gate D:** ورود کالای روزمره فقط از تلگرام؛ پنل فقط برای SEO / چند عکس / تخفیف پیچیده.
3. هر روز یک خط در لاگ هفته (پایین USER_TODO) بنویس؛ بعد از ۷ روز بگو هفته تمام شد.

### آنچه نباید تکرار شود

- Gate D را بدون Gate C Done نزن.
- پیش‌نویس تست ۶–۱۰ را Gate C حساب نکن.
- persona / MCP را دست نزن.

---

## تحویل — 2026-08-16 (P025)

یک فاز انجام شد؛ گزارش را از نو ننویس.

### حکم امروز

Audit سادهٔ ساخت از تلگرام فعال است: هر `create-product.sh` یک خط JSONL در `/opt/data/logs/shop-catalog-audit.jsonl` می‌نویسد (`source=telegram-owner`, `actorTelegramIds`, productId/name). `list-catalog-audit.sh --tail N` قابل tail/خواندن است. اثبات: ایجاد پیش‌نویس productId **10** → خط audit با `ok: true` / `productId: 10`؛ در جستجوی عمومی نیست. دیپلوی بک‌اند/migration لازم نبود.

| موج | Done | باقی |
|-----|------|------|
| W0 Harden P001–P008 | P001, P003–P008 | فقط P002 Deferred |
| W1 Catalog P009–P015 | P009–P015 | Gate B امضا |
| W2 Daily ops | P016–P020 Done · P021 In Progress | بستن Gate C |
| W3 Automate | **P022–P025 Done** | P026 Gate D (بعد از P021) |

### بعدی Agent چه کار کند

اول **بستن P021 Gate C** با کالای واقعی مالک؛ بعد فقط **P026**.

### مالک چه کار کند

- Gate C: کالای واقعی از تلگرام؛ بعد `Gate C ساخته شد؛ productId=N را verify کن.`
- پیش‌نویس‌های تست audit (۹ و ۱۰) را منتشر نکند مگر عمدی.
- رمز/توکن را در چت نگذارد.

### آنچه نباید تکرار شود

- جدول/migration سنگین برای همین DoD لازم نبود؛ JSONL کافی است.
- persona / Gate D را بدون بستن Gate C شروع نکن.

### فایل‌های P025

- `automation/hermes/skills/shop/shop-owner/scripts/append-catalog-audit.sh`
- `automation/hermes/skills/shop/shop-owner/scripts/list-catalog-audit.sh`
- `automation/hermes/skills/shop/shop-owner/scripts/create-product.sh` — append بعد از create
- `automation/hermes/skills/shop/shop-owner/SKILL.md` v1.6.0 — بخش Catalog audit

---

## تحویل — 2026-08-16 (P024)

یک فاز انجام شد؛ گزارش را از نو ننویس.

### حکم امروز

دسته/برند از مسیر مالک ساخته نمی‌شود. `match-taxonomy.sh --kind category --query گلدون` → بهترین match زنده **گلدان کریستالی (id=1)** با `created: false`؛ شمارش دسته‌های فعال قبل/بعد = ۵. `create-product.sh` با `categoryId=99999` رد می‌شود. SKILL v1.5.0 + SOUL قانون P024. دیپلوی بک‌اند لازم نبود.

| موج | Done | باقی |
|-----|------|------|
| W0 Harden P001–P008 | P001, P003–P008 | فقط P002 Deferred |
| W1 Catalog P009–P015 | P009–P015 | Gate B امضا |
| W2 Daily ops | P016–P020 Done · P021 In Progress | بستن Gate C |
| W3 Automate | **P022–P024 Done** | P025–P026 |

### بعدی Agent چه کار کند

فقط **P025** (audit ساخت از تلگرام) یا verify Gate C اگر مالک ساخت.

پرامپت:

```
فاز: P025 را طبق ROADMAP شروع کن. فقط همین فاز؛ بدون scope creep.
محدوده: Audit ساده — چه کسی از تلگرام چه محصولی ساخت (جدول یا لاگ قابل tail).
خارج: Gate C/D، persona، taxonomy بیشتر.
```

### مالک چه کار کند

- Gate C هنوز باز است: کالای واقعی از تلگرام؛ بعد `Gate C ساخته شد؛ productId=N را verify کن.`
- رمز/توکن را در چت نگذارد.

### آنچه نباید تکرار شود

- ساخت Category/Brand از Hermes/n8n را اضافه نکن.
- persona / Gate D / P025 را در این فاز قاطی نکن.

### فایل‌های P024

- `automation/hermes/skills/shop/shop-owner/scripts/match-taxonomy.sh`
- `automation/hermes/skills/shop/shop-owner/scripts/create-product.sh` — validate live ids
- `automation/hermes/skills/shop/shop-owner/SKILL.md` — Taxonomy match (P024)
- `automation/hermes/SOUL.md` — قانون taxonomy فارسی

---

## تحویل — 2026-08-16 (P023)

یک فاز انجام شد؛ گزارش را از نو ننویس.

### حکم امروز

دایجست روزانه سفارش پایدار و مستند شد. ورک‌فلو زنده `shop-orders-digest` فعال است: cron `0 8 * * *`، timezone `Asia/Tehran`، خلاصه ۲۴ ساعت اخیر با فقط GET سفارش‌ها → تلگرام مالک. اثبات دستی webhook → HTTP 200 / `ok: true` / متن «خلاصه صبحگاهی… موردی نیست». منطق عوض نشد؛ JSON تمیز در ریپو ذخیره شد. دیپلوی بک‌اند لازم نبود.

| موج | Done | باقی |
|-----|------|------|
| W0 Harden P001–P008 | P001, P003–P008 | فقط P002 Deferred |
| W1 Catalog P009–P015 | P009–P015 | Gate B امضا |
| W2 Daily ops | P016–P020 Done · P021 In Progress | بستن Gate C |
| W3 Automate | **P022–P023 Done** | P024–P026 |

### بعدی Agent چه کار کند

فقط **P024** (match برند/دسته از taxonomy زنده؛ ساخت خودکار ممنوع) یا verify Gate C اگر مالک ساخت.

پرامپت:

```
فاز: P024 را طبق ROADMAP شروع کن. فقط همین فاز؛ بدون scope creep.
محدوده: برند/دسته خودکار ساخته نشود؛ فقط match از لیست زنده taxonomy.
خارج: Gate C/D، persona، audit (P025).
```

### مالک چه کار کند

- صبح‌ها حدود ۸ تهران پیام «خلاصه صبحگاهی سفارش‌ها» را در بات چک کند (اختیاری).
- Gate C هنوز باز است: کالای واقعی از تلگرام؛ بعد `Gate C ساخته شد؛ productId=N را verify کن.`
- رمز/توکن را در چت نگذارد.

### آنچه نباید تکرار شود

- دایجست را به API key عوض نکن مگر فاز جدا؛ لاگین سرویس‌اکانت برای Orders عمدی است (P018).
- persona / Gate D / P024 را در این فاز قاطی نکن.

### فایل‌های P023

- `automation/n8n/shop-orders-digest.workflow.json` — منبع در ریپو (بدون متادیتای شخصی)
- `automation/hermes/skills/shop/shop-owner/SKILL.md` — اشاره به دایجست ۸:۰۰ (P023)

---

## تحویل — 2026-08-16 (P022)

یک فاز انجام شد؛ گزارش را از نو ننویس.

### حکم امروز

هشدار موجودی کم به تلگرام مالک فعال است. وقتی موجودی Offer به کمتر از ۲ می‌رسد: (۱) فوری از `shop-offer-update` پیام می‌رود؛ (۲) اسکن ساعتی `shop-low-stock-alert` با dedupe همان را پوشش می‌دهد (از جمله کاهش موجودی از سفارش). اثبات: offer ۷ → موجودی ۵ (بدون هشدار) → موجودی ۱ (`lowStockAlerted: true`)؛ اسکن `force` → `notified: true` / `lowCount: 2`. دیپلوی بک‌اند لازم نبود.

| موج | Done | باقی |
|-----|------|------|
| W0 Harden P001–P008 | P001, P003–P008 | فقط P002 Deferred |
| W1 Catalog P009–P015 | P009–P015 | Gate B امضا |
| W2 Daily ops | P016–P020 Done · P021 In Progress | بستن Gate C |
| W3 Automate | **P022 Done** | P023–P026 |

### بعدی Agent چه کار کند

فقط **P023** (دایجست سفارش ۸ صبح) یا اگر مالک Gate C را بست، verify همان.

پرامپت:

```
فاز: P023 را طبق ROADMAP شروع کن. فقط همین فاز؛ بدون scope creep.
محدوده: دایجست روزانه سفارش (ساعت ۸ تهران) را نگه دار و مستند کن.
خارج: Gate C/D، persona، audit (P025).
```

### مالک چه کار کند

- در تلگرام بات مالک، پیام «هشدار موجودی کم» را برای تست P022 چک کند (اختیاری).
- Gate C هنوز باز است: یک کالای واقعی کریستال از تلگرام بسازد؛ بعد بگوید `Gate C ساخته شد؛ productId=N را verify کن.`
- رمز/توکن را در چت نگذارد.

### آنچه نباید تکرار شود

- persona / toolset / Gate D را در این فاز دست نزن.
- `./deploy.sh backend` برای P022 لازم نبود؛ تگ محلی بک‌اند را با pull Hub خراب نکن.

### فایل‌های P022

- `automation/n8n/shop-low-stock-alert.workflow.json` — اسکن ساعتی + webhook
- `automation/n8n/shop-offer-update.workflow.json` — هوک فوری وقتی inventory < 2
- `automation/hermes/skills/shop/shop-owner/SKILL.md` — یادداشت هشدار خودکار

---

## تحویل — 2026-08-16 (P021 Gate C — شروع)

یک فاز در جریان؛ Done نیست تا مالک از تلگرام یک ظرف واقعی بسازد.

### حکم امروز

لولهٔ Gate C روی VPS سبز است: Hermes + n8n create با `X-Api-Key` + skill دوپیام (P020) + taxonomy زنده. پیش‌نویس‌های تست ۷/۸ هنوز غیرفعال‌اند و Gate C حساب نمی‌شوند. DoD = یک محصول واقعی کریستال (عکس+کپشن → خلاصه → تأیید → پیش‌نویس با Offer) بدون پنل ادمین.

| موج | Done | باقی |
|-----|------|------|
| W0 Harden P001–P008 | P001, P003–P008 | فقط P002 Deferred |
| W1 Catalog P009–P015 | P009–P015 | Gate B امضا |
| W2 Daily ops | P016–P020 Done · **P021 In Progress** | بستن Gate C با کالای واقعی |

### بعدی Agent چه کار کند

بعد از ساخت از تلگرام: `productId` جدید را verify کند (`IsActive=false`، Offer، نه در جستجوی عمومی) و چک‌لیست Gate C را امضا کند؛ سپس فقط P022.

پرامپت بعد از بستن:

```
فاز: P022 را طبق ROADMAP شروع کن. فقط همین فاز؛ بدون scope creep.
محدوده: هشدار موجودی کم به تلگرام مالک وقتی موجودی < ۲.
خارج: Gate D، persona، ساخت دسته/برند (P024).
```

### مالک چه کار کند

- از تلگرام بات Hermes: عکس واقعی ظرف کریستال + کپشن (نام، برند/دسته، قیمت، موجودی؛ اختیاری قطر/ارتفاع/تعداد پارچه).
- پیام ۱ فقط خلاصه + «تأیید»؛ پیام ۲ بگو `تأیید`.
- پیش‌نویس تست ۷/۸ را دوباره نساز؛ پنل ادمین باز نکن.
- بعد از ساخت، در چت Cursor بگو: `Gate C ساخته شد؛ productId=N را verify کن.`
- رمز/توکن را در چت نگذارد.

### آنچه نباید تکرار شود

- محصول تست ۷/۸ یا ساخت از Hermes به‌جای تلگرام مالک را Gate C حساب نکن.
- persona / toolset / P022 را در این فاز دست نزن.
- پنل ادمین برای ثبت این کالا استفاده نشود.

### آمادگی لوله (اثبات Agent)

- `P021_READY_FOR_OWNER`: Hermes/n8n/backend up؛ create workflow active با `X-Api-Key`؛ skill دوپیام + vessel fields؛ پیش‌نویس ۷/۸ هنوز در جستجوی عمومی نیستند.

---

## تحویل — 2026-08-16 (P019) Hermes `create-product.sh --diameter 18 --height 32 --piece-count 1` → n8n → `POST /api/CatalogItems` → productId **8** با مشخصات `قطر=18 سانتی‌متر`، `ارتفاع=32 سانتی‌متر`، `تعداد پارچه=1`. پیش‌نویس است و در جستجوی عمومی نیست.

| موج | Done | باقی |
|-----|------|------|
| W0 Harden P001–P008 | P001, P003–P008 | فقط P002 Deferred |
| W1 Catalog P009–P015 | P009–P015 | Gate B امضا |
| W2 / W3 | P016–P019 | **بعدی P020** سپس P021–P026 |

### بعدی Agent چه کار کند

فقط **P020**: هیچ write در همان پیام خلاصه؛ تست دوپیام.

پرامپت:

```
فاز: P020 را طبق ROADMAP شروع کن. فقط همین فاز؛ بدون scope creep.
محدوده: قانون سخت — هیچ write در همان پیام خلاصه؛ صبر برای تأیید؛ تست دوپیام.
خارج: Gate C، persona، هشدار موجودی (P022).
```

### مالک چه کار کند

- همان پرامپت P020 را در چت بفرستد.
- رمز/توکن/API key را در چت نگذارد.
- `./deploy.sh backend <sha-قدیمی>` نزنید؛ تگ زنده `p019-vessel` محلی است و pull از Hub آن را برمی‌گرداند.
- پیش‌نویس‌های تست (7 و 8) را منتشر نکند مگر برای تست بعدی.

### آنچه نباید تکرار شود

- persona / toolset تلگرام را عوض نکن.
- فیلد حجم عطر / SizeMl برنگردان.
- اسکریپت‌ها را به `127.0.0.1` برنگردان.

### فایل‌های P019

- `BackEnd/Application/Common/VesselCatalogSpecs.cs`
- `BackEnd/Application/Commands/CreateCatalogItemCommand.cs` — `Diameter` / `PieceCount`
- `BackEnd/Application/Handler/CommandHandler/CreateCatalogItemCommandHandler.cs`
- `automation/n8n/shop-product-create.workflow.json`
- `automation/hermes/skills/shop/shop-owner/scripts/create-product.sh`
- VPS image: `hamidrezafalahi/shop-backend:p019-vessel` (لوکال؛ روی Hub نیست)

---

## تحویل — 2026-08-16 (P018)

یک فاز انجام شد؛ گزارش را از نو ننویس.

### حکم امروز

سفارش از تلگرام/Hermes read-only است و «سفارش‌های امروز» تأیید نمی‌خواهد. Hermes `list-orders.sh --hours 24` → n8n `shop-orders-summary` → فقط `GET /api/Orders/open` → `ok: true` و `textFa` (الان موردی نیست). ورک‌فلو زنده دست نخورد (لاگین سرویس‌اکانت ماند؛ PUT/PATCH/DELETE ندارد). دایجست ۸ صبح را عوض نکردم (P023).

| موج | Done | باقی |
|-----|------|------|
| W0 Harden P001–P008 | P001, P003–P008 | فقط P002 Deferred |
| W1 Catalog P009–P015 | P009–P015 | Gate B امضا |
| W2 / W3 | P016–P018 | **بعدی P019** سپس P020–P026 |

### بعدی Agent چه کار کند

فقط **P019**: ابعاد / تعداد پارچه به‌صورت Specification.

پرامپت:

```
فاز: P019 را طبق ROADMAP شروع کن. فقط همین فاز؛ بدون scope creep.
محدوده: فیلد ظروف: ابعاد / تعداد پارچه به‌صورت Specification نه حجم عطر؛ گلدان با قطر/ارتفاع ذخیره می‌شود.
خارج: Gate C، persona، تست دوپیام (P020).
```

### مالک چه کار کند

- همان پرامپت P019 را در چت بفرستد.
- رمز/توکن/API key را در چت نگذارد.
- `./deploy.sh backend` نزنید؛ تگ زنده همچنان `p017-active` است (این فاز بک‌اند عوض نکرد).

### آنچه نباید تکرار شود

- persona / toolset تلگرام را عوض نکن.
- ورک‌فلو سفارش را به confirm/pay/cancel وصل نکن.
- نود Identity/login سفارش را در این فاز به API key عوض نکن (پایدار ماند).
- دایجست ۸ صبح را بازنویسی نکن (P023).

### فایل‌های P018

- `automation/n8n/shop-orders-summary.workflow.json` — کپی پایدار در ریپو؛ GET-only
- `automation/hermes/skills/shop/shop-owner/scripts/list-orders.sh`
- `automation/hermes/skills/shop/shop-owner/SKILL.md` — «سفارش‌های امروز» = `--hours 24` بدون تأیید

---

## تحویل — 2026-08-16 (P017)

یک فاز انجام شد؛ گزارش را از نو ننویس.

### حکم امروز

فعال/غیرفعال از مسیر مالک کار می‌کند. Hermes `set-product-active.sh --product-id 7 --active true` → n8n `shop-product-active` → `PUT /api/CatalogItems/active` با `X-Api-Key` → Product+Offer+Image با هم فعال شدند و در جستجوی عمومی ظاهر شدند. همان مسیر با `--active false` پیش‌نویس را از سایت برداشت. SKILL برای «بگذار روی سایت» قبل از write منتظر `تأیید` می‌ماند.

| موج | Done | باقی |
|-----|------|------|
| W0 Harden P001–P008 | P001, P003–P008 | فقط P002 Deferred |
| W1 Catalog P009–P015 | P009–P015 | Gate B امضا |
| W2 / W3 | P016, P017 | **بعدی P018** سپس P019–P026 |

### بعدی Agent چه کار کند

فقط **P018**: سفارش‌ها read-only؛ «سفارش‌های امروز» بدون تأیید.

پرامپت:

```
فاز: P018 را طبق ROADMAP شروع کن. فقط همین فاز؛ بدون scope creep.
محدوده: سفارش‌ها read-only (ورک‌فلو موجود را پایدار نگه دار)؛ «سفارش‌های امروز» بدون تأیید.
خارج: Gate C، persona، ابعاد ظروف (P019)، تست دوپیام (P020).
```

### مالک چه کار کند

- همان پرامپت P018 را در چت بفرستد.
- رمز/توکن/API key را در چت نگذارد.
- `./deploy.sh backend <sha-قدیمی>` نزنید؛ تگ زنده `p017-active` محلی است و pull از Hub آن را برمی‌گرداند.

### آنچه نباید تکرار شود

- persona / toolset تلگرام را عوض نکن.
- اسکریپت‌ها را به `127.0.0.1` برنگردان.
- نود Identity/login را به ورک‌فلو create / offer-update / product-active برنگردان.
- پیش‌نویس تست را روی سایت عمومی نگذار (بعد از تست P017 دوباره `IsActive=false` است).

### فایل‌های P017

- `BackEnd/Api/Controllers/CatalogItemsController.cs` — PUT `active` با policy `CatalogWrite`
- `BackEnd/Application/Handler/CommandHandler/SetCatalogItemActiveCommandHandler.cs`
- `BackEnd/Application.Tests/SetCatalogItemActiveCommandHandlerTests.cs`
- `automation/n8n/shop-product-active.workflow.json` — `X-Api-Key`؛ بدون Login
- VPS image: `hamidrezafalahi/shop-backend:p017-active` (لوکال؛ روی Hub نیست)

---

## تحویل — 2026-08-16 (P016)

یک فاز انجام شد؛ گزارش را از نو ننویس.

### حکم امروز

آپدیت قیمت/موجودی از مسیر مالک دیگر 403 نمی‌دهد. Hermes `update-offer.sh --offer-id 7 --price 10000 --inventory 1` → n8n `shop-offer-update` → `PUT /api/ProductOffers` با `X-Api-Key` → **HTTP 200**، `ok: true`. Offer 7 روی prod: قیمت 10000، موجودی 1، `IsActive=false`. نود Identity/login از این ورک‌فلو حذف شد.

| موج | Done | باقی |
|-----|------|------|
| W0 Harden P001–P008 | P001, P003–P008 | فقط P002 Deferred |
| W1 Catalog P009–P015 | P009–P015 | Gate B امضا |
| W2 / W3 | P016 | **بعدی P017** سپس P018–P026 |

### بعدی Agent چه کار کند

فقط **P017**: فعال/غیرفعال محصول از تلگرام (انتشار پیش‌نویس)؛ «بگذار روی سایت» فقط بعد از تأیید.

پرامپت:

```
فاز: P017 را طبق ROADMAP شروع کن. فقط همین فاز؛ بدون scope creep.
محدوده: فعال/غیرفعال محصول از تلگرام (انتشار پیش‌نویس)؛ «بگذار روی سایت» فقط بعد از تأیید.
خارج: Gate C، persona، ابعاد ظروف (P019).
```

### مالک چه کار کند

- همان پرامپت P017 را در چت بفرستد.
- رمز/توکن/API key را در چت نگذارد.
- پیش‌نویس «P015 Gate B draft» را از پنل منتشر نکند مگر برای تست P017.
- `./deploy.sh backend <sha-قدیمی>` نزنید؛ تگ زنده `p016-offer` محلی است و pull از Hub آن را برمی‌گرداند.

### آنچه نباید تکرار شود

- persona / toolset تلگرام را عوض نکن.
- اسکریپت‌ها را به `127.0.0.1` برنگردان.
- نود Identity/login را به ورک‌فلو create یا offer-update برنگردان.
- `PUT .../active` را در این فاز عوض نکن (P017).

### فایل‌های P016

- `BackEnd/Api/Controllers/ProductOffersController.cs` — PUT با policy `CatalogWrite`
- `BackEnd/Application.Tests/ProductOfferUpdateTests.cs` — ContentEditor staff bypass
- `automation/n8n/shop-offer-update.workflow.json` — `X-Api-Key`؛ بدون Login
- VPS image: `hamidrezafalahi/shop-backend:p016-offer` (لوکال؛ روی Hub نیست)

---

## تحویل — 2026-08-16 (P015 Gate B)

یک فاز انجام شد؛ گزارش را از نو ننویس.

### حکم امروز

یک پیش‌نویس تست روی prod ساخته شد: **productId 7** + offerId 7 + imageId 15، `IsActive=false`. در لیست و جستجوی عمومی نیست. مسیر: Hermes `create-product.sh` → n8n `shop-product-create` → `POST /api/CatalogItems` با `X-Api-Key` و فایل عکس. persona دست نخورد.

| موج | Done | باقی |
|-----|------|------|
| W0 Harden P001–P008 | P001, P003–P008 | فقط P002 Deferred |
| W1 Catalog P009–P015 | P009–P015 | Gate B امضا |
| W2 / W3 | هیچ | **بعدی P016** سپس P017–P026 |

### بعدی Agent چه کار کند

فقط **P016**: آپدیت قیمت/موجودی روی `ProductOffer` با staff bypass ContentEditor تا 403 ندهد.

پرامپت:

```
فاز: P016 را طبق ROADMAP شروع کن. فقط همین فاز؛ بدون scope creep.
محدوده: آپدیت قیمت/موجودی روی ProductOffer با staff bypass ContentEditor؛ دیگر 403 ندهد.
خارج: انتشار از تلگرام (P017)، Gate C، persona.
```

### مالک چه کار کند

- همان پرامپت P016 را در چت بفرستد.
- رمز/توکن/API key را در چت نگذارد.
- پیش‌نویس «P015 Gate B draft» را از پنل منتشر نکند مگر برای تست P017.
- `./deploy.sh backend <sha-قدیمی>` نزنید؛ تگ زنده `p015-gateb` محلی است و pull از Hub آن را برمی‌گرداند.

### آنچه نباید تکرار شود

- persona / toolset تلگرام را عوض نکن.
- اسکریپت‌ها را به `127.0.0.1` برنگردان.
- نود Identity/login را به ورک‌فلو create برنگردان.

### فایل‌های P015

- `BackEnd/Api/Controllers/CatalogItemsController.cs` — JSON و multipart
- `BackEnd/Application/Handler/CommandHandler/CreateCatalogItemCommandHandler.cs` — آپلود عکس در همان تراکنش
- `BackEnd/Infrastructure/Repository/ProductRepository.cs` — جستجو فقط `IsActive`
- `automation/n8n/shop-product-create.workflow.json`
- VPS image: `hamidrezafalahi/shop-backend:p015-gateb` (لوکال؛ روی Hub نیست)

---

## تحویل — 2026-08-16 (P014)

یک فاز انجام شد؛ گزارش را از نو ننویس.

### حکم امروز

از داخل `shop-hermes-prod` نام‌های `n8n` و `backend` resolve می‌شوند و HTTP به آن‌ها می‌رسد. `127.0.0.1:5678` داخل Hermes وصل نمی‌شود. create webhook بدون payload از Hermes به n8n می‌رسد (نه connection refused). کالای واقعی ساخته نشد. از تلگرام کالا نساز تا Gate B.

| موج | Done | باقی |
|-----|------|------|
| W0 Harden P001–P008 | P001, P003–P008 | فقط P002 Deferred |
| W1 Catalog P009–P015 | P009–P014 | **بعدی P015 Gate B** |
| W2 / W3 | هیچ | P016–P026 |

### بعدی Agent چه کار کند

فقط **P015 Gate B**: عکس+کپشن → خلاصه → تأیید → پیش‌نویس با Offer و عکس، `IsActive=false`. CatalogItems روی prod هنوز 404 است؛ دیپلوی بک‌اند مال همین فاز Gate B است نه P014.

پرامپت:

```
فاز: P015 را طبق ROADMAP شروع کن. فقط همین فاز؛ بدون scope creep.
محدوده: Gate B — عکس+کپشن → خلاصه → تأیید → پیش‌نویس با Offer و عکس؛ IsActive=false.
خارج: persona، چرخش اسرار، W2.
```

### مالک چه کار کند

- همان پرامپت P015 را در چت بفرستد.
- رمز/توکن/API key را در چت نگذارد.
- از تلگرام کالا نسازد تا Agent فاز P015 را شروع کند.

### آنچه نباید تکرار شود

- اسکریپت‌ها را به `127.0.0.1` برنگردان.
- persona / toolset تلگرام را عوض نکن.
- n8n زنده را قبل از دیپلوی CatalogItems ایمپورت نکن.

### فایل‌های P014

- `automation/hermes/skills/shop/shop-owner/scripts/*.sh` — بازنویسی URL لوپ‌بک به DNS داکر
- `automation/hermes/skills/shop/shop-owner/SKILL.md` v1.3.1
- VPS: همان bind-mount `/opt/shop/automation/hermes/skills/shop/shop-owner`

---

## تحویل — 2026-08-16 (P012)

یک فاز انجام شد؛ گزارش را از نو ننویس.

### حکم امروز

W0 Harden بسته است. مسیر کاتالوگ در ریپو یک تراکنش پیش‌نویس است. ورک‌فلو create دیگر پسورد Content Bot ندارد: `X-Api-Key` → `POST /api/CatalogItems`. **روی VPS:** CatalogItems هنوز 404 است؛ n8n زنده ایمپورت نشد. از تلگرام کالا نساز تا Gate B.

| موج | Done | باقی |
|-----|------|------|
| W0 Harden P001–P008 | P001, P003–P008 | فقط P002 Deferred |
| W1 Catalog P009–P015 | P009–P012 (ریپو) | **بعدی P013** سپس P014–P015 |
| W2 / W3 | هیچ | P016–P026 |

### بعدی Agent چه کار کند

فقط **P013**: یک اسکیل `shop-owner`؛ مسیر تکراری قدیمی را حذف کن. کالا نساز. n8n create را به Products برنگردان.

پرامپت:

```
فاز: P013 را طبق ROADMAP شروع کن. فقط همین فاز؛ بدون scope creep.
محدوده: یک اسکیل shop-owner؛ حذف مسیر تکراری قدیمی.
خارج: تلگرام persona، ساخت کالا، P014+.
```

### مالک چه کار کند

- همان پرامپت P013 را در چت بفرستد.
- رمز/توکن/API key را در چت نگذارد.
- روی VPS (وقتی بک‌اند CatalogItems دیپلوی شد) یک کلید تصادفی بلند بگذارد: `ContentAutomation__ApiKey` و همان مقدار در `CATALOG_API_KEY` برای n8n. مقدار را اینجا ننویس.
- از تلگرام کالا نسازد.
- ورک‌فلو جدید را روی n8n زنده ایمپورت نکند تا بک‌اند CatalogItems + ApiKey روی VPS باشد.

### آنچه نباید تکرار شود

- Gate A و persona تلگرام بسته شد؛ بازنویسی نکن.
- `CreateCatalogItem` و `IsActive=false` را بازنویسی نکن.
- نود `Identity/login` را به ورک‌فلو create برنگردان.
- ورک‌فلوهای SEO را در این فاز عوض نکن (هنوز لاگین رمز دارند — عمدی).
- محصول واقعی از تلگرام تا Gate B (P015) نساز.

### فایل‌های P012

- `BackEnd/Application/Common/CatalogApiKey.cs`
- `BackEnd/Api/Security/CatalogApiKeyAuthenticationHandler.cs`
- `BackEnd/Api/Controllers/CatalogItemsController.cs` — policy `CatalogWrite` (JWT یا X-Api-Key)
- `automation/n8n/shop-product-create.workflow.json` — بدون لاگین؛ هدر `X-Api-Key` از env
- `Application.Tests` — ۷ تست سبز

---

## تحویل — 2026-08-16 (P011)

یک فاز انجام شد؛ گزارش را از نو ننویس.

### حکم امروز

W0 Harden بسته است (Gate A = 5/5). مسیر Application کاتالوگ در ریپو درست است (P009+P010). ورک‌فلو create در ریپو به `POST /api/CatalogItems` کوتاه شد (P011). **روی VPS:** بک‌اند هنوز CatalogItems ندارد (POST = 404)؛ n8n زنده هنوز ۳ HTTP قدیمی است — عمداً ایمپورت نشد. از تلگرام کالا نساز تا Gate B.

| موج | Done | باقی |
|-----|------|------|
| W0 Harden P001–P008 | P001, P003–P008 | فقط P002 Deferred |
| W1 Catalog P009–P015 | P009, P010, P011 (ریپو) | **بعدی P012** سپس P013–P015 |
| W2 / W3 | هیچ | P016–P026 |

### بعدی Agent چه کار کند

فقط **P012**: سرویس‌اکانت با API key / JWT scoped به‌جای `Identity/login` با پسورد در ورک‌فلو create. Hermes/تلگرام/persona را دست نزن. کالا نساز.

پرامپت:

```
فاز: P012 را طبق ROADMAP شروع کن. فقط همین فاز؛ بدون scope creep.
محدوده: API key / JWT scoped به‌جای Identity/login با پسورد در ورک‌فلو n8n create.
خارج: تلگرام، persona، ساخت کالا، P013+.
```

### مالک چه کار کند

- همان پرامپت P012 را در چت بفرستد.
- رمز/توکن در چت نگذارد.
- P002 را هنوز skip نگه دارد مگر تصمیم به prod.
- از تلگرام کالا نسازد.
- ورک‌فلو جدید را روی n8n زنده ایمپورت نکند تا بک‌اند `POST /api/CatalogItems` روی VPS باشد.

### آنچه نباید تکرار شود

- Gate A و persona تلگرام بسته شد؛ بازنویسی نکن.
- bind Hermes و mask systemd انجام شده‌اند.
- `CreateCatalogItem` و `IsActive=false` روی همان مسیر نوشته شده؛ بازنویسی نکن.
- ورک‌فلو create را دوباره به Products/ProductImages/ProductOffers برنگردان.
- محصول واقعی از تلگرام تا Gate B (P015) نساز.
- مدل env `llama-3.3-70b-instruct:free` هنوز 404 است — عوض کردنش کار P012 نیست.

### فایل‌های P009–P011 (در ریپو؛ CatalogItems روی VPS هنوز نیست)

- `BackEnd/Application/Commands/CreateCatalogItemCommand.cs`
- `BackEnd/Application/Handler/CommandHandler/CreateCatalogItemCommandHandler.cs`
- `BackEnd/Api/Controllers/CatalogItemsController.cs` → `POST /api/CatalogItems`
- `BackEnd/Infrastructure/Persistence/EfUnitOfWork.cs`
- `BackEnd/Application.Tests` — ۴ تست سبز
- `automation/n8n/shop-product-create.workflow.json` — webhook همان `shop-product-create`؛ write فقط CatalogItems؛ لاگین رمز مانده برای P012

---

## AUDIT — واقعیت 2026-08-14

### حکم رک

**لوله وجود دارد؛ برای مالک هنوز قابل استفاده نیست.**  
Hermes داخل Docker کنار n8n است و به API فروشگاه از شبکهٔ داخلی می‌رسد. LLM در P005 دیگر 401 نیست. تلگرام در P006 toolset `terminal` ندارد. مسیر `CreateCatalogItem` پیش‌نویس است (`IsActive=false`)؛ پنل ادمین و پیش‌فرض `BaseEntity` هنوز true است.

| ادعای معماری | واقعیت |
|---------------|--------|
| Telegram → Hermes → n8n → API → Postgres | **نیمه‌کاره:** مسیر فایل‌ها هست؛ LLM در P005 دیگر 401 نیست؛ E2E تلگرام را مالک تأیید کند |
| Agent به DB وصل نیست | **درست** (اسکریپت‌ها webhook می‌زنند) |
| Agent به شل وصل نیست | **درست برای تلگرام (P006):** `platform_toolsets.telegram = [skills, vision]`؛ `terminal` declined. CLI داخل کانتینر هنوز terminal دارد |
| پیش‌نویس قبل از انتشار | **درست برای CreateCatalogItem (P010+P015):** Product+Offer+Image با `IsActive=false`. پنل `CreateProduct` هنوز true. n8n زنده create = CatalogItems + X-Api-Key |
| API جدا `/automation` | **درست که نیست** — از Products/Offers موجود استفاده می‌شود |
| Hermes روی root هاست | **درست که نیست** — unit `hermes-gateway` **masked**؛ runtime باید کانتینر `shop-hermes-prod` باشد |

### سرور

| مورد | مقدار |
|------|--------|
| Host | `ubuntu-4gb-hel1-2` — uptime حدود ۲ روز |
| فروشگاه | frontend + backend healthy؛ nginx 80/443 |
| n8n | `127.0.0.1:5678` — خوب |
| Hermes dashboard/API | listen روی `127.0.0.1:9119` و `127.0.0.1:8642` (P003) |
| UFW | default deny؛ فقط 22/80/443 باز — پورت Hermes از اینترنت باید بسته باشد |
| VPN | `amnezia-awg2` روی UDP 30666 |
| systemd | `hermes-gateway.service` = **masked** + inactive (P004، 2026-08-15) |

> **مشاهده P004 (2026-08-15):** فروشگاه Docker بالا است. `shop-hermes-prod` و `shop-n8n-prod` در `docker ps` نبودند. این را در P004 روشن نکردم.

### n8n فعال روی سرور

- Shop Product Create
- Shop Offer Update
- Shop Product Active
- Shop Orders Summary
- Shop Orders Digest (08:00 تهران)

ورک‌فلوهای SEO روی دیسک هستند؛ در لاگ استارت این نمونه فقط shopها Activate شده بودند.

### شکاف کد (rooshak-dev)

- `CreateProductCommand` فقط Product می‌سازد؛ قیمت/موجودی روی `ProductOffer` است.
- ورک‌فلو create زنده `POST /api/CatalogItems` با `X-Api-Key` است (P015). offer-update `PUT /api/ProductOffers` (P016). product-active `PUT /api/CatalogItems/active` (P017). ورک‌فلوهای SEO هنوز Identity/login دارند (عمدی).
- `IsCatalogStaff()` شامل ContentEditor است؛ PUT Offer با API key دیگر 403 نمی‌دهد (P016). انتشار پیش‌نویس Product+Offer+Image را با هم عوض می‌کند (P017).
- ابعاد ظروف از create به‌صورت Specification (`قطر` / `ارتفاع` / `تعداد پارچه`) ذخیره می‌شود (P019)؛ حجم عطر نیست.

### شکاف Hermes

- کلید OpenRouter در P005 دیگر 401 نیست.
- اسکیل زنده فقط `skills/shop/shop-owner` است (P013). از داخل Hermes به `n8n` / `backend` می‌رسد (P014). Gate B: create با عکس از همان اسکریپت سبز شد (P015).
- STT در P007 روی `fa` قفل شد (local/openai/groq).
- `TELEGRAM_ALLOWED_USERS` روی env ست است (یک کاربر).

---

## انجام‌شده — تکرار نکن مگر رگرسیون

- [x] P001 اسناد ROADMAP + EXECUTION_REPORT
- [x] P003 bind داشبورد/API هرمس روی `127.0.0.1:9119` و `127.0.0.1:8642`؛ n8n دست نخورده ماند
- [x] P004 mask کردن `hermes-gateway.service`؛ `systemctl is-enabled` = masked
- [x] P005 کلید OpenRouter معتبر؛ Hermes دیگر 401 User not found نمی‌دهد
- [x] P006 تلگرام بدون toolset `terminal`؛ resolve runtime `has_terminal=False`
- [x] P007 STT فارسی `stt.language: fa`؛ ویس تست به متن فارسی نزدیک تبدیل شد
- [x] Hermes از systemd هاست به Docker (`shop-hermes-prod`) منتقل شده (کار قبلی روی VPS)
- [x] n8n ورک‌فلوهای owner روی همین compose
- [x] اسکیل shop-owner و اسکریپت‌های create/update/active/orders در `rooshak-prod`
- [x] n8n به localhost bind شده
- [x] بک‌اند از داخل شبکهٔ Docker به Hermes جواب 200 می‌دهد
- [x] P011 ورک‌فلو n8n create در ریپو فقط `POST /api/CatalogItems` (لاگین رمز مانده برای P012)
- [x] P012 ورک‌فلو create با `X-Api-Key`؛ نود Identity/login حذف شد
- [x] P013 فقط `skills/shop/shop-owner`؛ کپی قدیمی `/root/.hermes` کنار گذاشته شد
- [x] P014 اسکریپت‌ها از Hermes با DNS داکر (`n8n` / `backend`)؛ لوپ‌بک بازنویسی می‌شود
- [x] P015 Gate B: CatalogItems روی prod + n8n X-Api-Key + پیش‌نویس تست با عکس، `IsActive=false`
- [x] P016 آپدیت Offer با `X-Api-Key` و staff bypass؛ Hermes update دیگر 403 نیست
- [x] P017 فعال/غیرفعال از Hermes؛ `PUT /api/CatalogItems/active`؛ پیش‌نویس 7 بعد از تست دوباره غیرفعال
- [x] P018 سفارش read-only از Hermes؛ `list-orders.sh --hours 24` بدون تأیید؛ ورک‌فلو زنده پایدار ماند
- [x] P019 ابعاد ظروف به‌صورت Specification (`قطر`/`ارتفاع`/`تعداد پارچه`)؛ پیش‌نویس productId 8
- [x] P020 قانون دو پیام: خلاصه بدون write؛ بعد از تأیید write؛ تست Hermes سبز

### P003 — جزئیات اجرا (2026-08-14)

- `docker-compose.automation.yml`: پیش‌فرض bind از `0.0.0.0` به `127.0.0.1`
- `/opt/shop/.env`: `HERMES_DASHBOARD_BIND` و `HERMES_API_BIND` = `127.0.0.1`
- recreate فقط سرویس `hermes`؛ `shop-n8n-prod` Up ماند
- تأیید `ss`: `127.0.0.1:9119` و `127.0.0.1:8642` (دیگر `0.0.0.0` نیست)
- دسترسی داشبورد در توسعه: `ssh -L 9119:127.0.0.1:9119 root@VPS` بعد `http://127.0.0.1:9119`
- P002 با تصمیم مالک در حالت توسعه skip شد؛ قبل از رفتن روی تولید باید انجام شود

### P004 — جزئیات اجرا (2026-08-15)

- `systemctl mask hermes-gateway.service` (idempotent؛ از 2026-08-14 14:51 به `/dev/null` لینک بود)
- تأیید: `is-enabled=masked` · `LoadState=masked` · `ActiveState=inactive` · `UnitFileState=masked`
- فروشگاه (`shop-frontend-prod` / `shop-backend-prod` / `shop-postgres-prod` / `shop-nginx-prod`) دست نخورده ماند
- **خارج از فاز:** `shop-hermes-prod` و `shop-n8n-prod` در `docker ps` نبودند. روشن کردن automation کار P004 نیست؛ قبل از P005 روی VPS: `cd /opt/shop && ./deploy.sh automation`

### P005 — جزئیات اجرا (2026-08-15)

- `OPENROUTER_API_KEY` روی `/opt/shop/.env` و `/root/.hermes/.env` موجود بود (مقدار در اسناد نیست)
- Probe OpenRouter `GET /api/v1/models` = **200** (دیگر 401 User not found نیست)
- مدل Hermes در `config.yaml`: `openrouter/free` — chat completion = **200** و به مدل رایگان واقعی resolve می‌شود
- `LLM_MODEL=meta-llama/llama-3.3-70b-instruct:free` در env برای همان کلید **404** است؛ Hermes از آن برای chat استفاده نکرد. عوض کردنش کار n8n/SEO است نه P005
- `./deploy.sh automation up`: `shop-hermes-prod` و `shop-n8n-prod` بالا آمدند؛ bind همان `127.0.0.1:9119` و `127.0.0.1:8642`
- تست مسیر LLM داخل Hermes: `POST http://127.0.0.1:8642/v1/chat/completions` → `200` و پاسخ فارسی «سلام»
- بات تلگرام `getMe` = ok (username در لاگ سرور). adapter: Connecting attempt 1/8 سپس ادامهٔ gateway؛ E2E تلگرام را مالک با یک «سلام» تأیید کند
- toolset و STT را عوض نکردم (P006 / P007)

### P006 — جزئیات اجرا (2026-08-15)

- `/root/.hermes/config.yaml` از قبل P006 بود؛ عوضش نکردم (بدون scope creep)
- `platform_toolsets.telegram` = `skills`, `vision` — بدون `terminal`
- `known_builtin_toolsets.telegram` = `terminal` → در Hermes یعنی **declined** (برنمی‌گردد با upgrade recently-shipped)
- Resolve زنده داخل `shop-hermes-prod`: `ENABLED[telegram]` بدون terminal (`has_terminal=False`). CLI کانتینر همچنان terminal دارد — عمدی؛ P006 فقط تلگرام است
- اسکیل `skills/shop/shop-owner` روی دیسک و bind-mount است. اسکریپت‌ها webhook به n8n می‌زنند نه SQL. اجرای bash از تلگرام بدون terminal ممکن نیست — این DoD است؛ executor محدود فاز بعدی است نه P006
- STT / Gate A / P013 را دست نزدم

### P007 — جزئیات اجرا (2026-08-15)

- `stt.enabled: true` و `stt.language: fa` از قبل بود
- `stt.openai.language` از خالی به `fa`؛ `stt.groq.language: fa` اضافه شد تا هر provider همان hint را بگیرد
- Resolve زنده: `provider=local`، `resolved_language=fa`
- ویس تست (espeak-ng فارسی → mp3): `transcribe_audio` موفق؛ خروجی خط فارسی نزدیک به «سلام … گلدان را سه میلیون کن» (کیفیت espeak ضعیف است؛ ویس واقعی مالک بهتر است)
- toolset تلگرام و Gate A را دست نزدم

### P008 — جزئیات اجرا (2026-08-15)

Gate A را با شواهد زنده امضا کردم؛ P009 در آن لحظه شروع نشد (بعداً به‌صورت Application انجام شد).

- **A1 سلام تلگرام:** **امضا (۱۵ اوت شب).** مالک تأیید کرد جواب درست آمد (فارسی نقش فروشگاه؛ دیگر LFM / Hello-first نیست). `SOUL.md` + `system_prompt` + `platform_hints.telegram.replace`.
- **A2 یوزر غیرمجاز:** امضا. `TELEGRAM_ALLOWED_USERS` = یک کاربر؛ `GATEWAY_ALLOW_ALL_USERS` ست نیست؛ در adapter بدون allowlist fail-closed است؛ `unauthorized_dm_behavior: pair` در config نیست پس DM غریبه pairing نمی‌شود. تست زنده با اکانت دوم انجام نشد.
- **A3 بدون شل:** امضا. resolve تلگرام: `has_terminal=False`، `code_execution=False`، `computer_use=False`. فقط `skills` / `vision` (+ bfl/kanban).
- **A4 داشبورد عمومی:** امضا. listen `127.0.0.1:9119` و `127.0.0.1:8642`. از IPv4 عمومی connect به 9119/8642 = `ECONNREFUSED`. UFW فقط 22/80/443 (+ 19546/8443 برای x-ui).
- **A5 systemd masked:** امضا. `is-enabled=masked`، `is-active=inactive`، کانتینر `shop-hermes-prod` Up.

### P009 — جزئیات اجرا (2026-08-15)

Use-case Application `CreateCatalogItem`: Product + Image? + Offer در **یک تراکنش** EF. n8n / Hermes / `IsActive` را دست نزدم. محصول از تلگرام ساخته نشد.

- Command: `CreateCatalogItemCommand` → `CatalogItemIdsDto` (ProductId, OfferId, ImageId?)
- Handler داخل `BeginTransaction` می‌سازد؛ اگر persist آفر خطا بدهد rollback می‌شود و Product یتیم نمی‌ماند
- `IUnitOfWork` / `EfUnitOfWork` روی همان `AppDbContext` scoped
- API نازک: `POST /api/CatalogItems` با همان نقش‌های کاتالوگ (`SuperAdmin,Admin,ContentEditor`) — برای P011؛ ورک‌فلو n8n عوض نشد
- عکس اختیاری با `ImageUrl` (نه آپلود فایل؛ آپلود همان مسیر فعلی `ProductImages` می‌ماند تا P011)
- تست DoD: `Application.Tests` — rollback سبز؛ بعد از P010 مجموعاً ۴ تست
- **عمداً خارج:** `IsActive=false` در آن لحظه (بعداً P010)، n8n یک‌کال (P011)، API key (P012)، ساخت تلگرامی

### P010 — جزئیات اجرا (2026-08-15)

همین مسیر `CreateCatalogItem` همیشه پیش‌نویس است. پیش‌فرض `BaseEntity` و `CreateProduct` پنل را عوض نکردم.

- بعد از factory، قبل از persist: `Product.SetActive(false)`، `ProductOffers.SetActive(false)`، در صورت عکس `ProductImage.SetActive(false)`
- کلاینت نمی‌تواند از این endpoint `IsActive=true` بفرستد (فیلد ندارد)
- لیست/لندینگ/جستجو/اسلاگ عمومی از قبل `p.IsActive` را فیلتر می‌کنند → پیش‌نویس در کاتالوگ عمومی نیست
- `GET /api/Products/{slug}` هنوز IsActive را فیلتر نمی‌کند (صفحهٔ محصول با اسلاگ مستقیم). تغییر آن admin edit را می‌شکند؛ انتشار جدا P017 است
- تست: `Catalog_item_is_always_created_inactive` سبز
- **عمداً خارج:** n8n (P011)، API key (P012)، فعال‌سازی از تلگرام (P017)، ساخت تلگرامی

### P011 — جزئیات اجرا (2026-08-16)

ورک‌فلو n8n create به همان یک write endpoint. API key / تلگرام / persona / ساخت کالا را دست نزدم.

- فایل: `automation/n8n/shop-product-create.workflow.json` — همان webhook `POST /webhook/shop-product-create` و همان secret
- HTTP write فقط `POST /api/CatalogItems` (دیگر Products + ProductImages + ProductOffers + GET Product نیست)
- لاگین `Identity/login` با Content Bot ماند — عمدی؛ حذف پسورد کار P012 است
- Hermes `create-product.sh` عوض نشد؛ فیلدهای form همان‌اند (`name`, `price`→`basePrice`, …)
- عکس فایل (multipart) دیگر آپلود نمی‌شود؛ فقط `imageUrl` رشته‌ای اگر در payload باشد. آپلود باینری بدون productId به CatalogItems نمی‌رود
- **روی VPS:** `POST http://backend:8080/api/CatalogItems` = **404** (DLL بدون CreateCatalogItem). فایل روی دیسک `/opt/shop/automation/n8n/` به‌روز شد؛ **ایمپورت زنده انجام نشد** تا create فعلی نشکند
- **عمداً خارج:** API key (P012)، دیپلوی بک‌اند، تلگرام، persona، ساخت کالای تست

### P012 — جزئیات اجرا (2026-08-16)

پسورد Content Bot از نود ورک‌فلو **create** حذف شد. تلگرام / persona / ساخت کالا / ورک‌فلوهای SEO را دست نزدم.

- Scheme `ApiKey`: هدر `X-Api-Key` در برابر `ContentAutomation:ApiKey` (مقایسه زمان‌ثابت). بدون هدر = NoResult تا JWT پنل کار کند
- Policy `CatalogWrite` فقط روی `POST /api/CatalogItems` — scoped؛ بقیه API همان Bearer است
- Principal سرویس‌اکانت seedشده (`ContentAutomation:ServiceAccountEmail`) با نقش `ContentEditor` تا `GetUserId` و FK آفر درست باشد
- ورک‌فلو create: نودهای Login / Keep Context / Logged In حذف؛ هدر از `$env.CATALOG_API_KEY` (کلید داخل json آیتم نمی‌رود)
- کلید واقعی در git نیست؛ `.env.example` فقط placeholder
- تست: ۷ سبز (۴ کاتالوگ + ۳ API key)
- **روی VPS:** CatalogItems هنوز 404. فایل ورک‌فلو روی دیسک به‌روز می‌شود؛ **ایمپورت زنده نشد**
- **عمداً خارج:** عوض کردن لاگین ورک‌فلوهای SEO، دیپلوی بک‌اند، تلگرام، ساخت کالا

### P013 — جزئیات اجرا (2026-08-16)

یک اسکیل زنده. persona / toolset تلگرام / ساخت کالا / P014 تست create را دست نزدم.

- Canonical: `/opt/shop/automation/hermes/skills/shop/shop-owner` روی کانتینر bind-mount است (`/opt/data/skills/shop`)
- کپی قدیمی `/root/.hermes/skills/shop/shop-owner` (SKILL v1.0 + پیش‌فرض `127.0.0.1`) به `/root/shop-owner.host-dup.p013.bak-20260816T055800Z`
- مسیر top-level `skills/shop-owner` از قبل نبود (بکاپ ۱۴ اوت)
- `SKILL.md` زنده دوبل/`=======` داشت؛ به یک سند تمیز v1.3.0 با frontmatter اصلاح شد
- کپی در ریپو: `automation/hermes/skills/shop/shop-owner/`
- داخل کانتینر: `/opt/data/skills/shop-owner` وجود ندارد؛ فقط `skills/shop/shop-owner`
- **عمداً خارج:** تست سبز create از داخل Hermes (P014)، ساخت کالا، عوض کردن persona

### P014 — جزئیات اجرا (2026-08-16)

اسکریپت‌های shop-owner فقط DNS داکر. persona / تلگرام / ساخت کالای واقعی / ایمپورت n8n را دست نزدم.

- داخل `shop-hermes-prod`: `n8n` → `172.18.0.9`، `backend` → `172.18.0.3`
- HTTP: `http://n8n:5678/` = 200؛ `GET /api/Products` روی backend = 200
- webhook create: GET = 404، POST خالی = 401 (n8n جواب داد؛ کالا ساخته نشد)
- `http://127.0.0.1:5678/` از داخل Hermes = connection refused (انتظار)
- env و `/opt/data/.env` برای URLهای shop از قبل `DOCKER_DNS` بودند (نه loopback)
- اگر env از قبل `127.0.0.1` / `localhost` / `::1` باشد، اسکریپت URL را به `n8n:5678` یا `backend:8080` برمی‌گرداند
- `create-product.sh` داخل کانتینر اجرا شد؛ بدون فیلد اجباری exit 2 (usage) — write نشد
- **عمداً خارج:** ساخت کالای تست، Gate B، دیپلوی CatalogItems، ایمپورت n8n زنده، persona

### P015 — جزئیات اجرا (2026-08-16)

Gate B. persona / toolset / P016 offer 403 را دست نزدم.

- بک‌اند prod با image محلی `hamidrezafalahi/shop-backend:p015-gateb` (نه pull از Hub)
- `ContentAutomation__ApiKey` و `CATALOG_API_KEY` روی `.env` یکسان شدند (مقدار در گزارش نیست)
- `POST /api/CatalogItems` بدون کلید = 401 (دیگر 404 نیست)
- ورک‌فلو زنده create ایمپورت شد: JSON یا multipart فایل → یک `POST /api/CatalogItems`
- عکس فایل در همان تراکنش UploadAsWebp می‌شود؛ شکست آپلود کل واحد را rollback می‌کند
- تست واحد: ۹ سبز (قبلی + شکست آپلود + ذخیره فایل)
- `SearchByNameAsync` فقط `IsActive` (جستجوی عمومی پیش‌نویس را نشان نمی‌داد)
- مسیر رسمی: `create-product.sh` از Hermes با PNG → productId **7** / offerId 7 / imageId 15
- لیست عمومی و جستجوی `P015 Gate B` این id را ندارند؛ فایل `uploads/products/7/*.webp` روی volume هست
- SKILL همچنان قبل از write منتظر `تأیید` می‌ماند (تست دوپیام تلگرام = P020)
- **عمداً خارج:** P016 staff bypass Offer، P017 انتشار، persona، چرخش اسرار، push به Docker Hub

### P016 — جزئیات اجرا (2026-08-16)

قیمت/موجودی از مسیر مالک دیگر 403 نمی‌دهد. persona / toolset / انتشار محصول را دست نزدم.

- علت 403 قبلی: ورک‌فلو زنده Identity/login می‌کرد و 401 را به پیام 403 staff-bypass نگاشت می‌کرد؛ PUT فقط Bearer/`[Authorize(Roles=...)]` بود
- PUT `/api/ProductOffers` الان policy `CatalogWrite` (Bearer یا `X-Api-Key`)؛ Create/Active/Delete همان Roles ماندند
- Handler از قبل `IsCatalogStaff()` داشت (SuperAdmin/Admin/ContentEditor هر Offer را می‌توانند عوض کنند)
- ورک‌فلو زنده offer-update ایمپورت شد: بدون Login؛ هدر از `$env.CATALOG_API_KEY`
- تست واحد: ContentEditor staff است؛ می‌تواند Offer تأمین‌کنندهٔ دیگر را عوض کند؛ Customer نمی‌تواند
- مسیر رسمی: `update-offer.sh` از Hermes → offerId **7** / قیمت 10000 / موجودی 1 / HTTP 200
- Offer 7 همچنان `IsActive=false`؛ جستجوی عمومی `P015 Gate B` خالی است
- **عمداً خارج:** P017 انتشار از تلگرام، Gate C، persona، ورک‌فلوهای SEO، push به Docker Hub

### P017 — جزئیات اجرا (2026-08-16)

انتشار پیش‌نویس از مسیر مالک. persona / toolset / سفارش / ابعاد را دست نزدم.

- ورک‌فلو زنده قبلی Identity/login می‌کرد و فقط `PUT /api/Products/active` می‌زد؛ Offer و عکس پیش‌نویس می‌ماندند
- `PUT /api/CatalogItems/active` با policy `CatalogWrite` در یک تراکنش Product + Offerهای غیرحذف + عکس‌ها را `SetActive` می‌کند
- انتشار بدون Offer رد می‌شود تا کالای بدون قیمت روی سایت نرود
- ورک‌فلو زنده product-active ایمپورت شد: بدون Login؛ هدر از `$env.CATALOG_API_KEY`
- SKILL: «بگذار روی سایت» write است؛ در همان پیام خلاصه اسکریپت زده نمی‌شود؛ پیش‌نویس با `productId` (جستجوی عمومی آن را نمی‌بیند)
- تست واحد: ۵ سبز (publish/unpublish/بدون Offer/بدون کاربر/یافت نشد). فیلتر کاتالوگ+offer: ۱۸ سبز
- مسیر رسمی: Hermes `--product-id 7 --active true` → جستجوی عمومی `P015 Gate B` شامل 7؛ بعد `--active false` → از جستجو رفت؛ Offer 7 دوباره `IsActive=false`
- **عمداً خارج:** Gate C، persona، P018 سفارش، P019 ابعاد، P020 تست دوپیام تلگرام، push به Docker Hub

### P018 — جزئیات اجرا (2026-08-16)

سفارش مالک read-only. persona / کاتالوگ / دایجست ۸ صبح را دست نزدم.

- ورک‌فلو زنده `shop-orders-summary` فعال بود و ماند؛ فقط `GET /api/Orders/open` و `GET /api/Orders/{id}`؛ بدون PUT/PATCH/DELETE و بدون confirm/pay/cancel
- لاگین سرویس‌اکانت عمداً ماند: `OrdersController` کل کلاس `[Authorize]` است و این فاز API key سفارش نساخت
- Hermes `list-orders.sh --hours 24` → `ok: true`، `textFa`: «سفارش‌های باز (24 ساعت اخیر) / موردی نیست.» (فروشگاه سفارش باز ندارد؛ لیست خالی معتبر است)
- `--hours 72` و `--hours 0 --status paid,shipped` هم همان شکل JSON را برگرداندند
- SKILL: «سفارش‌های امروز» فوراً `--hours 24`؛ بدون تأیید
- فایل ورک‌فلو در ریپو قفل شد تا با دیسک VPS یکی باشد؛ **ایمپورت مجدد نشد**
- **عمداً خارج:** P019 ابعاد، P020 تست دوپیام، P023 بازنویسی دایجست، عوض کردن لاگین سفارش به API key، دیپلوی بک‌اند

### P019 — جزئیات اجرا (2026-08-16)

ابعاد ظروف به‌صورت Specification. persona / Gate C / تست دوپیام را دست نزدم.

- `CreateCatalogItem` اختیاری: `Diameter`، `Height` (از ProductDimensionsDto)، `PieceCount`
- کلیدهای ثابت فارسی: `قطر` / `ارتفاع` / `تعداد پارچه` (نه SizeMl / حجم عطر)
- Hermes `create-product.sh` فلگ‌های `--diameter` / `--height` / `--piece-count`
- ورک‌فلو create ایمپورت شد (`versionId: p019-vessel-specs`)
- تست واحد: ۸ سبز روی CreateCatalogItem (شامل ذخیره Spec و بدون Spec)
- مسیر رسمی: Hermes create → productId **8**؛ `GET /api/Products/getspecifications/8` همان سه Spec؛ جستجوی عمومی خالی
- **عمداً خارج:** Gate C، persona، P020 تست دوپیام، push به Docker Hub

### P020 — جزئیات اجرا (2026-08-16)

قانون سخت تأیید قبل از write. persona / Gate C / بک‌اند را دست نزدم.

- SKILL: بخش `Two-message protocol (P020)` — turn1 فقط خلاصه؛ turn2 بعد از `تأیید`/`بله`/`ok` یک write
- SOUL فارسی همان قانون؛ روی `/root/.hermes/SOUL.md` و کانتینر Hermes recreate شد
- تست: chat Hermes برای تغییر قیمت آفر ۷ → خلاصه+تأیید، بدون اسکریپت write، قیمت DB همان ۱۰۰۰۰
- بعد از تأیید شبیه‌سازی‌شده: `update-offer.sh --price 10001` سبز؛ سپس restore به ۱۰۰۰۰
- **عمداً خارج:** Gate C، persona، P022، دیپلوی بک‌اند

### Persona تلگرام — 2026-08-15 (خارج از P011)

شواهد مالک: بات خود را LFM معرفی می‌کرد و به سلام انگلیسی جواب می‌داد.

- `/root/.hermes/SOUL.md` از هویت پیش‌فرض Hermes/Nous به دستیار مالک فروشگاه Rooshak عوض شد (کپی در `automation/hermes/SOUL.md`)
- `agent.system_prompt` و `platform_hints.telegram.append` همان نقش و فارسی پیش‌فرض
- نشست DM تلگرام قدیمی آرشیو شد؛ کانتینر `shop-hermes-prod` فقط restart شد
- **دست نخورده:** n8n، toolset تلگرام، STT، کاتالوگ، P011
- دور ۱: هویت LFM. دور ۲: `Hello! سلام!`. دور ۳: مالک تأیید کرد جواب درست است → **A1 امضا**

---

## Gate A — چک‌لیست (وقتی P008)

- [x] مالک از تلگرام سلام می‌فرستد و جواب فارسی نقش فروشگاه می‌گیرد — P008 A1 (تأیید مالک ۱۵ اوت شب)
- [x] یوزر غیرمجاز تلگرام رد می‌شود — P008 (allowlist یک کاربر؛ fail-closed؛ بدون GATEWAY_ALLOW_ALL)
- [x] از همان چت نمی‌توان دستور مخرب شل زد — P006/P008 (`has_terminal=False`)
- [x] داشبورد Hermes از اینترنت عمومی باز نمی‌شود (`127.0.0.1`) — P003/P008 (ECONNREFUSED روی 9119/8642)
- [x] unit قدیمی systemd masked است — P004

## Gate B — چک‌لیست (P015)

- [x] عکس + کپشن → خلاصه → «تأیید» → محصول با Offer و عکس — مسیر SKILL (تأیید قبل از write) + create از Hermes با فایل عکس؛ پیش‌نویس productId 7
- [x] `IsActive=false` ؛ در لیست و جستجوی عمومی نیست
- [x] اگر Offer/عکس خطا بدهد، Product یتیم نماند (یک تراکنش؛ تست واحد)

## Gate C — چک‌لیست (P021)

- [x] لوله آماده (Hermes/n8n create/`X-Api-Key`/دوپیام/taxonomy) — 2026-08-16
- [ ] یک ظرف کریستال واقعی با قیمت/موجودی/دسته درست (از تلگرام؛ نه id ۷/۸)
- [ ] بدون لاگین پنل ادمین
- [ ] verify: `IsActive=false` + Offer + نه در جستجوی عمومی + `productId` در REPORT

## Gate D — چک‌لیست (P026)

- [ ] Gate C امضا شده (پیش‌نیاز)
- [ ] تاریخ شروع هفته ثبت شده در EXECUTION_REPORT
- [ ] یک هفته ورود کالای روزمره از تلگرام (لاگ روزانه مالک)
- [ ] پنل فقط برای کار دقیق (SEO، چند عکس، تخفیف پیچیده)
- [ ] امضای نهایی Gate D در EXECUTION_REPORT

---

## Ops (کار تو — `USER_TODO.md`)

- [ ] P002 چرخش رمز SSH و Hermes — **Deferred در توسعه**؛ قبل از prod اجباری
- [ ] کلید SSH به‌جای پسورد root
- [x] کلید OpenRouter جدید (P005) — روی VPS معتبر است؛ در چت نفرست
- [ ] هرگز رمز/توکن را در چت نگذار

---

## تصمیم معماری قفل‌شده

```
مالک (Telegram allowlist)
        → Hermes (بدون terminal آزاد)
            → READ: ASP.NET GET
            → WRITE: n8n webhook + secret
                → CreateCatalogItem (P009)
                    → PostgreSQL
```

n8n جدا برای cron: دایجست سفارش (P023)، هشدار موجودی کم (P022).
