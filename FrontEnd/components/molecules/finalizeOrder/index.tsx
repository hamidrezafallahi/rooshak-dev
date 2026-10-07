"use client";

import React, { useState } from 'react';

import { useTranslations } from 'next-intl';

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
import { showErrorToast } from '@utils/core';

export interface IOrderSummarySnapshot {
  addressName: string;
  shippingMethodTitle: string;
  paymentMethodTitle: string;
  itemsTotal: number;
  shippingCost: number;
  discountCodeAmount: number;
  finalAmount: number;
}

interface OrderConfirmModalProps {
  onClose: () => void;
  orderId: number;
  snapshot: IOrderSummarySnapshot;
}

export default function FinalizeOrder({
  onClose,
  orderId,
  snapshot,
}: OrderConfirmModalProps) {
  const t = useTranslations();
  const [itemMutate, { isLoading }] = useGetConditionallyMutation();
  const [redirecting, setRedirecting] = useState(false);

  const handleFinalizeOrder = async () => {
    try {
      const res: IBaseQueryResponse<{ paymentUrl: string }> = await itemMutate({
        url: '/Payments/request',
        method: 'POST',
        body: { orderId },
      }).unwrap();

      if (res.isSuccess && res.data?.paymentUrl) {
        setRedirecting(true);
        window.location.href = res.data.paymentUrl;
        return;
      }

      showErrorToast(res.error ?? t('payment.payment_error'));
    } catch (err: any) {
      showErrorToast(
        err?.data?.error ?? err?.message ?? t('payment.payment_error')
      );
    }
  };

  if (redirecting || isLoading) {
    return (
      <Card className="bg-store-muted px-4 border-none rounded-lg w-full max-w-md">
        <RedirectToPayment paymentMethodTitle={snapshot.paymentMethodTitle} />
      </Card>
    );
  }

  return (
    <Card className="bg-store-muted px-4 border-none rounded-lg w-full max-w-md">
      <CardHeader>
        <CardTitle>{t('payment.final_confirm_title')}</CardTitle>
        <CardDescription>{t('payment.final_confirm_desc')}</CardDescription>
      </CardHeader>

      <CardContent className="space-y-3 text-sm">
        <div className="flex justify-between">
          <span className="text-store-subtle">{t('payment.shipping_address')}</span>
          <span className="font-medium">{snapshot.addressName}</span>
        </div>

        <div className="flex justify-between">
          <span className="text-store-subtle">{t('payment.shipping_method')}</span>
          <span className="font-medium">{snapshot.shippingMethodTitle}</span>
        </div>

        <div className="flex justify-between">
          <span className="text-store-subtle">{t('payment.payment_method')}</span>
          <span className="font-medium">{snapshot.paymentMethodTitle}</span>
        </div>

        <hr />

        <div className="flex justify-between">
          <span>{t('payment.items_total')}</span>
          <span>
            {snapshot.itemsTotal.toLocaleString()} {t('payment.currency')}
          </span>
        </div>

        <div className="flex justify-between">
          <span>{t('payment.shipping_cost')}</span>
          <span>
            {snapshot.shippingCost.toLocaleString()} {t('payment.currency')}
          </span>
        </div>

        {snapshot.discountCodeAmount > 0 && (
          <div className="flex justify-between text-red-500">
            <span>{t('payment.discount')}</span>
            <span>
              -{snapshot.discountCodeAmount.toLocaleString()}{' '}
              {t('payment.currency')}
            </span>
          </div>
        )}

        <div className="flex justify-between font-semibold text-base">
          <span>{t('payment.final_amount')}</span>
          <span>
            {snapshot.finalAmount.toLocaleString()} {t('payment.currency')}
          </span>
        </div>
      </CardContent>

      <CardFooter className="flex gap-2">
        <button
          onClick={onClose}
          disabled={isLoading}
          className="flex-1 px-4 py-2 border rounded-xl text-sm"
        >
          {t('payment.cancel')}
        </button>

        <button
          onClick={handleFinalizeOrder}
          disabled={isLoading}
          className="flex-1 bg-primary disabled:opacity-60 px-4 py-2 rounded-xl text-primary-foreground text-sm"
        >
          {isLoading
            ? t('payment.connecting_gateway')
            : t('payment.confirm_and_pay')}
        </button>
      </CardFooter>
    </Card>
  );
}
