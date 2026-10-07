'use client';

import { useEffect } from 'react';

import { Provider } from 'react-redux';
import { PersistGate } from 'redux-persist/integration/react';

import AppReduxStore, { persistor } from '@store/index';

/**
 * Starts rehydrating the persisted slices (cart, locale, theme) only after the
 * first client render. The server HTML and the first client render therefore both
 * use the slices' initial state, so the storefront can be server-rendered (SEO) and
 * hydrate without mismatches; persisted values then flow in right after mount.
 */
let persistStarted = false;

function PersistBoot() {
  useEffect(() => {
    // Once per page load (StrictMode runs effects twice in dev).
    if (persistStarted) return;
    persistStarted = true;
    persistor.persist();
  }, []);
  return null;
}

const ReduxProvider = ({ children }: { children: React.ReactNode }) => {
  return (
    <Provider store={AppReduxStore}>
      <PersistBoot />
      {children}
    </Provider>
  );
};

/**
 * Client-only gate for flows that must not render before persisted state is
 * available (admin, cart, checkout, orders, payment, sign-in). These routes are
 * noindex, so skipping server HTML there costs nothing.
 */
export const ClientOnlyPersistGate = ({
  children,
}: {
  children: React.ReactNode;
}) => (
  <PersistGate
    loading={
      <div className="absolute inset-0 flex flex-col justify-center items-center gap-3">
        <div className="mx-auto mb-4 border-primary border-t-2 border-b-2 rounded-full w-16 h-16 animate-spin"></div>
      </div>
    }
    persistor={persistor}
  >
    {children}
  </PersistGate>
);

export default ReduxProvider;
