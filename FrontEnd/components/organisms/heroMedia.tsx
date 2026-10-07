'use client';

import React, { useEffect, useRef, useState } from 'react';

import { getImageProps } from 'next/image';

import { isUploadMediaPath } from '@utils/toMediaUrl';

export type HeroMediaSet = {
  /** Poster / fallback image. Rendered first, so it is the LCP element. */
  image: string;
  /** Optional looping background video (muted, inline). */
  video?: string;
};

type Props = {
  desktop: HeroMediaSet;
  /** Phone version (< 768px). Missing parts fall back to the desktop ones (and vice versa for video). */
  mobile?: Partial<HeroMediaSet>;
  alt: string;
  playLabel: string;
  pauseLabel: string;
  /** Hide the dark readability scrim (e.g. on the sign-in split screen). */
  noScrim?: boolean;
};

type NetworkInformation = { saveData?: boolean; effectiveType?: string };

const MOBILE_QUERY = '(max-width: 767px)';

/**
 * Full-bleed hero media with separate desktop and mobile assets, each video with
 * its own poster. The poster is a <picture> (art-directed, so a phone never downloads
 * the desktop image); the video is only attached on the client, after choosing the
 * right file for the viewport, and only when it is safe: no reduced-motion preference,
 * no data-saver and not on a 2G-class connection. Users can always pause it (WCAG 2.2.2).
 */
export default function HeroMedia({
  desktop,
  mobile,
  alt,
  playLabel,
  pauseLabel,
  noScrim = false,
}: Props) {
  const videoRef = useRef<HTMLVideoElement>(null);
  const [allowVideo, setAllowVideo] = useState(false);
  const [isMobile, setIsMobile] = useState(false);
  const [ready, setReady] = useState(false);
  const [playing, setPlaying] = useState(true);

  const mobileImage = mobile?.image || desktop.image;
  // Each viewport falls back to the other one's video, so a single uploaded video is
  // used everywhere (the poster still comes from that viewport's own image).
  const mobileVideo = mobile?.video || desktop.video;
  const desktopVideo = desktop.video || mobile?.video;
  const hasMobileArt = mobileImage !== desktop.image;
  const anyVideo = Boolean(desktopVideo || mobileVideo);

  useEffect(() => {
    const mq = window.matchMedia(MOBILE_QUERY);
    const sync = () => setIsMobile(mq.matches);
    sync();
    mq.addEventListener('change', sync);
    return () => mq.removeEventListener('change', sync);
  }, []);

  useEffect(() => {
    if (!anyVideo) return;
    const reduce = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    const conn = (navigator as Navigator & { connection?: NetworkInformation }).connection;
    const slow = Boolean(conn?.saveData) || /(^|-)2g$/.test(conn?.effectiveType ?? '');
    setAllowVideo(!reduce && !slow);
  }, [anyVideo]);

  const videoSrc = isMobile ? mobileVideo : desktopVideo;
  const posterSrc = isMobile ? mobileImage : desktop.image;

  // A different file (viewport change) starts hidden again until it plays.
  useEffect(() => {
    setReady(false);
  }, [videoSrc]);

  const toggle = () => {
    const el = videoRef.current;
    if (!el) return;
    if (el.paused) {
      void el.play();
    } else {
      el.pause();
    }
  };

  const build = (src: string) =>
    getImageProps({
      src,
      alt,
      fill: true,
      priority: true,
      fetchPriority: 'high',
      sizes: '100vw',
      unoptimized: isUploadMediaPath(src),
      className: 'object-cover',
    }).props;

  const desktopImg = build(desktop.image);
  const mobileImg = hasMobileArt ? build(mobileImage) : null;

  return (
    <div className="absolute inset-0">
      {mobileImg ? (
        <link
          rel="preload"
          as="image"
          href={mobileImg.src}
          imageSrcSet={mobileImg.srcSet}
          imageSizes={mobileImg.sizes}
          media={MOBILE_QUERY}
          fetchPriority="high"
        />
      ) : null}
      <link
        rel="preload"
        as="image"
        href={desktopImg.src}
        imageSrcSet={desktopImg.srcSet}
        imageSizes={desktopImg.sizes}
        media={mobileImg ? '(min-width: 768px)' : undefined}
        fetchPriority="high"
      />

      <picture>
        {mobileImg ? (
          <source media={MOBILE_QUERY} srcSet={mobileImg.srcSet ?? mobileImg.src} sizes="100vw" />
        ) : null}
        <img {...desktopImg} alt={alt} />
      </picture>

      {allowVideo && videoSrc && (
        <video
          key={videoSrc}
          ref={videoRef}
          src={videoSrc}
          poster={posterSrc}
          autoPlay
          muted
          loop
          playsInline
          preload="metadata"
          aria-hidden
          tabIndex={-1}
          onPlaying={() => {
            setReady(true);
            setPlaying(true);
          }}
          onPause={() => setPlaying(false)}
          className={`absolute inset-0 w-full h-full object-cover transition-opacity duration-700 ${
            ready ? 'opacity-100' : 'opacity-0'
          }`}
        />
      )}

      {!noScrim && (
        <div className="absolute inset-0 bg-gradient-to-t from-black/60 via-black/10 to-black/40" />
      )}

      {allowVideo && videoSrc && ready && (
        <button
          type="button"
          onClick={toggle}
          aria-label={playing ? pauseLabel : playLabel}
          className="end-4 sm:end-8 bottom-4 sm:bottom-8 z-20 absolute inline-flex justify-center items-center bg-black/30 hover:bg-black/60 backdrop-blur-sm border border-white/60 rounded-full w-10 h-10 text-white transition-colors"
        >
          {playing ? (
            <svg width="14" height="14" viewBox="0 0 14 14" fill="currentColor" aria-hidden>
              <rect x="2" y="1" width="3.5" height="12" />
              <rect x="8.5" y="1" width="3.5" height="12" />
            </svg>
          ) : (
            <svg width="14" height="14" viewBox="0 0 14 14" fill="currentColor" aria-hidden>
              <path d="M3 1l10 6-10 6z" />
            </svg>
          )}
        </button>
      )}
    </div>
  );
}
