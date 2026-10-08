import type { DetailedHTMLProps, HTMLAttributes } from 'react';

/** Subset of the <model-viewer> element API used by the storefront / admin. */
export interface ModelViewerElement extends HTMLElement {
  canActivateAR: boolean;
  loaded: boolean;
  activateAR: () => Promise<void>;
}

type ModelViewerAttributes = {
  src?: string;
  'ios-src'?: string;
  poster?: string;
  alt?: string;
  ar?: boolean | '';
  'ar-modes'?: string;
  'ar-scale'?: 'auto' | 'fixed';
  'ar-placement'?: 'floor' | 'wall';
  'camera-controls'?: boolean | '';
  'touch-action'?: string;
  'shadow-intensity'?: string;
  'shadow-softness'?: string;
  'environment-image'?: string;
  exposure?: string;
  loading?: 'auto' | 'lazy' | 'eager';
  reveal?: 'auto' | 'interaction' | 'manual';
  'interaction-prompt'?: 'auto' | 'none';
  'auto-rotate'?: boolean | '';
};

declare module 'react' {
  namespace JSX {
    interface IntrinsicElements {
      'model-viewer': DetailedHTMLProps<HTMLAttributes<HTMLElement>, HTMLElement> &
        ModelViewerAttributes;
    }
  }
}
