import React from 'react';

import { getLocale, getTranslations } from 'next-intl/server';
import Link from 'next/link';

import { getMenu } from '@lib/config';
import ApiErrorNotice from '@components/molecules/apiErrorNotice';

export const dynamic = 'force-dynamic';

export default async function AdminPage() {
  const t = await getTranslations();
  const locale = await getLocale();
  const menu = await getMenu();
  const items = menu?.data ?? [];

  return (
    <div className="admin-page">
      <header className="admin-page-header">
        <div>
          <h1 className="admin-page-title">{t('admin.dashboardTitle')}</h1>
          <p className="admin-page-subtitle">{t('admin.dashboardSubtitle')}</p>
        </div>
        <Link href={`/${locale}`} className="admin-btn w-full sm:w-auto justify-center">
          {t('admin.openStore')}
        </Link>
      </header>

      <ApiErrorNotice message={menu.error} />

      <section className="gap-4 grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4">
        <div className="admin-stat-card">
          <span className="text-[var(--admin-text-muted)] text-xs uppercase tracking-wide">
            {t('admin.entities')}
          </span>
          <strong className="text-3xl text-primary">{items.length}</strong>
          <p className="text-[var(--admin-text-muted)] text-sm">
            {t('admin.quickAccessHint')}
          </p>
        </div>
        <div className="admin-stat-card sm:col-span-1 xl:col-span-3">
          <span className="font-medium text-[var(--admin-text)]">
            {t('admin.welcome')}
          </span>
          <p className="max-w-2xl text-[var(--admin-text-muted)] text-sm leading-relaxed">
            {t('admin.dashboardSubtitle')}
          </p>
        </div>
      </section>

      <section className="flex flex-col gap-4">
        <div className="admin-page-header">
          <div>
            <h2 className="font-semibold text-[var(--admin-text)] text-lg">
              {t('admin.quickAccess')}
            </h2>
            <p className="admin-page-subtitle">{t('admin.quickAccessHint')}</p>
          </div>
        </div>

        {items.length === 0 ? (
          <div className="gap-3 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
            <div className="admin-panel admin-empty sm:col-span-2 lg:col-span-3 xl:col-span-3">
              <p className="admin-empty-title">{t('admin.menuEmpty')}</p>
            </div>
            <Link
              href={`/${locale}/admin/backup`}
              className="admin-stat-card group"
            >
              <div className="flex items-center gap-3">
                <span className="flex justify-center items-center bg-[var(--admin-active)] rounded-xl w-11 h-11 text-primary text-sm font-semibold">
                  BK
                </span>
                <div className="min-w-0">
                  <p className="font-medium text-[var(--admin-text)] group-hover:text-primary truncate transition-colors">
                    {t('admin.backupNav')}
                  </p>
                  <p className="text-[var(--admin-text-muted)] text-xs truncate">
                    /admin/backup
                  </p>
                </div>
              </div>
            </Link>
          </div>
        ) : (
          <div className="gap-3 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
            {items.map((item, idx) => (
              <Link
                key={`${item.endPoint}-${idx}`}
                href={`/${locale}/admin/${item.endPoint}?ByConfig=true`}
                className="admin-stat-card group"
              >
                <div className="flex items-center gap-3">
                  <span
                    className="flex justify-center items-center bg-[var(--admin-active)] rounded-xl w-11 h-11 text-primary [&>svg]:w-5 [&>svg]:h-5"
                    dangerouslySetInnerHTML={{ __html: item.entityIconBase64 }}
                  />
                  <div className="min-w-0">
                    <p className="font-medium text-[var(--admin-text)] group-hover:text-primary truncate transition-colors">
                      {locale === 'fa'
                        ? item.persianDisplayName
                        : item.englishDisplayName}
                    </p>
                    <p className="text-[var(--admin-text-muted)] text-xs truncate">
                      /admin/{item.endPoint}
                    </p>
                  </div>
                </div>
              </Link>
            ))}
            <Link
              href={`/${locale}/admin/backup`}
              className="admin-stat-card group"
            >
              <div className="flex items-center gap-3">
                <span className="flex justify-center items-center bg-[var(--admin-active)] rounded-xl w-11 h-11 text-primary text-sm font-semibold">
                  BK
                </span>
                <div className="min-w-0">
                  <p className="font-medium text-[var(--admin-text)] group-hover:text-primary truncate transition-colors">
                    {t('admin.backupNav')}
                  </p>
                  <p className="text-[var(--admin-text-muted)] text-xs truncate">
                    /admin/backup
                  </p>
                </div>
              </div>
            </Link>
          </div>
        )}
      </section>
    </div>
  );
}
