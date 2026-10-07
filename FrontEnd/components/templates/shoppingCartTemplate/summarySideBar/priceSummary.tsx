import React from 'react';

import { useTranslations } from 'next-intl';
import { shallowEqual } from 'react-redux';

import {
  MinusIcon,
  PlusIcon2,
  RialIcon,
} from '@components/atoms/iconComponents';
import { useAppSelector } from '@store/index';

function PriceSummary() {
    const t = useTranslations();
      const { ShoppingCart } = useAppSelector(
        (state) => ({
          ShoppingCart: state.withPersist.ShoppingCart,
        }),
        shallowEqual
      );
    const kol =
      (ShoppingCart?.finalTotal ?? 0) +
      (ShoppingCart?.shippingMethod?.price ?? 0);
  return (
         <div className="space-y-2 py-2 border-store-border border-b">
        <div className="flex justify-between text-sm">
          <span className="text-store-subtle">{t("shoppingCart.subTotal")}</span>
          <span className="flex gap-2 font-medium">
            <span className='relative end-4'>{ShoppingCart.totalPrice.toFixed()}</span>
            <RialIcon />
          </span>
        </div>
        <div className="flex justify-between text-sm">
          <span className="text-store-subtle">{t("shoppingCart.totalDiscount")}</span>
          <span className="flex items-center gap-2 font-medium">
            {ShoppingCart.totalDiscount.toFixed()}<MinusIcon config={{className:"stroke-rose-800"}}/>
            <RialIcon />
          </span>
        </div>
        {ShoppingCart.discountCodeAmount > 0 &&<div className="flex justify-between text-sm">
          <span className="text-store-subtle">{t("shoppingCart.discountCodeAmount")} </span>
          <span className="flex items-center gap-2 font-medium">
            {ShoppingCart.discountCodeAmount.toFixed()}<MinusIcon config={{className:"stroke-rose-800"}}/>
            <RialIcon />
          </span>
        </div>}
        <div className="flex justify-between text-sm">
          <span className="text-store-subtle">{t("shoppingCart.shipping")}</span>
          <span className="flex items-center gap-2 font-medium">
            {ShoppingCart?.products?.length > 0
              ? (ShoppingCart?.shippingMethod?.price ?? 0).toFixed()
              : 0}
            <PlusIcon2 />
            <RialIcon />
          </span>
        </div>
        <div className="flex justify-between pt-3 font-semibold text-lg">
          <span>{t("shoppingCart.total")}</span>
          <span className="flex gap-2">
            <span className='relative end-4'> {ShoppingCart?.products?.length>0?kol:0}</span>
            <RialIcon />
          </span>
        </div>
      </div>
  )
}

export default PriceSummary