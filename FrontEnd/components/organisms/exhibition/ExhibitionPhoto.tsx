'use client';

import Image from 'next/image';
import { useState } from 'react';

type Props = {
  src: string;
  alt: string;
  width: number;
  height: number;
  priority?: boolean;
};

/**
 * Portrait exhibition photo with reserved aspect ratio + skeleton while decoding.
 */
export default function ExhibitionPhoto({
  src,
  alt,
  width,
  height,
  priority = false,
}: Props) {
  const [loaded, setLoaded] = useState(false);

  return (
    <figure
      className="exhibit-photo"
      style={{ aspectRatio: `${width} / ${height}` }}
    >
      {!loaded && (
        <div className="exhibit-photo-skeleton" aria-hidden>
          <span className="exhibit-photo-spinner" />
        </div>
      )}
      <Image
        src={src}
        alt={alt}
        width={width}
        height={height}
        className={`exhibit-photo-img${loaded ? ' is-loaded' : ''}`}
        sizes="(max-width: 1200px) 100vw, 1200px"
        quality={75}
        priority={priority}
        onLoad={() => setLoaded(true)}
      />
    </figure>
  );
}
