"use client";

import React, {
  useEffect,
  useRef,
} from 'react';

import { useTranslations } from 'next-intl';
import { useDispatch } from 'react-redux';

import {
  ChevronLeftIcon,
  ChevronRightIcon,
  ShoppingCartIcon,
} from '@components/atoms/iconComponents';
import MediaImage from '@components/atoms/MediaImage';
import { SpecialOffer } from '@models/specialOffer';
import { useGetConditionallyMutation } from '@services/base';
import {
  addToCart,
  synchronousCart,
} from '@slice/shoppingCartSlice';
import { getCookie } from '@utils/core';
import { toMediaUrl } from '@utils/toMediaUrl';

import CountdownDisplayClient from '../countdownDisplayClient';

interface Props {
  spacialOffers: SpecialOffer[];
}

const CARD_W = 200;
const GAP = 12;
const STEP = CARD_W + GAP;

export default function SpecialOfferCarouselClient({ spacialOffers }: Props) {
  const t = useTranslations();
  const containerRef = useRef<HTMLDivElement | null>(null);
  const isDragging = useRef(false);
  const startX = useRef(0);
  const scrollLeft = useRef(0);
  const containerLeft = useRef(0);
  const items = spacialOffers ?? [];
  const isAuthenticated = Boolean(getCookie("candySession"));
  const [itemMutate] = useGetConditionallyMutation();
  const dispatch = useDispatch();

  // Avoid scrollWidth/clientWidth reads during hydration (forced reflow / LCP).
  // Fixed card width: 2+ cards overflow typical mobile viewports.
  const showNav = items.length >= 2;

  const handlePrev = () => {
    containerRef.current?.scrollBy({ left: -STEP, behavior: "smooth" });
  };

  const handleNext = () => {
    containerRef.current?.scrollBy({ left: STEP, behavior: "smooth" });
  };

  const handleAdd = async (offer: SpecialOffer) => {
    const id = offer.product.id;
    const productOfferId = offer.product.productOfferId;

    if (isAuthenticated) {
      const syncCartResponse = await itemMutate({
        url: '/CartItems',
        body: { ProductId: id, ProductOfferId: productOfferId, Quantity: 1 },
      }).unwrap();
      if (syncCartResponse.isSuccess) {
        dispatch(synchronousCart(syncCartResponse.data));
      }
    } else {
      dispatch(
        addToCart({
          product: {
            id,
            productOfferId,
            name: offer.product.name,
            description: offer.product.description,
            price: offer.product.price,
            discountAmount: offer.product.discountAmount,
            discountIsPercent: offer.product.discountIsPercent,
            finalPrice: offer.product.finalPrice,
            quantity: 1,
            mainImage: offer.product.mainImage,
          },
        }),
      );
    }
  };

  useEffect(() => {
    const el = containerRef.current;
    if (!el) return;

    const onMouseDown = (e: MouseEvent) => {
      isDragging.current = true;
      // Cache geometry once per drag — don't re-read offsetLeft on every move.
      containerLeft.current = el.getBoundingClientRect().left;
      startX.current = e.pageX - containerLeft.current;
      scrollLeft.current = el.scrollLeft;
      document.body.style.userSelect = "none";
    };

    const endDrag = () => {
      isDragging.current = false;
      document.body.style.userSelect = "";
    };

    const onMouseMove = (e: MouseEvent) => {
      if (!isDragging.current) return;
      e.preventDefault();
      const x = e.pageX - containerLeft.current;
      el.scrollLeft = scrollLeft.current - (x - startX.current);
    };

    el.addEventListener("mousedown", onMouseDown);
    el.addEventListener("mouseleave", endDrag);
    el.addEventListener("mouseup", endDrag);
    el.addEventListener("mousemove", onMouseMove);

    return () => {
      el.removeEventListener("mousedown", onMouseDown);
      el.removeEventListener("mouseleave", endDrag);
      el.removeEventListener("mouseup", endDrag);
      el.removeEventListener("mousemove", onMouseMove);
    };
  }, []);

  if (!items || items.length === 0) {
    return (
      <div className="flex justify-center items-center w-full h-full text-white/90 text-sm">
        {t('landing.noOffers')}
      </div>
    );
  }

  return (
    <div className="relative w-full h-full">
      {showNav ? (
        <>
          <button
            type="button"
            onClick={handlePrev}
            aria-label={t('common.previous')}
            className="top-1/2 left-1 z-30 absolute flex justify-center items-center bg-white/70 hover:bg-white shadow rounded-full w-8 h-8 text-rose-600 -translate-y-1/2"
          >
            <ChevronLeftIcon config={{ size: 14 }} />
          </button>

          <button
            type="button"
            onClick={handleNext}
            aria-label={t('common.next')}
            className="top-1/2 right-1 z-30 absolute flex justify-center items-center bg-white/70 hover:bg-white shadow rounded-full w-8 h-8 text-rose-600 -translate-y-1/2"
          >
            <ChevronRightIcon config={{ size: 14 }} />
          </button>
        </>
      ) : null}

      <div
        ref={containerRef}
        className="hidden-show-scrollbar flex items-center gap-[12px] w-full h-full overflow-x-auto select-none"
      >
        {items.map((s, idx) => (
          <div
            key={`${s.id}-${idx}`}
            style={{ minWidth: `${CARD_W}px`, width: `${CARD_W}px` }}
            className="flex-shrink-0"
          >
            <CompactOfferCard offer={s} onAdd={() => handleAdd(s)} />
          </div>
        ))}
      </div>
    </div>
  );
}

function CompactOfferCard({
  offer,
  onAdd,
}: {
  offer: SpecialOffer;
  onAdd: () => void;
}) {
  const t = useTranslations();
  const target = new Date(offer.endDate).getTime();

  return (
    <article className="relative bg-white/95 dark:bg-gray-800 shadow-sm rounded-xl !h-full aspect-auto overflow-hidden">
      <div className="relative w-full h-64 overflow-hidden hover:scale-125">
        <MediaImage
          src={toMediaUrl(offer.product.mainImage) || "https://picsum.photos/seed/p/300/300"}
          alt={offer.product.name}
          fill
          className="object-cover"
          priority
          sizes="(max-width: 640px) 100vw, (max-width: 1024px) 50vw, 412px"
        />
        {offer.product.discountId > 0 && (
          <div className="top-2 absolute bg-yellow-400 px-2 py-1 rounded text-rose-700 text-xs end-2">
            <span>{offer.product.discountAmount}</span>
            <span className="text-xs">{t('common.tomanShort')}</span>
          </div>
        )}
      </div>
      <div className="z-10 absolute inset-0 flex flex-col justify-end items-end p-2 text-[12px]">
        <div className="font-medium line-clamp-1">{offer.product.name}</div>

        <div className="top-2 absolute rounded text-[11px] text-white start-2">
          <CountdownDisplayClient targetTime={target} />
        </div>

        <div className="flex justify-between items-center gap-2 w-full">
          <div className="bg-white bg-opacity-70 p-1 rounded w-full font-semibold text-rose-600 text-sm">
            {t('common.priceColon', { price: offer.product.finalPrice })}
          </div>
          <button
            type="button"
            onClick={onAdd}
            className="flex items-center gap-1 bg-rose-600 hover:bg-rose-700 px-2 py-1 rounded text-white text-xs"
          >
            <ShoppingCartIcon />
            {t('common.add')}
          </button>
        </div>
      </div>
    </article>
  );
}
