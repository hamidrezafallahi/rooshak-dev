"use client";

import React from 'react';

import {
  useLocale,
  useTranslations,
} from 'next-intl';
import { shallowEqual } from 'react-redux';

import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from '@/components/atoms/defaultElements/card';
import RedirectToPayment from '@components/templates/payment/redirectToPayment';
import { useGetConditionallyMutation } from '@services/base';
import { IBaseQueryResponse } from '@services/base/type';
import { useAppSelector } from '@store/index';

interface OrderConfirmModalProps {
  onClose: () => void;
  orderId: number;
}

export default function FinalizeOrder({
  onClose,
  orderId,
}: OrderConfirmModalProps) {
  const t = useTranslations();
  const locale = useLocale();

  const [itemMutate, { isLoading }] = useGetConditionallyMutation();

  const { ShoppingCart } = useAppSelector(
    (state) => ({
      ShoppingCart: state.withPersist.ShoppingCart,
    }),
    shallowEqual
  );

  const handleFinalizeOrder = async () => {
    alert(t('common.underDevelopment'));
    // const res: IBaseQueryResponse<{ orderId: number }> = await itemMutate({
    //   url: "api/Payments/request",
    //   body: { orderId },
    // });

    // if (res.isSuccess) {
    //   console.log(res);
    // }
  };

  const total =
    (ShoppingCart?.finalTotal ?? 0) +
    (ShoppingCart?.shippingMethod?.price ?? 0);

  return (
    <Card className="bg-zinc-900 px-4 border-none rounded-lg w-full max-w-md">
      {!isLoading ? (
        <>
          <CardHeader>
            <CardTitle>{t("payment.final_confirm_title")}</CardTitle>
            <CardDescription>
              {t("payment.final_confirm_desc")}
            </CardDescription>
          </CardHeader>

          <CardContent className="space-y-3 text-sm">
            <div className="flex justify-between">
              <span className="text-gray-500">
                {t("payment.shipping_address")}
              </span>
              <span className="font-medium">
                {ShoppingCart.address?.name}
              </span>
            </div>

            <div className="flex justify-between">
              <span className="text-gray-500">
                {t("payment.shipping_method")}
              </span>
              <span className="font-medium">
                {ShoppingCart.shippingMethod?.title}
              </span>
            </div>

            <div className="flex justify-between">
              <span className="text-gray-500">
                {t("payment.payment_method")}
              </span>
              <span className="font-medium">
                {ShoppingCart.paymentMethod?.title}
              </span>
            </div>

            <hr />

            <div className="flex justify-between">
              <span>{t("payment.items_total")}</span>
              <span>
                {ShoppingCart.finalTotal.toLocaleString()}{" "}
                {t("payment.currency")}
              </span>
            </div>

            <div className="flex justify-between">
              <span>{t("payment.shipping_cost")}</span>
              <span>
                {ShoppingCart.shippingMethod?.price.toLocaleString()}{" "}
                {t("payment.currency")}
              </span>
            </div>

            {ShoppingCart.discountCodeAmount > 0 && (
              <div className="flex justify-between text-red-500">
                <span>{t("payment.discount")}</span>
                <span>
                  -{ShoppingCart.discountCodeAmount.toLocaleString()}{" "}
                  {t("payment.currency")}
                </span>
              </div>
            )}

            <div className="flex justify-between font-semibold text-base">
              <span>{t("payment.final_amount")}</span>
              <span>
                {ShoppingCart?.products?.length > 0 ? total : 0}{" "}
                {t("payment.currency")}
              </span>
            </div>
          </CardContent>

          <CardFooter className="flex gap-2">
            <button
              onClick={onClose}
              disabled={isLoading}
              className="flex-1 px-4 py-2 border rounded-xl text-sm"
            >
              {t("payment.cancel")}
            </button>

            <button
              onClick={handleFinalizeOrder}
              disabled={isLoading}
              className="flex-1 bg-primary disabled:opacity-60 px-4 py-2 rounded-xl text-white text-sm"
            >
              {isLoading
                ? t("payment.connecting_gateway")
                : t("payment.confirm_and_pay")}
            </button>
          </CardFooter>
        </>
      ) : (
        <RedirectToPayment />
      )}
    </Card>
  );
}
