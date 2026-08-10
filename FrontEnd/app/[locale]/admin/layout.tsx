import { ReactNode } from 'react';

import AdminLayout from '@layout/admin';

import '../../../style/admin.css';

interface IProps {
  children: ReactNode;
}
export default async function BaseLayout({ children }: IProps) {
  return <AdminLayout>{children}</AdminLayout>;
}
