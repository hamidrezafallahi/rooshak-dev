'use client';

import { useEffect, useMemo, useState } from 'react';

type Payment = {
  id: number;
  orderId: number;
  amount: number;
  status: string | number;
  paymentDate: string;
  transactionId?: string | null;
};

type PaymentsResponse = {
  isSuccess?: boolean;
  error?: string | null;
  data?: {
    records?: Payment[];
    totalCount?: number;
  };
};

const pageSize = 10;
const statusStyleMap: Record<string, string> = {
  success: 'bg-emerald-100 text-emerald-700',
  failed: 'bg-red-100 text-red-700',
  pending: 'bg-amber-100 text-amber-700',
  cancelled: 'bg-slate-100 text-slate-600',
};

const statusLabelMap: Record<string, string> = {
  success: 'موفق',
  failed: 'ناموفق',
  pending: 'در انتظار',
  cancelled: 'لغو شده',
};

function normalizeStatus(status: string | number) {
  if (typeof status === 'number') {
    return ({ 1: 'pending', 2: 'success', 3: 'failed', 4: 'cancelled' } as Record<number, string>)[status] ?? 'unknown';
  }
  return status.toLowerCase();
}

function formatDate(value: string) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;
  return new Intl.DateTimeFormat('fa-IR-u-ca-persian', {
    dateStyle: 'medium',
    timeZone: 'Asia/Tehran',
  }).format(date);
}

