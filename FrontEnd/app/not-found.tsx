import Link from 'next/link';

/** Global 404 — rendered outside the locale layout, so copy is bilingual. */
export default function NotFound() {
  return (
    <main className="flex flex-col justify-center items-center gap-6 mx-auto px-4 py-32 min-h-screen max-w-xl text-center">
      <p className="text-store-subtle text-sm tracking-[0.3em]">404</p>
      <h1 className="font-normal text-3xl sm:text-5xl leading-tight">
        صفحه پیدا نشد
        <span className="block mt-2 text-store-subtle text-xl sm:text-2xl">Page not found</span>
      </h1>
      <p className="text-store-subtle text-sm leading-relaxed">
        این صفحه وجود ندارد یا پاک شده است. · The page you are looking for does not exist.
      </p>
      <div className="flex sm:flex-row flex-col gap-3 w-full sm:w-auto">
        <Link
          href="/fa"
          className="inline-flex justify-center items-center bg-primary hover:bg-transparent px-8 py-3 border border-primary text-primary-foreground hover:text-store-text text-sm transition-colors"
        >
          صفحه اصلی
        </Link>
        <Link
          href="/en"
          className="inline-flex justify-center items-center hover:bg-primary px-8 py-3 border border-store-strong text-store-text hover:text-primary-foreground text-sm transition-colors"
        >
          Home
        </Link>
      </div>
    </main>
  );
}
