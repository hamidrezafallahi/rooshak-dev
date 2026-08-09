"use client";

import React from 'react';

import { useTranslations } from 'next-intl';

function InvoiceList() {
  const t = useTranslations();
  const invoice = {
    invoiceNumber: 12,
    customerName: "ali",
    date: new Date().toISOString(),
    items: [
      {
        id: 1,
        title: "title",
        quantity: 12,
        unitPrice: 1200,
      },
    ],
  };

  return (
    <div
      className={`p-4 rounded-lg bg-zinc-900 border cursor-pointer space-y-4 transition-all ${
        true ? "border-primary shadow-primary shadow-lg" : "border-gray-700"
      }`}
    >
      {/* ================= Header ================= */}
      <div className="flex justify-between items-start pb-3 border-zinc-800 border-b">
        <div>
          <div className="font-semibold text-sm">
            {t("checkout.invoiceNumber", { number: invoice.invoiceNumber })}
          </div>
          <div className="mt-1 text-gray-400 text-xs">
            {t("checkout.customer", { name: invoice.customerName })}
          </div>
        </div>

        <div className="text-gray-400 text-xs">{invoice.date}</div>
      </div>

      {/* ================= Items ================= */}
      <div className="space-y-2">
        {invoice.items.map((item) => (
          <div
            key={item.id}
            className="flex justify-between pb-2 border-zinc-800 border-b text-xs"
          >
            <div className="flex-1">{item.title}</div>

            <div className="w-16 text-gray-400 text-center">
              {item.quantity}x
            </div>

            <div className="w-24 text-left">
              {(item.unitPrice * item.quantity).toLocaleString()}
            </div>
          </div>
        ))}
      </div>

      {/* ================= Totals ================= */}
      <div className="space-y-2 pt-3 border-zinc-800 border-t text-sm">
        <div className="flex justify-between text-gray-400">
          <span>{t("checkout.grandTotal")}</span>
          <span>{10000}</span>
        </div>

        <div className="flex justify-between font-semibold text-primary">
          <span>{t("checkout.payable")}</span>
          <span>{100000}</span>
        </div>
      </div>
    </div>
  );
}

export default InvoiceList;
