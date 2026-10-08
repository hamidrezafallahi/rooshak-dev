import { IBrand } from './brand';
import { ICategory } from './category';
import { ITag } from './tag';

export type TDimensions = { width: number, height: number, depth: number, weight: number }
export interface ISpecification {
    key: string;
    value: string;
}
export interface ISpecificationResponse {
    id: number;
    name: string;
    specifications: Record<string, string>[]
}
export interface IDetailedProduct {
    name: string;
    description: string;
    id: number;
    slug?: string | null;
    productOfferId?: number;
    bestOfferId?: number | null;
    mainImage: string;
    categoryId: number;
    categoryName?: string | null;
    brandId: number;
    brandName?: string | null;
    imageUrls: string[];
    dimensions: TDimensions;
    price?: number | null;
    finalPrice?: number | null;
    discountIsPercent?: boolean | null;
    discountAmount?: number | null;
    inventory?: number;
    inStock?: boolean;
    currency?: string;
    averageRate?: number;
    rateCount?: number;
    updatedAt?: string | null;
    seoTitleFa?: string | null;
    seoTitleEn?: string | null;
    metaDescriptionFa?: string | null;
    metaDescriptionEn?: string | null;
    brandSlug?: string | null;
    categorySlug?: string | null;
    model3D?: IProductModel3D | null;
}
export interface IProductModel3D {
    /** GLB / glTF (web viewer + Android AR). */
    modelUrl: string;
    /** Optional USDZ for iOS AR Quick Look. */
    usdzUrl?: string | null;
}
export interface IProductScanSource {
    id: number;
    fileUrl: string;
    kind: 'image' | 'video';
    sizeBytes: number;
}
export interface IProductModel3DAdmin {
    productId: number;
    modelId?: number | null;
    modelUrl?: string | null;
    modelSizeBytes?: number | null;
    usdzUrl?: string | null;
    usdzSizeBytes?: number | null;
    scanSources: IProductScanSource[];
}
export interface IDetailedProductOffer {
    id: number;
    slug?: string | null;
    productSlug?: string | null;
    basePrice: number
    createdAt: string
    finalPrice: number
    isActive: boolean
    productId: number
    productName: string
    productImage: string
    productDescription: string
    supplierDesc: string
    supplierId: number
    supplierImage: string
    supplierName: string
}
export interface ILandingProduct {
    id: number
    slug?: string | null
    bestOfferId: number
    name: string
    description: string
    price: number
    inventory: number
    persianCategoryName: string
    englishCategoryName: string
    brand: string
    discountIsPercent: boolean
    discountAmount: number
    finalPrice: number
    mainImage: string
    averageRate: number
    rateCount: number
}
export interface DBCartItems {
    cartItemId: number
    productId: number
    productOfferId: number
    name: string
    description: string;
    basePrice: number
    discountAmount: number
    finalPrice: number
    quantity: number
    mainImage: string
}




export interface SynchronousResponse {
    id: number
    userId: number
    items: DBCartItems[];
    finalTotal: number;
    totalDiscount: number;
    totalPrice: number;
}
export type TUserProduct = {
    brandId: number
    categoryId: number
    description: string
    id: number
    inventory: number
    mainImage: string
    name: string
    price: number
}


export interface IRelatedProduct {
    brand: IBrand;
    category: ICategory
    description: string
    finalPrice: number
    id: number
    mainImage: string
    name: string
    price: number
    tags: ITag[]

}


export interface ICartProduct {
    id: number;
    productOfferId: number;
    cartItemId?: number;
    name: string;
    description: string;
    price: number;
    discountAmount: number;
    discountIsPercent: boolean;
    finalPrice: number;
    quantity: number;
    mainImage: string;
}

export interface IProductTags {
    id: number
    productId: number
    tagId: number
    tagName: string
}

// @models/product.ts
export interface IProduct {
  id: number;
  name: string;
  description: string;
  price: number;
  finalPrice: number;
  inventory: number;
  categoryId: number;
  brandId: number;
  mainImage: string | null;
 
}
 

// برای پاسخ صفحه‌بندی شده
export interface ProductResponse {
  records: IProduct[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}