import Link from 'next/link';

import { CardSliderDots, CardSliderImages, CardSliderProvider } from './CardImageSlider';
import { IProductCardProps } from './type';

/**
 * Tall charcoal product card: the product photo sits in its own area at the top,
 * shown whole at its natural proportions (never cropped or stretched), with the
 * title + price pill, description, two badges and a white pill button below it.
 * Card size: the h-[380px] w-[240px] classes below.
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
        'group relative isolate flex flex-col overflow-hidden rounded-[26px] bg-[#232326]',
        'h-[380px] w-[240px] shrink-0 snap-center',
        'shadow-[0_22px_45px_-14px_rgba(15,15,20,0.45),0_8px_16px_-8px_rgba(15,15,20,0.18)]',
        'transition-[transform,box-shadow] duration-300 ease-out',
        'hover:-translate-y-[5px] hover:shadow-[0_32px_60px_-14px_rgba(15,15,20,0.55),0_10px_20px_-8px_rgba(15,15,20,0.22)]',
        'focus-within:-translate-y-[5px]',
        'motion-reduce:transition-none motion-reduce:hover:translate-y-0 motion-reduce:focus-within:translate-y-0',
      ].join(' ')}
    >
      {/* Photo area: fills the space above the text; images are object-contain */}
      <div className="relative flex-1 min-h-0">
        <CardSliderImages
          images={images}
          alt={product.imageAlt || product.name}
          priority={priority}
        />
      </div>

      {/* Content stack below the photo; fixed line counts keep all cards aligned */}
      <div className="flex flex-col shrink-0 px-3 pt-2.5 pb-3 text-white">
        {/* Slide indicator: one dot per image */}
        <CardSliderDots />

        <div className="flex justify-between items-center gap-2 px-1">
          <h3 className="min-w-0 font-semibold text-[15px] leading-6 truncate">{product.name}</h3>
          {product.priceLabel && (
            <span className="bg-white/10 px-2 py-[3px] rounded-full font-medium text-[10px] leading-4 whitespace-nowrap shrink-0">
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
              className="bg-white/10 px-2.5 py-[3px] rounded-full font-medium text-[10px] leading-4 whitespace-nowrap truncate"
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
            'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-white focus-visible:ring-offset-2 focus-visible:ring-offset-[#232326]',
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
