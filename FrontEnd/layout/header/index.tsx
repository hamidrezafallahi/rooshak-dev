import { getLocale } from 'next-intl/server';

import { getCurrentAnnouncement, resolveAnnouncementHref } from '@lib/announcement';
import { toMediaUrl } from '@utils/toMediaUrl';

import HeaderClient from './headerClient';

type Props = {
  /** Float transparently over a full-bleed hero (home page); turns solid after scroll. */
  overlay?: boolean;
};

const HEX = /^#[0-9a-fA-F]{6}$/;

/**
 * Server shell: loads the announcement bar that the admin scheduled for "now"
 * (text, link, colours, background image, height) and hands it to the client header.
 * The bar height is published as --store-announce-h so every page's top padding follows it.
 */
export default async function Header({ overlay = false }: Props) {
  const [locale, bar] = await Promise.all([getLocale(), getCurrentAnnouncement()]);

  const message = bar
    ? (locale === 'fa' ? bar.messageFa || bar.messageEn : bar.messageEn || bar.messageFa)?.trim()
    : '';
  const visible = Boolean(bar && message);

  const announcement =
    bar && visible
      ? {
          message: message as string,
          link: resolveAnnouncementHref(locale, bar.linkUrl),
          backgroundColor: HEX.test(bar.backgroundColor) ? bar.backgroundColor : '#000000',
          textColor: HEX.test(bar.textColor) ? bar.textColor : '#ffffff',
          backgroundImage: toMediaUrl(bar.backgroundImageUrl) || null,
        }
      : null;

  const heightPx = visible && bar ? Math.round(Number(bar.heightPx) || 36) : 0;

  return (
    <>
      <style
        dangerouslySetInnerHTML={{ __html: `:root:root{--store-announce-h:${heightPx}px}` }}
      />
      <HeaderClient overlay={overlay} announcement={announcement} />
    </>
  );
}
