"use client"
import {
  useRef,
} from 'react';

import {
  useLocale,
  useTranslations,
} from 'next-intl';
import Link from 'next/link';
import { useDispatch } from 'react-redux';

import {
  ChevronLeftIcon,
  ChevronRightIcon,
} from '@components/atoms/iconComponents';
import { IDetailedProductOffer } from '@models/product';
import { useGetConditionallyMutation } from '@services/base';
import {
  addToCart,
  synchronousCart,
} from '@slice/shoppingCartSlice';
import { getCookie } from '@utils/core';
import { toMediaUrl } from '@utils/toMediaUrl';

interface ProductsCarouselProps {
  items: IDetailedProductOffer[] | undefined;
  Loading?: boolean;
}
export default function SupplierProductsCarousel({
  items = [],
}: ProductsCarouselProps) {
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
      <div className="hidden sm:block relative">
        <button
          onClick={scrollLeft}
          className="top-1/2 -left-4 z-20 absolute flex justify-center items-center bg-white/80 hover:bg-white shadow rounded-full w-10 h-10 -translate-y-1/2"
        >
          <ChevronLeftIcon />
        </button>
        <div
          ref={scrollRef}
          className="hidden-show-scrollbar flex gap-6 px-2 overflow-x-auto scroll-smooth"
        >
          {items?.map((p) => (
            <div key={p.id} className="flex-shrink-0 w-1/4 min-w-[250px]">
              <ProductCard product={p} />
            </div>
          ))}
        </div>
        <button
          onClick={scrollRight}
          className="top-1/2 -right-4 z-20 absolute flex justify-center items-center bg-white/80 hover:bg-white shadow rounded-full w-10 h-10 -translate-y-1/2"
        >
          <ChevronRightIcon />
        </button>
      </div>
      <div className="hidden-show-scrollbar sm:hidden flex gap-4 px-2 pb-2 overflow-x-auto">
        {items?.map((p) => (
          <div key={p.id} className="flex-shrink-0 min-w-[70%]">
            <ProductCard product={p} />
          </div>
        ))}
      </div>
    </div>
  );
}

function ProductCard({ product }: { product: IDetailedProductOffer }) {
  const isAuthenticated = Boolean(getCookie("candySession"));
  const locale = useLocale();
  const t = useTranslations();
  const [addToShoppingCart] = useGetConditionallyMutation();
  const dispatch = useDispatch();
  const handleAddToCart = async (product: IDetailedProductOffer) => {
    if (isAuthenticated) {
      console.log(product)
      const syncCartResponse = await addToShoppingCart({
        url: '/CartItems',
        body: {
          productId: product.id,
          productOfferId: product.id,
          quantity: 1,
        },
      }).unwrap();
      if (syncCartResponse.isSuccess) {
        dispatch(synchronousCart(syncCartResponse.data));
      }
    } else {
dispatch(addToCart({
  product: {
    id: product.id,
    productOfferId: product.id,
    name: product.productName,
    description: product.productDescription,
    price: product.basePrice,
    discountAmount: 0,
    discountIsPercent: false,
    finalPrice: product.finalPrice ?? product.basePrice,
    quantity: 1,
    mainImage: product.productImage,
  }
}));
    }
  };
  return (
    <article className="flex-shrink-0 bg-store-surface shadow-sm hover:shadow-lg rounded-2xl w-64 overflow-hidden transition-shadow">
      <div className="relative w-full h-56 overflow-hidden">
        <img
          src={toMediaUrl(product.productImage)}
          alt={product.productName}
          className="w-full h-full object-cover"
          loading="lazy"
        />
        {/* {product.discountAmount > 0 && (
          <span className="top-3 left-3 absolute bg-red-600 px-2 py-1 rounded font-semibold text-white text-xs">
            -{product.discountAmount}
            {product.discountIsPercent && "%"}
          </span>
        )} */}
      </div>
      <div className="p-4">
        <h4 className="font-medium text-sm line-clamp-2">{product.productName}</h4>
        {/* <p className="text-store-subtle text-xs">{product.?.name}</p> */}

        <div className="flex justify-between items-end gap-2 mt-3">
          <div>
            <div className="font-semibold text-sm">{product.finalPrice}$</div>

            <div className="text-store-subtle text-xs line-through">
              {product.basePrice}$
            </div>
          </div>

          <div className="flex items-center gap-2">
            <Link
              href={`/${locale}/products/${product.productSlug || product.slug || product.productId}`}
              className="bg-store-muted hover:bg-store-muted px-3 py-2 rounded-lg text-xs"
            >
              {t('common.view')}
            </Link>
            <button
              onClick={() => {
                handleAddToCart(product);
              }}
              className="bg-primary hover:bg-primary/90 px-3 py-2 rounded-lg text-primary-foreground text-xs"
            >
              {t('common.add')}
            </button>
          </div>
        </div>
      </div>
    </article>
  );
}
