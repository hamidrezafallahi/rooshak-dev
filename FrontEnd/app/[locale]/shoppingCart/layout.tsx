import { ReactNode } from 'react';

import { ClientOnlyPersistGate } from '@store/provider';

/** noindex flow that depends on persisted state: render on the client after rehydration. */
export default function Layout({ children }: { children: ReactNode }) {
  return <ClientOnlyPersistGate>{children}</ClientOnlyPersistGate>;
}
