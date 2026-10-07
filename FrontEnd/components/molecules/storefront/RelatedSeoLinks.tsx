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
    <section className="mt-10 border-t border-store-border pt-8">
      <h2 className="mb-4 text-lg font-normal text-store-text">
        {title || t('relatedLinks')}
      </h2>
      <div className="flex flex-wrap gap-2">
        {items.map((item) => (
          <Link
            key={item.href}
            href={item.href}
            className="border border-store-border px-3.5 py-1.5 text-sm text-store-text transition-colors hover:border-store-strong hover:bg-store-muted"
          >
            {item.label}
          </Link>
        ))}
      </div>
    </section>
  );
}
