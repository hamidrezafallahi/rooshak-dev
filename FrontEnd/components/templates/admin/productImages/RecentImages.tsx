'use client';

import React, {
  useCallback,
  useEffect,
  useRef,
  useState,
} from 'react';

import { useTranslations } from 'next-intl';

import { toMediaUrl } from '@utils/toMediaUrl';

import {
  GalleryImage,
  getRecentImages,
} from './api';

const DESKTOP_QUERY = '(min-width: 1024px)';
const PAGE_SIZE = 24;

function useIsDesktop() {
  const [desktop, setDesktop] = useState<boolean | null>(null);
  useEffect(() => {
    const mq = window.matchMedia(DESKTOP_QUERY);
    const sync = () => setDesktop(mq.matches);
    sync();
    mq.addEventListener('change', sync);
    return () => mq.removeEventListener('change', sync);
  }, []);
  return desktop;
}

/** Page numbers around the current one, with `null` for the gaps. */
function pageWindow(current: number, total: number): (number | null)[] {
  const pages = new Set([1, total, current - 1, current, current + 1]);
  const sorted = [...pages].filter((p) => p >= 1 && p <= total).sort((a, b) => a - b);
  const out: (number | null)[] = [];
  sorted.forEach((p, i) => {
    if (i > 0 && p - sorted[i - 1] > 1) out.push(null);
    out.push(p);
  });
  return out;
}

/**
 * Every product image, newest upload first. Clicking one opens its product's gallery.
 * Desktop pages with numbered buttons; mobile appends the next page when the list end scrolls into view.
 */
