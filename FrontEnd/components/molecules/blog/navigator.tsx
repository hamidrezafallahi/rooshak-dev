import React from 'react';

import { getTranslations } from 'next-intl/server';

export async function Navigator({ ...props }: IProps) {
  const {   locale } = props;

  const t = await getTranslations({ locale });
  const isRTL = locale == "fa";
  return (
    <div className="p-6 border border-store-border">
      <h3 className={`font-normal text-lg mb-4 ${isRTL ? "text-right" : ""}`}>
        {t("blog.tableOfContents")}
      </h3>
      <nav className={`space-y-2 ${isRTL ? "text-right" : ""}`}>
        <a
          href="#section1"
          className="block py-2 text-store-subtle hover:text-store-text"
        >
        {t("blog.introduction")}
        </a>
        <a
          href="#section2"
          className="block py-2 text-store-subtle hover:text-store-text"
          >
            {t("blog.content")}
 
        </a>
        <a
          href="#section3"
          className="block py-2 text-store-subtle hover:text-store-text"
          >
            {t("blog.conclusion")}
        </a>
      </nav>
    </div>
  );
}
