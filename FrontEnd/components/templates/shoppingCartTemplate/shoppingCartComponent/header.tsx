import React from 'react';

import { useTranslations } from 'next-intl';
import { shallowEqual } from 'react-redux';

import BackToLandingPageButton
  from '@components/molecules/backToLandingPageButton';
import { useAppSelector } from '@store/index';

function Header({ ...props }) {
  const { locale } = props;
  const { ShoppingCart } = useAppSelector(
    (state) => ({
      ShoppingCart: state.withPersist.ShoppingCart,
    }),
    shallowEqual
  );
  const t = useTranslations();
  return (
    <div className="flex items-center gap-3 mb-4">
      <BackToLandingPageButton />

      <div className="flex flex-col">
        <h1 className="font-semibold text-xl">
          {t("shoppingCart.ShoppingCart")}
        </h1>
        <span className="text-store-subtle text-sm">
          {ShoppingCart.products.length} {t("shoppingCart.ItemsInYourCart")}
        </span>
      </div>
    </div>
  );
}

export default Header;
