import React from 'react';

import { useTranslations } from 'next-intl';

import {
  FastIcon,
  ResetIcon,
  ShieldCheckIcon,
} from '@components/atoms/iconComponents';

import { IProps } from '../type';
import PaymentMethod from './PaymentMethod';
import PriceSummary from './priceSummary';
import PromoCode from './PromoCode';
import ShippingMethod from './shippingMethod';
import SubmitButton from './submitButton';

function SummarySideBar({ ...props }: IProps) {
  const t = useTranslations();

  return (
    <div className="hidden-show-scrollbar flex flex-col justify-between gap-2 lg:col-span-1 bg-zinc-900 p-3 rounded-lg lg:h-[calc(100dvh-20px)] lg:overflow-y-auto">
      <h2 className="mb-2 font-semibold text-xl">
        {t("shoppingCart.shoppingCartSummary")}
      </h2>
      <p className="mb-6 text-store-subtle text-sm">
        {t("shoppingCart.reviewYourCartDetailsAndShippingInformation")}
      </p>

      {/* Shipping Method */}
      <ShippingMethod />
      {/* Payment Method */}
      <PaymentMethod />
      {/* Promo Code */}
      <PromoCode />
      {/* Price Summary */}
      <PriceSummary />

      {/* Features */}
      <div className="flex justify-between space-y-3">
        <div className="flex items-center gap-1 text-xs">
          <ResetIcon />
          <span>{t("shoppingCart.freeReturns")}</span>
        </div>
        <div className="flex items-center gap-1 text-xs">
          <ShieldCheckIcon />
          <span>{t("shoppingCart.securePayment")}</span>
        </div>
        <div className="flex items-center gap-1 text-xs">
          <FastIcon />
          <span>{t("shoppingCart.fastDelivery")}</span>
        </div>
      </div>

      {/* Checkout Button */}
      <SubmitButton {...props} />
    </div>
  );
}

export default SummarySideBar;
