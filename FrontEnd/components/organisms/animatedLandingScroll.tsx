"use client";
import React, { useRef, useEffect } from "react";

import { useTranslations } from 'next-intl';

export default function AnimatedLandingScroll() {
  const t = useTranslations('animatedLanding');
  const containerRef = useRef<HTMLDivElement>(null);
  const logoRef = useRef<HTMLDivElement>(null);
  const desc1Ref = useRef<HTMLDivElement>(null);
  const desc2Ref = useRef<HTMLDivElement>(null);
  const part1Ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const container = containerRef.current;
    const logo = logoRef.current;
    const desc1 = desc1Ref.current;
    const desc2 = desc2Ref.current;
    const part1 = part1Ref.current;

    if (!container || !logo || !desc1 || !desc2 || !part1) return;

    const handleScroll = () => {
      const scrollPosition = container.scrollTop + container.offsetHeight;
      const threshold = container.offsetHeight * 1.5;
      const contentHeight = part1.scrollHeight;

      const isInTargetRange =
        scrollPosition > threshold && scrollPosition < contentHeight;

      if (isInTargetRange) {
        logo.style.width = "100px";
        desc1.classList.remove("animate-popUpDisappear");
        desc2.classList.remove("animate-popUpDisappear");

        setTimeout(() => {
          desc1.classList.add("animate-popUpAppear");
        }, 300);
        setTimeout(() => {
          desc2.classList.add("animate-popUpAppear");
        }, 600);
      } else {
        logo.style.width = "0";
        desc1.classList.remove("animate-popUpAppear");
        desc2.classList.remove("animate-popUpAppear");
        desc1.classList.add("animate-popUpDisappear");
        desc2.classList.add("animate-popUpDisappear");
      }
    };

    container.addEventListener("scroll", handleScroll);
    return () => container.removeEventListener("scroll", handleScroll);
  }, []);

  return (
    <div
      ref={containerRef}
      className="h-screen overflow-y-auto scrollbarHidden"
    >
      <div ref={part1Ref}>
        <div className="flex justify-center items-center bg-gray-700 h-screen text-white">
          {t('brandName')}
        </div>
        <div className="relative bg-green-900 h-[200dvh]">
          <div className="top-[50dvh] sticky flex justify-center items-center">
            <div className="flex flex-col justify-center items-center gap-10 p-10">
              <div
                ref={logoRef}
                className="bg-red-700 py-2 rounded h-10 overflow-hidden text-white text-center transition-all duration-300"
              >
                {t('brandName')}
              </div>
              <div className="flex justify-center items-end gap-10 bg-red-400 h-20">
                <div
                  ref={desc1Ref}
                  className="relative bg-red-700 opacity-0 rounded w-20 h-10 overflow-hidden text-white text-center"
                >
                  {t('software')} desc1Ref
                </div>
                <div
                  ref={desc2Ref}
                  className="bg-red-700 opacity-0 rounded w-20 h-10 overflow-hidden text-white text-center"
                >
                  {t('software')} desc2Ref
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      <div className="h-screen overflow-y-auto snap-mandatory snap-y scrollbarHidden">
        <div className="bg-blue-200 h-screen snap-start"></div>
        <div className="bg-green-400 h-screen snap-start"></div>
        <div className="bg-yellow-400 h-screen snap-start"></div>
      </div>
    </div>
  );
}
