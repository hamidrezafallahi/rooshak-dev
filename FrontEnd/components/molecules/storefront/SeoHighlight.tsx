import { getTranslations } from 'next-intl/server';
import Link from 'next/link';

type Props = {
  title?: string | null;
  description?: string | null;
  locale: string;
};

export default async function SeoHighlight({ title, description, locale }: Props) {
  if (!title && !description) return null;

  const t = await getTranslations('storefront');

  return (
    <aside className="mb-6 border-s-2 border-store-strong bg-store-muted px-4 py-3">
      <p className="mb-1 text-xs font-medium uppercase text-store-subtle ltr:tracking-[0.14em]">
        {t('seoSummary')}
      </p>
      {title ? <h2 className="text-base font-medium text-store-text">{title}</h2> : null}
      {description ? (
        <p className="mt-1 text-sm leading-relaxed text-store-subtle">{description}</p>
      ) : null}
    </aside>
  );
}

type ChipProps = {
  href: string;
  label: string;
};

export function SeoRelatedChip({ href, label }: ChipProps) {
  return (
    <Link
      href={href}
      className="border border-store-border px-3 py-1 text-xs text-store-text transition-colors hover:border-store-strong"
    >
      {label}
    </Link>
  );
}
