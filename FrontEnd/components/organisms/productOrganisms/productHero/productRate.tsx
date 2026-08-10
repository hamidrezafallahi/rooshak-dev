import { getTranslations } from 'next-intl/server';

import { Rate } from '@components/atoms/defaultElements/customRate';
import { serverApiBaseUrl } from '@lib/api';
import { safeFetchJson } from '@lib/safeFetch';
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
    const result = await safeFetchJson<SimpleResponse<IRate>>(
      `${serverApiBaseUrl}/Rates/average?targetType=${EnumTargetType.Product}&targetId=${id}`,
      { next: { revalidate: 36 } },
    );
    resolvedAverage =
      result.ok && result.data?.isSuccess !== false
        ? result.data?.data?.average ?? 0
        : 0;
    resolvedCount =
      result.ok && result.data?.isSuccess !== false
        ? result.data?.data?.count ?? 0
        : 0;
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
