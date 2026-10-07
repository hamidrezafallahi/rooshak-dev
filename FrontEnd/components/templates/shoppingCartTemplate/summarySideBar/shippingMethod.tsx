import React, {
  useEffect,
  useState,
} from 'react';

import { useTranslations } from 'next-intl';
import { useDispatch } from 'react-redux';

import { Button } from '@components/atoms/defaultElements/customButton';
import { RialIcon } from '@components/atoms/iconComponents';
import { IShippingMethod } from '@models/shippingMethod';
import { useGetData } from '@services/base';
import { setShippingMethod } from '@slice/shoppingCartSlice';
import { useAppSelector } from '@store/index';

function ShippingMethod() {
  const t = useTranslations();
  const dispatch = useDispatch();
  const { shippingMethod } = useAppSelector((state) => ({
    shippingMethod: state.withPersist.ShoppingCart.shippingMethod,
  }));
  const [methods, setMethods] = useState<IShippingMethod[]>([]);
  const { data } = useGetData<any>({
    url: "/ShippingMethods",
  });
  const handleChangeShippingMethod = (el:IShippingMethod) => {
    if(shippingMethod?.id !== el.id){
      dispatch(setShippingMethod(el));
    }
  };
useEffect(() => {
  if (data?.isSuccess) {
    const records = data?.data?.records ?? [];
    setMethods(records);
    if (records.length === 0) return;
    const method =
      records.find((m: IShippingMethod) => m.isDefault) ?? records[0];
    dispatch(setShippingMethod(method));
  }
}, [data, dispatch]);


  return (
    <div className="mb-6">
      <label className="block mb-2 font-medium text-sm">
        {t("shoppingCart.shippingMethod")}
      </label>
      <div className="hidden-show-scrollbar flex flex-col gap-2 bg-store-muted p-3 border border-store-border rounded-lg max-h-40 overflow-y-auto">
        {methods.map((el, i) => (
          <Button
            variant={"ghost"}
            onClick={() => handleChangeShippingMethod(el)}
            key={i}
            className={`flex justify-between items-center p-2  ${
              shippingMethod?.id == el.id &&
              "shadow-md border border-primary  shadow-primary"
            }`}
          >
            <div className="flex justify-between w-full">
              <div className="font-medium text-sm">{el.title}</div>
              { el.isDefault && (
                <div>
                  <div className="text-store-subtle text-xs">
                    {t("general.default")}
                  </div>
                </div>
              )}
              <div className="flex gap-2 text-store-subtle text-xs">
                 <span>{el.estimatedDeliveryTime}</span>
                 <span>{t("shoppingCart.days")}</span> 
              </div>
            </div>
            <div className="flex gap-2 font-semibold text-sm">
              {el.price}
              <RialIcon />
            </div>
          </Button>
        ))}
      </div>
    </div>
  );
}

export default ShippingMethod;
