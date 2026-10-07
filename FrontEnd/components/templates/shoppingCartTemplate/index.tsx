"use client";

import { useEffect } from 'react';

import { useRouter } from 'next/navigation';

import { getCookie } from '@utils/core';

import ClientAddress from './clientAddress';
import ShoppingCartComponent from './shoppingCartComponent';
import SummarySideBar from './summarySideBar';
import { IProps } from './type';

export default function ShoppingCartTemplate({ ...props }: IProps) {
  const { locale } = props;
  const route=useRouter()
  const isAuthenticated = Boolean(getCookie("candySession"));

  useEffect(() => {
    if (!isAuthenticated) {
      route.push(`/${locale}/register`);
    }
  }, [isAuthenticated]);
    if (!isAuthenticated) {
    return null; 
  }
  return (
    <div className="hidden-show-scrollbar bg-store-surface p-4 min-h-screen lg:h-screen lg:overflow-hidden overflow-y-auto text-store-text">
      <div className="gap-4 grid grid-cols-1 lg:grid-cols-3 mx-auto max-w-7xl h-full">
        {/* Left side - Shopping Cart */}
        <div className="hidden-show-scrollbar flex flex-col gap-4 lg:col-span-2 lg:h-[calc(100dvh-20px)] lg:overflow-y-auto" >
        <ShoppingCartComponent locale={locale} />
        <ClientAddress/>

        </div>
        {/* Right side - Order Summary */}
        <SummarySideBar locale={locale}/>

      </div>
    </div>
  );
}
