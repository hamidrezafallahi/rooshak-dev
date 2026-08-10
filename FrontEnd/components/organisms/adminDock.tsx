'use client';

import { jwtDecode } from 'jwt-decode';
import { useLocale, useTranslations } from 'next-intl';
import Link from 'next/link';
import {
  useEffect,
  useState,
  type ReactNode,
} from 'react';

import {
  DiscountIcon,
  Home,
  Package,
  ShoppingBagIcon,
} from '@components/atoms/iconComponents';

const VISIBLE_ROLES = new Set([
  'SuperAdmin',
  'Admin',
  'StoreManager',
  'ContentEditor',
]);

const ROLE_CLAIM =
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';

type DockItem = {
  key: string;
  href: string;
  label: string;
  icon: ReactNode;
  emphasize?: boolean;
};

function readCookie(name: string): string | undefined {
  if (typeof document === 'undefined') return undefined;
  const match = document.cookie.match(
    new RegExp(`(?:^|; )${name.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}=([^;]*)`),
  );
  return match ? decodeURIComponent(match[1]) : undefined;
}

function readRole(token: string | undefined): string | null {
  if (!token) return null;

  try {
    const decoded = jwtDecode<Record<string, unknown>>(token);
    const role = decoded.role ?? decoded[ROLE_CLAIM];
    return typeof role === 'string' && role.length > 0 ? role : null;
  } catch {
    return null;
  }
}

/**
 * Client-only: reading auth cookies in an RSC would force Cache-Control: no-store
 * on every public page that renders the dock (breaks bfcache / ISR).
 */
export default function AdminDock() {
  const locale = useLocale();
  const t = useTranslations('admin');
  const [visible, setVisible] = useState(false);

  useEffect(() => {
    const role = readRole(readCookie('candyAccess'));
    setVisible(Boolean(role && VISIBLE_ROLES.has(role)));
  }, []);

  if (!visible) return null;

  const iconProps = { size: 18, strokeWidth: 1.75 } as const;

  const items: DockItem[] = [
    {
      key: 'orders',
      href: `/${locale}/admin/orders?ByConfig=true`,
      label: t('dock.orders'),
      icon: <ShoppingBagIcon config={iconProps} />,
    },
    {
      key: 'products',
      href: `/${locale}/admin/products?ByConfig=true`,
      label: t('dock.products'),
      icon: <Package config={iconProps} />,
    },
    {
      key: 'discounts',
      href: `/${locale}/admin/discounts?ByConfig=true`,
      label: t('dock.discounts'),
      icon: <DiscountIcon config={iconProps} />,
    },
    {
      key: 'panel',
      href: `/${locale}/admin`,
      label: t('dock.panel'),
      icon: <Home config={iconProps} />,
      emphasize: true,
    },
  ];

  return (
    <div className="admin-dock">
      <nav className="admin-dock-bar" aria-label={t('dock.aria')}>
        {items.map((item) => (
          <Link
            key={item.key}
            href={item.href}
            className={
              item.emphasize
                ? 'admin-dock-item admin-dock-item-primary'
                : 'admin-dock-item'
            }
            title={item.label}
          >
            <span className="admin-dock-icon" aria-hidden>
              {item.icon}
            </span>
            <span className="admin-dock-label">{item.label}</span>
          </Link>
        ))}
      </nav>
    </div>
  );
}
