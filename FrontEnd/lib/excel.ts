import { browserApiBaseUrl } from '@lib/api';

/** Mirrors Application.DataTransfer.Excel.ExcelEntityInfoDto. */
export type ExcelEntityInfo = {
  entity: string;
  displayName: string;
  canExport: boolean;
  canImport: boolean;
  sheets: { key: string; caption: string }[];
};

/**
 * Export options. `filters` holds entity-specific filters (e.g. { categoryId: 3 });
 * the backend ignores keys a profile does not support, so new filters need no API change.
 */
export type ExcelExportParams = {
  q?: string;
  onlyActives?: boolean;
  ids?: (string | number)[];
  sheets?: string[];
  filters?: Record<string, string | number | boolean | undefined>;
};

export type ExcelImportOptions = {
  dryRun?: boolean;
  allOrNothing?: boolean;
};

export type ExcelImportResult = {
  entity: string;
  dryRun: boolean;
  committed: boolean;
  created: number;
  updated: number;
  failed: number;
  sheets: { sheet: string; created: number; updated: number; failed: number; skipped: number }[];
  errors: { sheet: string; row: number; message: string }[];
};

type ApiResult<T> = { isSuccess: boolean; error?: string | null; data?: T | null };

const excelBase = `${browserApiBaseUrl}/Excel`;

let entitiesPromise: Promise<ExcelEntityInfo[]> | null = null;

/** Entities that support Excel for the current admin (cached per page load). */
export function getExcelEntities(): Promise<ExcelEntityInfo[]> {
  entitiesPromise ??= fetch(excelBase, { credentials: 'include', cache: 'no-store' })
    .then(async (res) => {
      if (!res.ok) return [];
      const body = (await res.json()) as ApiResult<ExcelEntityInfo[]>;
      return body?.isSuccess ? body.data ?? [] : [];
    })
    .catch(() => {
      entitiesPromise = null;
      return [];
    });
  return entitiesPromise;
}

export async function getExcelEntity(entity: string) {
  const list = await getExcelEntities();
  return list.find((e) => e.entity.toLowerCase() === entity.toLowerCase()) ?? null;
}

function exportQuery(params: ExcelExportParams) {
  const query = new URLSearchParams();
  if (params.q) query.set('q', params.q);
  if (params.onlyActives !== undefined) query.set('onlyActives', String(params.onlyActives));
  if (params.ids?.length) query.set('ids', params.ids.join(','));
  if (params.sheets?.length) query.set('sheets', params.sheets.join(','));
  Object.entries(params.filters ?? {}).forEach(([key, value]) => {
    if (value !== undefined && value !== '') query.set(key, String(value));
  });
  const text = query.toString();
  return text ? `?${text}` : '';
}

async function download(url: string, fallbackName: string) {
  const res = await fetch(url, { credentials: 'include', cache: 'no-store' });
  if (!res.ok) {
    let message: string | undefined;
    try {
      message = ((await res.json()) as ApiResult<unknown>)?.error ?? undefined;
    } catch {
      /* not JSON */
    }
    throw new Error(message || `HTTP ${res.status}`);
  }

  const disposition = res.headers.get('content-disposition') ?? '';
  const match = /filename\*=UTF-8''([^;]+)|filename="?([^";]+)"?/i.exec(disposition);
  const fileName = match ? decodeURIComponent(match[1] ?? match[2]) : fallbackName;

  const blob = await res.blob();
  const objectUrl = URL.createObjectURL(blob);
  const anchor = document.createElement('a');
  anchor.href = objectUrl;
  anchor.download = fileName;
  document.body.appendChild(anchor);
  anchor.click();
  anchor.remove();
  URL.revokeObjectURL(objectUrl);
}

export function downloadExcelExport(entity: string, params: ExcelExportParams = {}) {
  return download(
    `${excelBase}/${encodeURIComponent(entity)}/export${exportQuery(params)}`,
    `${entity}-export.xlsx`,
  );
}

export function downloadExcelTemplate(entity: string) {
  return download(`${excelBase}/${encodeURIComponent(entity)}/template`, `${entity}-template.xlsx`);
}

export type ExcelImportProgress =
  | { stage: 'uploading'; percent: number }
  | { stage: 'processing' };

/**
 * Uploads the workbook with XHR (fetch cannot report upload progress) and reports
 * "uploading n%" and then "processing" while the server validates and saves rows.
 */
export function importExcel(
  entity: string,
  file: File,
  options: ExcelImportOptions = {},
  onProgress?: (progress: ExcelImportProgress) => void,
): Promise<ApiResult<ExcelImportResult>> {
  const query = new URLSearchParams({
    dryRun: String(options.dryRun ?? false),
    allOrNothing: String(options.allOrNothing ?? true),
  });
  const body = new FormData();
  body.append('file', file);

  return new Promise((resolve) => {
    const xhr = new XMLHttpRequest();
    xhr.open('POST', `${excelBase}/${encodeURIComponent(entity)}/import?${query}`);
    xhr.withCredentials = true;

    xhr.upload.onprogress = (event) => {
      if (event.lengthComputable)
        onProgress?.({ stage: 'uploading', percent: Math.round((event.loaded / event.total) * 100) });
    };
    xhr.upload.onload = () => onProgress?.({ stage: 'processing' });

    xhr.onload = () => {
      try {
        resolve(JSON.parse(xhr.responseText) as ApiResult<ExcelImportResult>);
      } catch {
        resolve({ isSuccess: false, error: `HTTP ${xhr.status}` });
      }
    };
    xhr.onerror = () => resolve({ isSuccess: false, error: 'Upload failed' });
    xhr.ontimeout = () => resolve({ isSuccess: false, error: 'Timeout' });

    onProgress?.({ stage: 'uploading', percent: 0 });
    xhr.send(body);
  });
}
