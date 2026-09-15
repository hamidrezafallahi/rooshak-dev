import { getTranslations } from 'next-intl/server';

import ShareActions from './ShareActions';

type Props = {
  url: string;
  title: string;
  /** Rendered server-side so a camera can read it before the page hydrates. */
  qrSvg: string | null;
  locale: string;
};

/**
 * Share surface for an exhibition price list: a scannable QR for the stand,
 * plus native share / copy / messenger links for passing the URL around.
 */
export default async function SharePanel({ url, title, qrSvg, locale }: Props) {
  const t = await getTranslations({ locale, namespace: 'exhibition' });

  return (
    <aside className="exhibit-share" aria-labelledby="exhibit-share-title">
      <div className="exhibit-share-qr">
        {qrSvg ? (
          <div
            className="exhibit-qr"
            aria-label={t('qrAlt', { title })}
            dangerouslySetInnerHTML={{ __html: qrSvg }}
          />
        ) : null}
        <p className="exhibit-share-hint">{t('qrHint')}</p>
      </div>

      <div className="exhibit-share-body">
        <h2 id="exhibit-share-title" className="exhibit-share-title">
          {t('shareTitle')}
        </h2>
        <p className="exhibit-share-desc">{t('shareDesc')}</p>

        <p className="exhibit-share-url" dir="ltr">
          {url}
        </p>

        <ShareActions url={url} title={title} />
      </div>
    </aside>
  );
}
