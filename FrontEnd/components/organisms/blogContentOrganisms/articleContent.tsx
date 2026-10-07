import React from 'react';

import { getTranslations } from 'next-intl/server';

import { stripFaqSectionFromHtml } from '@lib/blogFaq';

import { IProps } from './type';

export async function ArticleContent({ ...props }: IProps) {
  const { blog, locale } = props;
  const isRTL = locale == 'fa';
  const t = await getTranslations({ locale });

  const rawContent = isRTL ? blog.contentFa : blog.contentEn || '';
  const contentHtml = stripFaqSectionFromHtml(rawContent);

  return (
    <>
      <span id="section1" className="scroll-mt-24 snap-start">
        {t('blog.introduction')}
      </span>
      <div
        className={`p-4 prose prose-lg max-w-none ${
          isRTL ? 'prose-rtl' : 'prose-ltr'
        }`}
        dangerouslySetInnerHTML={{
          __html: isRTL ? blog.introFa : blog.introEn || '',
        }}
      ></div>
      <span id="section2" className="scroll-mt-24 snap-start">
        {t('blog.content')}
      </span>
      <div
        className={` p-4 prose prose-lg max-w-none ${
          isRTL ? 'prose-rtl' : 'prose-ltr'
        }`}
        dangerouslySetInnerHTML={{
          __html: contentHtml,
        }}
      ></div>
      <span id="section3" className="scroll-mt-24 snap-start">
        {t('blog.conclusion')}
      </span>
      <div
        className={` p-4 prose prose-lg max-w-none ${
          isRTL ? 'prose-rtl' : 'prose-ltr'
        }`}
        dangerouslySetInnerHTML={{
          __html: isRTL ? blog.conclusionFa : blog.conclusionEn || '',
        }}
      ></div>
    </>
  );
}