export default function RecentImages({
  selectedProductId,
  activeImageId,
  refreshKey,
  onPick,
  onFirstImage,
}: {
  selectedProductId?: number;
  activeImageId?: number | null;
  refreshKey: number;
  onPick: (img: GalleryImage) => void;
  onFirstImage?: (img: GalleryImage) => void;
}) {
  const t = useTranslations('admin.productImages');
  const desktop = useIsDesktop();

  const [items, setItems] = useState<GalleryImage[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(0);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const sentinel = useRef<HTMLDivElement>(null);
  const listTop = useRef<HTMLDivElement>(null);
  const request = useRef(0);
  const firstReported = useRef(false);
  const onFirstRef = useRef(onFirstImage);
  onFirstRef.current = onFirstImage;

  const fetchPage = useCallback(async (target: number, append: boolean) => {
    const ticket = ++request.current;
    setLoading(true);
    const res = await getRecentImages(target, PAGE_SIZE);
    if (ticket !== request.current) return;
    setLoading(false);
    if (!res.isSuccess || !res.data) {
      setError(res.error || t('loadFailed'));
      return;
    }
    const records = res.data.records ?? [];
    setError(null);
    setPage(target);
    setTotalPages(res.data.totalPages ?? 0);
    setTotalCount(res.data.totalCount ?? 0);
    setItems((prev) => {
      if (!append) return records;
      const seen = new Set(prev.map((i) => i.id));
      return [...prev, ...records.filter((r) => !seen.has(r.id))];
    });
    if (!firstReported.current && target === 1 && records[0]) {
      firstReported.current = true;
      onFirstRef.current?.(records[0]);
    }
  }, [t]);

  // Start over when the layout mode changes; after an edit keep the desktop page, restart the mobile feed.
  useEffect(() => {
    if (desktop === null) return;
    fetchPage(1, false);
  }, [desktop, fetchPage]);

  const pageRef = useRef(page);
  pageRef.current = page;
  useEffect(() => {
    if (refreshKey === 0 || desktop === null) return;
    if (desktop) fetchPage(pageRef.current, false);
    else fetchPage(1, false);
  }, [refreshKey, desktop, fetchPage]);

  // Mobile: infinite scroll.
  const hasMore = page < totalPages;
  useEffect(() => {
    if (desktop !== false || !hasMore || loading || error) return;
    const node = sentinel.current;
    if (!node) return;
    const io = new IntersectionObserver(
      (entries) => {
        if (entries.some((e) => e.isIntersecting)) fetchPage(pageRef.current + 1, true);
      },
      { rootMargin: '400px 0px' },
    );
    io.observe(node);
    return () => io.disconnect();
  }, [desktop, hasMore, loading, error, fetchPage]);

  const goTo = (p: number) => {
    if (p < 1 || p > totalPages || p === page) return;
    fetchPage(p, false);
    listTop.current?.scrollTo({ top: 0 });
  };

  return (
    <aside className="admin-panel pig-recent" aria-label={t('recentTitle')}>
      <header className="pig-recent-head">
        <h2 className="pig-recent-title">{t('recentTitle')}</h2>
        {totalCount > 0 && <span className="pig-recent-count">{t('count', { count: totalCount })}</span>}
      </header>

      <div className="pig-recent-body" ref={listTop}>
        {error && items.length === 0 ? (
          <div className="pig-recent-note">
            <p>{error}</p>
            <button type="button" className="admin-btn" onClick={() => fetchPage(1, false)}>
              {t('retry')}
            </button>
          </div>
        ) : !loading && items.length === 0 && desktop !== null ? (
          <p className="pig-recent-note">{t('recentEmpty')}</p>
        ) : (
          <ul className={`pig-recent-grid${loading && desktop ? ' pig-recent-grid-busy' : ''}`}>
            {items.map((img) => {
              const selected = img.productId === selectedProductId;
              const active = img.id === activeImageId;
              return (
                <li key={img.id}>
                  <button
                    type="button"
                    className={`pig-recent-item${selected ? ' pig-recent-item-selected' : ''}${active ? ' pig-recent-item-active' : ''}${!img.isActive ? ' pig-recent-item-hidden' : ''}`}
                    onClick={() => onPick(img)}
                    aria-current={active || undefined}
                    title={img.productName ?? undefined}
                  >
                    <span className="pig-recent-thumb">
                      {/* eslint-disable-next-line @next/next/no-img-element */}
                      <img src={toMediaUrl(img.productImageUrl)} alt={img.productName ?? ''} loading="lazy" />
                      {img.isMain && <span className="pig-recent-main" aria-label={t('main')}>★</span>}
                    </span>
                    <span className="pig-recent-name">{img.productName ?? `#${img.productId}`}</span>
                  </button>
                </li>
              );
            })}
            {loading && items.length === 0 &&
              Array.from({ length: 9 }).map((_, i) => (
                <li key={`s${i}`}><span className="pig-recent-thumb pig-skeleton" /></li>
              ))}
          </ul>
        )}

        {desktop === false && (
          <div ref={sentinel} className="pig-recent-more">
            {loading && items.length > 0 && <span className="admin-spinner" />}
            {error && items.length > 0 && (
              <button type="button" className="admin-btn" onClick={() => fetchPage(page + 1, true)}>
                {t('retry')}
              </button>
            )}
            {!hasMore && items.length > 0 && !loading && <span>{t('recentEnd')}</span>}
          </div>
        )}
      </div>

      {desktop && totalPages > 1 && (
        <nav className="pig-pager" aria-label={t('recentPages')}>
          <button type="button" className="pig-page" onClick={() => goTo(page - 1)} disabled={page <= 1 || loading} aria-label={t('prevPage')}>
            ‹
          </button>
          {pageWindow(page, totalPages).map((p, i) =>
            p === null ? (
              <span key={`gap${i}`} className="pig-page-gap">…</span>
            ) : (
              <button
                key={p}
                type="button"
                className={`pig-page${p === page ? ' pig-page-current' : ''}`}
                aria-current={p === page ? 'page' : undefined}
                onClick={() => goTo(p)}
                disabled={loading}
              >
                {p.toLocaleString()}
              </button>
            ),
          )}
          <button type="button" className="pig-page" onClick={() => goTo(page + 1)} disabled={page >= totalPages || loading} aria-label={t('nextPage')}>
            ›
          </button>
        </nav>
      )}
    </aside>
  );
}
