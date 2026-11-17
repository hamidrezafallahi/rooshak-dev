'use client';

import React, {
  useCallback,
  useEffect,
  useRef,
  useState,
} from 'react';

import { useTranslations } from 'next-intl';
import {
  usePathname,
  useRouter,
} from 'next/navigation';

import { DynamicSelect } from '@components/atoms/defaultElements/dynamicSelect';
import { toMediaUrl } from '@utils/toMediaUrl';
import {
  showErrorToast,
  showSuccessToast,
} from '@utils/core';

import {
  addProductImage,
  deleteProductImage,
  downloadInfo,
  GalleryImage,
  getProductGallery,
  refreshStorefrontImages,
  replaceProductImage,
  setMainProductImage,
  setProductImageActive,
} from './api';
import RecentImages from './RecentImages';
import './gallery.css';

const MAX_BYTES = 10 * 1024 * 1024;

type Upload = {
  key: string;
  file: File;
  preview: string;
  status: 'uploading' | 'queued' | 'failed';
  error?: string;
};

type IconName = 'download' | 'star' | 'swap' | 'eye' | 'eyeOff' | 'trash' | 'upload' | 'retry' | 'close' | 'image';

const PATHS: Record<IconName, React.ReactNode> = {
  download: <><path d="M12 4v11.5" /><path d="M7.5 11L12 15.5 16.5 11" /><path d="M4 15v4.5h16V15" /></>,
  star: <path d="M12 3.5l2.6 5.3 5.9.9-4.3 4.1 1 5.8L12 16.9l-5.2 2.7 1-5.8-4.3-4.1 5.9-.9z" />,
  swap: <><path d="M4 8h13l-3.5-3.5" /><path d="M20 16H7l3.5 3.5" /></>,
  eye: <><path d="M2.5 12S6 5.5 12 5.5 21.5 12 21.5 12 18 18.5 12 18.5 2.5 12 2.5 12z" /><circle cx="12" cy="12" r="3" /></>,
  eyeOff: <><path d="M3 3l18 18" /><path d="M10.6 6A9.7 9.7 0 0112 5.5c6 0 9.5 6.5 9.5 6.5a17 17 0 01-2.9 3.6M6.4 7.6C3.9 9.3 2.5 12 2.5 12S6 18.5 12 18.5c1.6 0 3-.4 4.2-1" /><path d="M9.9 9.9a3 3 0 004.2 4.2" /></>,
  trash: <><path d="M4 7h16" /><path d="M9.5 7V4.5h5V7" /><path d="M6.5 7l1 13h9l1-13" /></>,
  upload: <><path d="M12 15.5V4" /><path d="M7.5 8.5L12 4l4.5 4.5" /><path d="M4 15v4.5h16V15" /></>,
  retry: <><path d="M4.5 12a7.5 7.5 0 0113-5.1L20 9.5" /><path d="M20 4.5v5h-5" /><path d="M19.5 12a7.5 7.5 0 01-13 5.1L4 14.5" /><path d="M4 19.5v-5h5" /></>,
  close: <><path d="M6 6l12 12" /><path d="M18 6L6 18" /></>,
  image: <><rect x="3.5" y="4.5" width="17" height="15" /><circle cx="9" cy="10" r="1.8" /><path d="M20.5 16l-5-5-9 8.5" /></>,
};

function Icon({ name, filled = false, size = 18 }: { name: IconName; filled?: boolean; size?: number }) {
  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 24 24"
      fill={filled ? 'currentColor' : 'none'}
      stroke="currentColor"
      strokeWidth={1.6}
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden
    >
      {PATHS[name]}
    </svg>
  );
}

function ToolButton({
  icon,
  label,
  onClick,
  danger = false,
  active = false,
  disabled = false,
}: {
  icon: IconName;
  label: string;
  onClick: () => void;
  danger?: boolean;
  active?: boolean;
  disabled?: boolean;
}) {
  return (
    <button
      type="button"
      title={label}
      aria-label={label}
      aria-pressed={active || undefined}
      disabled={disabled}
      onClick={onClick}
      className={`pig-tool${danger ? ' pig-tool-danger' : ''}${active ? ' pig-tool-active' : ''}`}
    >
      <Icon name={icon} filled={active && icon === 'star'} />
    </button>
  );
}

