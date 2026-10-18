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

const CARD_W = 240;
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
      <div className="flex justify-center items-center w-full h-full text-white/80 text-sm">
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
            className="top-1/2 left-1 z-30 absolute flex justify-center items-center bg-white hover:bg-black text-black hover:text-white w-9 h-9 -translate-y-1/2 transition-colors"
          >
            <ChevronLeftIcon config={{ size: 14 }} />
          </button>

          <button
            type="button"
            onClick={handleNext}
            aria-label={t('common.next')}
            className="top-1/2 right-1 z-30 absolute flex justify-center items-center bg-white hover:bg-black text-black hover:text-white w-9 h-9 -translate-y-1/2 transition-colors"
          >
            <ChevronRightIcon config={{ size: 14 }} />
          </button>
        </>
      ) : null}

      <div
        ref={containerRef}
        className="hidden-show-scrollbar flex items-stretch gap-[12px] w-full h-full overflow-x-auto select-none"
      >
        {items.map((s, idx) => (
          <div
            key={`${s.id}-${idx}`}
            style={{ minWidth: `${CARD_W}px`, width: `${CARD_W}px` }}
            className="flex-shrink-0 h-full"
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
    <article className="flex flex-col bg-white h-full text-black">
      <div className="relative flex-1 min-h-0 overflow-hidden">
        <MediaImage
          src={toMediaUrl(offer.product.mainImage)}
          alt={offer.product.name}
          fill
          className="object-cover"
          sizes="240px"
          loading="lazy"
        />
        {offer.product.discountId > 0 && (
          <div className="top-2 absolute bg-black px-2 py-1 text-white text-xs end-2">
            <span>{offer.product.discountAmount}</span>{' '}
            <span>{t('common.tomanShort')}</span>
          </div>
        )}
        <div className="bottom-0 absolute inset-x-0 bg-black/60 px-2 py-1 text-[11px] text-white text-center">
          <CountdownDisplayClient targetTime={target} />
        </div>
      </div>
      <div className="flex flex-col gap-2 p-3">
        <h3 className="font-normal text-sm line-clamp-1">{offer.product.name}</h3>
        <div className="flex justify-between items-center gap-2">
          <span className="font-medium text-sm">
            {t('common.priceColon', { price: offer.product.finalPrice })}
          </span>
          <button
            type="button"
            onClick={onAdd}
            className="inline-flex items-center gap-1 bg-black hover:bg-white px-3 py-1.5 border border-black text-white hover:text-black text-xs transition-colors"
          >
            <ShoppingCartIcon />
            {t('common.add')}
          </button>
        </div>
      </div>
    </article>
  );
}
