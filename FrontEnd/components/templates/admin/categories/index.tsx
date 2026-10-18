"use client";

import { useState } from 'react';

import { useTranslations } from 'next-intl';

import Tree, { ITreeContext } from '@components/atoms/defaultElements/tree';
import ExcelToolbar from '@components/organisms/excelToolbar';
import FormGenerator from '@components/organisms/formGenerator';
import { IFormConfig } from '@components/organisms/formGenerator/type';

interface IProps {
  categories: ITreeContext[];
  entityFormConfig: IFormConfig;
}

export default function AdminCategoryTemplate({ ...props }: IProps) {
  const { categories, entityFormConfig } = props;
  const [defaultValues, setDefaultValues] = useState<
    Partial<ITreeContext> | undefined
  >(undefined);
  const mapCategoryToContext = (cat: ITreeContext): ITreeContext => ({
    id: cat.id,
    parentCategoryId: cat.parentCategoryId,
    persianName: cat.persianName,
    englishName: cat.englishName,
    categoryEnglishDesc: cat.categoryEnglishDesc,
    categoryPersianDesc: cat.categoryPersianDesc,
    categoryCover: cat.categoryCover,
    isShowInLanding: cat.isShowInLanding,
    isActive: cat.isActive,
    subCategories: cat.subCategories?.map(mapCategoryToContext) ?? [],
  });
  const t = useTranslations();
  const content: ITreeContext[] = categories.map(mapCategoryToContext);

  return (
    <div className="admin-page">
      <header className="admin-page-header">
        <div>
          <h1 className="admin-page-title">
            {t('admin.listTitle', { entity: entityFormConfig.persianDisplayName || entityFormConfig.englishDisplayName })}
          </h1>
          <p className="admin-page-subtitle">{t('admin.listSubtitle')}</p>
        </div>
        <div className="flex flex-wrap items-center gap-2">
          <ExcelToolbar entity={entityFormConfig.endPoint || 'categories'} />
          <button
            type="button"
            className="admin-btn admin-btn-primary w-full sm:w-auto"
            onClick={() => setDefaultValues(undefined)}
          >
            {t('general.new')}
          </button>
        </div>
      </header>

      <div className="gap-4 sm:gap-5 grid grid-cols-1 xl:grid-cols-2 items-start">
        <div className="admin-panel p-3 sm:p-4 min-w-0 overflow-hidden">
          <Tree
            data={content}
            clickable
            endPoint={entityFormConfig.endPoint}
            onClick={(node) => setDefaultValues(node)}
          />
        </div>
        <div className="min-w-0">
          <FormGenerator
            entityFormConfig={entityFormConfig}
            defaultValues={defaultValues}
            embedded
          />
        </div>
      </div>
    </div>
  );
}
