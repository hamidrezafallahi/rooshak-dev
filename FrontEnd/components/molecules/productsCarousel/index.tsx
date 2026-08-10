"use client";
import React, {
  useRef,
} from 'react';

import { useLocale, useTranslations } from 'next-intl';
import Link from 'next/link';
import { useDispatch } from 'react-redux';

import { Rate } from '@components/atoms/defaultElements/customRate';
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

  const scrollAmount = 300;

  const scrollLeft = () => {
    if (!scrollRef.current) return;
    scrollRef.current.scrollBy({ left: -scrollAmount, behavior: "smooth" });
  };

  const scrollRight = () => {
    if (!scrollRef.current) return;
    scrollRef.current.scrollBy({ left: scrollAmount, behavior: "smooth" });
  };
  return (
    <div className="relative w-full">
      {/* عنوان */}
      <h2 className="mb-6 font-extrabold text-rose-600 text-2xl text-center">
        {t('landing.selectedProducts')}
      </h2>

      {/* دسکتاپ: کاروسل با دکمه */}
      <div className="hidden sm:block relative">
        {/* دکمه سمت چپ */}
        <button
          type="button"
          onClick={scrollLeft}
          aria-label={t('common.scrollPrev')}
          className="top-1/2 left-0 z-20 absolute flex justify-center items-center bg-white/80 hover:bg-white shadow rounded-full w-10 h-10 -translate-y-1/2"
        >
          <ChevronLeftIcon />
        </button>

        {/* نوار اسکرول */}
        <div
          ref={scrollRef}
          className="hidden-show-scrollbar flex gap-6 px-2 overflow-x-auto scroll-smooth"
        >
          {Loading ? (
            <>
              <ProductCardSkeleton />
              <ProductCardSkeleton />
              <ProductCardSkeleton />
              <ProductCardSkeleton />
            </>
          ) : (
            items?.map((p) => (
              <div key={p.id} className="flex-shrink-0 w-1/4 min-w-[250px]">
                <ProductCard product={p} />
              </div>
            ))
          )}
        </div>

        {/* دکمه سمت راست */}
        <button
          type="button"
          onClick={scrollRight}
          aria-label={t('common.scrollNext')}
          className="top-1/2 right-0 z-20 absolute flex justify-center items-center bg-white/80 hover:bg-white shadow rounded-full w-10 h-10 -translate-y-1/2"
        >
          <ChevronRightIcon />
        </button>
      </div>

      {/* موبایل: اسکرول افقی ساده */}
      <div className="hidden-show-scrollbar sm:hidden flex gap-4 px-2 pb-2 overflow-x-auto">
        {Loading ? (
          <>
            <ProductCardSkeleton />
            <ProductCardSkeleton />
            <ProductCardSkeleton />
            <ProductCardSkeleton />
          </>
        ) : (
          items?.map((p) => (
            <div key={p.id} className="flex-shrink-0 min-w-[70%]">
              <ProductCard product={p} />
            </div>
          ))
        )}
      </div>
    </div>
  );
}

function ProductCard({ product }: { product: ILandingProduct }) {
  const t = useTranslations();
  const isAuthenticated = Boolean(getCookie("candySession"));
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
 
  return (
    <article className="flex-shrink-0 bg-white shadow-sm hover:shadow-lg rounded-2xl w-64 overflow-hidden transition-shadow">
      <div className="relative w-full h-56 overflow-hidden">
        <MediaImage
          src={product.mainImage}
          alt={product.name}
          fill
          className="object-cover"
          sizes="256px"
          loading="lazy"
        />
        {product.discountAmount > 0 && (
          <span className="top-3 left-3 absolute bg-red-600 px-2 py-1 rounded font-semibold text-white text-xs">
            -{product.discountAmount}
            {product.discountIsPercent && "%"}
          </span>
        )}
        <div className="bottom-0 left-[50%] absolute flex items-center gap-1 bg-black/50 px-2 py-1 rounded max-w-full -translate-x-[50%]">
          <Rate value={product.averageRate} />
          {/* <span className="text-white text-xs">
    ({product.rateCount})
  </span> */}
        </div>
      </div>
      <div className="p-4">
        <h3 className="font-medium text-sm line-clamp-2">{product.name}</h3>
        <p className="text-gray-600 text-xs">{product.brand}</p>

        <div className="flex justify-between items-end gap-2 mt-3">
          <div>
            <div className="font-semibold text-sm">{product.finalPrice}$</div>

            {product.discountAmount > 0 && (
              <div className="text-gray-600 text-xs line-through">
                {product.price}$
              </div>
            )}
          </div>

          <div className="flex items-center gap-2">
            <Link
              href={`/${locale}/products/${product.slug || product.id}`}
              className="bg-gray-100 hover:bg-gray-200 px-3 py-2 rounded-lg text-xs"
            >
              {t('common.view')}
            </Link>
            <button
              type="button"
              onClick={() => {
                handleAddToCart(product);
              }}
              aria-label={t('common.addToCartAria', { name: product.name })}
              className="bg-rose-600 hover:bg-rose-700 px-3 py-2 rounded-lg text-white text-xs"
            >
              {t('common.add')}
            </button>
          </div>
        </div>
      </div>
    </article>
  );
}
function ProductCardSkeleton() {
  return (
    <article
      role="status"
      className="flex-shrink-0 bg-white shadow-sm rounded-2xl w-64 overflow-hidden animate-pulse"
    >
      {/* تصویر */}
      <div className="relative bg-gray-200 dark:bg-gray-700 w-full h-56">
        <div className="absolute inset-0 flex justify-center items-center">
          <svg
            className="w-10 h-10 text-gray-300 dark:text-gray-600"
            xmlns="http://www.w3.org/2000/svg"
            fill="currentColor"
            viewBox="0 0 20 18"
            aria-hidden="true"
          >
            <path d="M18 0H2a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2V2a2 2 0 0 0-2-2Zm-5.5 4a1.5 1.5 0 1 1 0 3 1.5 1.5 0 0 1 0-3Zm4.376 10.481A1 1 0 0 1 16 15H4a1 1 0 0 1-.895-1.447l3.5-7A1 1 0 0 1 7.468 6a.965.965 0 0 1 .9.5l2.775 4.757 1.546-1.887a1 1 0 0 1 1.618.1l2.541 4a1 1 0 0 1 .028 1.011Z" />
          </svg>
        </div>
      </div>

      {/* متن */}
      <div className="space-y-3 p-4">
        {/* نام محصول */}
        <div className="bg-gray-200 dark:bg-gray-700 rounded w-3/4 h-4"></div>

        {/* برند */}
        <div className="bg-gray-200 dark:bg-gray-700 rounded w-1/2 h-3"></div>

        {/* قیمت و دکمه‌ها */}
        <div className="flex justify-between items-end gap-2 mt-3">
          <div className="space-y-2">
            <div className="bg-gray-200 dark:bg-gray-700 rounded w-16 h-4"></div>
            <div className="bg-gray-200 dark:bg-gray-700 rounded w-12 h-3"></div>
          </div>

          <div className="flex items-center gap-2">
            <div className="bg-gray-200 dark:bg-gray-700 rounded-lg w-16 h-8"></div>
            <div className="bg-gray-200 dark:bg-gray-700 rounded-lg w-16 h-8"></div>
          </div>
        </div>
      </div>
    </article>
  );
}
