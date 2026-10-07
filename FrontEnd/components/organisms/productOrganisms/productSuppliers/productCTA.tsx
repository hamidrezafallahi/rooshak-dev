"use client";

import { useTranslations } from 'next-intl';
import { useDispatch } from 'react-redux';

import { useGetConditionallyMutation } from '@services/base';
import { synchronousCart } from '@slice/shoppingCartSlice';
import { getCookie } from '@utils/core';

export default function ProductCTA({
  id,
  productId,
}: {
  id: number;
  productId: number;
}) {
  const t = useTranslations();
  const isAuthenticated = Boolean(getCookie("candySession"));
  const [addToShoppingCart] = useGetConditionallyMutation();
  const dispatch = useDispatch();
  const handleAddToCart = async () => {
    if (isAuthenticated) {
      const syncCartResponse = await addToShoppingCart({
        url: '/CartItems',
        body: {
          productId,
          productOfferId: id,
          quantity: 1,
        },
      }).unwrap();
      if (syncCartResponse.isSuccess) {
        dispatch(synchronousCart(syncCartResponse.data));
      }
    } else {
      // dispatch(addToCart({ product: product }));
    }
  };
  return (
    <div className="flex flex-col sm:flex-row gap-2 w-full min-w-0">
      <button
        type="button"
        onClick={handleAddToCart}
        className="flex-1 min-w-0 bg-primary hover:bg-transparent px-4 py-3 border border-primary font-medium text-primary-foreground hover:text-store-text text-sm text-center transition-colors"
      >
        {t('common.addToCart')}
      </button>
      <button
        type="button"
        className="flex-1 min-w-0 hover:bg-store-text px-4 py-3 border border-store-strong font-medium text-store-text hover:text-store-surface text-sm text-center transition-colors"
      >
        {t('common.wishlist')}
      </button>
    </div>
  );
}
