import { useTranslations } from 'next-intl';

import { OrderStatusText } from '@models/order';
import { useGetData } from '@services/base';
import { toMediaUrl } from '@utils/toMediaUrl';

import { IOrder } from './type';

export default function OrderDetails({
  selectedOrderId,
}: {
  selectedOrderId: number | null;
}) {
  const t = useTranslations();
  const { data, isLoading } = useGetData<IOrder>({
    url: `/orders/${selectedOrderId}`,
    method: "GET",
    skip: !selectedOrderId,
  });

  if (!selectedOrderId)
    return (
      <div className="mt-40 text-store-subtle text-center">
        {t("order.selectOrder")}
      </div>
    );

  const order = data?.data;
  return (
    <>
      {isLoading ? (
        <div className="flex justify-center items-center w-full h-full">
          <div className="mx-auto mb-4 border-primary border-t-2 border-b-2 rounded-full w-16 h-16 animate-spin"></div>
        </div>
      ) : (
        <div className="space-y-6 h-[calc(100dvh-350px)]">
          <h2 className="font-semibold text-xl">{t("order.orderDetails")}</h2>
          <div className="bg-store-muted p-4 border border-store-border rounded-lg">
            <div className="flex justify-between">
              <span>{t("order.status")}</span>
              {order?.status !== undefined && (
                <span>{t(OrderStatusText[order.status])}</span>
              )}
            </div>
          </div>

          {/* Items */}
          <div className="hidden-show-scrollbar space-y-3 h-full overflow-y-auto">
            {order?.items.map((item, index) => (
              <div
                key={index}
                className="flex gap-4 bg-store-muted p-3 border border-store-border rounded-lg"
              >
                <img
                  src={toMediaUrl(item.product.image)}
                  className="rounded w-20 h-20 object-cover"
                />

                <div className="flex flex-col flex-1 justify-between">
                  <div className="text-sm">{item.product.name}</div>
                  <div className="text-store-subtle text-xs">
                    {t("common.quantityCount", { count: item.quantity })}
                  </div>
                  <div className="text-store-subtle text-xs">
                    {item.product.description}
                  </div>
                </div>

                <div className="text-right">
                  <div className="font-semibold text-sm">
                    {item.unitPrice} {t("common.currency")}
                  </div>
                </div>
              </div>
            ))}
          </div>

          {/* Summary */}
          <div className="bg-store-muted p-4 border border-store-border rounded-lg">
            <div className="flex justify-between mb-1 text-sm">
              <span>{t("order.orderTotal")}</span>
              <span>
                {order?.totalPrice} {t("common.currency")}
              </span>
            </div>
            <div className="flex justify-between mb-1 text-store-subtle text-sm">
              <span>{t("order.shippingCost")}</span>
              <span>
                {order?.shippingMethod.cost} {t("common.currency")}
              </span>
            </div>
            <div className="flex justify-between mb-1 text-green-400 text-sm">
              <span>{t("order.discount")}</span>
              <span>
                {order?.discountPrice} {t("common.currency")}
              </span>
            </div>

            <div className="flex justify-between mt-4 font-bold text-lg">
              <span>{t("order.finalAmount")}</span>
              <span>
                {(order?.totalPrice! + order?.shippingMethod?.cost!) -
                  order?.discountPrice!}{" "}
                {t("common.currency")}
              </span>
            </div>
          </div>
        </div>
      )}
    </>
  );
}
