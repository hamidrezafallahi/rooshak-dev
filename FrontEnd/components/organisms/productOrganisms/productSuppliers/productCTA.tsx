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
    <div className="flex gap-3 mt-4">
      <button
        onClick={handleAddToCart}
        className="bg-primary px-6 py-3 rounded-xl text-white"
      >
        {t('common.addToCart')}
      </button>
      <button className="px-6 py-3 border rounded-xl">
        ❤️ {t('common.wishlist')}
      </button>
    </div>
  );
}
