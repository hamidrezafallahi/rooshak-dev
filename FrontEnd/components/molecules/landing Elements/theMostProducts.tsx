import React from 'react';

import { getLandingProductsByTabs } from '@lib/landing';

import TheMostProductsClient from './theMostProductsClient';

export default async function TheMostProducts() {
  const { bestSeller, theNewest, discounters } = await getLandingProductsByTabs();

  return (
    <section className="mx-auto px-4 py-12 w-full max-w-7xl">
      <TheMostProductsClient
        bestSeller={bestSeller}
        theNewest={theNewest}
        discounters={discounters}
      />
    </section>
  );
}
