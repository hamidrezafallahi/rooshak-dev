import { ReactNode } from 'react';

import AdminLayout from '@layout/admin';

interface IProps {
  children: ReactNode;
}
export default async function BaseLayout({ children }: IProps) {
  return <AdminLayout>{children}</AdminLayout>;
}
