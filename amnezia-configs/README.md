# کانفیگ Amnezia برای گوشی

سرور: `65.109.212.237` — پورت `39900/udp` — پروتکل **AmneziaWG 3.1** (کانتینر `amnezia-awg2`)

Peer جدید ساخته شده:

| مورد | مقدار |
|---|---|
| نام | `Phone` |
| IP داخل تونل | `10.8.1.3/32` |
| PublicKey | `I4ExMEFR7ixHjKMmzzWCBkS1lKMD0PbNKC+CbPgx+RQ=` |
| AllowedIPs | `0.0.0.0/0` (تمام ترافیک از VPN) |

## فایل‌ها

- **`phone-awg.conf`** ← فایل اصلی، همین را به گوشی انتقال بده
- **`phone-vpn-link.txt`** ← لینک `vpn://` (جایگزین، اگر ایمپورت فایل کار نکرد)
- **`gen-vpn-uri.mjs`** ← اسکریپت تولید لینک از روی فایل conf

## نصب روی گوشی

### روش ۱ — فایل `.conf`
1. مطمئن شو **AmneziaVPN نسخه ۵.۰.۲.۱ یا جدیدتر** باشد (حداقل ۵.۰.۱.۵).
   نسخه‌های قدیمی‌تر پارامترهای HeaderProtectionKey و بقیه پارامترهای AWG 3.x را
   موقع ایمپورت `.conf` بی‌صدا حذف می‌کنند و اتصال روی «در حال اتصال» می‌ماند.
2. فایل `phone-awg.conf` را به گوشی بفرست (تلگرام/ایمیل/کابل).
3. در اپ: **➕ → Connection settings file → فایل را انتخاب کن → Continue → Connect**.

### روش ۲ — لینک `vpn://` (مطمئن‌تر در نسخه‌های قدیمی)
1. متن داخل `phone-vpn-link.txt` را کپی کن.
2. در اپ: **➕ → Enter key → چسباندن لینک `vpn://` → Connect**.

نکته: اگر QR می‌سازی، حتماً **محتوای لینک `vpn://`** را در QR قرار بده،
نه متن داخل فایل `.conf` (QR ساخته‌شده از `.conf` همان باگ را دارد).

## حذف Peer گوشی (اگر لازم شد)

```bash
docker exec amnezia-awg2 awg set awg0 peer I4ExMEFR7ixHjKMmzzWCBkS1lKMD0PbNKC+CbPgx+RQ= remove
```

و بلوک `[Peer]` مربوطه را از `/opt/amnezia/awg/awg0.conf` داخل کانتینر و ورودی `Phone`
را از `clientsTable` پاک کن.

## بکاپ

قبل از هر تغییری، نسخهٔ بکاپ با تاریخ ساخته شد:
`awg0.conf.bak-20261008-081519` و `clientsTable.bak-20261008-081519`

⚠️ کانفیگ فقط داخل لایهٔ نوشتنی کانتینر است ( mount نشده)؛ با `docker rm` پاک می‌شود.
