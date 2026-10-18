# گزارش کار: نمایش سه‌بعدی و AR محصول (Rooshak)

**شاخه:** `feature/product-3d-ar` (از `master`، هنوز commit نشده)
**وضعیت:** پیاده‌سازی شده و تست‌شده تا حد توضیح‌داده‌شده در بخش «وضعیت تست». **روی Luca هنوز اجرا نشده.**
**هدف این سند:** وقتی تست‌های واقعی روی Rooshak تمام شد، همین سند را به یک agent (یا Claude) بدهید تا همان تغییرات را روی `luca-dev` اعمال کند.

---

## ۱. خلاصه‌ی قابلیت‌ها

1. **مدل سه‌بعدی هر محصول** (GLB یا glTF تک‌فایلی، و USDZ اختیاری برای iOS) در جدول جداگانه ذخیره می‌شود.
2. **پنل ادمین:** در صفحه‌ی ویرایش محصول (`/admin/products/{id}`)، زیر فرم محصول، بخش «مدل سه‌بعدی و اسکن» اضافه شده: آپلود/حذف GLB و USDZ، پیش‌نمایش سه‌بعدی، و **اسکن با گوشی**.
3. **اسکن با گوشی:** دکمه‌های «گرفتن عکس» و «ضبط ویدیو» (دوربین گوشی را مستقیم باز می‌کنند)، «انتخاب از گالری»، و «ادامه با گوشی (QR)» که صفحه‌ی ادمین را روی گوشی باز می‌کند. فایل‌ها به‌عنوان *ورودی خام اسکن* ذخیره می‌شوند.
4. **صفحه‌ی محصول (مشتری):** اگر محصول مدل داشته باشد، کنار گالری دو دکمه ظاهر می‌شود: «نمایش سه‌بعدی» و «مشاهده روی میز (واقعیت افزوده)».
   - موبایل: AR با دوربین (Android: WebXR/Scene Viewer، iPhone: AR Quick Look).
   - دسکتاپ: دیالوگ **QR** تا مشتری با گوشی ادامه دهد (صفحه با `?view=3d` باز می‌شود).
5. **سرعت:** کتابخانه‌ی `model-viewer`/`three.js` فقط پس از کلیک مشتری (dynamic import) دانلود می‌شود؛ روی LCP و بارگذاری اولیه‌ی صفحه‌ی محصول اثری ندارد.

### محدودیت‌های شناخته‌شده (صادقانه)
- **تبدیل خودکار اسکن به GLB انجام نمی‌شود.** هیچ سرویس photogrammetry/AI داخل پروژه نیست. جریان کار: عکس/ویدیو با گوشی ← آپلود در ادمین ← اسکن با RealityScan/Polycam/KIRI Engine (بیرون از سایت) ← دانلود GLB ← آپلود در بخش بالای همان پنل. دلیل: این ابزارها API عمومی ندارند یا پولی/سنگین‌اند و تصمیم محصولی/هزینه‌ای می‌خواهند.
- فرمت‌هایی مثل OBJ/FBX/STL **پذیرفته نمی‌شوند** (مرورگر و AR نمی‌توانند مستقیم نشان دهند؛ باید قبلاً به GLB تبدیل شوند).
- ابعاد AR از **خود فایل GLB** می‌آید (واحد متر). فیلد `Dimensions` محصول عمداً برای مقیاس استفاده نشد چون واحدش (cm/mm) مستند نیست. مدل باید با اندازه‌ی واقعی ساخته شود.
- حد حجم: مدل و USDZ هر کدام ۲۵MB؛ هر فایل اسکن ۵۰MB (سقف فعلی Kestrel/nginx).
- شیشه/کریستال با اسکن گوشی خوب درنمی‌آید؛ برای آن مدل‌سازی (Blender) توصیه می‌شود. شفافیت در iOS Quick Look ساده‌تر نمایش داده می‌شود.

