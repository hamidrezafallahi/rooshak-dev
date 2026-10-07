"use client";
import { useEffect } from 'react';

import {
  useLocale,
  useTranslations,
} from 'next-intl';
import { useRouter } from 'next/navigation';

import { Button } from '@components/atoms/defaultElements/customButton';
import BackToLandingPageButton
  from '@components/molecules/backToLandingPageButton';
import { OrderStatusText } from '@models/order';
import { useGetData } from '@services/base';
import { getCookie } from '@utils/core';

import {
  IOrder,
  OrderListProps,
} from './type';

function OrderList({...props}: OrderListProps) {
  const { setSelectedOrderId,selectedOrderId } = props
  const route = useRouter();
  const isAuthenticated = Boolean(getCookie("candySession"));
  const t = useTranslations();
  const locale = useLocale();
  useEffect(() => {
    if (!isAuthenticated) {
      route.push(`/${locale}`);
    }
  }, [isAuthenticated]);

  const { data } = useGetData<IOrder[]>({
    url: "/Orders/user",
    skip: !isAuthenticated,
  });

  useEffect(() => {
    if (data?.isSuccess) {
      console.log(data.data);
    }
  }, [data]);

  // ✔ اینجا همیشه یک مقدار برمی‌گردانیم
  // وقتی هنوز احراز هویت مشخص نیست → return null
  if (!isAuthenticated) return null;
  const formatToJalali = (iso: string) => {
    const date = new Date(iso);

    return new Intl.DateTimeFormat("fa-IR-u-ca-persian", {
      year: "numeric",
      month: "2-digit",
      day: "2-digit",
      hour: "2-digit",
      minute: "2-digit",
    }).format(date);
  };

  return (
            <div className="space-y-4 lg:col-span-1 bg-zinc-900 rounded-lg h-[calc(100dvh-60px)] overflow-hidden">
      <div className="flex items-center gap-3 mb-4 font-semibold text-xl">
        <BackToLandingPageButton />
        <div>{t("order.myOrders")}</div>
      </div>
            <div className="hidden-show-scrollbar flex flex-col gap-2 h-[calc(100dvh-120px)] overflow-y-auto">
      {data?.data?.map((order: IOrder) => (
        <Button
          key={order.id}
          className={`!block bg-zinc-800 hover:bg-zinc-700 border border-gray-700 h-full 
            ${order.id ==selectedOrderId && "border-primary shadow-primary shadow-lg" }
            `}
          onClick={() => {
            setSelectedOrderId(order.id);
          }}
        >
          <div className="flex justify-between text-sm">
            <span>{t("order.orderNumber", { id: order.id })}</span>
            {order?.status !== undefined && (
              <span className='text-store-text text-xs'>{t(OrderStatusText[order.status])}</span>
            )}
          </div>

          <div className="mt-2 text-store-subtle text-xs">
            {t("order.itemsSummary", {
              count: order.items.length,
              total: order.totalPrice,
              currency: t("common.currency"),
            })}
          </div>

          <div className="mt-1 text-store-subtle text-xs">
            {t("order.date", { date: formatToJalali(order.orderDate) })}
          </div>
          
        </Button>
      ))}
    </div>
        </div>

  );
}

export default OrderList;
