/**
 * Empty stub replacing Next.js `polyfill-module`.
 *
 * Next ships Array.at / flat / Object.hasOwn / etc. polyfills into the
 * framework chunk for all browsers. Our storefront targets modern browsers
 * (Chrome/Edge/Firefox 111+, Safari 16.4+) where those APIs are native, so
 * we drop ~11.5 KiB of legacy JS flagged by Lighthouse.
 *
 * Do NOT use this if you must support Safari < 15.4 or similarly old engines.
 */
export {};
