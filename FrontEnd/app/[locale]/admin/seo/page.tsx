import React from 'react';

import { getLocale, getTranslations } from 'next-intl/server';
import Link from 'next/link';

import { getAll } from '@lib/getAll';

export const dynamic = 'force-dynamic';

const seoActionLinks = [
  { key: 'rules', href: '/admin/seoSettings?ByConfig=true' },
  { key: 'blogs', href: '/admin/blogs?ByConfig=true' },
  { key: 'products', href: '/admin/products?ByConfig=true' },
  { key: 'categories', href: '/admin/categories?ByConfig=true' },
  { key: 'brands', href: '/admin/brands?ByConfig=true' },
];

export default async function SeoDashboardPage() {
  const t = await getTranslations();
  const locale = await getLocale();
  const seoRules = await getAll('seoSettings', {
    page: 1,
    pageSize: 1,
    byConfig: false,
    revalidate: false,
  });

  const totalSeoRules = seoRules?.data?.totalCount ?? 0;

  return (
    <div className="admin-page">
      <header className="admin-page-header">
        <div>
          <h1 className="admin-page-title">{t('admin.seoDashboardTitle')}</h1>
          <p className="admin-page-subtitle">{t('admin.seoDashboardSubtitle')}</p>
        </div>
        <Link href={`/${locale}/admin/seoSettings?ByConfig=true`} className="admin-btn w-full sm:w-auto justify-center">
          {t('admin.seoManageRules')}
        </Link>
      </header>

      <section className="gap-4 grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4">
        <div className="admin-stat-card">
          <span className="text-[var(--admin-text-muted)] text-xs uppercase tracking-wide">
            {t('admin.seoRulesCount')}
          </span>
          <strong className="text-3xl text-primary">{totalSeoRules}</strong>
          <p className="text-[var(--admin-text-muted)] text-sm">
            {t('admin.seoRulesHint')}
          </p>
        </div>

        <div className="admin-stat-card sm:col-span-1 xl:col-span-3">
          <span className="font-medium text-[var(--admin-text)]">
            {t('admin.seoStrategyTitle')}
          </span>
          <p className="max-w-3xl text-[var(--admin-text-muted)] text-sm leading-relaxed">
            {t('admin.seoStrategyHint')}
          </p>
        </div>
      </section>

      <section className="flex flex-col gap-4">
        <div className="admin-page-header">
          <div>
            <h2 className="font-semibold text-[var(--admin-text)] text-lg">
              {t('admin.seoQuickActions')}
            </h2>
            <p className="admin-page-subtitle">{t('admin.seoQuickActionsHint')}</p>
          </div>
        </div>

        <div className="gap-3 grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3">
          {seoActionLinks.map((item) => (
            <Link
              key={item.key}
              href={`/${locale}${item.href}`}
              className="admin-stat-card group"
            >
              <div className="flex flex-col gap-2">
                <p className="font-medium text-[var(--admin-text)] group-hover:text-primary transition-colors">
                  {t(`admin.seoCards.${item.key}.title`)}
                </p>
                <p className="text-[var(--admin-text-muted)] text-sm leading-relaxed">
                  {t(`admin.seoCards.${item.key}.description`)}
                </p>
              </div>
            </Link>
          ))}
        </div>
      </section>

      <section className="admin-panel flex flex-col gap-3">
        <h2 className="font-semibold text-[var(--admin-text)] text-lg">
          {t('admin.seoWorkflowTitle')}
        </h2>
        <p className="text-[var(--admin-text-muted)] text-sm leading-relaxed">
          {t('admin.seoWorkflowDescription')}
        </p>
        <div className="gap-3 grid grid-cols-1 md:grid-cols-3">
          <div className="bg-[var(--admin-surface-muted)] p-4 border border-[var(--admin-border)] rounded-2xl">
            <p className="font-medium text-[var(--admin-text)]">{t('admin.seoWorkflow.step1Title')}</p>
            <p className="mt-2 text-[var(--admin-text-muted)] text-sm">{t('admin.seoWorkflow.step1Desc')}</p>
          </div>
          <div className="bg-[var(--admin-surface-muted)] p-4 border border-[var(--admin-border)] rounded-2xl">
            <p className="font-medium text-[var(--admin-text)]">{t('admin.seoWorkflow.step2Title')}</p>
            <p className="mt-2 text-[var(--admin-text-muted)] text-sm">{t('admin.seoWorkflow.step2Desc')}</p>
          </div>
          <div className="bg-[var(--admin-surface-muted)] p-4 border border-[var(--admin-border)] rounded-2xl">
            <p className="font-medium text-[var(--admin-text)]">{t('admin.seoWorkflow.step3Title')}</p>
            <p className="mt-2 text-[var(--admin-text-muted)] text-sm">{t('admin.seoWorkflow.step3Desc')}</p>
          </div>
        </div>
      </section>
    </div>
  );
}
