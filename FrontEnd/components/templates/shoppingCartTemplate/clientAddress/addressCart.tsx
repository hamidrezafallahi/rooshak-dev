import React, {
  Dispatch,
  SetStateAction,
} from 'react';

import { useTranslations } from 'next-intl';
import {
  shallowEqual,
  useDispatch,
} from 'react-redux';

import { Button } from '@components/atoms/defaultElements/customButton';
import { Checkbox } from '@components/atoms/defaultElements/customCheckbox';
import {
  EditIcon,
  SpinnerIcon,
  Trash2Icon,
} from '@components/atoms/iconComponents';
import { useGetConditionallyMutation } from '@services/base';
import { setAddress } from '@slice/shoppingCartSlice';
import { useAppSelector } from '@store/index';

import { IAddress } from '../type';

interface IProps {
  address: IAddress;
  refetch: () => void;
  setShowAddForm: Dispatch<SetStateAction<boolean>>;
  setNewAddress: Dispatch<SetStateAction<IAddress>>;
}
function AddressCart({...props}: IProps) {
  const {address, refetch, setNewAddress, setShowAddForm} = props;
  const t = useTranslations();
  const [defaultMutate, {isLoading: defaultLoading}] =
    useGetConditionallyMutation();
  const [deleteMutate, {isLoading: deleteLoading}] =
    useGetConditionallyMutation();
  const dispatch = useDispatch();
  const selectedAddress = useAppSelector(
    (state) => state.withPersist.ShoppingCart.address,
    shallowEqual,
  );
  const handleEditAddress = (addr: IAddress) => {
    setShowAddForm(true);
    setNewAddress({...addr, isEdit: true});
  };
  const handleSetDefault = async (addressId: number) => {
    try {
      const res = await defaultMutate({
        url: `/Address/set-default`,
        method: "PUT",
        body: {
          id: addressId,
        },
      }).unwrap();

      if (res.isSuccess) {
        refetch();
      }
    } catch {}
  };
  const handleDeleteAddress = async (addressId: number) => {
    try {
      const res = await deleteMutate({
        url: `/address/${addressId}`,
        method: "DELETE",
      }).unwrap();

      if (res.isSuccess) {
        refetch();
      }
    } catch {}
  };

  return (
    <div
      className={`p-4 rounded-lg bg-zinc-900 border cursor-pointer space-y-3 ${
        selectedAddress?.id === address.id
          ? "border-primary shadow-primary shadow-lg"
          : "border-store-border"
      }`}
      onClick={() => dispatch(setAddress({address}))}
    >
      {/* --- اطلاعات آدرس --- */}
      <div className="flex justify-between items-center">
        <div className="space-y-1 text-sm">
          <div>
            {address.name} {address.phoneNumber}
          </div>

          <div className="text-store-subtle text-xs">
            {address.postalCode} {address.fullAddress}
          </div>

          <div className="text-store-subtle text-xs">{address.state}</div>
        </div>

        {/* لیبل نمایش Default */}
        {address.isDefault && (
          <span className="text-store-text text-xs">{t("general.default")}</span>
        )}
      </div>

      {/* --- چک‌باکس پیش‌فرض (Footer کارت) --- */}
      <div className="flex justify-between items-center gap-2 pt-2 border-store-border border-t">
        <div className="flex justify-between items-center gap-2">
          <span className="text-store-subtle text-xs">
            {t("address.chooseAsDefault")}
          </span>
          {defaultLoading ? (
            <SpinnerIcon />
          ) : (
            <Checkbox
              checked={address.isDefault}
              onClick={(e) => e.stopPropagation()}
              onChange={(e) => handleSetDefault(address.id!)}
              className="bg-store-muted p-2 rounded"
            />
          )}
        </div>
        <Button
          onClick={(e) => {
            e.stopPropagation();
            handleDeleteAddress(address.id!);
          }}
          className="flex justify-between items-center gap-2 !bg-transparent"
        >
          <span className="text-store-subtle text-xs">{t("general.delete")}</span>
          {deleteLoading ? (
            <SpinnerIcon />
          ) : (
            <Trash2Icon config={{stroke: "#e11d48"}} />
          )}
        </Button>
        <Button
          onClick={(e) => {
            e.stopPropagation();
            handleEditAddress(address);
          }}
          className="flex justify-between items-center gap-2 !bg-transparent"
        >
          <span className="text-store-subtle text-xs">{t("general.edit")}</span>
          <EditIcon />
        </Button>
      </div>
    </div>
  );
}

export default AddressCart;
