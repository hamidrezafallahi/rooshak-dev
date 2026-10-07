// @components/molecules/productCard.tsx
"use client";

import { useState } from 'react';

import { useTranslations } from 'next-intl';
import Link from 'next/link';

import MediaImage from '@components/atoms/MediaImage';

import { IDetailedProductCardProps } from './type';

export function DetailedProductCard({
  product,
  locale,
}: IDetailedProductCardProps) {
  const t = useTranslations('common');
  const [isHovered, setIsHovered] = useState(false);
  // محاسبه تخفیف
  const hasDiscount =
    product.finalPrice > 0 && product.finalPrice < product.price;
  const discountPercentage = hasDiscount
    ? Math.round(((product.price - product.finalPrice) / product.price) * 100)
    : 0;

  // قیمت نهایی
  const finalPrice =
    product.finalPrice > 0 ? product.finalPrice : product.price;

  // موجودی
  const isOutOfStock = product.inventory <= 0;
  const isLowStock = product.inventory > 0 && product.inventory <= 10;

  // تصویر محصول
  const imageSrc = product.mainImage || product.imageUrl;

  return (
    <div className="group relative bg-store-surface shadow-sm hover:shadow-2xl border border-store-border rounded-2xl overflow-hidden transition-all duration-300">
      {/* تخفیف */}
      {hasDiscount && (
        <div className="top-3 left-3 z-10 absolute bg-red-500 px-2 py-1 rounded-full font-bold text-white text-xs">
          {discountPercentage}% {t('percentOff')}
        </div>
      )}

      {/* وضعیت موجودی */}
      {isOutOfStock && (
        <div className="top-3 right-3 z-10 absolute bg-gray-500 px-2 py-1 rounded-full font-medium text-white text-xs">
          {t('outOfStock')}
        </div>
      )}
      {isLowStock && !isOutOfStock && (
        <div className="top-3 right-3 z-10 absolute bg-amber-500 px-2 py-1 rounded-full font-medium text-white text-xs">
          {t('lowStockShort')}
        </div>
      )}

      {/* تصویر محصول */}
      <Link
        href={`/${locale}/products/${product.slug || product.id}`}
        className="block relative bg-store-muted h-64 overflow-hidden"
        onMouseEnter={() => setIsHovered(true)}
        onMouseLeave={() => setIsHovered(false)}
      >
        <MediaImage
          src={imageSrc}
          fallbackSrc="/images/default-product.jpg"
          alt={product.name}
          fill
          className={`object-contain transition-transform duration-500 ${
            isHovered ? "scale-110" : "scale-100"
          }`}
          sizes="(max-width: 640px) 100vw, (max-width: 768px) 50vw, (max-width: 1024px) 33vw, 25vw"
          quality={85}
        />

        {/* افکت hover */}
        <div className="absolute inset-0 bg-black/0 group-hover:bg-black/5 transition-colors duration-300" />
      </Link>

      {/* اطلاعات محصول */}
      <div className="p-4">
        {/* دسته‌بندی و برند */}
        {(product.categoryName || product.brandName) && (
          <div className="flex items-center gap-2 mb-2">
            {product.categoryName && (
              <span className="bg-store-muted px-2 py-1 rounded-full text-store-subtle text-xs">
                {product.categoryName}
              </span>
            )}
            {product.brandName && (
              <span className="bg-blue-50 px-2 py-1 rounded-full text-blue-600 text-xs">
                {product.brandName}
              </span>
            )}
          </div>
        )}

        {/* نام محصول */}
        <Link href={`/${locale}/products/${product.slug || product.id}`}>
          <h3 className="mb-2 font-semibold text-store-text hover:text-primary line-clamp-1 transition-colors">
            {product.name}
          </h3>
        </Link>

        {/* توضیحات کوتاه */}
        <p className="mb-3 min-h-[2.5rem] text-store-subtle text-sm line-clamp-2">
          {product.description.trim()}
        </p>

        {/* قیمت */}
        <div className="flex justify-between items-center mb-3">
          <div className="flex items-center gap-2">
            {/* قیمت اصلی اگر تخفیف داشته باشد */}
            {hasDiscount && (
              <span className="text-store-subtle text-sm line-through">
                {product.price?.toLocaleString()}
              </span>
            )}

            {/* قیمت نهایی */}
            <span
              className={`text-xl font-bold ${
                hasDiscount ? "text-red-600" : "text-store-text"
              }`}
            >
              {finalPrice?.toLocaleString()}
            </span>

            {/* واحد پول */}
            <span className="text-store-subtle text-sm">{t('currency')}</span>
          </div>

          {/* رتبه‌بندی (اختیاری) */}
          {product.rating !== undefined && (
            <div className="flex items-center gap-1">
              <div className="flex text-amber-400">
                {[...Array(5)].map((_, i) => (
                  <svg
                    key={i}
                    className={`w-4 h-4 ${i < Math.floor(product.rating!) ? "fill-current" : "fill-gray-300"}`}
                    viewBox="0 0 20 20"
                  >
                    <path d="M10 15l-5.878 3.09 1.123-6.545L.489 6.91l6.572-.955L10 0l2.939 5.955 6.572.955-4.756 4.635 1.123 6.545z" />
                  </svg>
                ))}
              </div>
              <span className="text-store-subtle text-xs">
                {product.rating.toFixed(1)}
              </span>
            </div>
          )}
        </div>

        {/* دکمه‌های عملیاتی */}
        <div className="flex gap-2">
          <Link
            href={`/${locale}/products/${product.slug || product.id}`}
            className="flex-1 bg-primary hover:bg-primary/90 px-4 py-2 rounded-lg font-medium text-primary-foreground text-center transition-colors"
          >
            {t('view')}
          </Link>

          <button
            disabled={isOutOfStock}
            onClick={() => {
              // افزودن به سبد خرید
              console.log("Add to cart:", product.id);
            }}
            className={`flex-1 text-center py-2 px-4 rounded-lg font-medium transition-colors ${
              isOutOfStock
                ? "bg-store-muted text-store-subtle cursor-not-allowed"
                : "bg-store-muted text-store-text hover:bg-store-muted"
            }`}
          >
            {t('addToCart')}
          </button>
        </div>
      </div>
    </div>
  );
}
