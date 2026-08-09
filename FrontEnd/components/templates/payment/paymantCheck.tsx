'use client';

import { useEffect, useRef } from 'react';

import { useLocale, useTranslations } from 'next-intl';
import { useRouter, useSearchParams } from 'next/navigation';
import { useDispatch } from 'react-redux';

import { useGetConditionallyMutation } from '@services/base';
import { IBaseQueryResponse } from '@services/base/type';
import { resetShoppingCart } from '@slice/shoppingCartSlice';

interface PaymentVerifyData {
  isSuccess: boolean;
  orderId: number;
  transactionId: string;
  amount: number;
  errorMessage?: string;
}

export default function PaymentCheck() {
  const t = useTranslations();
  const locale = useLocale();
  const router = useRouter();
  const searchParams = useSearchParams();
  const dispatch = useDispatch();
  const [verifyPayment] = useGetConditionallyMutation();
  const started = useRef(false);

  useEffect(() => {
    if (started.current) return;
    started.current = true;

    const verify = async () => {
      const authority =
        searchParams.get('Authority') ?? searchParams.get('authority');
      const gatewayStatus =
        searchParams.get('Status') ?? searchParams.get('status') ?? '';
      const orderIdParam =
        searchParams.get('orderId') ?? searchParams.get('OrderId');

      if (!authority) {
        router.replace(
          `/${locale}/payment?status=failed&errorCode=missing_authority&errorMessage=${encodeURIComponent(
            t('payment.payment_error')
          )}`
        );
        return;
      }

      try {
        const res: IBaseQueryResponse<PaymentVerifyData> = await verifyPayment({
          url: '/Payments/verify',
          method: 'POST',
          body: {
            authority,
            status: gatewayStatus,
            orderId: orderIdParam ? Number(orderIdParam) : undefined,
          },
        }).unwrap();

        if (res.isSuccess && res.data?.isSuccess) {
          dispatch(resetShoppingCart());
          router.replace(
            `/${locale}/payment?status=success&orderId=${res.data.orderId}&transactionId=${encodeURIComponent(
              res.data.transactionId ?? ''
            )}&amount=${res.data.amount ?? 0}`
          );
          return;
        }

        const orderId = res.data?.orderId ?? '';
        const errorMessage =
          res.data?.errorMessage ?? res.error ?? t('payment.paymentFailed');

        router.replace(
          `/${locale}/payment?status=failed&errorCode=verify_failed&orderId=${orderId}&errorMessage=${encodeURIComponent(
            errorMessage
          )}`
        );
      } catch (err: any) {
        router.replace(
          `/${locale}/payment?status=failed&errorCode=verification_error&errorMessage=${encodeURIComponent(
            err?.data?.error ?? err?.message ?? t('payment.payment_error')
          )}`
        );
      }
    };

    void verify();
  }, [dispatch, locale, router, searchParams, t, verifyPayment]);

  return (
    <div className="flex justify-center items-center bg-black min-h-screen text-white">
      <div className="text-center">
        <div className="mx-auto mb-4 border-primary border-t-2 border-b-2 rounded-full w-16 h-16 animate-spin"></div>
        <p className="text-lg">{t('payment.verifying')}</p>
        <p className="mt-2 text-gray-400 text-sm">{t('payment.waiting')}</p>
      </div>
    </div>
  );
}