---

## ۲. فایل‌های تغییر کرده / اضافه‌شده

### بک‌اند (`BackEnd/`)
| فایل | کار |
|---|---|
| `Domain/Entities/ProductModel3D.cs` (جدید) | موجودیت مدل: `ProductId`, `ModelUrl`, `ModelSizeBytes`, `UsdzUrl?`, `UsdzSizeBytes?` |
| `Domain/Entities/ProductScanSource.cs` (جدید) | موجودیت فایل خام اسکن: `ProductId`, `FileUrl`, `Kind` (image/video), `SizeBytes` |
| `Domain/Interfaces/IProductModel3DRepository.cs` (جدید) | دو اینترفیس ریپازیتوری |
| `Infrastructure/Repository/ProductModel3DRepository.cs` (جدید) | پیاده‌سازی ریپازیتوری‌ها |
| `Infrastructure/Persistence/Configurations/ProductModel3DConfiguration.cs` (جدید) | کانفیگ EF؛ **ایندکس یکتای فیلترشده** (`ProductId` وقتی `IsDeleted=false`) تا هر محصول یک مدل زنده داشته باشد |
| `Infrastructure/Migrations/20261008074503_AddProductModel3D*.cs` (جدید) | مایگریشن: فقط ایجاد ۲ جدول `ProductModels3D` و `ProductScanSources` (به‌صورت بررسی‌شده بدون تغییر در جداول دیگر). همراه `AppDbContextModelSnapshot.cs` |
| `Infrastructure/Persistence/AppDbContext.cs` | دو `DbSet` جدید |
| `Infrastructure/Persistence/DependencyInjection.cs` | ثبت ریپازیتوری‌ها و `IModelFileStorage` |
| `Application/Common/Interfaces/IModelFileStorage.cs` (جدید) | اینترفیس ذخیره‌ی فایل مدل/اسکن. *عمداً جدا از `IUploaderService`* تا `FakeUploader` در تست‌های موجود نشکند |
| `Infrastructure/Services/UploaderServices/ModelFileStorage.cs` (جدید) | ذخیره **بدون تبدیل** + اعتبارسنجی: امضای `glTF` و نسخه ۲ برای GLB؛ JSON معتبر و بدون وابستگی خارجی برای glTF؛ امضای zip (`PK\x03\x04`) برای USDZ؛ نوع فایل اسکن (jpg/png/webp/heic، mp4/mov/webm)؛ حد حجم |
| `Application/Common/UploadPaths.cs` | `ProductModels(id)` ← `uploads/products/{id}/models` ، `ProductScans(id)` ← `uploads/products/{id}/scans` |
| `Application/Dtos/ProductModel3DDtos.cs` (جدید) | `ProductModel3DPublicDto`, `ProductScanSourceDto`, `ProductModel3DAdminDto` |
| `Application/Commands/ProductModel3DCommands.cs` (جدید) | Upsert / Delete مدل، افزودن/حذف فایل اسکن |
| `Application/Queries/ProductModel3DQueries.cs` (جدید) | کوئری ادمین |
| `Application/Handler/CommandHandler/ProductModel3DCommandsHandler.cs` (جدید) | همه‌ی هندلرها. فایل جدید اول ذخیره می‌شود، بعد ردیف DB، و **آخر** فایل قدیمی پاک می‌شود |
| `Api/Controllers/ProductModels3DController.cs` (جدید) | `GET/PUT/DELETE api/ProductModels3D/{productId}` ، `POST …/{productId}/scan-sources` ، `DELETE …/scan-sources/{id}` ؛ نقش‌ها: `SuperAdmin,Admin,ContentEditor` |
| `Application/Dtos/ProductDtos.cs` | فیلد `Model3D` (`ProductModel3DPublicDto?`) به `ProductByDetailDto` (فقط صفحه‌ی جزئیات؛ کارت‌ها/لیست‌ها دست نخوردند) |
| `Application/Handler/QueryHandler/ProductQueryHandler.cs` | تزریق `IProductModel3DRepository` و پر کردن `Model3D` در `GetProductByIdQuery` |
| `Api/Program.cs` | `UseStaticFiles` با `FileExtensionContentTypeProvider` برای `.glb/.gltf/.usdz` (بدون آن ASP.NET این پسوندها را ۴۰۴ می‌دهد؛ برای dev که بک‌اند فایل را سرو می‌کند) |
| `Application.Tests/ProductModel3DTests.cs` (جدید) + `Application.Tests.csproj` | ۱۳ تست؛ به csproj رفرنس `Infrastructure` اضافه شد |

