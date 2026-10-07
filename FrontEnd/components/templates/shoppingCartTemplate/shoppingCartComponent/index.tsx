import React from 'react';

import { shallowEqual } from 'react-redux';

import {
  MinusIcon,
  PlusIcon2,
  RialIcon,
} from '@components/atoms/iconComponents';
import MediaImage from '@components/atoms/MediaImage';
import { useAppSelector } from '@store/index';

import DecreaseButton
  from '../../../molecules/shoppingCartButtons/decreaseButton';
import IncreaseButton
  from '../../../molecules/shoppingCartButtons/increaseButton';
import RemoveButton from '../../../molecules/shoppingCartButtons/removeButton';
import Header from './header';

interface IProps {
  locale: string;
}
function ShoppingCartComponent({ ...props }: IProps) {
  const { locale } = props;
  const { ShoppingCart } = useAppSelector(
    (state) => ({
      ShoppingCart: state.withPersist.ShoppingCart,
    }),
    shallowEqual,
  );

  return (
    <>
      <Header locale={locale} />
      <div className="space-y-4 mt-6">
        {ShoppingCart.products.map((item, index) => {
          return (
            <div key={index} className="flex gap-4 bg-store-muted p-6 rounded-lg">
              <div className="flex-shrink-0 bg-store-muted rounded-lg w-24 h-24 overflow-hidden">
                <MediaImage
                  src={item.mainImage}
                  fallbackSrc="/images/default-product.jpg"
                  alt={item.name}
                  width={96}
                  height={96}
                  className="w-full h-full object-cover"
                />
              </div>

              <div className="flex flex-col flex-1 justify-between">
                <div>
                  <h3 className="font-medium text-base line-clamp-3">{item.description}</h3>
                </div>

                <div className="flex items-center gap-3">
                  <IncreaseButton
                    id={item.id}
                    productOfferId={item.productOfferId}
                    content={<PlusIcon2 />}
                    className="flex justify-center items-center hover:bg-store-muted border border-store-border rounded w-8 h-8 text-store-text"
                  />
                  <span className="w-8 text-center">{item.quantity}</span>
                  <DecreaseButton
                    id={item.id}
                    productOfferId={item.productOfferId}
                    content={<MinusIcon />}
                    className="flex justify-center items-center hover:bg-store-muted border border-store-border rounded w-8 h-8 text-store-text"
                  />
                </div>
              </div>

              <div className="flex flex-col justify-between items-end">
                <RemoveButton
                  id={item.id}
                  productOfferId={item.productOfferId}
                  cartItemId={item.cartItemId}
                  className="flex-shrink-0 text-store-subtle hover:text-store-text"
                />
                <div className="text-right">
                  <div className="flex gap-2 font-semibold text-lg">
                    {item.finalPrice * item.quantity}

                    <RialIcon />
                  </div>
                  {item.discountAmount > 0 && (
                    <div className="flex gap-2 text-store-subtle text-sm line-through">
                      {item.price * item.quantity}

                      <RialIcon />
                    </div>
                  )}
                </div>
              </div>
            </div>
          );
        })}
      </div>
    </>
  );
}

export default ShoppingCartComponent;
