/** Matches the storefront convention: stored decimals are shown as-is with a تومان unit. */
export function formatMoney(value: number, locale: string) {
  return new Intl.NumberFormat(locale === 'fa' ? 'fa-IR' : 'en-US').format(value);
}

export function discountPercent(base: number, final: number) {
  if (!base || final >= base) return 0;
  return Math.round(((base - final) / base) * 100);
}
