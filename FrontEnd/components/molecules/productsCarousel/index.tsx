"use client";
import React, {
  useRef,
} from 'react';

import { useLocale, useTranslations } from 'next-intl';
import Link from 'next/link';
import { useDispatch } from 'react-redux';

import {
  ChevronLeftIcon,
  ChevronRightIcon,
} from '@components/atoms/iconComponents';
import MediaImage from '@components/atoms/MediaImage';
import { ILandingProduct } from '@models/product';
import { useGetConditionallyMutation } from '@services/base';
import {
  addToCart,
  synchronousCart,
} from '@slice/shoppingCartSlice';
import { getCookie } from '@utils/core';

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
                <ProductCard product={p} />
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

function formatMoney(value: number, locale: string) {
  return new Intl.NumberFormat(locale === 'fa' ? 'fa-IR' : 'en-US').format(value);
}

function ProductCard({ product }: { product: ILandingProduct }) {
  const t = useTranslations();
  const isAuthenticated = Boolean(getCookie('candySession'));
  const locale = useLocale();
  const [addToShoppingCart] = useGetConditionallyMutation();
  const dispatch = useDispatch();
  const handleAddToCart = async (product: ILandingProduct) => {
    if (isAuthenticated) {
      const syncCartResponse = await addToShoppingCart({
        url: '/CartItems',
        body: {
          productId: product.id,
          productOfferId: product.bestOfferId,
          quantity: 1,
        },
      }).unwrap();
      if (syncCartResponse.isSuccess) {
        dispatch(synchronousCart(syncCartResponse.data));
      }
    } else {
      dispatch(
        addToCart({
          product: {
            id: product.id,
            productOfferId: product.bestOfferId,
            name: product.name,
            description: product.description,
            price: product.price,
            discountAmount: product.discountAmount,
            discountIsPercent: product.discountIsPercent,
            finalPrice: product.finalPrice,
            quantity: 1,
            mainImage: product.mainImage,
          },
        }),
      );
    }
  };

  const href = `/${locale}/products/${product.slug || product.id}`;
  const hasDiscount = product.discountAmount > 0;

  return (
    <article className="group flex flex-col h-full text-store-text">
      <Link href={href} className="block relative bg-store-muted aspect-[4/5] overflow-hidden">
        <MediaImage
          src={product.mainImage}
          alt={product.name}
          fill
          className="object-cover group-hover:scale-105 transition-transform duration-700"
          sizes="(max-width: 640px) 62vw, (max-width: 1024px) 34vw, 24vw"
          loading="lazy"
        />
        {hasDiscount && (
          <span className="top-3 start-3 absolute bg-primary px-2.5 py-1 font-medium text-[0.7rem] text-primary-foreground">
            -{product.discountAmount}
            {product.discountIsPercent && '%'}
          </span>
        )}
      </Link>
      <div className="flex flex-col flex-1 gap-1 pt-4">
        <p className="text-store-subtle text-xs">{product.brand}</p>
        <h3 className="font-normal text-sm sm:text-base line-clamp-2">
          <Link href={href} className="hover:underline underline-offset-4">
            {product.name}
          </Link>
        </h3>
        <div className="flex flex-wrap items-baseline gap-x-2 mt-1 text-sm">
          <span className="font-medium">
            {formatMoney(product.finalPrice, locale)} {t('common.currency')}
          </span>
          {hasDiscount && (
            <span className="text-store-subtle text-xs line-through">
              {formatMoney(product.price, locale)}
            </span>
          )}
        </div>
        <button
          type="button"
          onClick={() => {
            handleAddToCart(product);
          }}
          aria-label={t('common.addToCartAria', { name: product.name })}
          className="self-start mt-3 pb-0.5 border-current border-b font-medium text-sm hover:opacity-60 transition-opacity"
        >
          {t('common.addToCart')}
        </button>
      </div>
    </article>
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
