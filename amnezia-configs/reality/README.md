# Reality (XRay) با بالاترین مقاومت در برابر Active Probing / DPI

هیچ کلید یا UUID واقعی اینجا نیست؛ با دستورهای زیر روی سرور تولید کن.

## ۱) نصب از داخل اپ Amnezia (روش پیشنهادی)

1. در اپ: **سرور ← Add container ← XRay (VLESS Reality)**
2. **Port = 443** (TCP). پورت غیر 443 در ایران مشکوک‌تر است.
3. **Site name to mimic (SNI)**: پیش‌فرض (`googletagmanager.com`) را عوض کن. معیارها:
   - سایتی که TLS 1.3 + H2 دارد و واقعاً از IP سرور قابل دسترسی است
   - ترجیحاً در همان دیتاسنتر/ASN (Hetzner) یا هم‌منطقه، تا IP و SNI با هم همخوان باشند
   - در ایران مسدود نباشد و ترافیک عادی داشته باشد
   - آزمایش: `openssl s_client -connect <site>:443 -tls1_3 -alpn h2 </dev/null`
   - مثال‌های متداول: `www.microsoft.com`، `www.samsung.com`، `dl.google.com`. بهتر است چند گزینه را با
     `xray tls ping <site>` از روی سرور بسنجی و یکی را انتخاب کنی.
4. Connect و کلید `vpn://` را روی گوشی/لپ‌تاپ وارد کن.

## ۲) سخت‌سازی (بعد از نصب، داخل کانتینر `amnezia-xray`)

فایل: `/opt/amnezia/xray/server.json`. مقادیر کلیدی که باید باشند (نمونه: `server.template.json`):

| مورد | مقدار | دلیل |
|---|---|---|
| flow | `xtls-rprx-vision` | حذف الگوی TLS-in-TLS که DPI با آن تشخیص می‌دهد |
| security | `reality` | هر probe غیرمجاز به سایت واقعی `dest` فوروارد می‌شود |
| dest | `<SNI>:443` | همان سایت انتخابی |
| serverNames | فقط همان SNI | |
| shortIds | یک مقدار تصادفی 16 hex (و `""` را حذف کن) | کسی بدون shortId نمی‌تواند پروکسی را تشخیص دهد |
| fingerprint (کلاینت) | `chrome` | uTLS شبیه ClientHello مرورگر |
| ALPN | `h2,http/1.1` | |

تولید کلید و شناسه‌ها:

```bash
docker exec amnezia-xray xray x25519          # PrivateKey (سرور) و PublicKey (کلاینت)
docker exec amnezia-xray xray uuid
openssl rand -hex 8                           # shortId
```

بعد از تغییر: `docker restart amnezia-xray`

## ۳) سخت‌سازی سرور در برابر probing

- روی 443 فقط XRay گوش کند. هیچ سرویس دیگری (وب‌سرور/پنل) روی همان IP:443 نباشد.
- پورت‌های اضافی (SSH روی 22 و ...) را با فایروال محدود کن، مثلاً SSH فقط با کلید و پورت غیر استاندارد:
  ```bash
  ufw default deny incoming
  ufw allow 443/tcp
  ufw allow <SSH_PORT>/tcp
  ufw enable
  ```
- پاسخ ICMP را می‌توان بست؛ ping نباید لازم باشد.
- کانتینر AmneziaWG فعلی (`39900/udp`) را اگر لازم نیست ببند. پورت UDP با پارامترهای WG قابل اثرانگشت است.

## ۴) تنظیم کلاینت برای حداکثر عبور

- Fragment (در v2rayNG/Hiddify/Streisand): `tlshello`، طول `100-200`، فاصله `10-20` ms
- `MTU` را 1280-1400 نگه دار
- Mux را خاموش کن (با Vision سازگار نیست)
- اگر IP سرور خودش بلاک شد، هیچ تنظیمی کمک نمی‌کند → از CDN-based (XHTTP پشت Cloudflare) یا IP جدید استفاده کن

## محدودیت‌های صادقانه

- Reality از probing مقاوم است، اما **IP/ASN دیتاسنتر** (Hetzner) و رفتار حجم/زمان ترافیک هنوز قابل شناسایی است.
- «۱۰۰٪ غیرقابل شناسایی» وجود ندارد؛ این پیکربندی بهترین ترکیب عملی فعلی است.