### فرانت (`FrontEnd/`)
| فایل | کار |
|---|---|
| `package.json`, `yarn.lock` | `@google/model-viewer@^4.3.1` ، **`three@^0.183.0`** (peer dependency الزامی؛ بدون آن build/dev با `Can't resolve 'three'` خطا می‌دهد) ، `@types/three` (dev) |
| `types/model-viewer.d.ts` (جدید) | تایپ JSX برای `<model-viewer>` (ماژول‌augmentation برای React 19) |
| `models/product.ts` | `model3D?` در `IDetailedProduct` + تایپ‌های `IProductModel3D`, `IProductScanSource`, `IProductModel3DAdmin` |
| `components/organisms/productOrganisms/productHero/product3DViewer.tsx` (جدید) | wrapper مدل‌ویوئر، بارگذاری lazy، درخواست AR، حالت خطا/لودینگ |
| `components/organisms/productOrganisms/productHero/productGallery.tsx` | دکمه‌ها و جابه‌جایی عکس/سه‌بعدی، `?view=3d`، دیالوگ QR (viewer با `next/dynamic` و `ssr:false`) |
| `components/molecules/qrDialog/index.tsx` (جدید) | دیالوگ QR (از `qrcode` که از قبل نصب بود) |
| `components/organisms/productOrganisms/product3DAdminPanel/index.tsx` (جدید) | بخش مدیریت در ادمین |
| `app/[locale]/admin/[field]/[id]/page.tsx` | وقتی `field === 'products'`، پنل را زیر `FormGenerator` نشان می‌دهد |
| `messages/fa.json`, `messages/en.json` | کلیدهای `model3d.*` و `model3dAdmin.*` |
| `middleware.ts` | `glb|gltf|usdz` به لیست پسوندهای مستثنا از middleware زبان اضافه شد (فایل‌های public با این پسوند دیگر به `/fa/...` ریدایرکت نمی‌شوند). مسیر `uploads/*` از قبل مستثنا بود |

### زیرساخت
| فایل | کار |
|---|---|
| `docker/nginx/nginx.conf` | بلوک `types { model/gltf-binary glb; model/gltf+json gltf; model/vnd.usdz+zip usdz; }` بعد از `include mime.types` (بدون آن iOS Quick Look فایل را قبول نمی‌کند). بلوک جداگانه‌ای داخل `location` گذاشته نشد چون `types` در location کل mime.types را override می‌کند |

---

## ۳. وضعیت تست

| مورد | نتیجه |
|---|---|
| `dotnet build OnlineShop.sln` | ✅ ۰ خطا |
| `dotnet test Application.Tests` | ✅ ۴۶/۴۶ (۱۳ تست جدید) |
| مایگریشن EF | ✅ ساخته شد؛ فقط `CreateTable`/`CreateIndex` |
| `tsc --noEmit` (فرانت) | ✅ بدون خطا |
| ESLint روی فایل‌های جدید | ✅ ۰ خطا |
| viewer در مرورگر (dev server، GLB تستی ۱۲×۲۰×۱۲ سانتی‌متر) | ✅ مدل رندر و لود شد؛ `getDimensions()` دقیقاً `0.12 × 0.20 × 0.12` متر (مقیاس واقعی حفظ می‌شود) |
| دکمه‌ی AR روی دسکتاپ | ✅ `canActivateAR=false` ← دیالوگ QR با تصویر QR نمایش داده شد |
| پنل ادمین (UI) در RTL | ✅ رندر درست؛ آپلود مدل، پیش‌نمایش، آپلود عکس با input دارای `capture=environment`، نمایش thumbnail — **در برابر backend ساختگی (mock)** |

