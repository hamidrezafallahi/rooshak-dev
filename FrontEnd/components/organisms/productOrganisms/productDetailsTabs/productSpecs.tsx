import { getTranslations } from 'next-intl/server';

import { serverApiBaseUrl } from '@lib/api';
import { SimpleResponse } from '@models/base';
import { ISpecificationResponse } from '@models/product';

export default async function ProductSpecs({ id }: {id:number}) { 
  const t = await getTranslations();
   const response = await fetch(
      `${serverApiBaseUrl}/Products/getSpecifications/${id}`,
      {
        next: { revalidate: 36 }, // ISR
      },
    );
  
    if (!response.ok) {
      throw new Error("Failed to fetch comments");
    }
  
    const specs:SimpleResponse<ISpecificationResponse> = await response.json();
  if (!specs.data || specs.data.specifications.length === 0) {
    return (
      <p className="text-gray-400 text-sm">
        {t('product.noSpecs')}
      </p>
    );
  }

  return (
    <ul className="space-y-3 text-sm">
      {specs.data.specifications.map((s, i) => (
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
