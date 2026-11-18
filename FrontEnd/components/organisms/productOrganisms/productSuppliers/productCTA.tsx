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
      try {
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
      } catch {
        // baseQueryByToken already showed the reason.
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
        className="flex-1 px-4 min-w-0 store-btn store-btn-primary"
      >
        {t('common.addToCart')}
      </button>
      <button
        type="button"
        className="flex-1 px-4 min-w-0 store-btn"
      >
        {t('common.wishlist')}
      </button>
    </div>
  );
}
