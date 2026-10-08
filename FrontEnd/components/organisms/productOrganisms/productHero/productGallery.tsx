'use client';

import {
  useCallback,
  useEffect,
  useState,
} from 'react';

import { useTranslations } from 'next-intl';
import dynamic from 'next/dynamic';

import MediaImage from '@components/atoms/MediaImage';
import QrDialog from '@components/molecules/qrDialog';
import { IDetailedProduct } from '@models/product';
import { toMediaUrl } from '@utils/toMediaUrl';

// The 3D viewer (model-viewer + three.js) is only downloaded once the visitor asks for it.
const Product3DViewer = dynamic(() => import('./product3DViewer'), { ssr: false });

export default function ProductGallery({ product }: { product: IDetailedProduct }) {
  const t = useTranslations('model3d');
  const [activeImage, setActiveImage] = useState<string>(toMediaUrl(product?.mainImage));
  const [show3d, setShow3d] = useState(false);
  const [arRequest, setArRequest] = useState(0);
  const [qrOpen, setQrOpen] = useState(false);
  const [pageUrl, setPageUrl] = useState('');

  const model = product.model3D;

  useEffect(() => {
    if (product?.mainImage) {
      setActiveImage(toMediaUrl(product.mainImage));
    }
  }, [product?.mainImage]);

  // A phone opened from the desktop QR code lands directly in the 3D view.
  useEffect(() => {
    const url = new URL(window.location.href);
    url.searchParams.set('view', '3d');
    setPageUrl(url.toString());
    if (model && new URLSearchParams(window.location.search).get('view') === '3d') {
      setShow3d(true);
    }
  }, [model]);

  const handleArUnavailable = useCallback(() => setQrOpen(true), []);

  const openAr = () => {
    setShow3d(true);
    setArRequest((n) => n + 1);
  };

  const defaultImage = '/images/default-product.jpg';
  const thumbs = product.imageUrls ?? [];

  return (
    <div className="flex flex-col gap-3">
      <div className="relative bg-store-muted aspect-square overflow-hidden">
        {show3d && model ? (
          <Product3DViewer
            modelUrl={model.modelUrl}
            usdzUrl={model.usdzUrl}
            poster={activeImage || undefined}
            alt={product.name || 'Product 3D model'}
            arRequest={arRequest}
            onArUnavailable={handleArUnavailable}
          />
        ) : (
          (activeImage || product?.mainImage) && (
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
          )
        )}
      </div>

      {model && (
        <div className="flex flex-col gap-2">
          <div className="flex flex-wrap items-center gap-2">
            <button
              type="button"
              onClick={() => setShow3d((v) => !v)}
              aria-pressed={show3d}
              className="border border-store-strong px-4 py-2 text-sm transition-colors hover:bg-store-muted"
            >
              {show3d ? t('viewImages') : t('view3d')}
            </button>
            <button
              type="button"
              onClick={openAr}
              className="bg-store-text px-4 py-2 text-sm text-store-surface transition-opacity hover:opacity-90"
            >
              {t('viewOnTable')}
            </button>
          </div>
          <p className="text-xs leading-5 text-store-subtle">
            {show3d ? t('hint') : t('arHint')}
          </p>
        </div>
      )}

      {thumbs.length > 1 && (
        <div className="hidden-show-scrollbar flex items-center gap-2 overflow-x-auto">
          {thumbs.map((img, index) => {
            const selected = !show3d && toMediaUrl(img) === activeImage;
            return (
              <button
                key={`${img}-${index}`}
                type="button"
                onClick={() => {
                  setShow3d(false);
                  setActiveImage(toMediaUrl(img));
                }}
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

      <QrDialog
        open={qrOpen}
        title={t('qrTitle')}
        body={`${t('qrBody')} ${t('arUnsupported')}`}
        url={pageUrl}
        closeLabel={t('close')}
        onClose={() => setQrOpen(false)}
      />
    </div>
  );
}
