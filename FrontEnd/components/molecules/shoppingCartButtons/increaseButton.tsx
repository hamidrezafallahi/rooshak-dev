import React from 'react';

import { useDispatch } from 'react-redux';

import { SpinnerIcon } from '@components/atoms/iconComponents';
import { useGetConditionallyMutation } from '@services/base';
import {
  increaseToCart,
  synchronousCart,
} from '@slice/shoppingCartSlice';
import { getCookie } from '@utils/core';

import { ShoppingCartButtonProps } from './type';

function IncreaseButton({ ...props }: ShoppingCartButtonProps) {
  const { id, productOfferId, className, content } = props;
  const isAuthenticated = Boolean(getCookie("candySession"));
  const [itemMutate, { isLoading }] = useGetConditionallyMutation();

  const dispatch = useDispatch();
  const increaseHandler = async () => {
    if (isAuthenticated) {
      try {
        const syncCartResponse = await itemMutate({
          url: '/CartItems',
          body: { ProductId: id, ProductOfferId: productOfferId,Quantity:1},
        }).unwrap();
        if (syncCartResponse.isSuccess) {
          dispatch(synchronousCart(syncCartResponse.data));
        }
      } catch {
        // baseQueryByToken already showed the reason.
      }
    } else {
      dispatch(increaseToCart({ id ,productOfferId}));
    }
  };
  return (
    <button
      disabled={isLoading}
      onClick={() => {
        increaseHandler();
      }}
      className={className}
    >
      {isLoading ? <SpinnerIcon /> : (content ?? <>+</>)}
    </button>
  );
}

export default IncreaseButton;
