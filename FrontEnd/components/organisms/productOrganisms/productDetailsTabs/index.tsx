// ProductDetailsTabs.tsx
import { getTranslations } from 'next-intl/server';

import { IDetailedProduct } from '@models/product';

import ProductComments from './productComments';
import ProductDetailsTabsClient from './productDetailsTabsClient';
import ProductSpecs from './productSpecs';

interface Props {
  product: IDetailedProduct;
}

export async function ProductDetailsTabs({ product }: Props) {
  const t = await getTranslations();
  return (
    <section
      className="bg-white shadow-sm mt-12 p-6 rounded-2xl"
      aria-labelledby="product-tabs"
    >
      <h2 id="product-tabs" className="sr-only">
        {t('product.productDetails')}
      </h2>

      <ProductDetailsTabsClient>
        {/* desc */}
        <article className="text-gray-700 text-sm leading-8">
          {product.description}
        </article>
        {/* specs */}
        <ProductSpecs id={product.id} />

        {/* comments */}
        <ProductComments   id={product.id} />
      </ProductDetailsTabsClient>
    </section>
  );
}
