import { IUser } from '@models/user';

export interface IDetailedProductCardProps {
  product: {
    id: number;
    slug?: string | null;
    name: string;
    description: string;
    price: number;
    finalPrice: number;
    inventory: number;
    mainImage: string | null;
    imageUrl?: string;
    categoryName?: string;
    brandName?: string;
    discount?: number;
    rating?: number;
    isAvailable?: boolean;
  };
  locale: string;
}
export interface ISimpleProduct {
    id: number;
    slug?: string | null;
    name: string;
    description: string;
    mainImage: string | null;
    price?: number;
    finalPrice?: number;
    inventory?: number;
    suppliers?:IUser[]
}
 