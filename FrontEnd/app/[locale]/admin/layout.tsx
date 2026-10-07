import { ReactNode } from 'react';

import AdminLayout from '@layout/admin';
import { ClientOnlyPersistGate } from '@store/provider';

interface IProps {
  children: ReactNode;
}
export default async function BaseLayout({ children }: IProps) {
  return (
    <ClientOnlyPersistGate>
      <AdminLayout>{children}</AdminLayout>
    </ClientOnlyPersistGate>
  );
}
