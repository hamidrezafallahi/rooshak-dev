"use client";
import React, {
  Suspense,
  useState,
} from 'react';

import { useTranslations } from 'next-intl';
import { shallowEqual } from 'react-redux';

import { Button } from '@components/atoms/defaultElements/customButton';
import {
  ShoppingCartIcon,
  SpinnerIcon,
} from '@components/atoms/iconComponents';
import { useAppSelector } from '@store/index';

const ShoppingCartHeaderComponent = React.lazy(
  () => import("./shoppingCartHeaderComponent")
);

function ShoppingCart() {
  const t = useTranslations();
  const [isOpen, setIsOpen] = useState(false);

  const { ShoppingCart } = useAppSelector(
    (state) => ({
      ShoppingCart: state.withPersist.ShoppingCart,
    }),
    shallowEqual
  );

  return (
    <>
      <div className="relative">
        <Button
          aria-label={t("header.shopping cart")}
          variant="ghost"
          className="relative flex justify-center items-center rounded-none w-10 h-10 hover:opacity-60 hover:bg-transparent text-inherit transition-opacity"
          onMouseEnter={() => setIsOpen(true)}
          onClick={() => setIsOpen(!isOpen)}
        >
          <ShoppingCartIcon config={{ size: 20 }} />
          {ShoppingCart?.products.length > 0 && (
            <span className="top-0 end-0 absolute flex justify-center items-center bg-primary rounded-full min-w-4 h-4 px-1 text-[0.625rem] text-primary-foreground leading-none">
              {ShoppingCart?.products.length}
            </span>
          )}
        </Button>

        {isOpen && (
          <Suspense
            fallback={
              <div className="top-2 absolute start-2">
                <SpinnerIcon />
              </div>
            }
          >
            <ShoppingCartHeaderComponent
              setIsOpen={setIsOpen}
            />
          </Suspense>
        )}
      </div>
    </>
  );
}

export default ShoppingCart;
