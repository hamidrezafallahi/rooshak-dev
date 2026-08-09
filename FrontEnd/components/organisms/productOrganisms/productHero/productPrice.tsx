import { getTranslations } from 'next-intl/server';

type ProductPriceProps = {
  price?: number | null;
  finalPrice?: number | null;
  currency?: string;
  inStock?: boolean;
  inventory?: number;
  locale: string;
};

function formatMoney(value: number, locale: string) {
  return new Intl.NumberFormat(locale === 'fa' ? 'fa-IR' : 'en-US').format(value);
}

export default async function ProductPrice({
  price,
  finalPrice,
  currency = 'IRR',
  inStock,
  inventory,
  locale,
}: ProductPriceProps) {
  if (price == null && finalPrice == null) return null;

  const t = await getTranslations();
  const base = price ?? finalPrice ?? 0;
  const final = finalPrice ?? price ?? 0;
  const hasDiscount = final > 0 && base > 0 && final < base;
  const unit =
    currency === 'IRR' ? t('common.currency') : currency;
  const stockLabel =
    inStock === false
      ? t('common.outOfStock')
      : inventory != null && inventory > 0 && inventory <= 10
        ? t('common.lowStock', { count: inventory })
        : null;

  return (
    <div className="flex flex-col gap-2 rounded-xl border border-white/10 bg-white/5 px-4 py-3">
      <div className="flex flex-wrap items-baseline gap-2">
        {hasDiscount && (
          <span className="text-sm text-white/50 line-through">
            {formatMoney(base, locale)}
          </span>
        )}
        <span className={`text-2xl font-bold ${hasDiscount ? 'text-amber-300' : 'text-white'}`}>
          {formatMoney(final, locale)}
        </span>
        <span className="text-sm text-white/70">{unit}</span>
      </div>
      {stockLabel && (
        <span
          className={`w-fit rounded-full px-2.5 py-0.5 text-xs ${
            inStock === false
              ? 'bg-rose-500/20 text-rose-200'
              : 'bg-amber-500/20 text-amber-200'
          }`}
        >
          {stockLabel}
        </span>
      )}
    </div>
  );
}
