import { IDetailedProduct } from '@models/product';

import ProductGallery from './productGallery';
import ProductInfo from './productInfo';

interface Props {
  product: IDetailedProduct;
}
export default function ProductHero({ product }: Props) {
  return (
    <section className="items-start gap-8 lg:gap-16 grid grid-cols-1 lg:grid-cols-[minmax(0,1.25fr)_minmax(0,1fr)]">
      <ProductGallery product={product} />
      <ProductInfo product={product} />
    </section>
  );
}
