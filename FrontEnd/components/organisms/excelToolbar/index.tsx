'use client';

import { useEffect, useRef, useState } from 'react';

import { useTranslations } from 'next-intl';
import { useRouter } from 'next/navigation';

import { Modal } from '@components/atoms/defaultElements/customModal';
import {
  downloadExcelExport,
  downloadExcelTemplate,
  ExcelEntityInfo,
  ExcelExportParams,
  ExcelImportResult,
  getExcelEntity,
  importExcel,
} from '@lib/excel';
import { showErrorToast, showSuccessToast } from '@utils/core';

type ExcelToolbarProps = {
  /** Admin endpoint name, e.g. "products". */
  entity: string;
  /** Rows selected in the grid: export only these. */
  selectedIds?: (string | number)[];
  /** Current search text of the list. */
  search?: string;
  /** Entity-specific export filters, forwarded as-is to the API. */
  filters?: ExcelExportParams['filters'];
};

type Busy = 'export' | 'template' | 'import' | null;

/**
 * Export / import / blank-template buttons for an admin list. Renders nothing when the
 * backend has no Excel profile for the entity (or the user's role may not use it).
 */
export default function ExcelToolbar({ entity, selectedIds, search, filters }: ExcelToolbarProps) {
  const t = useTranslations('admin.excel');
  const router = useRouter();
  const [info, setInfo] = useState<ExcelEntityInfo | null>(null);
  const [busy, setBusy] = useState<Busy>(null);
  const [importOpen, setImportOpen] = useState(false);

  useEffect(() => {
    let alive = true;
    getExcelEntity(entity).then((value) => alive && setInfo(value));
    return () => {
      alive = false;
    };
  }, [entity]);

  if (!info || (!info.canExport && !info.canImport)) return null;

  const run = async (kind: Exclude<Busy, null>, action: () => Promise<void>) => {
    setBusy(kind);
    try {
      await action();
    } catch (error) {
      showErrorToast(error instanceof Error && error.message ? error.message : t('downloadError'));
    } finally {
      setBusy(null);
    }
  };

  const selectedCount = selectedIds?.length ?? 0;

  return (
    <>
      <div className="flex flex-wrap items-center gap-2" role="group" aria-label={t('groupLabel')}>
        {info.canExport && (
          <button
            type="button"
            className="admin-btn"
            disabled={busy !== null}
            onClick={() =>
              run('export', () =>
                downloadExcelExport(entity, {
                  q: search || undefined,
                  ids: selectedCount > 0 ? selectedIds : undefined,
                  filters,
                }),
              )
            }
          >
            <ExcelIcon />
            {busy === 'export'
              ? t('exporting')
              : selectedCount > 0
                ? t('exportSelected', { count: selectedCount })
                : t('export')}
          </button>
        )}
        {info.canImport && (
          <>
            <button
              type="button"
              className="admin-btn"
              disabled={busy !== null}
              onClick={() => setImportOpen(true)}
            >
              <UploadIcon />
              {t('import')}
            </button>
            <button
              type="button"
              className="admin-btn admin-btn-ghost"
              disabled={busy !== null}
              onClick={() => run('template', () => downloadExcelTemplate(entity))}
              title={t('templateHint')}
            >
              <DownloadIcon />
              {busy === 'template' ? t('exporting') : t('template')}
            </button>
          </>
        )}
      </div>

      {importOpen && (
        <ImportDialog
          entity={entity}
          info={info}
          onClose={() => setImportOpen(false)}
          onCommitted={() => router.refresh()}
          onDownloadTemplate={() => run('template', () => downloadExcelTemplate(entity))}
        />
      )}
    </>
  );
}