const hasFiles = (e: React.DragEvent) => Array.from(e.dataTransfer.types).includes('Files');

export default function ProductImageStudio({ initialProductId }: { initialProductId?: number }) {
  const t = useTranslations('admin.productImages');
  const router = useRouter();
  const pathname = usePathname();

  const [productId, setProductId] = useState<number | undefined>(initialProductId);
  const [images, setImages] = useState<GalleryImage[]>([]);
  const [loading, setLoading] = useState(false);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [busy, setBusy] = useState<Record<number, boolean>>({});
  const [swapPreview, setSwapPreview] = useState<Record<number, string>>({});
  const [confirmDelete, setConfirmDelete] = useState<number | null>(null);
  const [uploads, setUploads] = useState<Upload[]>([]);
  const [dropActive, setDropActive] = useState(false);
  const [tileDrop, setTileDrop] = useState<number | null>(null);
  const [focusImageId, setFocusImageId] = useState<number | null>(null);
  const [recentKey, setRecentKey] = useState(0);
  const galleryTop = useRef<HTMLDivElement>(null);

  const addInput = useRef<HTMLInputElement>(null);
  const swapInput = useRef<HTMLInputElement>(null);
  const swapTarget = useRef<number | null>(null);
  const queueRunning = useRef(false);
  const productRef = useRef(productId);
  productRef.current = productId;

  const load = useCallback(async (id: number, quiet = false) => {
    if (!quiet) setLoading(true);
    const res = await getProductGallery(id);
    if (productRef.current !== id) return;
    if (res.isSuccess) {
      setImages(res.data ?? []);
      setLoadError(null);
    } else {
      setLoadError(res.error ?? t('loadFailed'));
    }
    setLoading(false);
  }, [t]);

  useEffect(() => {
    setImages([]);
    setUploads([]);
    setConfirmDelete(null);
    setLoadError(null);
    if (productId) load(productId);
  }, [productId, load]);

  // Local previews are object URLs; release them when they leave the screen.
  useEffect(() => () => {
    uploads.forEach((u) => URL.revokeObjectURL(u.preview));
    Object.values(swapPreview).forEach((url) => URL.revokeObjectURL(url));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const pickProduct = (value: string | number) => {
    const id = Number(value);
    if (!Number.isFinite(id) || id <= 0) return;
    setFocusImageId(null);
    setProductId(id);
    router.replace(`${pathname}?productId=${id}`, { scroll: false });
  };

  // A click in the recent list opens that image's product and marks the image in its gallery.
  const pickFromList = (img: GalleryImage) => {
    if (img.productId !== productId) pickProduct(img.productId);
    setFocusImageId(img.id);
    if (!window.matchMedia('(min-width: 1024px)').matches) {
      galleryTop.current?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
  };

  // With no product in the URL, open the product of the newest image instead of an empty page.
  const openNewest = (img: GalleryImage) => {
    if (productRef.current) return;
    pickProduct(img.productId);
  };

  const validate = (file: File): string | null => {
    if (!file.type.startsWith('image/')) return t('notImage', { name: file.name });
    if (file.size > MAX_BYTES) return t('tooLarge', { name: file.name });
    return null;
  };

  const withBusy = async (id: number, job: () => Promise<boolean>) => {
    setBusy((b) => ({ ...b, [id]: true }));
    try {
      const ok = await job();
      if (ok && productId) {
        await load(productId, true);
        setRecentKey((k) => k + 1);
        refreshStorefrontImages();
      }
    } finally {
      setBusy((b) => {
        const next = { ...b };
        delete next[id];
        return next;
      });
    }
  };

  const report = (res: { isSuccess: boolean; error?: string | null }, success: string) => {
    if (res.isSuccess) showSuccessToast(success);
    else showErrorToast(res.error || t('saveFailed'));
    return res.isSuccess;
  };

  const makeMain = (img: GalleryImage) =>
    withBusy(img.id, async () => report(await setMainProductImage(img.id), t('mainChanged')));

  const toggleActive = (img: GalleryImage) =>
    withBusy(img.id, async () =>
      report(await setProductImageActive(img.id, !img.isActive), img.isActive ? t('hiddenDone') : t('shownDone')),
    );

  const remove = (img: GalleryImage) => {
    setConfirmDelete(null);
    return withBusy(img.id, async () => report(await deleteProductImage(img.id), t('deleted')));
  };

  const swapFile = (img: GalleryImage, file: File) => {
    const problem = validate(file);
    if (problem) {
      showErrorToast(problem);
      return;
    }
    const preview = URL.createObjectURL(file);
    setSwapPreview((p) => ({ ...p, [img.id]: preview }));
    withBusy(img.id, async () => report(await replaceProductImage(img.id, file), t('replaced'))).finally(() => {
      URL.revokeObjectURL(preview);
      setSwapPreview((p) => {
        const next = { ...p };
        delete next[img.id];
        return next;
      });
    });
  };

  // Uploads go one at a time so each file shows its own state and a failure stops nothing else.
  const runQueue = useCallback(async () => {
    if (queueRunning.current) return;
    queueRunning.current = true;
    try {
      for (;;) {
        const id = productRef.current;
        const next = await new Promise<Upload | undefined>((resolve) =>
          setUploads((list) => {
            const item = list.find((u) => u.status === 'queued');
            resolve(item);
            return item ? list.map((u) => (u.key === item.key ? { ...u, status: 'uploading' } : u)) : list;
          }),
        );
        if (!next || !id) break;
        const res = await addProductImage(id, next.file);
        if (productRef.current !== id) break;
        if (res.isSuccess) {
          URL.revokeObjectURL(next.preview);
          await load(id, true);
          setUploads((list) => list.filter((u) => u.key !== next.key));
        } else {
          setUploads((list) =>
            list.map((u) => (u.key === next.key ? { ...u, status: 'failed', error: res.error || t('uploadFailed') } : u)),
          );
        }
      }
      setRecentKey((k) => k + 1);
      refreshStorefrontImages();
    } finally {
      queueRunning.current = false;
    }
  }, [load, t]);

  const enqueue = (files: FileList | File[] | null) => {
    if (!files || !productId) return;
    const accepted: Upload[] = [];
    Array.from(files).forEach((file) => {
      const problem = validate(file);
      if (problem) {
        showErrorToast(problem);
        return;
      }
      accepted.push({
        key: `${Date.now()}-${Math.random().toString(36).slice(2)}`,
        file,
        preview: URL.createObjectURL(file),
        status: 'queued',
      });
    });
    if (!accepted.length) return;
    setUploads((list) => [...list, ...accepted]);
    setTimeout(runQueue, 0);
  };

  const retry = (key: string) => {
    setUploads((list) => list.map((u) => (u.key === key ? { ...u, status: 'queued', error: undefined } : u)));
    setTimeout(runQueue, 0);
  };

  const dismiss = (key: string) =>
    setUploads((list) => {
      const item = list.find((u) => u.key === key);
      if (item) URL.revokeObjectURL(item.preview);
      return list.filter((u) => u.key !== key);
    });

  const openSwap = (id: number) => {
    swapTarget.current = id;
    swapInput.current?.click();
  };

  const productName = images.find((i) => i.productName)?.productName;
  const busyUploading = uploads.some((u) => u.status !== 'failed');
  const isEmpty = !loading && !loadError && images.length === 0 && uploads.length === 0;

  return (
    <div className="admin-page pig">
      <header className="admin-page-header">
        <div>
          <h1 className="admin-page-title">{t('title')}</h1>
          <p className="admin-page-subtitle max-w-2xl">{t('subtitle')}</p>
        </div>
        {productId && (
          <button
            type="button"
            className="admin-btn admin-btn-primary"
            onClick={() => addInput.current?.click()}
          >
            <Icon name="upload" />
            {t('addImages')}
          </button>
        )}
      </header>

      <section className="admin-panel pig-picker">
        <label className="pig-picker-label">{t('pickProduct')}</label>
        <div className="pig-picker-select">
          <DynamicSelect
            options={[]}
            fetchConfig={{ api: 'Products/selectOption' }}
            fetchSize={20}
            placeholder={productName ?? t('pickProductPlaceholder')}
            value={productId}
            onChange={pickProduct}
          />
        </div>
        {productId && (
          <p className="pig-picker-meta">
            {productName && <strong>{productName}</strong>}
            <span>{t('count', { count: images.length })}</span>
          </p>
        )}
      </section>

      <div className="pig-layout">
      <div className="pig-main" ref={galleryTop}>
      {!productId && (
        <div className="admin-panel admin-empty pig-empty">
          <span className="pig-empty-icon"><Icon name="image" size={34} /></span>
          <p className="admin-empty-title">{t('pickProductHint')}</p>
        </div>
      )}

      {productId && loadError && (
        <div className="admin-panel admin-empty pig-empty">
          <p className="admin-empty-title">{loadError}</p>
          <button type="button" className="admin-btn" onClick={() => load(productId)}>
            <Icon name="retry" />
            {t('retry')}
          </button>
        </div>
      )}

      {productId && loading && (
        <div className="pig-grid" aria-busy>
          {[0, 1, 2, 3, 4].map((i) => (
            <div key={i} className={`pig-tile pig-skeleton${i === 0 ? ' pig-tile-main' : ''}`} />
          ))}
        </div>
      )}

      {productId && !loading && !loadError && (
        <div
          className={`pig-grid${dropActive ? ' pig-grid-drop' : ''}`}
          onDragOver={(e) => {
            if (!hasFiles(e)) return;
            e.preventDefault();
            if (tileDrop === null) setDropActive(true);
          }}
          onDragLeave={(e) => {
            if (!e.currentTarget.contains(e.relatedTarget as Node)) setDropActive(false);
          }}
          onDrop={(e) => {
            if (!hasFiles(e)) return;
            e.preventDefault();
            setDropActive(false);
            enqueue(e.dataTransfer.files);
          }}
        >
          {images.map((img) => {
            const isBusy = Boolean(busy[img.id]);
            const src = swapPreview[img.id] ?? toMediaUrl(img.productImageUrl);
            const dl = downloadInfo(img, productName);
            return (
              <figure
                key={img.id}
                className={`pig-tile${img.isMain ? ' pig-tile-main' : ''}${!img.isActive ? ' pig-tile-hidden' : ''}${tileDrop === img.id ? ' pig-tile-drop' : ''}${focusImageId === img.id ? ' pig-tile-focus' : ''}`}
                onDragOver={(e) => {
                  if (!hasFiles(e) || isBusy) return;
                  e.preventDefault();
                  e.stopPropagation();
                  setDropActive(false);
                  setTileDrop(img.id);
                }}
                onDragLeave={(e) => {
                  if (!e.currentTarget.contains(e.relatedTarget as Node)) setTileDrop(null);
                }}
                onDrop={(e) => {
                  if (!hasFiles(e)) return;
                  e.preventDefault();
                  e.stopPropagation();
                  setTileDrop(null);
                  const file = e.dataTransfer.files[0];
                  if (file) swapFile(img, file);
                }}
              >
                <div className="pig-frame">
                  {/* eslint-disable-next-line @next/next/no-img-element */}
                  <img src={src} alt={productName ? `${productName} - ${img.id}` : ''} loading="lazy" />
                  <div className="pig-badges">
                    {img.isMain && <span className="pig-badge pig-badge-main"><Icon name="star" filled size={13} />{t('main')}</span>}
                    {!img.isActive && <span className="pig-badge">{t('hidden')}</span>}
                  </div>
                  {tileDrop === img.id && <div className="pig-veil">{t('dropToReplace')}</div>}
                  {isBusy && <div className="pig-veil"><span className="admin-spinner" /></div>}
                </div>

                <figcaption className="pig-bar">
                  {confirmDelete === img.id ? (
                    <div className="pig-confirm">
                      <span>{t('confirmDelete')}</span>
                      <div className="flex gap-1">
                        <button type="button" className="pig-text-btn pig-text-btn-danger" onClick={() => remove(img)}>
                          {t('delete')}
                        </button>
                        <button type="button" className="pig-text-btn" onClick={() => setConfirmDelete(null)}>
                          {t('cancel')}
                        </button>
                      </div>
                    </div>
                  ) : (
                    <>
                      {img.isMain ? (
                        <span className="pig-main-note">{t('mainHint')}</span>
                      ) : (
                        <ToolButton icon="star" label={t('setMain')} onClick={() => makeMain(img)} disabled={isBusy} />
                      )}
                      <div className="pig-bar-tools">
                        <a
                          className="pig-tool"
                          href={toMediaUrl(dl.path)}
                          download={dl.fileName}
                          title={dl.isOriginal ? t('download') : t('downloadCompressed')}
                          aria-label={dl.isOriginal ? t('download') : t('downloadCompressed')}
                        >
                          <Icon name="download" />
                        </a>
                        <ToolButton icon="swap" label={t('replace')} onClick={() => openSwap(img.id)} disabled={isBusy} />
                        <ToolButton
                          icon={img.isActive ? 'eye' : 'eyeOff'}
                          label={img.isActive ? t('hide') : t('show')}
                          onClick={() => toggleActive(img)}
                          disabled={isBusy || (img.isMain && img.isActive)}
                        />
                        <ToolButton icon="trash" label={t('delete')} danger onClick={() => setConfirmDelete(img.id)} disabled={isBusy} />
                      </div>
                    </>
                  )}
                </figcaption>
              </figure>
            );
          })}

          {uploads.map((u) => (
            <figure key={u.key} className={`pig-tile pig-tile-pending${u.status === 'failed' ? ' pig-tile-failed' : ''}`}>
              <div className="pig-frame">
                {/* eslint-disable-next-line @next/next/no-img-element */}
                <img src={u.preview} alt="" />
                <div className="pig-veil">
                  {u.status === 'failed' ? (
                    <span className="pig-fail-text">{u.error}</span>
                  ) : (
                    <>
                      <span className="admin-spinner" />
                      <span>{u.status === 'uploading' ? t('uploading') : t('queued')}</span>
                    </>
                  )}
                </div>
              </div>
              <figcaption className="pig-bar">
                <span className="pig-file-name" title={u.file.name}>{u.file.name}</span>
                {u.status === 'failed' && (
                  <div className="pig-bar-tools">
                    <ToolButton icon="retry" label={t('retry')} onClick={() => retry(u.key)} />
                    <ToolButton icon="close" label={t('dismiss')} onClick={() => dismiss(u.key)} />
                  </div>
                )}
              </figcaption>
            </figure>
          ))}

          <button
            type="button"
            className={`pig-tile pig-add${isEmpty ? ' pig-add-wide' : ''}`}
            onClick={() => addInput.current?.click()}
          >
            <span className="pig-add-icon"><Icon name="upload" size={26} /></span>
            <span className="pig-add-title">{isEmpty ? t('emptyTitle') : t('dropTitle')}</span>
            <span className="pig-add-hint">{isEmpty ? t('emptyHint') : t('dropHint')}</span>
            {busyUploading && <span className="admin-progress w-2/3"><span className="admin-progress-bar admin-progress-indeterminate" /></span>}
          </button>
        </div>
      )}
      </div>

      <RecentImages
        selectedProductId={productId}
        activeImageId={focusImageId}
        refreshKey={recentKey}
        onPick={pickFromList}
        onFirstImage={initialProductId ? undefined : openNewest}
      />
      </div>

      <input
        ref={addInput}
        type="file"
        accept="image/*"
        multiple
        hidden
        onChange={(e) => {
          enqueue(e.target.files);
          e.target.value = '';
        }}
      />
      <input
        ref={swapInput}
        type="file"
        accept="image/*"
        hidden
        onChange={(e) => {
          const file = e.target.files?.[0];
          const img = images.find((i) => i.id === swapTarget.current);
          if (file && img) swapFile(img, file);
          e.target.value = '';
        }}
      />
    </div>
  );
}
