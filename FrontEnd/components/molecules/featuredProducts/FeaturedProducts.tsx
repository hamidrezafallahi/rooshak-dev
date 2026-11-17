import ProductCard from './ProductCard';
import { IFeaturedProduct } from './type';

type Props = {
  /** Only the first three are rendered. */
  products: IFeaturedProduct[];
  ctaLabel: string;
  /** Visually hidden section heading (keeps the outline semantic). */
  heading: string;
};

/**
 * Three floating product cards on an off-white band.
 * - md+: one centered row.
 * - < md: horizontal swipe with scroll-snap; the next card peeks in.
 */
export default function FeaturedProducts({ products, ctaLabel, heading }: Props) {
  const items = products.slice(0, 3);
  if (!items.length) return null;

  return (
    <section aria-label={heading} className="bg-[#F5F5F7] py-10 md:py-20 w-full overflow-hidden">
      <h2 className="sr-only">{heading}</h2>
      <ul
        className={[
          'mx-auto flex max-w-[1440px] items-center',
          // mobile/small tablet: swipeable row
          'snap-x snap-mandatory gap-5 overflow-x-auto overscroll-x-contain px-[12vw] py-8',
          '[scrollbar-width:none] [&::-webkit-scrollbar]:hidden',
          // md+: static centered row
          'md:snap-none md:justify-center md:gap-7 md:overflow-visible md:px-6 lg:gap-10',
        ].join(' ')}
      >
        {items.map((product) => (
          <li key={product.id} className="snap-center shrink-0">
            <ProductCard product={product} ctaLabel={ctaLabel} />
          </li>
        ))}
      </ul>
    </section>
  );
}
