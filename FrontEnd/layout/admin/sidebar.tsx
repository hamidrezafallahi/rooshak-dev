'use client';

import React, { useMemo } from 'react';

import {
  useLocale,
  useTranslations,
} from 'next-intl';
import Link from 'next/link';
import { usePathname } from 'next/navigation';

import {
  CloseIcon,
  FolderIcon,
} from '@components/atoms/iconComponents';
import { cn } from '@lib/utils';
import { menuResponse } from '@models/config';

interface SidebarProps {
  menu: menuResponse;
  open?: boolean;
  onClose?: () => void;
}

export default function Sidebar({
  menu,
  open = false,
  onClose,
}: SidebarProps) {
  const t = useTranslations();
  const locale = useLocale();
  const pathname = usePathname()!;

  const items = menu?.data ?? [];

  const activeEndpoint = useMemo(() => {
    const parts = pathname?.split('/') ?? [];
    const adminIndex = parts.findIndex((part) => part === 'admin');
    if (adminIndex < 0) return '';
    return parts[adminIndex + 1] ?? '';
  }, [pathname]);

  return (
    <aside
      id="admin-sidebar"
      data-open={open ? 'true' : 'false'}
      className={cn('admin-sidebar', open && 'is-open')}
    >
      <div className="admin-sidebar-panel">
        <div className="flex justify-between items-center gap-2 px-4 border-[var(--admin-border)] border-b h-16">
          <div className="flex flex-col justify-center gap-0.5 min-w-0">
            <Link
              href={`/${locale}/admin`}
              onClick={onClose}
              className="font-semibold tracking-tight text-[var(--admin-text)] text-lg transition-colors hover:text-primary truncate"
            >
              {t('admin.dashboardTitle')}
            </Link>
            <p className="text-[var(--admin-text-muted)] text-xs">
              {t('admin.dashboard')}
            </p>
          </div>

          <button
            type="button"
            className="admin-icon-btn shrink-0"
            aria-label={t('admin.closeMenu')}
            onClick={onClose}
          >
            <CloseIcon config={{ size: 16 }} />
          </button>
        </div>

        <nav className="flex flex-col flex-1 gap-1 p-3 min-h-0 overflow-y-auto">
          {items.length === 0 ? (
            <div className="px-2 py-6 text-[var(--admin-text-muted)] text-xs text-center">
              {t('admin.menuEmpty')}
            </div>
          ) : (
            items.map((item, idx) => {
              const isActive =
                activeEndpoint.toLowerCase() === item.endPoint?.toLowerCase();
              return (
                <Link
                  key={`${item.endPoint}-${idx}`}
                  href={`/${locale}/admin/${item.endPoint}?ByConfig=true`}
                  title={
                    locale === 'fa'
                      ? item.persianDisplayName
                      : item.englishDisplayName
                  }
                  onClick={onClose}
                  className={cn(
                    'admin-nav-item',
                    isActive && 'admin-nav-item-active',
                  )}
                >
                  <span
                    className="flex justify-center items-center w-6 h-6 shrink-0 [&>svg]:w-5 [&>svg]:h-5"
                    dangerouslySetInnerHTML={{ __html: item.entityIconBase64 }}
                  />
                  <span className="truncate">
                    {locale === 'fa'
                      ? item.persianDisplayName
                      : item.englishDisplayName}
                  </span>
                </Link>
              );
            })
          )}

          <Link
            href={`/${locale}/admin/backup`}
            title={t('admin.backupNav')}
            onClick={onClose}
            className={cn(
              'admin-nav-item mt-1',
              activeEndpoint.toLowerCase() === 'backup' && 'admin-nav-item-active',
            )}
          >
            <span className="flex justify-center items-center w-6 h-6 shrink-0 [&>svg]:w-5 [&>svg]:h-5">
              <FolderIcon />
            </span>
            <span className="truncate">{t('admin.backupNav')}</span>
          </Link>
        </nav>

        <div className="flex justify-center items-center gap-2 px-3 border-[var(--admin-border)] border-t min-h-16">
          <Link
            href={`/${locale}`}
            onClick={onClose}
            className="admin-btn admin-btn-ghost justify-center text-xs w-full"
          >
            {t('admin.openStore')}
          </Link>
        </div>
      </div>
    </aside>
  );
}