### آنچه تست نشده و باید شما بررسی کنید
1. **اجرای واقعی بک‌اند + PostgreSQL:** روی این ماشین Docker/PostgreSQL نبود. پس مایگریشن روی دیتابیس واقعی اجرا نشده، و endpoint‌های جدید و ذخیره‌ی فایل با HTTP واقعی دیده نشده‌اند (فقط build + تست واحد). **اولین کار:** `docker compose -f docker-compose.dev.yml up`، ورود به ادمین، آپلود یک GLB واقعی.
2. **AR واقعی روی گوشی:** در محیط من گوشی نبود. باید روی یک Android (Chrome) و یک iPhone (Safari) امتحان شود. بررسی‌های مهم iPhone: اگر USDZ آپلود نشود model-viewer خودش آن را از GLB می‌سازد؛ شفافیت کریستال در Quick Look ساده‌تر نمایش داده می‌شود.
3. **سرو فایل در production:** nginx باید پس از تغییر reload/rebuild شود؛ سپس `curl -I https://<domain>/uploads/products/<id>/models/<file>.glb` باید `Content-Type: model/gltf-binary` بدهد.
4. **ورودی دوربین گوشی در ادمین** (`capture`) فقط روی موبایل واقعی دوربین را باز می‌کند.
5. **مسیر دیپلوی:** `deploy.sh` و `/opt/shop` روی VPS بررسی نشد؛ اگر `yarn install` با lockfile انجام می‌شود، `yarn.lock` به‌روز شده است.

---

## ۴. دستور انتقال به Luca (برای agent بعدی)

پس از تأیید تست‌ها روی Rooshak، این‌ها را در `C:\fallahi\luca-dev` اعمال کنید (ساختار دو پروژه یکسان است و `Domain/Entities/Product.cs` بعد از حذف تفاوت پایان خطوط یکسان بود؛ ولی چند موجودیت دیگر مثل `Brand.cs`/`Category.cs` تفاوت دارند و بررسی نشدند، پس پیش از کپی، diff را ببینید):

1. روی شاخه‌ی جدید در `luca-dev` همان تغییرات §۲ را اعمال کنید (می‌توان `git diff master...feature/product-3d-ar` را از Rooshak با `git apply --3way` استفاده کرد، یا فایل‌ها را کپی کرد). **نام فایل مایگریشن و snapshot را کپی نکنید؛** در Luca دوباره بسازید: `dotnet ef migrations add AddProductModel3D -p Infrastructure -s Api` چون snapshot دو پروژه ممکن است متفاوت باشد.
2. `yarn add @google/model-viewer three@^0.183.0` و `yarn add -D @types/three@^0.183.0`.
3. تفاوت‌های Luca که باید رعایت شوند:
   - طراحی Luca «مربع، بدون سایه، بدون border-radius» است (`LUCA_MIGRATION.md`). کلاس‌های دکمه‌ها در `productGallery.tsx` و `qrDialog` و پنل ادمین از توکن‌های `store-*` استفاده می‌کنند؛ ظاهر را با `luca-chip`/`luca-badge` یا کلاس‌های Luca هماهنگ کنید.
   - `next.config.ts` در Luca ممکن است hostnameهای متفاوتی داشته باشد (دست‌زدن لازم نیست؛ مدل‌ها از `/uploads` می‌آیند).
   - `public/` در Luca ناقص است (`LUCA_MIGRATION.md` بند ۱)؛ `images/default-product.jpg` ممکن است نباشد.
