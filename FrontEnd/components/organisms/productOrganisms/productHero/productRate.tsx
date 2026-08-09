import { getTranslations } from 'next-intl/server';

import { Rate } from '@components/atoms/defaultElements/customRate';
import { serverApiBaseUrl } from '@lib/api';
import { SimpleResponse } from '@models/base';
import { EnumTargetType } from '@models/comment';
import { IRate } from '@models/rate';

type Props = {
  id: number;
  average?: number;
  count?: number;
};

export default async function ProductRate({ id, average, count }: Props) {
  const t = await getTranslations();
  let resolvedAverage = average;
  let resolvedCount = count;

  if (resolvedAverage == null || resolvedCount == null) {
    const response = await fetch(
      `${serverApiBaseUrl}/Rates/average?targetType=${EnumTargetType.Product}&targetId=${id}`,
      { next: { revalidate: 36 } },
    );
    const rate: SimpleResponse<IRate> = await response.json();
    resolvedAverage = rate.data?.average ?? 0;
    resolvedCount = rate.data?.count ?? 0;
  }

  return (
    <div className="flex items-center gap-2">
      <Rate value={resolvedAverage ?? 0} />
      <span className="text-gray-200 text-sm">
        ({t('common.reviewsCount', { count: resolvedCount ?? 0 })})
      </span>
    </div>
  );
}
