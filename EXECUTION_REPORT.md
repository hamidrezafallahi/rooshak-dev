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
فاز: P011 را طبق ROADMAP شروع کن. فقط همین فاز؛ بدون scope creep.
```

| بگو | یعنی |
|-----|------|
| `فاز: P00X را طبق ROADMAP شروع کن.` | فقط همان فاز |
| `فاز: P00X را طبق ROADMAP شروع کن. فقط همین فاز؛ بدون scope creep.` | همان + جلوگیری از کار اضافه |
| `P002 را در توسعه skip کن` | چرخش رمز را عقب بینداز (الان انجام شد) |

نیازی نیست کل ROADMAP را دوباره بفرستی.

---

## TODO — صف ادامه کار (همین را به Agent بگو)

> به‌روز شده: **2026-08-15 شب — کار بسته شد.**  
> **الان کجا هستیم:** P001–P010 Done · P002 Deferred · **P008 Gate A = 5/5**. بعدی فردا: **P011**. ساخت کالا از تلگرام تا Gate B ممنوع است.

**جملهٔ آماده برای چت فردا:**

```
فاز: P011 را طبق ROADMAP شروع کن. فقط همین فاز؛ بدون scope creep.
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
| W1+ | P011–P026 | Not started | **بعدی P011** |

---

## تحویل به فردا — 2026-08-16

کار ۱۵ اوت این‌جا متوقف شد. فردا **یک فاز** را ادامه بده؛ گزارش را از نو ننویس.

### حکم امشب

W0 Harden بسته است (Gate A = 5/5). مسیر Application کاتالوگ در ریپو درست است (P009+P010) و هنوز روی VPS deploy نشده. n8n هنوز سه HTTP قدیمی می‌زند. از تلگرام کالا نساز تا Gate B.

| موج | Done | باقی |
|-----|------|------|
| W0 Harden P001–P008 | P001, P003–P008 | فقط P002 Deferred |
| W1 Catalog P009–P015 | P009, P010 | **بعدی P011** سپس P012–P015 |
| W2 / W3 | هیچ | P016–P026 |

### فردا Agent چه کار کند

فقط **P011**: ورک‌فلو n8n create را به `POST /api/CatalogItems` ببر. API key نساز (P012). Hermes/تلگرام/persona را دست نزن مگر رگرسیون واضح باشد.

پرامپت:

```
فاز: P011 را طبق ROADMAP شروع کن. فقط همین فاز؛ بدون scope creep.
```

### فردا مالک چه کار کند

- همان پرامپت P011 را در چت بفرستد.
- رمز/توکن در چت نگذارد.
- P002 را هنوز skip نگه دارد مگر تصمیم به prod.
- از تلگرام کالا نسازد.

### آنچه نباید فردا تکرار شود

- Gate A و persona تلگرام بسته شد؛ بازنویسی نکن.
- bind Hermes و mask systemd انجام شده‌اند.
- `CreateCatalogItem` و `IsActive=false` روی همان مسیر نوشته شده؛ بازنویسی نکن.
- محصول واقعی از تلگرام تا Gate B (P015) نساز.
- مدل env `llama-3.3-70b-instruct:free` هنوز 404 است — عوض کردنش کار P011 نیست.

### فایل‌های کد P009/P010 (در ریپو؛ روی VPS deploy نشده)

- `BackEnd/Application/Commands/CreateCatalogItemCommand.cs`
- `BackEnd/Application/Handler/CommandHandler/CreateCatalogItemCommandHandler.cs`
- `BackEnd/Api/Controllers/CatalogItemsController.cs` → `POST /api/CatalogItems`
- `BackEnd/Infrastructure/Persistence/EfUnitOfWork.cs`
- `BackEnd/Application.Tests` — ۴ تست سبز

n8n روی VPS هنوز مسیر قدیمی را می‌زند. تا P011 و deploy بک‌اند، این کد روی prod زنده نیست.

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
| پیش‌نویس قبل از انتشار | **درست برای CreateCatalogItem (P010):** Product+Offer+Image با `IsActive=false`. پنل `CreateProduct` و پیش‌فرض `BaseEntity` هنوز true؛ n8n هنوز به این endpoint وصل نیست (P011) |
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
- n8n الان ۳ فراخوانی می‌کند: login با ایمیل/رمز → Products → ProductImages → ProductOffers. اگر Offer بشکند، محصول یتیم می‌ماند.
- `IsCatalogStaff()` شامل ContentEditor است؛ README قبلی 403 روی Offer را گزارش کرده — باید در P016 با تست واقعی بسته شود.
- ابعاد در ورک‌فلو create صفر سخت‌کد شده؛ برای ظروف کافی نیست.

### شکاف Hermes

- کلید OpenRouter در P005 دیگر 401 نیست.
- اسکیل تکراری: `skills/shop-owner` (قدیمی، `127.0.0.1`) و `skills/shop/shop-owner` (جدید، اسکریپت کامل).
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

- [ ] عکس + کپشن → خلاصه → «تأیید» → محصول با Offer و عکس
- [ ] `IsActive=false` ؛ در کاتالوگ عمومی نیست
- [ ] اگر Offer خطا بدهد، Product یتیم نماند (یک تراکنش)

## Gate C — چک‌لیست (P021)

- [ ] یک ظرف کریستال واقعی با قیمت/موجودی/دسته درست
- [ ] بدون لاگین پنل ادمین

## Gate D — چک‌لیست (P026)

- [ ] یک هفته ورود کالای روزمره از تلگرام
- [ ] پنل فقط برای کار دقیق (SEO، چند عکس، تخفیف پیچیده)

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

n8n جدا برای cron: دایجست سفارش، بعداً موجودی کم.
