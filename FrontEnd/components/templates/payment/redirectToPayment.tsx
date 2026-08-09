'use client';

import { useTranslations } from 'next-intl';

interface RedirectToPaymentProps {
  paymentMethodTitle?: string;
}

export default function RedirectToPayment({
  paymentMethodTitle,
}: RedirectToPaymentProps) {
  const t = useTranslations();

  return (
    <div className="flex justify-center items-center bg-black py-8 text-white">
      <div className="text-center">
        <div className="mx-auto mb-4 border-primary border-t-2 border-b-2 rounded-full w-16 h-16 animate-spin"></div>
        <p className="text-lg">{t('payment.redirect')}</p>
        <p className="mt-2 text-gray-400 text-sm">{t('payment.waiting')}</p>
        {paymentMethodTitle ? (
          <p className="mt-2 text-gray-400 text-sm">{paymentMethodTitle}</p>
        ) : null}
      </div>
    </div>
  );
}
