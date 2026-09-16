import type { NextConfig } from 'next';
import createNextIntlPlugin from 'next-intl/plugin';
import path from 'path';

const withNextIntl = createNextIntlPlugin();

/** Drop Next's always-on modern-API polyfills (Lighthouse "Legacy JavaScript"). */
const modernPolyfillStub = path.join(process.cwd(), 'lib/modern-polyfill.js');

const nextConfig: NextConfig = {
  // ─── TypeScript ──────────────────────────────────────────────────────────
  // Fail build on TypeScript errors in production
  typescript: {
    ignoreBuildErrors: process.env.NODE_ENV === 'development',
  },

  // ─── ESLint ──────────────────────────────────────────────────────────────
  eslint: {
    ignoreDuringBuilds: true,
  },

  // ─── React ───────────────────────────────────────────────────────────────
  reactStrictMode: process.env.NODE_ENV === 'development',

  // ─── Output ──────────────────────────────────────────────────────────────
  // Standalone mode for Docker: produces minimal self-contained output
  output: 'standalone',

  // ─── Images ──────────────────────────────────────────────────────────────
  images: {
    remotePatterns: [
      {
        protocol: "http",
        hostname: "localhost",
        pathname: "/uploads/**",
      },
      {
        protocol: "http",
        hostname: "backend",
        pathname: "/uploads/**",
      },
      {
        protocol: "https",
        hostname: "rooshakshop.ir",
        pathname: "/uploads/**",
      },
      {
        protocol: "https",
        hostname: "www.rooshakshop.ir",
        pathname: "/uploads/**",
      },
      {
        protocol: "https",
        hostname: "i.pravatar.cc",
      },
    ],

    // Include mid sizes so card/hero displays (~240–412px @2x) avoid w=640.
    deviceSizes: [384, 480, 640, 750, 828, 1080, 1200, 1920, 2048, 3840],
    imageSizes: [16, 32, 48, 64, 96, 128, 256, 320, 384],
    // Prefer WebP for smaller images
    formats: ['image/avif', 'image/webp'],
    // Cache optimized images for 7 days
    minimumCacheTTL: 604800,
  },

  // ─── Compiler ────────────────────────────────────────────────────────────
  // Remove console.log in production (except errors)
  compiler: {
    removeConsole: process.env.NODE_ENV === 'production' ? {
      exclude: ['error', 'warn'],
    } : false,
  },

  // ─── Experimental ────────────────────────────────────────────────────────
  experimental: {
    extensionAlias: {
      '.js': ['.js', '.ts', '.tsx'],
    },
    // Residual server actions (revalidate); uploads go through /auth/bff
    serverActions: {
      bodySizeLimit: '50mb',
    },
    // Avoid nginx 429 during SSG when many slug pages fetch in parallel.
    staticGenerationMaxConcurrency: 3,
    staticGenerationRetryCount: 2,
    // Inline CSS into <style> instead of render-blocking <link> (production only).
    // Worth it here: total CSS is ~16 KiB (Tailwind + font). Trade-off: no
    // separate stylesheet cache; HTML grows slightly on first document.
    inlineCss: true,
  },

  // Keep title/description/canonical in the initial <head> for all UAs
  // (including Lighthouse). Next 15.2+ otherwise streams metadata into <body>.
  htmlLimitedBots: /.*/,

  // Next injects polyfill-module into the app chunk regardless of browserslist.
  // Alias it to an empty stub for modern-only targets (see package.json browserslist).
  turbopack: {
    resolveAlias: {
      '../build/polyfills/polyfill-module': './lib/modern-polyfill.js',
      'next/dist/build/polyfills/polyfill-module': './lib/modern-polyfill.js',
    },
  },

  webpack(config) {
    config.resolve.alias = {
      ...config.resolve.alias,
      '../build/polyfills/polyfill-module': modernPolyfillStub,
      'next/dist/build/polyfills/polyfill-module': modernPolyfillStub,
    };
    return config;
  },

  // ─── Headers ─────────────────────────────────────────────────────────────
  async headers() {
    return [
      {
        source: '/(.*)',
        headers: [
          {
            key: 'X-Frame-Options',
            value: 'SAMEORIGIN',
          },
          {
            key: 'X-Content-Type-Options',
            value: 'nosniff',
          },
          {
            key: 'X-XSS-Protection',
            value: '1; mode=block',
          },
          {
            key: 'Referrer-Policy',
            value: 'strict-origin-when-cross-origin',
          },
          {
            // Isolate browsing context; allow payment/OAuth popups.
            key: 'Cross-Origin-Opener-Policy',
            value: 'same-origin-allow-popups',
          },
        ],
      },
      {
        source: '/_next/static/(.*)',
        headers: [
          {
            key: 'Cache-Control',
            value: 'public, max-age=31536000, immutable',
          },
        ],
      },
      {
        source: '/images/(.*)',
        headers: [
          {
            key: 'Cache-Control',
            value: 'public, max-age=86400, stale-while-revalidate=604800',
          },
        ],
      },
    ];
  },

  // ─── Powered By Header ──────────────────────────────────────────────────
  poweredByHeader: false,

  // Uploads live on the shared Docker volume / ASP.NET wwwroot — not in Next public/.
  // When the browser hits the frontend container (or locale middleware rewrites),
  // proxy /uploads/* to the backend so MediaImage src="/uploads/..." always works.
  async rewrites() {
    const uploadsOrigin = (
      process.env.INTERNAL_UPLOADS_ORIGIN?.trim() ||
      process.env.INTERNAL_SERVER_SIDE_API_URL?.trim() ||
      process.env.NEXT_PUBLIC_INTERNAL_API_URL?.trim() ||
      'http://backend:8080'
    ).replace(/\/+$/, '');

    return [
      {
        source: '/uploads/:path*',
        destination: `${uploadsOrigin}/uploads/:path*`,
      },
    ];
  },
};

export default withNextIntl(nextConfig);