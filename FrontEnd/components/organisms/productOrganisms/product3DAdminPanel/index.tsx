'use client';

import {
  useCallback,
  useEffect,
  useRef,
  useState,
} from 'react';

import { useTranslations } from 'next-intl';
import dynamic from 'next/dynamic';

import QrDialog from '@components/molecules/qrDialog';
import { browserApiBaseUrl } from '@lib/api';
import { IProductModel3DAdmin } from '@models/product';
import { toMediaUrl } from '@utils/toMediaUrl';

const Product3DViewer = dynamic(
  () => import('../productHero/product3DViewer'),
  { ssr: false },
);

const ENDPOINT = `${browserApiBaseUrl}/ProductModels3D`;

type Envelope<T> = { isSuccess: boolean; data: T | null; error?: string | null };

function formatSize(bytes?: number | null) {
  if (!bytes) return '';
  return bytes >= 1024 * 1024
    ? `${(bytes / 1024 / 1024).toFixed(1)} MB`
    : `${Math.max(1, Math.round(bytes / 1024))} KB`;
}

async function call<T>(url: string, init?: RequestInit): Promise<Envelope<T>> {
  try {
    const res = await fetch(url, { credentials: 'include', cache: 'no-store', ...init });
    return (await res.json()) as Envelope<T>;
  } catch (e) {
    return { isSuccess: false, data: null, error: e instanceof Error ? e.message : 'Network error' };
  }
}

/**
 * Admin section on the product edit page: upload the GLB / USDZ, preview it, and collect
 * phone captures (photos / video) that are fed to a scanning tool to build the GLB.
 */
