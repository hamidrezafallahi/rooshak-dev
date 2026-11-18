import type { Metadata } from 'next';
import { getTranslations, setRequestLocale } from 'next-intl/server';
import Link from 'next/link';

import PageHeader from '@components/molecules/storefront/PageHeader';
import { type AboutSection, getAboutContent } from '@lib/about';
import { buildPageMetadata } from '@lib/seo';

type Props = {
  params: Promise<{ locale: string }>;
};

/** ISR: the admin edits these texts under «درباره ما»; saving revalidates the `aboutSections` tag. */
export const revalidate = 300;

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: 'aboutPage' });
  const content = await getAboutContent(locale);
  const hero = content?.get('hero');

  return buildPageMetadata({
    locale,
    path: 'about',
    title: hero?.title || t('title'),
    description: (hero?.body || t('description')).slice(0, 160),
  });
}

/** Internal paths get the locale prefix; full http(s) URLs are kept. */
const toHref = (locale: string, url: string) =>
  /^https?:\/\//i.test(url) ? url : `/${locale}/${url.replace(/^\/+/, '')}`;

const FACT_KEYS = ['fact-1', 'fact-2', 'fact-3'];
const SPECIAL = new Set(['hero', 'quote', 'cta', ...FACT_KEYS]);

// Literal class names so Tailwind keeps them; the fact count varies.
const MD_COLS: Record<number, string> = { 1: 'md:grid-cols-1', 2: 'md:grid-cols-2', 3: 'md:grid-cols-3' };

export default async function Page({ params }: Props) {
  const { locale } = await params;
  setRequestLocale(locale);
  const t = await getTranslations({ locale, namespace: 'aboutPage' });

  // Every text below comes from the AboutSections table (admin → «درباره ما»).
  // Switched-off sections are missing and empty texts are not rendered.
  const content = await getAboutContent(locale);
  const sections: AboutSection[] = content ? [...content.values()] : [];
  const hero = content?.get('hero');
  const quote = content?.get('quote');
  const cta = content?.get('cta');
  const blocks = sections.filter((s) => !SPECIAL.has(s.key) && (s.title || s.body));
  const facts = FACT_KEYS.map((key) => content?.get(key)).filter(
    (f): f is AboutSection => Boolean(f?.title),
  );

  return (
    <article className="store-page !pt-6">
      <PageHeader
        eyebrow={hero?.eyebrow || undefined}
        title={hero?.title || t('title')}
        description={hero?.body || (content ? undefined : t('description'))}
        align="center"
      />

      {blocks.map((block) => (
        <section key={block.key} className="flex flex-col gap-3 mx-auto w-full max-w-3xl">
          {block.eyebrow ? <span className="text-store-subtle text-sm">{block.eyebrow}</span> : null}
          {block.title ? <h2 className="font-normal text-2xl sm:text-3xl leading-tight store-heading">{block.title}</h2> : null}
          {block.body ? (
            <p className="text-store-text leading-loose whitespace-pre-line">{block.body}</p>
          ) : null}
          {block.linkText && block.linkUrl ? (
            <Link href={toHref(locale, block.linkUrl)} className="self-start pb-0.5 border-current border-b text-sm">
              {block.linkText}
            </Link>
          ) : null}
        </section>
      ))}

      {quote?.body ? (
        <figure className="flex flex-col items-center gap-3 mx-auto p-6 sm:p-10 w-full max-w-4xl text-center store-panel">
          <blockquote className="m-0 text-xl sm:text-2xl leading-relaxed">«{quote.body}»</blockquote>
          {quote.title ? <figcaption className="text-store-subtle text-sm">{quote.title}</figcaption> : null}
        </figure>
      ) : null}

      {facts.length > 0 ? (
        <dl className={`grid grid-cols-1 ${MD_COLS[facts.length] ?? 'md:grid-cols-3'} gap-4 mx-auto m-0 w-full max-w-5xl`}>
          {facts.map((fact) => (
            <div key={fact.key} className="flex flex-col-reverse items-center gap-1 p-6 text-center store-panel">
              <dt className="text-store-subtle text-sm">{fact.body}</dt>
              <dd className="m-0 text-4xl sm:text-5xl">{fact.title}</dd>
            </div>
          ))}
        </dl>
      ) : null}

      {cta && (cta.title || cta.linkText) ? (
        <div className="flex flex-col items-center gap-3 mx-auto p-5 sm:p-6 w-full max-w-6xl text-center store-panel">
          {cta.title ? <p className="text-sm sm:text-base">{cta.title}</p> : null}
          {cta.linkText ? (
            <Link href={toHref(locale, cta.linkUrl || 'cooperation')} className="store-btn store-btn-primary">
              {cta.linkText}
            </Link>
          ) : null}
        </div>
      ) : null}
    </article>
  );
}
