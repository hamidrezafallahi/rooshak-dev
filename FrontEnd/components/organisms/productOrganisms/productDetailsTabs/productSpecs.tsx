import { getTranslations } from 'next-intl/server';

import { serverApiBaseUrl } from '@lib/api';
import { safeFetchJson } from '@lib/safeFetch';
import { SimpleResponse } from '@models/base';
import { ISpecificationResponse } from '@models/product';

export default async function ProductSpecs({ id }: { id: number }) {
  const t = await getTranslations();
  const result = await safeFetchJson<SimpleResponse<ISpecificationResponse>>(
    `${serverApiBaseUrl}/Products/getSpecifications/${id}`,
    { next: { revalidate: 36 } },
  );
  const specs =
    result.ok && result.data?.isSuccess !== false ? result.data?.data : null;

  if (!specs || !specs.specifications?.length) {
    return <p className="text-gray-400 text-sm">{t('product.noSpecs')}</p>;
  }

  return (
    <ul className="space-y-3 text-sm">
      {specs.specifications.map((s, i) => (
        <li
          key={i}
          className="flex justify-between gap-4 pb-2 border-b w-fit"
        >
          <span className="text-gray-500">{s.key} :</span>
          <span className="font-medium">{s.value}</span>
        </li>
      ))}
    </ul>
  );
}
