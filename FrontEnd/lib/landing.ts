import 'server-only';

import { ILandingProduct } from '@models/product';

 import { requireAbsoluteUrl, serverApiBaseUrl } from './api';
import { describeStatus, errorTextOf } from './apiError';
import { logger } from './logger';

type LandingTab = 'BestSeller' | 'TheNewest' | 'Discounters';

type LandingProductsEnvelope = {
  isSuccess?: boolean;
  data?: ILandingProduct[] | null;
  error?: string | null;
};

export type LandingSlide = {
  id?: number;
  bannerUrl: string;
  videoUrl?: string;
  mobileBannerUrl?: string;
  mobileVideoUrl?: string;
  firstUrl: string;
  secondUrl?: string;
  /** Text of the optional second link, per locale (both are required when secondUrl is set). */
  secondUrlTextFa?: string;
  secondUrlTextEn?: string;
  /** 0..100: opacity of the banner title and description (100 = fully visible). */
  titleOpacity?: number;
  bannerTitle?: string;
  bannerDescription?: string;
  isHero?: boolean;
  isActive?: boolean;
};

/** A slide of the mid-page home slider (HomeSlides table; separate from the hero banner). */
export type HomeSlide = {
  id: number;
  imageUrl: string;
  mobileImageUrl?: string;
  linkUrl?: string;
  titleFa?: string;
  titleEn?: string;
  displayOrder?: number;
};

export async function getSlides<T = LandingSlide>(): Promise<T[]> {
  const url = requireAbsoluteUrl(
    `${serverApiBaseUrl}/Landing/slide?OnlyActives=true`,
    'getSlides URL',
  );
  try {
    const res = await fetch(url, {
      // `landing` is the tag the admin panel revalidates after a save.
      next: { revalidate: 60, tags: ['Landing/slide', 'landing'] },
    });

    if (!res.ok) {
      logger.error('getSlides HTTP error', {
        scope: 'landing',
        source: 'server',
        url,
        status: res.status,
      });
      return [];
    }

    const data = await res.json();
    return (data?.data ?? []) as T[];
  } catch (err) {
    logger.error('getSlides failed', { scope: 'landing', source: 'server', url }, err);
    return [];
  }
}

/**
 * The full-bleed hero: the active slide flagged `isHero`, else the first one with a
 * video, else the first one. The other hero slides are not shown anywhere: the
 * slider further down the page has its own table and API (see getHomeSlides).
 */
export function pickHeroSlide(slides: LandingSlide[]): LandingSlide | null {
  if (!slides.length) return null;
  return (
    slides.find((s) => s.isHero) ??
    slides.find((s) => Boolean(s.videoUrl)) ??
    slides[0]
  );
}

/** Active slides of the mid-page home slider, in display order (HomeSlides/public). */
export async function getHomeSlides(): Promise<HomeSlide[]> {
  const url = requireAbsoluteUrl(`${serverApiBaseUrl}/HomeSlides/public`, 'getHomeSlides URL');
  try {
    const res = await fetch(url, {
      next: { revalidate: 60, tags: ['homeSlides'] },
    });
    if (!res.ok) {
      logger.error('getHomeSlides HTTP error', { scope: 'landing', source: 'server', url, status: res.status });
      return [];
    }
    const data = await res.json();
    return ((data?.data ?? []) as HomeSlide[]).filter((s) => Boolean(s?.imageUrl));
  } catch (err) {
    logger.error('getHomeSlides failed', { scope: 'landing', source: 'server', url }, err);
    return [];
  }
}

/**
 * Fetches landing product carousels from Products/landings.
 * Backend evaluates flags exclusively (BestSeller → TheNewest → else/Discounters).
 */
export async function getLandingProducts(
  tab: LandingTab,
): Promise<{ products: ILandingProduct[]; error: string | null }> {
  const params = new URLSearchParams({
    BestSeller: String(tab === 'BestSeller'),
    TheNewest: String(tab === 'TheNewest'),
    Discounters: String(tab === 'Discounters'),
  });

  const url = `${serverApiBaseUrl}/Products/landings?${params.toString()}`;

  try {
    const res = await fetch(url, {
      next: { revalidate: 60, tags: ['Products/landings'] },
    });
    const payload = (await res.json().catch(() => null)) as
      | LandingProductsEnvelope
      | ILandingProduct[]
      | null;
    if (!res.ok) {
      console.error(`getLandingProducts failed: ${res.status} ${url}`);
      return {
        products: [],
        error: errorTextOf(payload) ?? describeStatus(res.status),
      };
    }

    if (Array.isArray(payload)) return { products: payload, error: null };
    if (payload?.isSuccess === false) {
      console.error(
        `getLandingProducts error: ${payload.error ?? 'unknown'} ${url}`,
      );
      return { products: [], error: errorTextOf(payload) ?? describeStatus(500) };
    }
    return { products: payload?.data ?? [], error: null };
  } catch (e) {
    console.error(`getLandingProducts error for ${tab}`, e);
    return { products: [], error: describeStatus(503) };
  }
}

export async function getLandingProductsByTabs() {
  const [bestSeller, theNewest, discounters] = await Promise.all([
    getLandingProducts('BestSeller'),
    getLandingProducts('TheNewest'),
    getLandingProducts('Discounters'),
  ]);

  return {
    bestSeller: bestSeller.products,
    theNewest: theNewest.products,
    discounters: discounters.products,
    error: bestSeller.error ?? theNewest.error ?? discounters.error,
  };
}
