import React from 'react';

import { useLocale } from 'next-intl';
import Link from 'next/link';

import {
  ArrowLongLeft,
  ArrowLongRight,
} from '@components/atoms/iconComponents';

function BackToLandingPageButton() {
    const locale = useLocale()
  return (
         <Link
        className="border border-store-border hover:border-store-strong p-2 transition-colors"
        href={`/${locale}`}
      >
           {locale == "fa"? <ArrowLongRight/>:<ArrowLongLeft/>}
      </Link>
  )
}

export default BackToLandingPageButton