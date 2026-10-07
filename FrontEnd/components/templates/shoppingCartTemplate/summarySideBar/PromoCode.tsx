import React, { useState } from 'react';

import { useTranslations } from 'next-intl';
import {
  shallowEqual,
  useDispatch,
} from 'react-redux';

import { useGetConditionallyMutation } from '@services/base';
import { setPromotionCode } from '@slice/shoppingCartSlice';
import { useAppSelector } from '@store/index';
import {
  showErrorToast,
  showSuccessToast,
} from '@utils/core';

function PromoCode() {
  const t = useTranslations();
  const [promoCode, setPromoCode] = useState<string>("");
  const {promotionCode}=useAppSelector((state)=>({promotionCode:state.withPersist.ShoppingCart.promoCode}),shallowEqual)
  const [getPromoCode] = useGetConditionallyMutation();
  const dispatch = useDispatch();
  const handleSubmitPromoCode = async () => {
    if (promoCode && promoCode.trim().length > 0) {
      const res = await getPromoCode({
        url: `/DiscountCodes/getCode/${promoCode?.trim()}`,
        method: "GET",
        id:"none"
      }).unwrap();
      if (res.isSuccess == false) {
          showErrorToast(res.error);
        } else if (res.isSuccess) {
          dispatch(setPromotionCode({ promo: res.data}));
        showSuccessToast(t("shoppingCart.promoCodeSubmit"));
      }
    }
  };
  const handleRemovePromoCode =  () => {
    setPromoCode("")
 dispatch(setPromotionCode({ promo: null }));
  };
  return (
    <div className="mb-6">
      <label className="block mb-2 font-medium text-sm">
        {t("shoppingCart.promoCode")}
      </label>
      <div className="flex gap-2">
        <input
          type="text"
          placeholder={t("shoppingCart.promoCodePlaceHolder")}
          value={promoCode}
          onChange={(e) => setPromoCode(e.target.value)}
          className="flex-1 bg-store-surface px-3 py-2 border border-store-border focus:border-store-strong rounded-lg focus:outline-none placeholder:text-store-subtle text-sm"
        />
        <button
          onClick={handleSubmitPromoCode}
          className="bg-primary hover:bg-primary/90 px-4 py-2 border border-primary rounded-lg font-medium text-primary-foreground text-sm"
        >
          {t("general.apply")}
        </button>
        {promotionCode?.length>0 && <button
          onClick={handleRemovePromoCode}
          className="bg-store-surface hover:bg-store-muted px-4 py-2 border border-store-border rounded-lg font-medium text-xs"
        >
          {t("general.delete")}
        </button>}
       
      </div>
    </div>
  );
}

export default PromoCode;
