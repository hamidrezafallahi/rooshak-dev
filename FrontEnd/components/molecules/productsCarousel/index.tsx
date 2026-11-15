"use client";
import React, {
  useRef,
} from 'react';

import { useTranslations } from 'next-intl';

import {
  ChevronLeftIcon,
  ChevronRightIcon,
} from '@components/atoms/iconComponents';
import VariantProductCard from '@components/molecules/productCard/variantProductCard';
import { ILandingProduct } from '@models/product';

interface ProductsCarouselProps {
  items: ILandingProduct[] | undefined;
  Loading: boolean;
}

export default function ProductsCarousel({
  items = [],
  Loading,
}: ProductsCarouselProps) {
  const t = useTranslations();
  const scrollRef = useRef<HTMLDivElement>(null);

  const scrollByCard = (dir: 1 | -1) => {
    const el = scrollRef.current;
    if (!el) return;
    // RTL scrollLeft semantics are handled by the browser; dir is visual (right = 1).
    el.scrollBy({ left: dir * Math.max(el.clientWidth * 0.8, 280), behavior: 'smooth' });
  };

  const arrowClass =
    'hidden sm:flex top-[38%] z-20 absolute justify-center items-center bg-store-surface hover:bg-primary border border-store-strong w-11 h-11 text-store-text hover:text-primary-foreground transition-colors';

  return (
    <div className="relative w-full">
      <button
        type="button"
        onClick={() => scrollByCard(-1)}
        aria-label={t('common.scrollPrev')}
        className={`${arrowClass} left-2`}
      >
        <ChevronLeftIcon />
      </button>

      <div
        ref={scrollRef}
        className="hidden-show-scrollbar flex gap-4 md:gap-6 pb-2 overflow-x-auto snap-x snap-mandatory scroll-smooth"
      >
        {Loading
          ? Array.from({ length: 4 }).map((_, i) => (
              <div key={i} className="flex-shrink-0 w-[62%] sm:w-[34%] lg:w-[23.5%]">
                <ProductCardSkeleton />
              </div>
            ))
          : items?.map((p) => (
              <div
                key={p.id}
                className="flex-shrink-0 snap-start w-[62%] sm:w-[34%] lg:w-[23.5%]"
              >
                <VariantProductCard product={p} />
              </div>
            ))}
      </div>

      <button
        type="button"
        onClick={() => scrollByCard(1)}
        aria-label={t('common.scrollNext')}
        className={`${arrowClass} right-2`}
      >
        <ChevronRightIcon />
      </button>
    </div>
  );
}

function ProductCardSkeleton() {
  return (
    <article role="status" className="animate-pulse">
      <div className="bg-store-muted aspect-[4/5]" />
      <div className="space-y-2 pt-4">
        <div className="bg-store-muted w-1/3 h-3" />
        <div className="bg-store-muted w-3/4 h-4" />
        <div className="bg-store-muted w-1/2 h-4" />
      </div>
    </article>
  );
}
