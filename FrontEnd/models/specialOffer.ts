export interface SpecialOffer {
  id: number;
  product: {
    id: number;
    productId?: number;
    productOfferId: number;
    name: string;
    description: string;
    price: number;
    inventory: number;
    categoryId: number;
    brandId: number;
    mainImage: string;
    discountId: number,
    discountIsPercent: boolean,
    discountAmount: number,
    finalPrice: number,
    imageUrls: string[],
    tags: { id: number, name: string }[]
  };
  startDate: string;
  endDate: string;
  displayOrder: number;
}