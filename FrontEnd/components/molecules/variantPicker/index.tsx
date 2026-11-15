'use client';

import { useTranslations } from 'next-intl';

import type { IProductVariant } from '@models/product';

/**
 * Picks the option shown first: the server's best offer when it is a variant,
 * otherwise the first in-stock option, otherwise the first one.
 */
export function pickDefaultVariant(
  variants: IProductVariant[] | undefined,
  bestOfferId?: number | null,
): IProductVariant | null {
  if (!variants?.length) return null;
  return (
    variants.find((v) => v.offerId === bestOfferId && v.inStock) ??
    variants.find((v) => v.inStock) ??
    variants[0]
  );
}

type Props = {
  variants: IProductVariant[];
  selectedOfferId: number | null;
  onSelect: (variant: IProductVariant) => void;
  /** `sm` for product cards, `md` for the product page. */
  size?: 'sm' | 'md';
  /** Shows the group title (e.g. «سایز») above the chips. */
  showLabel?: boolean;
  align?: 'start' | 'center';
};

/**
 * Chips for a product's options (e.g. size or colour).
 * A radio group for assistive tech; arrow keys move between options.
 */
export default function VariantPicker({
  variants,
  selectedOfferId,
  onSelect,
  size = 'md',
  showLabel = true,
  align = 'start',
}: Props) {
  const t = useTranslations();
  if (!variants.length) return null;

  const groupName = variants.find((v) => v.name)?.name || t('product.variantDefaultLabel');
  const selected = variants.find((v) => v.offerId === selectedOfferId);

  const move = (from: number, step: number) => {
    const next = variants[(from + step + variants.length) % variants.length];
    onSelect(next);
    requestAnimationFrame(() => {
      document.getElementById(`variant-${next.offerId}`)?.focus();
    });
  };

  const chipSize =
    size === 'sm'
      ? 'h-8 min-w-[2.75rem] px-2.5 text-xs'
      : 'h-11 min-w-[4.25rem] px-4 text-sm';

  return (
    <div className={`flex flex-col gap-2 w-full ${align === 'center' ? 'items-center' : 'items-start'}`}>
      {showLabel && (
        <p className="text-[13px] text-store-subtle">
          {groupName}
          {selected && <span className="ms-1.5 font-medium text-store-text" dir="ltr">{selected.value}</span>}
        </p>
      )}
      <div
        role="radiogroup"
        aria-label={t('product.variantGroupAria', { name: groupName })}
        className={`flex flex-wrap gap-1.5 ${align === 'center' ? 'justify-center' : ''}`}
      >
        {variants.map((variant, index) => {
          const active = variant.offerId === selectedOfferId;
          return (
            <button
              key={variant.offerId}
              id={`variant-${variant.offerId}`}
              type="button"
              role="radio"
              aria-checked={active}
              tabIndex={active || (selectedOfferId == null && index === 0) ? 0 : -1}
              onClick={() => onSelect(variant)}
              onKeyDown={(e) => {
                const rtl = getComputedStyle(e.currentTarget).direction === 'rtl';
                if (e.key === 'ArrowRight' || e.key === 'ArrowDown') {
                  e.preventDefault();
                  move(index, e.key === 'ArrowDown' || !rtl ? 1 : -1);
                } else if (e.key === 'ArrowLeft' || e.key === 'ArrowUp') {
                  e.preventDefault();
                  move(index, e.key === 'ArrowUp' || !rtl ? -1 : 1);
                }
              }}
              title={variant.inStock ? undefined : t('common.outOfStock')}
              className={`inline-flex items-center justify-center border transition-colors ${chipSize} ${
                active
                  ? 'border-store-text bg-store-text text-store-surface font-medium'
                  : 'border-store-border bg-store-surface text-store-text hover:border-store-text'
              } ${variant.inStock ? '' : 'line-through decoration-1 opacity-60'}`}
            >
              <span dir="ltr">{variant.value}</span>
            </button>
          );
        })}
      </div>
    </div>
  );
}