function ImportDialog({
  entity,
  info,
  onClose,
  onCommitted,
  onDownloadTemplate,
}: {
  entity: string;
  info: ExcelEntityInfo;
  onClose: () => void;
  onCommitted: () => void;
  onDownloadTemplate: () => void;
}) {
  const t = useTranslations('admin.excel');
  const inputRef = useRef<HTMLInputElement>(null);
  const [file, setFile] = useState<File | null>(null);
  const [dryRun, setDryRun] = useState(false);
  const [allOrNothing, setAllOrNothing] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [result, setResult] = useState<ExcelImportResult | null>(null);

  const submit = async () => {
    if (!file) return;
    setSubmitting(true);
    setResult(null);
    const response = await importExcel(entity, file, { dryRun, allOrNothing });
    setSubmitting(false);

    if (!response.isSuccess || !response.data) {
      showErrorToast(response.error || t('importError'));
      return;
    }

    setResult(response.data);
    if (response.data.committed) {
      showSuccessToast(t('importDone'));
      onCommitted();
    }
  };

  const status = result && statusOf(result);

  return (
    <Modal open onClose={submitting ? () => undefined : onClose} className="max-w-3xl max-h-[90vh] overflow-auto">
      <div className="flex flex-col gap-4 p-5" dir="rtl">
        <div>
          <h2 className="font-semibold text-lg">{t('importTitle', { entity: info.displayName })}</h2>
          <p className="mt-1 text-[var(--admin-text-muted)] text-sm">
            {t('importHint')}{' '}
            <button type="button" className="text-[var(--primary-color)] underline" onClick={onDownloadTemplate}>
              {t('template')}
            </button>
          </p>
          {info.sheets.length > 1 && (
            <p className="mt-1 text-[var(--admin-text-muted)] text-xs">
              {t('sheetsHint', { sheets: info.sheets.map((s) => s.caption).join('، ') })}
            </p>
          )}
        </div>

        <div className="flex flex-wrap items-center gap-3">
          <input
            ref={inputRef}
            type="file"
            accept=".xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            className="hidden"
            onChange={(e) => {
              setFile(e.target.files?.[0] ?? null);
              setResult(null);
            }}
          />
          <button type="button" className="admin-btn" onClick={() => inputRef.current?.click()} disabled={submitting}>
            <UploadIcon />
            {t('chooseFile')}
          </button>
          <span className="text-sm truncate max-w-[18rem]" dir="ltr">
            {file?.name ?? t('noFile')}
          </span>
        </div>

        <fieldset className="flex flex-col gap-2 text-sm" disabled={submitting}>
          <label className="flex items-center gap-2">
            <input type="checkbox" checked={dryRun} onChange={(e) => setDryRun(e.target.checked)} />
            {t('dryRun')}
          </label>
          <label className="flex items-center gap-2">
            <input type="radio" name="excel-mode" checked={allOrNothing} onChange={() => setAllOrNothing(true)} />
            {t('allOrNothing')}
          </label>
          <label className="flex items-center gap-2">
            <input type="radio" name="excel-mode" checked={!allOrNothing} onChange={() => setAllOrNothing(false)} />
            {t('partial')}
          </label>
        </fieldset>

        {result && status && (
          <div className="flex flex-col gap-3">
            <div
              className={`admin-badge ${status.ok ? 'admin-badge-success' : 'admin-badge-warning'} self-start`}
              role="status"
            >
              {t(status.key)}
            </div>

            <table className="w-full text-sm">
              <thead>
                <tr className="text-[var(--admin-text-muted)] text-start">
                  <th className="py-1 text-start">{t('sheet')}</th>
                  <th className="py-1 text-start">{t('created')}</th>
                  <th className="py-1 text-start">{t('updated')}</th>
                  <th className="py-1 text-start">{t('failed')}</th>
                </tr>
              </thead>
              <tbody>
                {result.sheets.map((s) => (
                  <tr key={s.sheet} className="border-[var(--admin-border)] border-t">
                    <td className="py-1">{s.sheet}</td>
                    <td className="py-1">{s.created}</td>
                    <td className="py-1">{s.updated}</td>
                    <td className="py-1">{s.failed}</td>
                  </tr>
                ))}
              </tbody>
            </table>

            {result.errors.length > 0 && (
              <div className="max-h-64 overflow-auto admin-panel">
                <table className="w-full text-sm">
                  <thead className="top-0 sticky bg-[var(--admin-surface-elevated)]">
                    <tr>
                      <th className="p-2 text-start">{t('sheet')}</th>
                      <th className="p-2 text-start">{t('row')}</th>
                      <th className="p-2 text-start">{t('error')}</th>
                    </tr>
                  </thead>
                  <tbody>
                    {result.errors.map((e, i) => (
                      <tr key={`${e.sheet}-${e.row}-${i}`} className="border-[var(--admin-border)] border-t align-top">
                        <td className="p-2 whitespace-nowrap">{e.sheet}</td>
                        <td className="p-2">{e.row}</td>
                        <td className="p-2">{e.message}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        )}

        <div className="flex justify-end gap-2">
          <button type="button" className="admin-btn admin-btn-ghost" onClick={onClose} disabled={submitting}>
            {t('close')}
          </button>
          <button type="button" className="admin-btn admin-btn-primary" onClick={submit} disabled={!file || submitting}>
            {submitting ? t('importing') : dryRun ? t('validate') : t('submit')}
          </button>
        </div>
      </div>
    </Modal>
  );
}

function statusOf(result: ExcelImportResult): { ok: boolean; key: string } {
  if (result.dryRun) return result.failed === 0 ? { ok: true, key: 'dryRunOk' } : { ok: false, key: 'dryRunErrors' };
  if (!result.committed) return { ok: false, key: 'rolledBack' };
  return result.failed === 0 ? { ok: true, key: 'committed' } : { ok: false, key: 'committedPartial' };
}

const iconProps = {
  width: 16,
  height: 16,
  viewBox: '0 0 24 24',
  fill: 'none',
  stroke: 'currentColor',
  strokeWidth: 2,
  strokeLinecap: 'round' as const,
  strokeLinejoin: 'round' as const,
  'aria-hidden': true,
};

function ExcelIcon() {
  return (
    <svg {...iconProps}>
      <rect x="3" y="3" width="18" height="18" rx="2" />
      <path d="m8 8 8 8M16 8l-8 8" />
    </svg>
  );
}

function UploadIcon() {
  return (
    <svg {...iconProps}>
      <path d="M12 16V4m0 0-4 4m4-4 4 4M4 20h16" />
    </svg>
  );
}

function DownloadIcon() {
  return (
    <svg {...iconProps}>
      <path d="M12 4v12m0 0-4-4m4 4 4-4M4 20h16" />
    </svg>
  );
}
