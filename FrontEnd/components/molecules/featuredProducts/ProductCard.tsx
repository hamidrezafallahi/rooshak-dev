import Link from 'next/link';


import { CardSliderDots, CardSliderImages, CardSliderProvider } from './CardImageSlider';
import { IProductCardProps } from './type';

/**
 * Tall full-bleed product card: photo behind everything, dark/frosted gradient
 * toward the bottom, title + price pill, short description, two badges and a
 * white pill button. Card size: the h-[380px] w-[240px] classes below.
 *
 * Server component; only the photo slider + its dots are client-side
 * (CardImageSlider). Slides start at the main image and change every 2s.
 */
export default function ProductCard({ product, ctaLabel, priority }: IProductCardProps) {
  const badges = (product.badges ?? []).filter(Boolean).slice(0, 2);
  const images = product.images.length ? product.images : [''];

  return (
    <CardSliderProvider count={images.length}>
    <article
      className={[
        'group relative isolate overflow-hidden rounded-[26px] bg-[#2a2a2e]',
        'h-[380px] w-[240px] shrink-0 snap-center',
        'shadow-[0_22px_45px_-14px_rgba(15,15,20,0.45),0_8px_16px_-8px_rgba(15,15,20,0.18)]',
        'transition-[transform,box-shadow] duration-300 ease-out',
        'hover:-translate-y-[5px] hover:shadow-[0_32px_60px_-14px_rgba(15,15,20,0.55),0_10px_20px_-8px_rgba(15,15,20,0.22)]',
        'focus-within:-translate-y-[5px]',
        'motion-reduce:transition-none motion-reduce:hover:translate-y-0 motion-reduce:focus-within:translate-y-0',
      ].join(' ')}
    >
      {/* Full-bleed photographs (cross-fading slider, main image first) */}
      <CardSliderImages
        images={images}
        alt={product.imageAlt || product.name}
        priority={priority}
        className="-z-20 transition-transform duration-300 ease-out group-hover:scale-[1.03] motion-reduce:transition-none motion-reduce:group-hover:scale-100"
      />

      {/* Dark readability gradient */}
      <div
        aria-hidden
        className="-z-10 absolute inset-0"
        style={{
          background:
            'linear-gradient(to bottom, transparent 30%, rgba(0,0,0,0.12) 46%, rgba(0,0,0,0.5) 72%, rgba(0,0,0,0.68) 100%)',
        }}
      />

      {/* Content stack pinned to the bottom; fixed line counts keep all cards aligned */}
      <div className="absolute inset-x-0 bottom-0 flex flex-col px-3 pb-3 text-white">
        {/* Slide indicator: one dot per image */}
        <CardSliderDots />

        <div className="flex justify-between items-center gap-2 px-1">
          <h3 className="min-w-0 font-semibold text-[15px] leading-6 truncate">{product.name}</h3>
          {product.priceLabel && (
            <span className="bg-black/55 backdrop-blur-sm px-2 py-[3px] rounded-full font-medium text-[10px] leading-4 whitespace-nowrap shrink-0">
              {product.priceLabel}
            </span>
          )}
        </div>

        <p className="mt-1 px-1 min-h-[3.2em] font-light text-[11px] text-white/80 leading-[1.6] line-clamp-2">
          {product.description}
        </p>

        <ul className="flex gap-1.5 mt-2 px-1 min-h-[22px] overflow-hidden">
          {badges.map((badge) => (
            <li
              key={badge}
              className="bg-white/15 backdrop-blur-md px-2.5 py-[3px] rounded-full font-medium text-[10px] leading-4 whitespace-nowrap truncate"
            >
              {badge}
            </li>
          ))}
        </ul>

        {/* Stretched link: the whole card is clickable, but only one tab stop */}
        <Link
          href={product.href}
          aria-label={`${ctaLabel}: ${product.name}`}
          className={[
            'mt-3 flex h-10 w-full items-center justify-center rounded-full bg-white',
            'text-[13px] font-semibold text-[#111114]',
            'transition-colors duration-300 hover:bg-[#ececef] group-hover:bg-[#f7f7f8]',
            'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-white focus-visible:ring-offset-2 focus-visible:ring-offset-black/40',
            "after:absolute after:inset-0 after:content-['']",
          ].join(' ')}
        >
          {ctaLabel}
        </Link>
      </div>
    </article>
    </CardSliderProvider>
  );
}