4. **ملاحظه‌ی محصول Luca (لوپ دندانپزشکی):** لوپ روی صورت پوشیده می‌شود، پس «مشاهده روی میز» برای آن ارزش کمی دارد. پیشنهاد: برای Luca متن دکمه را عوض کنید (کلید `model3d.viewOnTable` در `messages/*.json`) یا دکمه‌ی AR را پشت یک پرچم پنهان کنید و فقط «نمایش سه‌بعدی» را نگه دارید.
5. nginx Luca: همان بلوک `types` را به `docker/nginx/nginx.conf` اضافه کنید.
6. همان تست‌های بخش ۳ را تکرار کنید.

---

## ۵. دستورهای مفید

```bash
# بک‌اند
cd BackEnd && dotnet build OnlineShop.sln && dotnet test Application.Tests

# فرانت
cd FrontEnd && yarn install && npx tsc --noEmit -p .

# بهینه‌سازی مدل قبل از آپلود (هدف زیر ۳–۵MB)
npx @gltf-transform/cli optimize in.glb out.glb --compress meshopt --texture-compress webp
```

---

## ۶. افزوده‌ی دوم (شاخه‌ی `feature/3d-real-size`): اندازه‌ی واقعی + راهنمای موبایل

هدف: مالک محصول همه‌چیز را فقط با گوشی انجام دهد.

- **اندازه‌ی واقعی:** در پنل ادمین، پیش‌نمایش مدل همیشه نمایش داده می‌شود و اندازه‌ی فعلی آن (عرض × ارتفاع × عمق به cm) خوانده می‌شود. مالک یک بُعد واقعی (پیش‌فرض: بُعد محصول که در دیتابیس به cm است) را می‌نویسد و «اعمال اندازه» را می‌زند. فرانت ضریب = هدف ÷ اندازه‌ی فعلی را به `POST api/ProductModels3D/{productId}/rescale` می‌فرستد.
- **بک‌اند:** `Application/Common/GltfScaler.cs` فقط تبدیل node‌های ریشه‌ی صحنه (`scale`/`translation`/`matrix`) را در JSON فایل GLB/glTF ویرایش می‌کند؛ داده‌ی vertexها و chunk باینری دست‌نخورده می‌ماند. ضریب باید بین ۰٫۰۰۱ و ۱۰۰۰ باشد. فایل جدید ذخیره می‌شود، ردیف DB عوض می‌شود و فایل قدیمی پاک می‌شود.
- **محدودیت:** USDZ دستی‌آپلودشده تغییر اندازه نمی‌کند (فقط GLB/glTF). اگر USDZ آپلود نشده باشد، iPhone آن را خودکار از GLB اصلاح‌شده می‌سازد.
- **راهنمای موبایل:** بخش «راهنما: ساخت مدل فقط با گوشی (۵ مرحله)» در پنل؛ ورودی فایل GLB حالا `application/octet-stream` را هم می‌پذیرد (برخی انتخابگرهای اندروید فایل GLB را با همین نوع نشان می‌دهند).
- **بدون migration جدید.**

### تست
- ۵۷ تست واحد (۱۱ تست جدید برای `GltfScaler`).
- روی بک‌اند Docker واقعی: ضریب نامعتبر رد شد، بدون لاگین ۴۰۱، و ضریب ۰٫۷۶۸ روی بطری نمونه ارتفاع را از ۲۶٫۰ به ۲۰٫۰ سانتی‌متر رساند (با model-viewer اندازه‌گیری شد؛ فایل قدیمی پاک شد).
- در مرورگر: پنل اندازه‌ی فعلی را خواند و با هدف ۱۰ cm ضریب `0.5` ارسال کرد (با backend ساختگی برای این یک مرحله).
- **تست‌نشده:** ورود واقعی به ادمین از مرورگر و تجربه روی گوشی واقعی.
