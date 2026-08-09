import { getTranslations } from 'next-intl/server';
import Link from 'next/link';

type LinkItem = {
  href: string;
  label: string;
};

type Props = {
  locale: string;
  title?: string;
  links: LinkItem[];
};

export default async function RelatedSeoLinks({ locale, title, links }: Props) {
  const t = await getTranslations('storefront');
  const items = links.filter((link) => link.href && link.label);
  if (!items.length) return null;

  return (
    <section className="mt-8 rounded-2xl border border-white/10 bg-white/[0.03] p-5">
      <h2 className="mb-3 text-lg font-semibold text-white">
        {title || t('relatedLinks')}
      </h2>
      <div className="flex flex-wrap gap-2">
        {items.map((item) => (
          <Link
            key={item.href}
            href={item.href}
            className="rounded-full border border-white/15 bg-white/5 px-3 py-1.5 text-sm text-white/85 transition hover:border-white/35 hover:bg-white/10"
          >
            {item.label}
          </Link>
        ))}
      </div>
    </section>
  );
}
