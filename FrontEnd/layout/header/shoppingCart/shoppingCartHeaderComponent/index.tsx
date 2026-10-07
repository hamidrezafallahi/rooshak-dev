import React, {
  Dispatch,
  SetStateAction,
  useEffect,
} from 'react';

import {
  useLocale,
  useTranslations,
} from 'next-intl';
import Link from 'next/link';
import {
  shallowEqual,
  useDispatch,
} from 'react-redux';

import { Button } from '@components/atoms/defaultElements/customButton';
import {
  CreditCardIcon,
  RialIcon,
} from '@components/atoms/iconComponents';
import { IShippingMethod } from '@models/shippingMethod';
import { useGetData } from '@services/base';
import { setShippingMethod } from '@slice/shoppingCartSlice';
import { useAppSelector } from '@store/index';

import ItemCart from './itemCart';

interface IProps {
  setIsOpen: Dispatch<SetStateAction<boolean>>;
 
}
function ShoppingCartHeaderComponent({ ...props }: IProps) {
  const { setIsOpen } = props;
  const locale = useLocale()
  const t = useTranslations();
  const dispatch = useDispatch();

  const { ShoppingCart } = useAppSelector(
    (state) => ({
      ShoppingCart: state.withPersist.ShoppingCart,
    }),
    shallowEqual
  );
  const { data, isSuccess } = useGetData<any, IShippingMethod[]>({
    url: "/ShippingMethods",
    skip: ShoppingCart.products.length == 0,
  });
  useEffect(() => {
    if (data?.isSuccess) {
      const records = data.data?.records ?? [];
      if (records.length === 0) return;
      const method =
        records.find((m: IShippingMethod) => m.isDefault) ?? records[0];
      dispatch(setShippingMethod(method));
    }
  }, [data, dispatch]);
  const kol =
    (ShoppingCart?.finalTotal ?? 0) +
    (ShoppingCart?.shippingMethod?.price ?? 0);

  return (
    <div
      className="top-full z-50 absolute mt-0 end-0"
      onMouseLeave={() => setIsOpen(false)}
    >
      <div className="bg-store-surface shadow-2xl p-4 border border-store-border w-[300px] text-store-text">
        <div className="flex justify-between items-center mb-1">
          <h2 className="font-normal text-xl">
            {t("shoppingCart.header.yourCart")}
          </h2>
          <span className="bg-store-muted px-2 py-0.5 rounded-full text-xs">
            {ShoppingCart?.products.length} {t("shoppingCart.header.items")}
          </span>
        </div>

        <p className="mb-3 text-store-subtle text-xs">
          {t("shoppingCart.header.reviewBeforeCheckout")}
        </p>

        <div
          className="hidden-show-scrollbar space-y-3 mb-4 max-h-[calc(100dvh-400px)] overflow-y-auto"
          onScroll={(e) => {
            e.stopPropagation();
          }}
        >
          {ShoppingCart?.products.map((item, index) => (
            <ItemCart key={index} item={item} />
          ))}
        </div>

        <div className="space-y-2 mb-4 pt-3 border-store-border border-t">
          <div className="flex justify-between text-xs">
            <span className="text-store-subtle">
              {t("shoppingCart.header.totalDiscount")}
            </span>
            <span className="flex gap-2 font-medium">
              {ShoppingCart?.totalDiscount.toFixed()}{" "}
              <RialIcon config={{ size: 20 }} />
            </span>
          </div>
          <div className="flex justify-between text-xs">
            <span className="text-store-subtle">
              {t("shoppingCart.header.shipping")}
            </span>
            {ShoppingCart.products.length !== 0 && (
              <div className="flex gap-2">
                <span className="font-medium">
                  {" "}
                  {ShoppingCart?.shippingMethod?.price || 0}
                </span>
                <RialIcon config={{ size: 20 }} />
              </div>
            )}
          </div>
          <div className="flex justify-between pt-2 border-store-border border-t font-semibold text-base">
            <span>{t("shoppingCart.header.total")}</span>
            <span className="flex gap-2">
              {ShoppingCart?.products?.length > 0 ? kol : 0}
              <RialIcon />
            </span>
          </div>
        </div>

        <div className="flex justify-center items-center gap-1 mb-3 text-[10px] text-store-subtle text-center">
          <span className="flex justify-center items-center border border-gray-400 rounded-full w-3 h-3 text-[8px]">
            ✓
          </span>
          {t("shoppingCart.header.freeShippingOver")}
        </div>

        <div className="space-y-1.5">
          <Button className="bg-primary hover:bg-primary/90 rounded-none w-full h-10 font-medium text-primary-foreground text-sm">
            <CreditCardIcon />
            {t("shoppingCart.header.checkout")}
          </Button>
          <Link
            // variant="ghost"
            className="flex justify-center items-center hover:bg-store-muted w-full h-8 text-store-text text-xs"
            href={`/${locale}/shoppingCart`}
          >
            {t("shoppingCart.header.viewCart")} →
          </Link>
        </div>
      </div>
    </div>
  );
}

export default ShoppingCartHeaderComponent;