export default function Product3DAdminPanel({ productId }: { productId: number }) {
  const t = useTranslations('model3dAdmin');
  const tv = useTranslations('model3d');

  const [info, setInfo] = useState<IProductModel3DAdmin | null>(null);
  const [loadFailed, setLoadFailed] = useState(false);
  const [busy, setBusy] = useState(false);
  const [message, setMessage] = useState<{ ok: boolean; text: string } | null>(null);
  const [showPreview, setShowPreview] = useState(false);
  const [qrOpen, setQrOpen] = useState(false);
  const [pageUrl, setPageUrl] = useState('');

  const modelInput = useRef<HTMLInputElement>(null);
  const usdzInput = useRef<HTMLInputElement>(null);
  const photoInput = useRef<HTMLInputElement>(null);
  const videoInput = useRef<HTMLInputElement>(null);
  const galleryInput = useRef<HTMLInputElement>(null);

  const refresh = useCallback(async () => {
    const res = await call<IProductModel3DAdmin>(`${ENDPOINT}/${productId}`);
    if (res.isSuccess && res.data) {
      setInfo(res.data);
      setLoadFailed(false);
    } else {
      setLoadFailed(true);
    }
  }, [productId]);

  useEffect(() => {
    void refresh();
    setPageUrl(window.location.href);
  }, [refresh]);

  const report = (res: Envelope<unknown>, okText = t('uploaded')) =>
    setMessage(res.isSuccess ? { ok: true, text: okText } : { ok: false, text: res.error || 'Error' });

  const uploadModel = async () => {
    const model = modelInput.current?.files?.[0];
    const usdz = usdzInput.current?.files?.[0];
    if (!model && !usdz) return;
    const body = new FormData();
    if (model) body.append('Model', model);
    if (usdz) body.append('Usdz', usdz);
    setBusy(true);
    const res = await call<IProductModel3DAdmin>(`${ENDPOINT}/${productId}`, { method: 'PUT', body });
    setBusy(false);
    report(res);
    if (res.isSuccess && res.data) {
      setInfo(res.data);
      if (modelInput.current) modelInput.current.value = '';
      if (usdzInput.current) usdzInput.current.value = '';
    }
  };

  const removeUsdz = async () => {
    if (!window.confirm(t('confirmDelete'))) return;
    const body = new FormData();
    body.append('RemoveUsdz', 'true');
    setBusy(true);
    const res = await call<IProductModel3DAdmin>(`${ENDPOINT}/${productId}`, { method: 'PUT', body });
    setBusy(false);
    report(res);
    if (res.isSuccess && res.data) setInfo(res.data);
  };

  const deleteModel = async () => {
    if (!window.confirm(t('confirmDelete'))) return;
    setBusy(true);
    const res = await call<unknown>(`${ENDPOINT}/${productId}`, { method: 'DELETE' });
    setBusy(false);
    report(res);
    setShowPreview(false);
    await refresh();
  };

  const uploadCaptures = async (files: FileList | null) => {
    if (!files || files.length === 0) return;
    const body = new FormData();
    Array.from(files).forEach((f) => body.append('Files', f));
    setBusy(true);
    const res = await call<IProductModel3DAdmin>(`${ENDPOINT}/${productId}/scan-sources`, {
      method: 'POST',
      body,
    });
    setBusy(false);
    report(res);
    if (res.isSuccess && res.data) setInfo(res.data);
    [photoInput, videoInput, galleryInput].forEach((r) => {
      if (r.current) r.current.value = '';
    });
  };

  const deleteCapture = async (id: number) => {
    if (!window.confirm(t('confirmDelete'))) return;
    setBusy(true);
    const res = await call<unknown>(`${ENDPOINT}/scan-sources/${id}`, { method: 'DELETE' });
    setBusy(false);
    report(res, '✓');
    await refresh();
  };

  const btn =
    'admin-btn disabled:opacity-50 disabled:cursor-not-allowed';
  const btnPrimary = `${btn} admin-btn-primary`;

  return (
    <section className="admin-panel mt-4 flex flex-col gap-6 p-4 sm:p-5 md:p-6" aria-labelledby="model3d-title">
      <header>
        <h2 id="model3d-title" className="text-base font-semibold">
          {t('title')}
        </h2>
        <p className="mt-1 text-sm opacity-70">{t('subtitle')}</p>
      </header>

      {loadFailed && <p className="text-sm text-error">{t('loadError')}</p>}

      {message && (
        <p role="status" className={`text-sm ${message.ok ? 'text-success' : 'text-error'}`}>
          {message.text}
        </p>
      )}

      {/* ── Model ─────────────────────────────────────────────── */}
      <div className="flex flex-col gap-3">
        <h3 className="text-sm font-semibold">{t('currentModel')}</h3>
        {info?.modelUrl ? (
          <div className="flex flex-col gap-2 text-sm">
            <div className="flex flex-wrap items-center gap-2">
              <a href={toMediaUrl(info.modelUrl)} target="_blank" rel="noreferrer" className="underline" dir="ltr">
                GLB · {formatSize(info.modelSizeBytes)}
              </a>
              {info.usdzUrl && (
                <a href={toMediaUrl(info.usdzUrl)} target="_blank" rel="noreferrer" className="underline" dir="ltr">
                  USDZ · {formatSize(info.usdzSizeBytes)}
                </a>
              )}
            </div>
            <div className="flex flex-wrap gap-2">
              <button type="button" className={btn} onClick={() => setShowPreview((v) => !v)}>
                {t('preview')}
              </button>
              {info.usdzUrl && (
                <button type="button" className={btn} onClick={removeUsdz} disabled={busy}>
                  {t('removeUsdz')}
                </button>
              )}
              <button type="button" className={btn} onClick={deleteModel} disabled={busy}>
                {t('deleteModel')}
              </button>
            </div>
            {showPreview && (
              <div className="relative aspect-square max-w-md overflow-hidden bg-store-muted">
                <Product3DViewer
                  modelUrl={info.modelUrl}
                  usdzUrl={info.usdzUrl}
                  alt="3D preview"
                  onArUnavailable={() => setQrOpen(true)}
                />
              </div>
            )}
          </div>
        ) : (
          <p className="text-sm opacity-70">{t('noModel')}</p>
        )}

        <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm">
            <span>{t('modelFile')}</span>
            <input
              ref={modelInput}
              type="file"
              accept=".glb,.gltf,model/gltf-binary,model/gltf+json"
              className="text-sm"
            />
            <span className="text-xs opacity-70">{t('modelHelp')}</span>
          </label>
          <label className="flex flex-col gap-1 text-sm">
            <span>{t('usdzFile')}</span>
            <input ref={usdzInput} type="file" accept=".usdz,model/vnd.usdz+zip" className="text-sm" />
            <span className="text-xs opacity-70">{t('usdzHelp')}</span>
          </label>
        </div>
        <div>
          <button type="button" className={btnPrimary} onClick={uploadModel} disabled={busy}>
            {busy ? t('uploading') : t('upload')}
          </button>
        </div>
      </div>

      {/* ── Phone scan ────────────────────────────────────────── */}
      <div className="flex flex-col gap-3 border-t border-store-border pt-5">
        <h3 className="text-sm font-semibold">{t('scanTitle')}</h3>
        <p className="text-sm leading-6 opacity-80">{t('scanIntro')}</p>

        <div className="flex flex-wrap gap-2">
          {/* `capture` opens the phone camera directly; desktop browsers fall back to a file picker. */}
          <input
            ref={photoInput}
            type="file"
            accept="image/*"
            capture="environment"
            hidden
            onChange={(e) => uploadCaptures(e.target.files)}
          />
          <input
            ref={videoInput}
            type="file"
            accept="video/*"
            capture="environment"
            hidden
            onChange={(e) => uploadCaptures(e.target.files)}
          />
          <input
            ref={galleryInput}
            type="file"
            accept="image/*,video/*"
            multiple
            hidden
            onChange={(e) => uploadCaptures(e.target.files)}
          />
          <button type="button" className={btnPrimary} disabled={busy} onClick={() => photoInput.current?.click()}>
            {t('takePhoto')}
          </button>
          <button type="button" className={btnPrimary} disabled={busy} onClick={() => videoInput.current?.click()}>
            {t('recordVideo')}
          </button>
          <button type="button" className={btn} disabled={busy} onClick={() => galleryInput.current?.click()}>
            {t('pickFiles')}
          </button>
          <button type="button" className={btn} onClick={() => setQrOpen(true)}>
            {t('continueOnPhone')}
          </button>
        </div>

        <ul className="list-disc space-y-1 ps-5 text-xs leading-5 opacity-80">
          <li>{t('tip1')}</li>
          <li>{t('tip2')}</li>
          <li>{t('tip3')}</li>
          <li>{t('tip4')}</li>
        </ul>
        <p className="text-xs opacity-80" dir="ltr">
          <a className="underline" href="https://www.unrealengine.com/realityscan" target="_blank" rel="noreferrer">RealityScan</a>
          {' · '}
          <a className="underline" href="https://poly.cam" target="_blank" rel="noreferrer">Polycam</a>
          {' · '}
          <a className="underline" href="https://www.kiriengine.app" target="_blank" rel="noreferrer">KIRI Engine</a>
        </p>

        <h4 className="text-sm font-semibold">{t('captures')}</h4>
        {info && info.scanSources.length > 0 ? (
          <ul className="grid grid-cols-2 gap-3 sm:grid-cols-4 md:grid-cols-6">
            {info.scanSources.map((s) => (
              <li key={s.id} className="flex flex-col gap-1 text-xs">
                <a href={toMediaUrl(s.fileUrl)} target="_blank" rel="noreferrer" className="block aspect-square overflow-hidden bg-store-muted">
                  {s.kind === 'image' ? (
                    // eslint-disable-next-line @next/next/no-img-element
                    <img src={toMediaUrl(s.fileUrl)} alt="" loading="lazy" className="h-full w-full object-cover" />
                  ) : (
                    <video src={toMediaUrl(s.fileUrl)} preload="metadata" muted className="h-full w-full object-cover" />
                  )}
                </a>
                <span className="flex items-center justify-between gap-1">
                  <span dir="ltr">{formatSize(s.sizeBytes)}</span>
                  <button type="button" className="underline" onClick={() => deleteCapture(s.id)} disabled={busy}>
                    ✕
                  </button>
                </span>
              </li>
            ))}
          </ul>
        ) : (
          <p className="text-sm opacity-70">{t('noCaptures')}</p>
        )}
      </div>

      <QrDialog
        open={qrOpen}
        title={t('continueOnPhone')}
        body={t('qrAdminBody')}
        url={pageUrl}
        closeLabel={tv('close')}
        onClose={() => setQrOpen(false)}
      />
    </section>
  );
}
