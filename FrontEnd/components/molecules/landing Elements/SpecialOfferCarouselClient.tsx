"use client";

import React, {
  useEffect,
  useRef,
} from 'react';

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

export default function SpecialOfferCarouselClient({ spacialOffers }: Props) {
  const containerRef = useRef<HTMLDivElement | null>(null);
  const leftBtnRef = useRef<HTMLButtonElement | null>(null);
  const rightBtnRef = useRef<HTMLButtonElement | null>(null);
  const isDragging = useRef(false);
  const startX = useRef(0);
  const scrollLeft = useRef(0);
  const CARD_W = 200;
  const GAP = 12;
  const step = CARD_W + GAP;
  const items = spacialOffers ?? [];
  const isAuthenticated = Boolean(getCookie("candySession"));
  const [itemMutate] = useGetConditionallyMutation();
  const dispatch = useDispatch();

  const handlePrev = () => {
    const el = containerRef.current;
    if (!el) return;
    el.scrollBy({ left: -step, behavior: "smooth" });
  };

  const handleNext = () => {
    const el = containerRef.current;
    if (!el) return;
    el.scrollBy({ left: step, behavior: "smooth" });
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

    // --- بررسی اسکرول‌پذیر بودن ---
    const checkScrollable = () => {
      const hasScroll = el.scrollWidth > el.clientWidth + 2;
      const displayValue = hasScroll ? "flex" : "none";
      if (leftBtnRef.current) leftBtnRef.current.style.display = displayValue;
      if (rightBtnRef.current) rightBtnRef.current.style.display = displayValue;
    };

    checkScrollable();
    window.addEventListener("resize", checkScrollable);

    // --- هندل drag-to-scroll ---
    const onMouseDown = (e: MouseEvent) => {
      isDragging.current = true;
      startX.current = e.pageX - el.offsetLeft;
      scrollLeft.current = el.scrollLeft;
      document.body.style.userSelect = "none"; // جلوگیری از انتخاب متن
    };

    const onMouseLeave = () => {
      isDragging.current = false;
      document.body.style.userSelect = "";
    };

    const onMouseUp = () => {
      isDragging.current = false;
      document.body.style.userSelect = "";
    };

    const onMouseMove = (e: MouseEvent) => {
      if (!isDragging.current) return;
      e.preventDefault();
      const x = e.pageX - el.offsetLeft;
      const walk = (x - startX.current) * 1; // سرعت اسکرول، هرچی بیشتر، سریع‌تر
      el.scrollLeft = scrollLeft.current - walk;
    };

    el.addEventListener("mousedown", onMouseDown);
    el.addEventListener("mouseleave", onMouseLeave);
    el.addEventListener("mouseup", onMouseUp);
    el.addEventListener("mousemove", onMouseMove);

    return () => {
      window.removeEventListener("resize", checkScrollable);
      el.removeEventListener("mousedown", onMouseDown);
      el.removeEventListener("mouseleave", onMouseLeave);
      el.removeEventListener("mouseup", onMouseUp);
      el.removeEventListener("mousemove", onMouseMove);
    };
  }, []);

  if (!items || items.length === 0) {
    return (
      <div className="flex justify-center items-center w-full h-full text-white/90 text-sm">
        پیشنهادی موجود نیست
      </div>
    );
  }

  return (
    <div className="relative w-full h-full">
      <button
        ref={leftBtnRef}
        onClick={handlePrev}
        aria-label="قبلی"
        style={{ display: "none" }}
        className="top-1/2 left-1 z-30 absolute justify-center items-center bg-white/70 hover:bg-white shadow rounded-full w-8 h-8 text-rose-600 -translate-y-1/2"
      >
        <ChevronLeftIcon config={{ size: 14 }} />
      </button>

      <button
        ref={rightBtnRef}
        onClick={handleNext}
        aria-label="بعدی"
        style={{ display: "none" }}
        className="top-1/2 right-1 z-30 absolute justify-center items-center bg-white/70 hover:bg-white shadow rounded-full w-8 h-8 text-rose-600 -translate-y-1/2"
      >
        <ChevronRightIcon config={{ size: 14 }} />
      </button>

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
          sizes="(max-width: 640px) 100vw, (max-width: 1024px) 50vw, 33vw"
        />
        {offer.product.discountId > 0 && (
          <div className="top-2 absolute bg-yellow-400 px-2 py-1 rounded text-rose-700 text-xs end-2">
            <span>{offer.product.discountAmount}</span>
            <span className="text-xs">{"ت"}</span>
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
            قیمت : {offer.product.finalPrice}
          </div>
          <button
            onClick={onAdd}
            className="flex items-center gap-1 bg-rose-600 hover:bg-rose-700 px-2 py-1 rounded text-white text-xs"
          >
            <ShoppingCartIcon />
            افزودن
          </button>
        </div>
      </div>
    </article>
  );
}
