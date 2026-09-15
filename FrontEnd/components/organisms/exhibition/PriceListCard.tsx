import Link from 'next/link';
import { getTranslations } from 'next-intl/server';

import MediaImage from '@components/atoms/MediaImage';
import { IPriceListItem } from '@models/exhibition';

import { discountPercent, formatMoney } from './money';

type Props = {
  item: IPriceListItem;
  locale: string;
  currency: string;
};

/**
 * One product on the exhibition sheet: image, catalog code, name, vessel specs
 * and the struck-through base price next to the discounted one.
 */
export default async function PriceListCard({ item, locale, currency }: Props) {
  const t = await getTranslations();

  const base = item.price ?? item.finalPrice ?? 0;
  const final = item.finalPrice ?? item.price ?? 0;
  const hasDiscount = item.hasDiscount && base > 0 && final < base;
  const percent = discountPercent(base, final);
  const unit = currency === 'IRR' ? t('common.currency') : currency;

  const specs = [
    item.diameter ? { label: t('exhibition.diameter'), value: item.diameter } : null,
    item.height ? { label: t('exhibition.height'), value: item.height } : null,
    item.pieceCount ? { label: t('exhibition.pieces'), value: item.pieceCount } : null,
  ].filter(Boolean) as { label: string; value: string }[];

  return (
    <article className="group exhibit-card">
      <Link
        href={`/${locale}/products/${item.slug || item.productId}`}
        className="exhibit-card-media"
        aria-label={item.name}
      >
        {item.mainImage ? (
          <MediaImage
            src={item.mainImage}
            alt={item.name}
            fill
            className="object-contain p-4 transition-transform duration-500 group-hover:scale-105"
            sizes="(max-width: 640px) 50vw, (max-width: 1024px) 33vw, 260px"
          />
        ) : (
          <span className="exhibit-card-placeholder" aria-hidden>
            {item.code}
          </span>
        )}

        {hasDiscount && percent > 0 && (
          <span className="exhibit-badge-discount">
            {new Intl.NumberFormat(locale === 'fa' ? 'fa-IR' : 'en-US').format(percent)}
            {t('common.percentOff')}
          </span>
        )}

        {!item.inStock && (
          <span className="exhibit-badge-stock">{t('common.outOfStock')}</span>
        )}
      </Link>

      <div className="exhibit-card-body">
        <p className="exhibit-code">{item.code}</p>

        <h3 className="exhibit-name">
          <Link href={`/${locale}/products/${item.slug || item.productId}`}>
            {item.name}
          </Link>
        </h3>

        {specs.length > 0 && (
          <dl className="exhibit-specs">
            {specs.map((spec) => (
              <div key={spec.label}>
                <dt>{spec.label}</dt>
                <dd>{spec.value}</dd>
              </div>
            ))}
          </dl>
        )}

        <div className="exhibit-price">
          {hasDiscount && (
            <span className="exhibit-price-base">{formatMoney(base, locale)}</span>
          )}
          <span className="exhibit-price-final">{formatMoney(final, locale)}</span>
          <span className="exhibit-price-unit">{unit}</span>
        </div>
      </div>
    </article>
  );
}
