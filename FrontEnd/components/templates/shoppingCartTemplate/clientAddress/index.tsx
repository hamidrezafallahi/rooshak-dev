"use client";

import {
  useEffect,
  useRef,
  useState,
} from 'react';

import { useTranslations } from 'next-intl';
import { useDispatch } from 'react-redux';

import { SpinnerIcon } from '@components/atoms/iconComponents';
import {
  useGetConditionallyMutation,
  useGetData,
} from '@services/base';
import { setAddress } from '@slice/shoppingCartSlice';

import { IAddress } from '../type';
import AddressCart from './addressCart';

export default function ClientAddress() {


  const t = useTranslations();
  const { data, refetch } = useGetData<IAddress[]>({
    url: '/Address/ByUser',
    method: "GET",
  });
  const [itemMutate, { isLoading: addLoading }] = useGetConditionallyMutation();
  const [editMutate, { isLoading: editLoading }] = useGetConditionallyMutation();

  const [showAddForm, setShowAddForm] = useState(false);
  const dispatch = useDispatch();
  const firstLoad = useRef(true);
  const [newAddress, setNewAddress] = useState<IAddress>({
    id: undefined,
    name: "",
    phoneNumber: "",
    city: "",
    state: "",
    postalCode: "",
    fullAddress: "",
    isDefault: false,
    isEdit: false,
  });

  const handleAddAddress = async () => {
    try {
      const res = await itemMutate({
        url: `/address`,
        method: "POST",
        body: newAddress,
      }).unwrap();
      if (res.isSuccess) {
        setShowAddForm(false);
        refetch();
      }
    } catch {}
  };
  const handleShowAddForm = () => {
    setShowAddForm(true);
    setNewAddress({ ...newAddress, isEdit: false, id: undefined });
  };

  const handleFetchEditAddress = async () => {
     try {
      const res = await editMutate({
        url: `/address`,
        method: "PUT",
        body: newAddress,
      }).unwrap();

      if (res.isSuccess) {
        setShowAddForm(false);
        refetch();
      }
    } catch {}
  };


  useEffect(() => {
    if (!data?.isSuccess) return;
    if (firstLoad.current) {
      const defaultAddress = data.data.find((addr) => addr.isDefault);
      if (defaultAddress) {
        dispatch(setAddress({ address: defaultAddress }));
      }
      firstLoad.current = false;
    }
  }, [data]);

  return (
    <div className="bg-store-surface py-4 rounded-lg text-store-text">
      {/* ADDRESSES */}
      <section>
        <h2 className="flex justify-between mb-4">
          <span className="font-semibold text-xl">
            {t("address.addresses")}
          </span>
          {!showAddForm && (
            <button
              onClick={handleShowAddForm}
              className="bg-store-surface hover:bg-store-muted px-4 py-2 border border-store-border rounded-lg text-xs"
            >
              {data?.data?.length === 0
                ? t("address.addFirstAddress")
                : t("address.addNewAddress")}
            </button>
          )}
        </h2>
        {/* Address List */}
        {data && data.data?.length > 0 && (
          <div className="space-y-3">
            {data?.data.map((addr) => (
              <AddressCart 
              setNewAddress={setNewAddress}
              setShowAddForm={setShowAddForm}
              key={addr.id} 
              address={addr} 
              refetch={refetch} />

            ))}
          </div>
        )}
        {/* ADD ADDRESS FORM */}
        {showAddForm && (
          <div className="space-y-3 bg-store-muted mt-4 p-4 border border-store-border rounded-lg">
            <input
              className="bg-store-surface border border-store-border p-2 rounded w-full"
              placeholder={t("address.name")}
              value={newAddress.name}
              onChange={(e) =>
                setNewAddress({ ...newAddress, name: e.target.value })
              }
            />
            <input
              className="bg-store-surface border border-store-border p-2 rounded w-full"
              placeholder={t("address.phoneNumber")}
              value={newAddress.phoneNumber}
              onChange={(e) =>
                setNewAddress({ ...newAddress, phoneNumber: e.target.value })
              }
            />
            <input
              className="bg-store-surface border border-store-border p-2 rounded w-full"
              placeholder={t("address.state")}
              value={newAddress.state}
              onChange={(e) =>
                setNewAddress({ ...newAddress, state: e.target.value })
              }
            />
            <input
              className="bg-store-surface border border-store-border p-2 rounded w-full"
              placeholder={t("address.city")}
              value={newAddress.city}
              onChange={(e) =>
                setNewAddress({ ...newAddress, city: e.target.value })
              }
            />
            <textarea
              className="bg-store-surface border border-store-border p-2 rounded w-full"
              placeholder={t("address.fullAddress")}
              value={newAddress.fullAddress}
              onChange={(e) =>
                setNewAddress({ ...newAddress, fullAddress: e.target.value })
              }
            />
            <input
              className="bg-store-surface border border-store-border p-2 rounded w-full"
              placeholder={t("address.postalCode")}
              value={newAddress.postalCode}
              onChange={(e) =>
                setNewAddress({ ...newAddress, postalCode: e.target.value })
              }
            />

            <button
              onClick={() => {
                if (newAddress.isEdit) {
                  handleFetchEditAddress();
                } else {
                  handleAddAddress();
                }
              }}
              className="bg-blue-600 hover:bg-blue-700 py-2 rounded w-full"
              disabled={addLoading}
            >
              {newAddress.isEdit ? (
                editLoading ? (
                  <SpinnerIcon />
                ) : (
                  t("address.editAddress")
                )
              ) : addLoading ? (
                <SpinnerIcon />
              ) : (
                t("address.saveAddress")
              )}
            </button>

            <button
              onClick={() => {
                setShowAddForm(false);
                setNewAddress({ ...newAddress, isEdit: false, id: undefined });
              }}
              className="mt-2 text-store-subtle text-sm"
            >
              {t("address.cancel")}
            </button>
          </div>
        )}
      </section>
    </div>
  );
}
