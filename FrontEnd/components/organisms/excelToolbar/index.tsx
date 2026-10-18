'use client';

import { useEffect, useRef, useState } from 'react';

import { useTranslations } from 'next-intl';
import { createPortal } from 'react-dom';
import { useRouter } from 'next/navigation';

import {
  downloadExcelExport,
  downloadExcelTemplate,
  ExcelEntityInfo,
  ExcelExportParams,
  ExcelImportProgress,
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
  const [dragging, setDragging] = useState(false);
  const [dryRun, setDryRun] = useState(false);
  const [allOrNothing, setAllOrNothing] = useState(true);
  const [progress, setProgress] = useState<ExcelImportProgress | null>(null);
  const [elapsed, setElapsed] = useState(0);
  const [result, setResult] = useState<ExcelImportResult | null>(null);
  const submitting = progress !== null;

  // Close on Escape (not while a request is running) and lock page scroll.
  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && !submitting) onClose();
    };
    const overflow = document.body.style.overflow;
    document.body.style.overflow = 'hidden';
    window.addEventListener('keydown', onKey);
    return () => {
      window.removeEventListener('keydown', onKey);
      document.body.style.overflow = overflow;
    };
  }, [submitting, onClose]);

  // Elapsed-seconds counter while the request is running.
  useEffect(() => {
    if (!submitting) return;
    setElapsed(0);
    const timer = window.setInterval(() => setElapsed((s) => s + 1), 1000);
    return () => window.clearInterval(timer);
  }, [submitting]);

  const pickFile = (picked: File | undefined | null) => {
    if (!picked) return;
    if (!picked.name.toLowerCase().endsWith('.xlsx')) {
      showErrorToast(t('onlyXlsx'));
      return;
    }
    setFile(picked);
    setResult(null);
  };

  const submit = async () => {
    if (!file) return;
    setResult(null);
    const response = await importExcel(entity, file, { dryRun, allOrNothing }, setProgress);
    setProgress(null);

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

  // Portal: an ancestor (.admin-panel) uses backdrop-filter, which would trap position:fixed.
  return createPortal(
    <div className="admin-dialog-overlay" onClick={() => !submitting && onClose()}>
      <div
        className="admin-dialog"
        role="dialog"
        aria-modal="true"
        aria-labelledby="excel-import-title"
        dir="rtl"
        onClick={(e) => e.stopPropagation()}
      >
        <header className="admin-dialog-header">
          <div>
            <h2 id="excel-import-title" className="font-semibold text-lg">
              {t('importTitle', { entity: info.displayName })}
            </h2>
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
          <button
            type="button"
            className="admin-icon-btn shrink-0"
            onClick={onClose}
            disabled={submitting}
            aria-label={t('close')}
          >
            ✕
          </button>
        </header>

        <div className="flex flex-col gap-4 p-5">
          <input
            ref={inputRef}
            type="file"
            accept=".xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            className="hidden"
            onChange={(e) => {
              pickFile(e.target.files?.[0]);
              e.target.value = '';
            }}
          />

          {submitting ? (
            <ProgressPanel progress={progress} elapsed={elapsed} fileName={file?.name} />
          ) : (
            <>
              <button
                type="button"
                className={`admin-dropzone ${dragging ? 'admin-dropzone-active' : ''}`}
                onClick={() => inputRef.current?.click()}
                onDragOver={(e) => {
                  e.preventDefault();
                  setDragging(true);
                }}
                onDragLeave={() => setDragging(false)}
                onDrop={(e) => {
                  e.preventDefault();
                  setDragging(false);
                  pickFile(e.dataTransfer.files?.[0]);
                }}
              >
                <UploadIcon size={28} />
                {file ? (
                  <span className="flex flex-col items-center gap-0.5">
                    <span className="font-medium" dir="ltr">{file.name}</span>
                    <span className="text-[var(--admin-text-muted)] text-xs">
                      {formatSize(file.size)} · {t('changeFile')}
                    </span>
                  </span>
                ) : (
                  <span className="flex flex-col items-center gap-0.5">
                    <span className="font-medium">{t('dropHere')}</span>
                    <span className="text-[var(--admin-text-muted)] text-xs">{t('onlyXlsx')}</span>
                  </span>
                )}
              </button>

              <fieldset className="flex flex-col gap-2 text-sm">
                <label className="flex items-center gap-2 cursor-pointer">
                  <input type="checkbox" checked={dryRun} onChange={(e) => setDryRun(e.target.checked)} />
                  {t('dryRun')}
                </label>
                <label className="flex items-center gap-2 cursor-pointer">
                  <input type="radio" name="excel-mode" checked={allOrNothing} onChange={() => setAllOrNothing(true)} />
                  {t('allOrNothing')}
                </label>
                <label className="flex items-center gap-2 cursor-pointer">
                  <input type="radio" name="excel-mode" checked={!allOrNothing} onChange={() => setAllOrNothing(false)} />
                  {t('partial')}
                </label>
              </fieldset>
            </>
          )}

          {result && status && !submitting && (
            <div className="flex flex-col gap-3">
              <div className={`admin-alert ${status.ok ? 'admin-alert-success' : 'admin-alert-warning'}`} role="status">
                {t(status.key)}
              </div>

              <table className="w-full text-sm">
                <thead>
                  <tr className="text-[var(--admin-text-muted)]">
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
                <div className="border-[var(--admin-border)] border rounded-xl max-h-64 overflow-auto">
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
        </div>

        <footer className="admin-dialog-footer">
          <button type="button" className="admin-btn admin-btn-ghost" onClick={onClose} disabled={submitting}>
            {t('close')}
          </button>
          <button type="button" className="admin-btn admin-btn-primary" onClick={submit} disabled={!file || submitting}>
            {submitting && <span className="admin-spinner" aria-hidden />}
            {submitting ? t('importing') : dryRun ? t('validate') : t('submit')}
          </button>
        </footer>
      </div>
    </div>,
    document.body,
  );
}

function ProgressPanel({
  progress,
  elapsed,
  fileName,
}: {
  progress: ExcelImportProgress | null;
  elapsed: number;
  fileName?: string;
}) {
  const t = useTranslations('admin.excel');
  const uploading = progress?.stage === 'uploading';
  const percent = progress?.stage === 'uploading' ? progress.percent : 100;

  return (
    <div className="flex flex-col items-center gap-4 py-6 text-center" aria-live="polite">
      <span className="admin-spinner admin-spinner-lg" aria-hidden />
      <div className="flex flex-col gap-1">
        <span className="font-medium">{uploading ? t('uploading', { percent }) : t('processing')}</span>
        <span className="text-[var(--admin-text-muted)] text-xs" dir="ltr">{fileName}</span>
      </div>
      <div className="w-full max-w-sm admin-progress">
        <div
          className={uploading ? 'admin-progress-bar' : 'admin-progress-bar admin-progress-indeterminate'}
          style={uploading ? { width: `${percent}%` } : undefined}
        />
      </div>
      <div className="flex items-center gap-3 text-xs">
        <Step done={!uploading} active={uploading} label={t('stepUpload')} />
        <span className="text-[var(--admin-text-muted)]">←</span>
        <Step done={false} active={!uploading} label={t('stepProcess')} />
      </div>
      {!uploading && (
        <span className="text-[var(--admin-text-muted)] text-xs">
          {t('elapsed', { seconds: elapsed })}
          {elapsed >= 10 && ` · ${t('processingHint')}`}
        </span>
      )}
    </div>
  );
}

function Step({ done, active, label }: { done: boolean; active: boolean; label: string }) {
  const tone = active
    ? 'font-semibold text-[var(--primary-color)]'
    : done
      ? 'text-[var(--admin-text)]'
      : 'text-[var(--admin-text-muted)]';
  return (
    <span className={`inline-flex items-center gap-1 ${tone}`}>
      {done ? '✓' : '•'} {label}
    </span>
  );
}

function formatSize(bytes: number) {
  return bytes < 1024 * 1024
    ? `${Math.max(1, Math.round(bytes / 1024))} KB`
    : `${(bytes / 1024 / 1024).toFixed(1)} MB`;
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

function UploadIcon({ size = 16 }: { size?: number }) {
  return (
    <svg {...iconProps} width={size} height={size}>
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
