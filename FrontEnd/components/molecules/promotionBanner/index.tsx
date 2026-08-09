"use client";

import { useTranslations } from 'next-intl';

function PromotionBanner() {
  const t = useTranslations('promotion');
  const hasBanner = false;

  if (!hasBanner) return null;

  return (
    <div className="bg-red-600 p-4 w-full h-12 font-bold text-white text-lg text-center">
      {t('yaldaBanner')}
    </div>
  );
}
export default PromotionBanner;
