import React, {
  Suspense,
  useState,
} from 'react';

import { useTranslations } from 'next-intl';
import { shallowEqual } from 'react-redux';

import type { IOrderSummarySnapshot } from '@components/molecules/finalizeOrder';
import { Button } from '@components/atoms/defaultElements/customButton';
import { Modal } from '@components/atoms/defaultElements/customModal';
import {
  ListIcon,
  SpinnerIcon,
} from '@components/atoms/iconComponents';
import { useGetConditionallyMutation } from '@services/base';
import { IBaseQueryResponse } from '@services/base/type';
import { useAppSelector } from '@store/index';
import { showErrorToast } from '@utils/core';

import { IProps } from '../type';

const FinalizeOrder = React.lazy(
  () => import('@components/molecules/finalizeOrder')
);

function SubmitButton(_: IProps) {
  const [itemMutate, { isLoading }] = useGetConditionallyMutation();
  const [modalData, setModalData] = useState<{
    open: boolean;
    id: number | undefined;
    snapshot: IOrderSummarySnapshot | undefined;
  }>({ open: false, id: undefined, snapshot: undefined });
  const t = useTranslations();

  const { ShoppingCart } = useAppSelector(
    (state) => ({
      ShoppingCart: state.withPersist.ShoppingCart,
    }),
    shallowEqual
  );

  const canPlaceOrder =
    ShoppingCart?.address?.id != null &&
    (ShoppingCart?.products?.length ?? 0) > 0 &&
    ShoppingCart?.shippingMethod?.id != null &&
    ShoppingCart?.paymentMethod?.id != null;

  const handleSetOrder = async () => {
    if (!canPlaceOrder) return;

    try {
      const order: IBaseQueryResponse<{ orderId: number }> = await itemMutate({
        url: '/Orders/CheckoutCart',
        body: {
          shippingAddressId: ShoppingCart.address!.id,
          shippingMethodId: ShoppingCart.shippingMethod!.id,
          paymentMethodId: ShoppingCart.paymentMethod!.id,
          shippingCost: ShoppingCart.shippingMethod!.price,
          discountAmount: ShoppingCart.discountCodeAmount,
          discountCode: ShoppingCart.promoCode || null,
        },
        method: 'POST',
      }).unwrap();

      if (order.isSuccess) {
        const itemsTotal = ShoppingCart.finalTotal ?? 0;
        const shippingCost = ShoppingCart.shippingMethod?.price ?? 0;
        const snapshot: IOrderSummarySnapshot = {
          addressName: ShoppingCart.address?.name ?? '',
          shippingMethodTitle: ShoppingCart.shippingMethod?.title ?? '',
          paymentMethodTitle: ShoppingCart.paymentMethod?.title ?? '',
          itemsTotal,
          shippingCost,
          discountCodeAmount: ShoppingCart.discountCodeAmount ?? 0,
          finalAmount: itemsTotal + shippingCost,
        };

        setModalData({
          open: true,
          id: order.data.orderId,
          snapshot,
        });
      } else {
        showErrorToast(order.error ?? t('payment.payment_error'));
      }
    } catch (err: any) {
      showErrorToast(
        err?.data?.error ?? err?.message ?? t('payment.payment_error')
      );
    }
  };

  const closeModal = () => {
    setModalData({ open: false, id: undefined, snapshot: undefined });
  };

  return (
    <>
      <Button
        className="flex justify-center items-center gap-2 bg-white hover:bg-gray-100 py-3 w-full font-medium text-black"
        onClick={handleSetOrder}
        disabled={!canPlaceOrder || isLoading}
      >
        {isLoading ? (
          <SpinnerIcon />
        ) : (
          <>
            <ListIcon />
            {ShoppingCart?.address?.id == null
              ? t('shoppingCart.chooseAddress')
              : ShoppingCart?.products?.length == 0
              ? t('shoppingCart.emptyCart')
              : ShoppingCart?.shippingMethod?.id == null
              ? t('shoppingCart.shippingMethod')
              : ShoppingCart?.paymentMethod?.id == null
              ? t('general.paymentMethod')
              : t('shoppingCart.placeOrder')}
          </>
        )}
      </Button>
      {modalData.open && modalData.snapshot && modalData.id != null && (
        <Suspense
          fallback={
            <div className="top-2 absolute start-2">
              <SpinnerIcon />
            </div>
          }
        >
          <Modal open={modalData.open} onClose={closeModal}>
            <FinalizeOrder
              orderId={modalData.id}
              snapshot={modalData.snapshot}
              onClose={closeModal}
            />
          </Modal>
        </Suspense>
      )}
    </>
  );
}

export default SubmitButton;
