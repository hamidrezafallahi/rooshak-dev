'use client';

import {
  useEffect,
  useState,
} from 'react';

import MediaImage from '@components/atoms/MediaImage';
import { IDetailedProduct } from '@models/product';
import { toMediaUrl } from '@utils/toMediaUrl';

export default function ProductGallery({ product }: { product: IDetailedProduct }) {
  const [activeImage, setActiveImage] = useState<string>(toMediaUrl(product?.mainImage));

  useEffect(() => {
    if (product?.mainImage) {
      setActiveImage(toMediaUrl(product.mainImage));
    }
  }, [product?.mainImage]);
  const defaultImage = '/images/default-product.jpg';
  const thumbs = product.imageUrls ?? [];

  return (
    <div className="flex flex-col gap-3">
      <div className="relative bg-store-muted aspect-square overflow-hidden">
        {(activeImage || product?.mainImage) && (
          <MediaImage
            src={activeImage || product.mainImage}
            fallbackSrc={defaultImage}
            alt={product.name || 'Product image'}
            fill
            priority={true}
            sizes="(max-width: 1024px) 100vw, 55vw"
            quality={85}
            className="object-contain transition-opacity duration-300"
            loading="eager"
            onError={(e) => {
              const target = e.target as HTMLImageElement;
              target.src = defaultImage;
            }}
          />
        )}
      </div>

      {thumbs.length > 1 && (
        <div className="hidden-show-scrollbar flex items-center gap-2 overflow-x-auto">
          {thumbs.map((img, index) => {
            const selected = toMediaUrl(img) === activeImage;
            return (
              <button
                key={`${img}-${index}`}
                type="button"
                onClick={() => setActiveImage(toMediaUrl(img))}
                aria-current={selected}
                className={`relative flex-shrink-0 bg-store-muted w-16 sm:w-20 aspect-square overflow-hidden border transition-colors ${
                  selected ? 'border-store-strong' : 'border-transparent opacity-70 hover:opacity-100'
                }`}
                aria-label={`View image ${index + 1}`}
              >
                <MediaImage
                  src={img}
                  alt={`${product.name} - view ${index + 1}`}
                  fill
                  sizes="80px"
                  className="object-contain"
                  loading={index < 4 ? 'eager' : 'lazy'}
                  priority={false}
                />
              </button>
            );
          })}
        </div>
      )}
    </div>
  );
}
