import React from 'react';

import Image from 'next/image';

type BrandLogoProps = {
  className?: string;
  /** Force the light-on-dark rendering (e.g. over a hero image). */
  onDark?: boolean;
  priority?: boolean;
  alt: string;
};

/**
 * The Rooshak logo is an opaque image on a white canvas; the `brand-logo`
 * class (globals.css) blends the white away and adapts it to dark surfaces.
 */
const BrandLogo: React.FC<BrandLogoProps> = ({
  className = '',
  onDark = false,
  priority = false,
  alt,
}) => (
  <Image
    src="/brand/rooshak-logo.png"
    alt={alt}
    width={921}
    height={301}
    priority={priority}
    data-on-dark={onDark}
    className={`brand-logo h-auto ${className}`}
  />
);

export default BrandLogo;
