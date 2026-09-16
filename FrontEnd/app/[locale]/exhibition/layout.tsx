import React, { ReactNode } from 'react';

/** Bare shell — price-list sheets are full-bleed photo stacks with no chrome. */
function ExhibitionLayout({ children }: { children: ReactNode }) {
  return <div className="exhibit-root min-h-screen">{children}</div>;
}

export default ExhibitionLayout;
