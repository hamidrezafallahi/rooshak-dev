import React, { ReactNode } from 'react';

import Footer from '@layout/footer';
import Header from '@layout/header';

function CooperationLayout({ children }: { children: ReactNode }) {
  return (
    <>
      <Header />
      <div className="pt-[var(--store-header-h)] min-h-[70vh]">{children}</div>
      <Footer />
    </>
  );
}

export default CooperationLayout;
