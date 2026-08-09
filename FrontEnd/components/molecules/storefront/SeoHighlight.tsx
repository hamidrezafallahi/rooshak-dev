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
    <aside className="mb-6 rounded-2xl border border-amber-300/20 bg-gradient-to-l from-amber-500/10 to-transparent px-4 py-3">
      <p className="mb-1 text-xs font-medium uppercase tracking-wide text-amber-200/80">
        {t('seoSummary')}
      </p>
      {title ? <h2 className="text-base font-semibold text-white">{title}</h2> : null}
      {description ? (
        <p className="mt-1 text-sm leading-relaxed text-white/75">{description}</p>
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
      className="rounded-full border border-white/15 bg-white/5 px-3 py-1 text-xs text-white/80 transition hover:border-white/30 hover:bg-white/10"
    >
      {label}
    </Link>
  );
}
