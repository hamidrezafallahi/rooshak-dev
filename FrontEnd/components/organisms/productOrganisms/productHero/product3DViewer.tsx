'use client';

import {
  useCallback,
  useEffect,
  useRef,
  useState,
} from 'react';

import { useTranslations } from 'next-intl';

import type { ModelViewerElement } from '@/types/model-viewer';
import { toMediaUrl } from '@utils/toMediaUrl';

interface Props {
  modelUrl: string;
  usdzUrl?: string | null;
  poster?: string;
  alt: string;
  /** Increment to ask the viewer to open the AR session (after the model has loaded). */
  arRequest?: number;
  /** Called when AR cannot start on this device (desktop / unsupported phone). */
  onArUnavailable?: () => void;
  /** Model size in metres once loaded (x = width, y = height, z = depth). */
  onModelLoad?: (size: { x: number; y: number; z: number }) => void;
}

/**
 * <model-viewer> wrapper. The library (~three.js) is imported only when this component
 * mounts, i.e. after the visitor asks for the 3D view, so it never costs the product page
 * any LCP / TBT.
 *
 * AR: WebXR / Scene Viewer on Android, AR Quick Look on iOS. The GLB is authored in metres,
 * and `ar-scale="fixed"` keeps it at real size on the table.
 */
export default function Product3DViewer({
  modelUrl,
  usdzUrl,
  poster,
  alt,
  arRequest = 0,
  onArUnavailable,
  onModelLoad,
}: Props) {
  const t = useTranslations('model3d');
  const ref = useRef<ModelViewerElement | null>(null);
  const [ready, setReady] = useState(false);
  const [loaded, setLoaded] = useState(false);
  const [failed, setFailed] = useState(false);
  const handledRequest = useRef(0);

  useEffect(() => {
    let cancelled = false;
    import('@google/model-viewer')
      .then(() => {
        if (!cancelled) setReady(true);
      })
      .catch(() => {
        if (!cancelled) setFailed(true);
      });
    return () => {
      cancelled = true;
    };
  }, []);

  const startAr = useCallback(async () => {
    const el = ref.current;
    if (!el) return;
    if (!el.canActivateAR) {
      onArUnavailable?.();
      return;
    }
    try {
      await el.activateAR();
    } catch {
      // Browsers may drop the tap's user-activation while the model was still loading;
      // the viewer's own AR button (and our button) stay available, so just let the visitor retry.
    }
  }, [onArUnavailable]);

  // Attach load / error listeners once the custom element exists.
  useEffect(() => {
    const el = ref.current;
    if (!ready || !el) return;
    const onLoad = () => {
      setLoaded(true);
      onModelLoad?.(el.getDimensions());
    };
    const onError = () => setFailed(true);
    el.addEventListener('load', onLoad);
    el.addEventListener('error', onError);
    if (el.loaded) onLoad();
    return () => {
      el.removeEventListener('load', onLoad);
      el.removeEventListener('error', onError);
    };
  }, [ready, onModelLoad]);

  // Run a pending AR request as soon as the model is loaded.
  useEffect(() => {
    if (!loaded || arRequest === 0 || handledRequest.current === arRequest) return;
    handledRequest.current = arRequest;
    void startAr();
  }, [arRequest, loaded, startAr]);

  if (failed) {
    return (
      <div className="absolute inset-0 flex items-center justify-center p-4 text-center text-sm text-store-subtle">
        {t('loadError')}
      </div>
    );
  }

  return (
    <>
      {ready && (
        <div className="absolute inset-0">
        <model-viewer
          ref={ref as React.Ref<HTMLElement>}
          src={toMediaUrl(modelUrl)}
          ios-src={usdzUrl ? toMediaUrl(usdzUrl) : undefined}
          poster={poster}
          alt={alt}
          ar
          ar-modes="webxr scene-viewer quick-look"
          ar-scale="fixed"
          ar-placement="floor"
          camera-controls
          touch-action="pan-y"
          shadow-intensity="1"
          environment-image="neutral"
          interaction-prompt="none"
          style={{ width: '100%', height: '100%', background: 'transparent' }}
        />
        </div>
      )}
      {!loaded && (
        <div
          className="absolute inset-0 flex items-center justify-center bg-store-muted/60 text-sm"
          role="status"
        >
          {t('loading')}
        </div>
      )}
    </>
  );
}
