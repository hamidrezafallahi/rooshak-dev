'use client';

import React, { useEffect, useRef, useState } from 'react';

import MediaImage from '@components/atoms/MediaImage';

type Props = {
  /** Poster / fallback image. Rendered first, so it is the LCP element. */
  imageSrc: string;
  /** Optional looping background video (muted, inline). */
  videoSrc?: string;
  alt: string;
  playLabel: string;
  pauseLabel: string;
};

type NetworkInformation = { saveData?: boolean; effectiveType?: string };

/**
 * Full-bleed hero media. The image paints immediately (priority); the video is only
 * attached on the client when it is safe: no reduced-motion preference, no data-saver,
 * and not on a 2G-class connection. Users can always pause it (WCAG 2.2.2).
 */
export default function HeroMedia({
  imageSrc,
  videoSrc,
  alt,
  playLabel,
  pauseLabel,
}: Props) {
  const videoRef = useRef<HTMLVideoElement>(null);
  const [allowVideo, setAllowVideo] = useState(false);
  const [ready, setReady] = useState(false);
  const [playing, setPlaying] = useState(true);

  useEffect(() => {
    if (!videoSrc) return;
    const reduce = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    const conn = (navigator as Navigator & { connection?: NetworkInformation })
      .connection;
    const slow = Boolean(conn?.saveData) || /(^|-)2g$/.test(conn?.effectiveType ?? '');
    setAllowVideo(!reduce && !slow);
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

  return (
    <div className="absolute inset-0">
      <MediaImage
        src={imageSrc}
        alt={alt}
        fill
        priority
        fetchPriority="high"
        sizes="100vw"
        className="object-cover"
      />

      {allowVideo && videoSrc && (
        <video
          ref={videoRef}
          src={videoSrc}
          poster={imageSrc}
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

      {/* readability scrim */}
      <div className="absolute inset-0 bg-gradient-to-t from-black/60 via-black/10 to-black/40" />

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
