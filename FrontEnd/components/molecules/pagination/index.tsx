'use client';

import React, {
  Suspense,
  useEffect,
  useMemo,
  useState,
} from 'react';

import { useLocale, useTranslations } from 'next-intl';
import { usePathname, useRouter, useSearchParams } from 'next/navigation';

import { Button } from '@components/atoms/defaultElements/customButton';
import {
  ChevronLeftIcon,
  ChevronRightIcon,
} from '@components/atoms/iconComponents';

interface PaginationProps {
  className?: string;
  current?: number;
  total: number;
  pageSize: number;
  onChange?: (page: number, pageSize: number) => void;
  showSizeChanger?: boolean;
  showTitle?: boolean;
  prevIcon?: React.ReactNode;
  nextIcon?: React.ReactNode;
  jumpPrevIcon?: React.ReactNode;
  jumpNextIcon?: React.ReactNode;
  pageSizeOptions?: number[];
}

function PaginationFallback({ className = '' }: { className?: string }) {
  return (
    <div
      className={`h-12 w-48 animate-pulse rounded-xl border border-[var(--store-border)] bg-[var(--store-surface-muted)] ${className}`}
      aria-hidden
    />
  );
}

const CustomPaginationInner: React.FC<PaginationProps> = ({
  className = '',
  current = 1,
  total,
  pageSize: initialPageSize = 10,
  onChange,
  showSizeChanger = false,
  showTitle = false,
  prevIcon,
  nextIcon,
  jumpPrevIcon,
  jumpNextIcon,
  pageSizeOptions = [5, 10, 20, 50, 100],
}) => {
  const [page, setPage] = useState(current);
  const [pageSize, setPageSize] = useState(initialPageSize);
  const isFirstRender = React.useRef(true);
  const t = useTranslations();
  const locale = useLocale();
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();

  useEffect(() => {
    setPage(current);
  }, [current]);

  useEffect(() => {
    setPageSize(initialPageSize);
  }, [initialPageSize]);

  const totalPages = Math.max(1, Math.ceil(total / pageSize));
  const maxVisible = 3;

  const pages = useMemo(() => {
    const list: (number | string)[] = [];
    if (totalPages <= maxVisible + 2) {
      for (let i = 1; i <= totalPages; i++) list.push(i);
    } else {
      const left = Math.max(2, page - 1);
      const right = Math.min(totalPages - 1, page + 1);
      list.push(1);
      if (left > 2) list.push('jumpPrev');
      for (let i = left; i <= right; i++) list.push(i);
      if (right < totalPages - 1) list.push('jumpNext');
      list.push(totalPages);
    }
    return list;
  }, [page, totalPages]);

  useEffect(() => {
    if (isFirstRender.current) {
      isFirstRender.current = false;
      return;
    }

    if (onChange) {
      onChange(page, pageSize);
      return;
    }

    const params = new URLSearchParams(searchParams.toString());
    params.set('page', String(page));
    if (showSizeChanger) {
      params.set('pageSize', String(pageSize));
    }
    router.push(`${pathname}?${params.toString()}`, { scroll: true });
  }, [page, pageSize]);

  const handlePageChange = (_pageSize: number, newPage: number) => {
    if (newPage < 1 || newPage > totalPages) return;
    setPage(newPage);
  };

  const handlePageSizeChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const newSize = Number(e.target.value);
    setPageSize(newSize);
    setPage(1);
  };

  return (
    <nav
      dir={locale === 'fa' ? 'rtl' : 'ltr'}
      className={`flex flex-wrap items-center gap-2 bg-[var(--store-surface-solid)] px-3 py-2 border border-[var(--store-border)] rounded-xl w-fit ${className}`}
      aria-label="Pagination"
    >
      <Button
        className="admin-icon-btn !m-0 !p-0 !w-8 !h-8"
        disabled={page === 1}
        onClick={() => handlePageChange(pageSize, page - 1)}
        type="button"
      >
        {prevIcon ?? <ChevronRightIcon />}
      </Button>

      {pages.map((p, index) => {
        if (typeof p === 'number') {
          const active = p === page;
          return (
            <Button
              key={index}
              onClick={() => handlePageChange(pageSize, p)}
              type="button"
              className={`!px-3 !py-1 !rounded-lg !min-w-8 ${
                active
                  ? 'admin-btn-primary !border-primary'
                  : 'admin-btn !py-1'
              }`}
              aria-current={active ? 'page' : undefined}
            >
              {p}
            </Button>
          );
        }

        if (p === 'jumpPrev') {
          return (
            <span
              key={index}
              onClick={() => handlePageChange(pageSize, Math.max(page - 5, 1))}
              className="px-2 text-[var(--store-text-muted)] cursor-pointer"
            >
              {jumpPrevIcon ?? <span>...</span>}
            </span>
          );
        }

        if (p === 'jumpNext') {
          return (
            <span
              key={index}
              onClick={() =>
                handlePageChange(pageSize, Math.min(page + 5, totalPages))
              }
              className="px-2 text-[var(--store-text-muted)] cursor-pointer"
            >
              {jumpNextIcon ?? <span>...</span>}
            </span>
          );
        }

        return null;
      })}

      <Button
        className="admin-icon-btn !m-0 !p-0 !w-8 !h-8"
        disabled={page === totalPages}
        onClick={() => handlePageChange(pageSize, page + 1)}
        type="button"
      >
        {nextIcon ?? <ChevronLeftIcon />}
      </Button>

      {showSizeChanger && (
        <div className="flex items-center gap-1 text-[var(--store-text-muted)] text-sm ms-2">
          <span>{t('admin.pageSize')}:</span>
          <select
            value={pageSize}
            onChange={handlePageSizeChange}
            className="bg-[var(--store-surface-muted)] p-1 border border-[var(--store-border)] rounded-md focus:outline-none focus:ring-1 focus:ring-primary text-sm"
          >
            {pageSizeOptions.map((size) => (
              <option key={size} value={size}>
                {size}
              </option>
            ))}
          </select>
        </div>
      )}

      {showTitle && (
        <span className="text-[var(--store-text-muted)] text-sm ms-2">
          {t('admin.page', { page, total: totalPages })} ·{' '}
          {t('admin.totalRecords', { count: total })}
        </span>
      )}
    </nav>
  );
};

const CustomPagination: React.FC<PaginationProps> = (props) => (
  <Suspense fallback={<PaginationFallback className={props.className} />}>
    <CustomPaginationInner {...props} />
  </Suspense>
);

export default CustomPagination;
