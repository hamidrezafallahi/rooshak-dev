import React from 'react';

import { getLandingProductsByTabs } from '@lib/landing';

import TheMostProductsClient from './theMostProductsClient';

export default async function TheMostProducts() {
  const { bestSeller, theNewest, discounters } = await getLandingProductsByTabs();

  return (
    <section className="mx-auto px-4 sm:px-6 lg:px-10 py-14 md:py-20 w-full max-w-[1440px]">
      <TheMostProductsClient
        bestSeller={bestSeller}
        theNewest={theNewest}
        discounters={discounters}
      />
    </section>
  );
}