export default function AdminPaymentsPage() {
  const [payments, setPayments] = useState<Payment[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [query, setQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');
  const [currentPage, setCurrentPage] = useState(1);

  useEffect(() => {
    const controller = new AbortController();

    async function loadPayments() {
      setIsLoading(true);
      setError(null);
      try {
        const response = await fetch('/auth/bff/admin/payments?page=1&pageSize=100', {
          credentials: 'include',
          cache: 'no-store',
          signal: controller.signal,
        });
        const payload = (await response.json()) as PaymentsResponse;
        if (!response.ok || payload.isSuccess === false) {
          throw new Error(payload.error || `HTTP ${response.status}`);
        }
        setPayments(payload.data?.records ?? []);
        setTotalCount(payload.data?.totalCount ?? payload.data?.records?.length ?? 0);
      } catch (loadError) {
        if (!controller.signal.aborted) {
          setError(loadError instanceof Error ? loadError.message : 'خطا در دریافت پرداخت‌ها');
        }
      } finally {
        if (!controller.signal.aborted) setIsLoading(false);
      }
    }

    void loadPayments();
    return () => controller.abort();
  }, []);

  const filteredPayments = useMemo(() => {
    const normalizedQuery = query.trim().toLowerCase();

    return payments.filter((payment) => {
      const matchesQuery = normalizedQuery.length === 0 ||
        String(payment.id).includes(normalizedQuery) ||
        String(payment.orderId).includes(normalizedQuery) ||
        (payment.transactionId ?? '').toLowerCase().includes(normalizedQuery);

      const matchesStatus =
        statusFilter === 'all' || normalizeStatus(payment.status) === statusFilter;

      return matchesQuery && matchesStatus;
    });
  }, [query, statusFilter]);

  const totalPages = Math.max(1, Math.ceil(filteredPayments.length / pageSize));
  const safePage = Math.min(currentPage, totalPages);
  const paginatedPayments = filteredPayments.slice(
    (safePage - 1) * pageSize,
    safePage * pageSize,
  );

  const formatAmount = (amount: number) =>
    new Intl.NumberFormat('fa-IR').format(amount) + ' تومان';

  return (
    <div className="space-y-6 p-6">
      <div className="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
        <div>
          <h1 className="text-2xl font-bold text-slate-800">لیست تراکنش‌های پرداخت</h1>
          <p className="text-sm text-slate-500">نمایش وضعیت پرداخت‌ها و تاریخ انجام تراکنش‌ها</p>
        </div>
        <span className="rounded-full bg-blue-50 px-3 py-1 text-sm font-medium text-blue-700">
          {totalCount} تراکنش
        </span>
      </div>

      <div className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm">
        <div className="grid gap-3 md:grid-cols-3">
          <input
            value={query}
            onChange={(e) => {
              setQuery(e.target.value);
              setCurrentPage(1);
            }}
            placeholder="جستجو بر اساس شناسه یا تراکنش"
            className="rounded-xl border border-slate-200 bg-slate-50 px-3 py-2 text-sm outline-none focus:border-blue-400"
          />

          <select
            value={statusFilter}
            onChange={(e) => {
              setStatusFilter(e.target.value);
              setCurrentPage(1);
            }}
            className="rounded-xl border border-slate-200 bg-slate-50 px-3 py-2 text-sm outline-none focus:border-blue-400"
          >
            <option value="all">همه وضعیت‌ها</option>
            <option value="success">موفق</option>
            <option value="pending">در انتظار</option>
            <option value="failed">ناموفق</option>
          </select>

          <div className="flex items-center justify-end text-sm text-slate-500">
            صفحه {safePage} از {totalPages}
          </div>
        </div>
      </div>

      <div className="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm">
        <div className="overflow-x-auto">
          <table className="min-w-full text-right">
            <thead className="bg-slate-100 text-sm text-slate-700">
              <tr>
                <th className="px-4 py-3">ID</th>
                <th className="px-4 py-3">سفارش</th>
                <th className="px-4 py-3">مبلغ</th>
                <th className="px-4 py-3">وضعیت</th>
                <th className="px-4 py-3">تاریخ</th>
                <th className="px-4 py-3">شناسه تراکنش</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-200 text-sm text-slate-700">
              {isLoading ? (
                <tr>
                  <td colSpan={6} className="px-4 py-8 text-center text-slate-500">در حال دریافت پرداخت‌ها...</td>
                </tr>
              ) : error ? (
                <tr>
                  <td colSpan={6} className="px-4 py-8 text-center text-red-600">{error}</td>
                </tr>
              ) : paginatedPayments.length === 0 ? (
                <tr>
                  <td colSpan={6} className="px-4 py-8 text-center text-slate-500">
                    رکوردی پیدا نشد.
                  </td>
                </tr>
              ) : (
                paginatedPayments.map((payment) => (
                  <tr key={payment.id} className="hover:bg-slate-50">
                    <td className="px-4 py-3 font-medium">#{payment.id}</td>
                    <td className="px-4 py-3">#{payment.orderId}</td>
                    <td className="px-4 py-3">{formatAmount(payment.amount)}</td>
                    <td className="px-4 py-3">
                      <span
                          className={`rounded-full px-2.5 py-1 text-xs font-medium ${statusStyleMap[normalizeStatus(payment.status)] ?? 'bg-slate-100 text-slate-600'}`}
                      >
                        {statusLabelMap[normalizeStatus(payment.status)] ?? payment.status}
                      </span>
                    </td>
                    <td className="px-4 py-3">{formatDate(payment.paymentDate)}</td>
                    <td className="px-4 py-3 font-mono text-xs">{payment.transactionId || 'بدون شناسه'}</td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        <div className="flex items-center justify-between border-t border-slate-200 bg-slate-50 px-4 py-3">
          <button
            type="button"
            onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
            disabled={safePage <= 1}
            className="rounded-lg border border-slate-200 bg-white px-3 py-1.5 text-sm disabled:cursor-not-allowed disabled:opacity-40"
          >
            قبلی
          </button>

          <span className="text-sm text-slate-600">
            {safePage} / {totalPages}
          </span>

          <button
            type="button"
            onClick={() => setCurrentPage((p) => Math.min(totalPages, p + 1))}
            disabled={safePage >= totalPages}
            className="rounded-lg border border-slate-200 bg-white px-3 py-1.5 text-sm disabled:cursor-not-allowed disabled:opacity-40"
          >
            بعدی
          </button>
        </div>
      </div>
    </div>
  );
}
