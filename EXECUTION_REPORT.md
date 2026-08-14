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
فاز: P004 را طبق ROADMAP شروع کن. فقط همین فاز؛ بدون scope creep.
```

| بگو | یعنی |
|-----|------|
| `فاز: P00X را طبق ROADMAP شروع کن.` | فقط همان فاز |
| `فاز: P00X را طبق ROADMAP شروع کن. فقط همین فاز؛ بدون scope creep.` | همان + جلوگیری از کار اضافه |
| `P002 را در توسعه skip کن` | چرخش رمز را عقب بینداز (الان انجام شد) |

نیازی نیست کل ROADMAP را دوباره بفرستی.

---

## TODO — صف ادامه کار (همین را به Agent بگو)

> به‌روز شده: **2026-08-14**.  
> **الان کجا هستیم:** P001 Done · P002 **Deferred (توسعه)** · **P003 Done** (Hermes فقط روی 127.0.0.1). بعدی **P004**. LLM هنوز 401 است.

**جملهٔ آماده برای چت بعدی:**

```
فاز: P004 را طبق ROADMAP شروع کن. فقط همین فاز؛ بدون scope creep.
```

| ترتیب | فاز | وضعیت | Dep |
|-------|-----|--------|-----|
| W0-1 | P001 اسناد ROADMAP/REPORT | **Done** | — |
| W0-2 | P002 چرخش اسرار | **Deferred (توسعه)** — قبل از prod اجباری | P001 ✓ |
| W0-3 | P003 bind Hermes روی 127.0.0.1 | **Done** (2026-08-14) | P001 ✓ |
| W0-4 | **P004 mask systemd hermes-gateway** | **Not started** ← بعدی | P001 ✓ |
| W0-5 | P005 کلید OpenRouter | Not started (Blocked: 401) | P002 deferred؛ با کلید فعلی تست می‌شود |
| W0-6 | P006 حذف terminal از تلگرام | Not started | P005 |
| W0-7 | P007 STT فارسی | Not started | P005 |
| W0-8 | P008 Gate A | Not started | P003–P007 |
| W1+ | P009–P026 | Not started | بعد از Gate A |

---

## AUDIT — واقعیت 2026-08-14

### حکم رک

**لوله وجود دارد؛ برای مالک هنوز قابل استفاده نیست.**  
Hermes داخل Docker کنار n8n است و به API فروشگاه از شبکهٔ داخلی می‌رسد. n8n ورک‌فلوهای ساخت محصول / قیمت / فعال / سفارش را فعال کرده. ولی مدل OpenRouter `401 User not found` می‌دهد، تلگرام هنوز toolset `terminal` دارد، و ساخت محصول در دامنهٔ فعلی `IsActive=true` است (بعد از تأیید، کالا همان لحظه روی سایت می‌آید).

| ادعای معماری | واقعیت |
|---------------|--------|
| Telegram → Hermes → n8n → API → Postgres | **نیمه‌کاره:** مسیر فایل‌ها و ورک‌فلو هست؛ LLM خراب |
| Agent به DB وصل نیست | **درست** (اسکریپت‌ها webhook می‌زنند) |
| Agent به شل وصل نیست | **غلط:** `platform_toolsets.telegram = [terminal]` |
| پیش‌نویس قبل از انتشار | **غلط:** `BaseEntity.IsActive` پیش‌فرض true؛ n8n Active=false نمی‌زند |
| API جدا `/automation` | **درست که نیست** — از Products/Offers موجود استفاده می‌شود |
| Hermes روی root هاست | **درست که نیست** — کانتینر `shop-hermes-prod`؛ unit قدیمی failed/disabled |

### سرور

| مورد | مقدار |
|------|--------|
| Host | `ubuntu-4gb-hel1-2` — uptime حدود ۲ روز |
| فروشگاه | frontend + backend healthy؛ nginx 80/443 |
| n8n | `127.0.0.1:5678` — خوب |
| Hermes dashboard/API | listen روی `0.0.0.0:9119` و `8642` |
| UFW | default deny؛ فقط 22/80/443 باز — پورت Hermes از اینترنت باید بسته باشد |
| VPN | `amnezia-awg2` روی UDP 30666 |
| systemd | `hermes-gateway.service` = failed + disabled (باقی‌مانده) |

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

- کلید OpenRouter رد می‌شود.
- اسکیل تکراری: `skills/shop-owner` (قدیمی، `127.0.0.1`) و `skills/shop/shop-owner` (جدید، اسکریپت کامل).
- STT `language: en`.
- `TELEGRAM_ALLOWED_USERS` روی env ست است (یک کاربر).

---

## انجام‌شده — تکرار نکن مگر رگرسیون

- [x] P001 اسناد ROADMAP + EXECUTION_REPORT
- [x] P003 bind داشبورد/API هرمس روی `127.0.0.1:9119` و `127.0.0.1:8642`؛ n8n دست نخورده ماند
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

---

## Gate A — چک‌لیست (وقتی P008)

- [ ] مالک از تلگرام یک سلام می‌فرستد و جواب فارسی می‌گیرد (دیگر 401 نیست)
- [ ] یوزر غیرمجاز تلگرام رد می‌شود
- [ ] از همان چت نمی‌توان دستور مخرب شل زد
- [x] داشبورد Hermes از اینترنت عمومی باز نمی‌شود (`127.0.0.1`) — P003
- [ ] unit قدیمی systemd masked است

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
- [ ] کلید OpenRouter جدید (P005)
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
