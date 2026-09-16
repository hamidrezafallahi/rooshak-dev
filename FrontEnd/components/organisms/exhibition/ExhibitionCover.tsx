'use client';

import Image from 'next/image';
import { useState } from 'react';

type Props = {
  src: string;
  alt: string;
  width: number;
  height: number;
};

export default function ExhibitionCover({ src, alt, width, height }: Props) {
  const [loaded, setLoaded] = useState(false);

  return (
    <span
      className="exhibit-index-media"
      style={{ aspectRatio: `${width} / ${height}` }}
    >
      {!loaded && (
        <span className="exhibit-photo-skeleton" aria-hidden>
          <span className="exhibit-photo-spinner" />
        </span>
      )}
      <Image
        src={src}
        alt={alt}
        width={width}
        height={height}
        className={`exhibit-index-cover${loaded ? ' is-loaded' : ''}`}
        sizes="(max-width: 640px) 100vw, 420px"
        quality={70}
        priority
        onLoad={() => setLoaded(true)}
      />
    </span>
  );
}
