// app/[locale]/payment/success/page.tsx
"use client";

import { useEffect } from 'react';

import {
  useLocale,
  useTranslations,
} from 'next-intl';
import Link from 'next/link';
import {
  useRouter,
  useSearchParams,
} from 'next/navigation';

import {
  Card,
  CardContent,
} from '@components/atoms/defaultElements/card';
import { Button } from '@components/atoms/defaultElements/customButton';
import {
  CheckCircle,
  Download,
  Home,
  Package,
  Printer,
  RialIcon,
} from '@components/atoms/iconComponents';

interface IPaymentSuccessProps {
  params?: {};
  searchParams: {
    orderId?: string;
    transactionId?: string;
    amount?: string;
  };
}

export default function PaymentSuccess({ params }: IPaymentSuccessProps) {
  const locale = useLocale();
  const t = useTranslations('paymentSuccess');
  const searchParams = useSearchParams();
  const router = useRouter();

  const orderId = searchParams.get('orderId') || 'N/A';
  const transactionId = searchParams.get('transactionId') || 'N/A';
  const amount = searchParams.get('amount') || '0';
  const date = new Date().toLocaleDateString(
    locale === 'fa' ? 'fa-IR' : 'en-US',
  );

  // در صورت نیاز، می‌توانید داده‌ها را به سرور هم ارسال کنید
  useEffect(() => {
    // ثبت لاگ یا آمار موفقیت پرداخت
    console.log('Payment successful:', { orderId, transactionId, amount });
  }, []);

  const handlePrint = () => {
    window.print();
  };

  const handleDownloadInvoice = () => {
    // منطق دانلود فاکتور
    console.log('Downloading invoice...');
  };

  return (
    <div className="bg-black p-4 min-h-screen text-white">
      <div className="mx-auto max-w-4xl">
        {/* هدر صفحه */}
        <div className="mb-8 pt-8 text-center">
          <div className="flex justify-center mb-4">
            <div className="bg-green-900/30 p-4 rounded-full">
              <CheckCircle config={{className:"w-16 h-16 text-green-500"}}  />
            </div>
          </div>
          <h1 className="mb-2 font-bold text-3xl">
            {t('title')}
          </h1>
          <p className="text-gray-400">
            {t('subtitle')}
          </p>
        </div>

        <div className="gap-6 grid grid-cols-1 lg:grid-cols-3">
          {/* بخش اصلی اطلاعات */}
          <div className="space-y-6 lg:col-span-2">
            {/* کارت تبریک */}
            <Card className="bg-zinc-900 border-zinc-800">
              <CardContent className="p-6">
                <div className="flex items-start gap-4">
                  <div className="bg-green-900/20 p-3 rounded-full">
                    <CheckCircle config={{className:"w-6 h-6 text-green-500"}} />
                  </div>
                  <div>
                    <h3 className="mb-2 font-semibold text-xl">
                      {t('thankYou')}
                    </h3>
                    <p className="text-gray-400">
                      {t('thankYouDesc', { orderId })}
                    </p>
                  </div>
                </div>
              </CardContent>
            </Card>

            {/* جزئیات سفارش */}
            <Card className="bg-zinc-900 border-zinc-800">
              <CardContent className="p-6">
                <h3 className="mb-4 font-semibold text-xl">
                  {t('orderDetails')}
                </h3>
                <div className="space-y-4">
                  <div className="gap-4 grid grid-cols-2">
                    <div>
                      <p className="text-gray-400 text-sm">
                        {t('orderNumber')}
                      </p>
                      <p className="font-mono font-semibold">{orderId}</p>
                    </div>
                    <div>
                      <p className="text-gray-400 text-sm">
                        {t('orderDate')}
                      </p>
                      <p>{date}</p>
                    </div>
                    <div>
                      <p className="text-gray-400 text-sm">
                        {t('transactionId')}
                      </p>
                      <p className="font-mono text-sm">{transactionId}</p>
                    </div>
                    <div>
                      <p className="text-gray-400 text-sm">
                        {t('amountPaid')}
                      </p>
                      <div className="flex items-center gap-1 font-semibold">
                        {amount}
                        <RialIcon config={{className:"w-4 h-4"}} />
                      </div>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>

            {/* مراحل بعدی */}
            <Card className="bg-zinc-900 border-zinc-800">
              <CardContent className="p-6">
                <h3 className="mb-4 font-semibold text-xl">
                  {t('nextSteps')}
                </h3>
                <div className="space-y-4">
                  <div className="flex items-center gap-3">
                    <div className="bg-blue-900/30 p-2 rounded-full">
                      <Package config={{className:"w-5 h-5 text-blue-400"}}  />
                    </div>
                    <div>
                      <p className="font-medium">
                        {t('processing')}
                      </p>
                      <p className="text-gray-400 text-sm">
                        {t('processingDesc')}
                      </p>
                    </div>
                  </div>
                  <div className="flex items-center gap-3">
                    <div className="bg-purple-900/30 p-2 rounded-full">
                      <Package config={{className:"w-5 h-5 text-purple-400"}} />
                    </div>
                    <div>
                      <p className="font-medium">
                        {t('trackOrder')}
                      </p>
                      <p className="text-gray-400 text-sm">
                        {t('trackOrderDesc')}
                      </p>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>

          {/* سایدبار اقدامات */}
          <div className="space-y-4">
            <Card className="bg-zinc-900 border-zinc-800">
              <CardContent className="p-6">
                <h3 className="mb-4 font-semibold text-xl">
                  {t('actions')}
                </h3>
                <div className="space-y-3">
                  <Button
                    onClick={handlePrint}
                    className="bg-zinc-800 hover:bg-zinc-700 w-full text-white"
                    variant="outline"
                  >
                    <Printer config={{className:"ml-2 w-4 h-4"}}   />
                    {t('printReceipt')}
                  </Button>
                  <Button
                    onClick={handleDownloadInvoice}
                    className="bg-zinc-800 hover:bg-zinc-700 w-full text-white"
                    variant="outline"
                  >
                    <Download config={{className:"ml-2 w-4 h-4"}} />
                    {t('downloadInvoice')}
                  </Button>
                  <Link href={`/${locale}/orders`}>
                    <Button className="bg-primary hover:bg-primary/90 w-full text-white">
                      <Package config={{className:"ml-2 w-4 h-4"}} />
                      {t('viewOrders')}
                    </Button>
                  </Link>
                  <Link href={`/${locale}`}>
                    <Button className="bg-zinc-800 hover:bg-zinc-700 w-full text-white">
                      <Home config={{className:"ml-2 w-4 h-4"}} />
                      {t('backHome')}
                    </Button>
                  </Link>
                </div>
              </CardContent>
            </Card>

            {/* پشتیبانی */}
            <Card className="bg-zinc-900 border-zinc-800">
              <CardContent className="p-6">
                <h4 className="mb-2 font-semibold">
                  {t('needHelp')}
                </h4>
                <p className="mb-4 text-gray-400 text-sm">
                  {t('needHelpDesc')}
                </p>
                <Link href={`/${locale}/contact`}>
                  <Button variant="link" className="p-0 text-primary">
                    {t('contactSupport')}
                  </Button>
                </Link>
              </CardContent>
            </Card>
          </div>
        </div>
      </div>
    </div>
  );
}
