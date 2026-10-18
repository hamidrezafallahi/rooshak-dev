'use client';

import React, {
  ChangeEvent,
  useEffect,
  useRef,
  useState,
} from 'react';

import {
  useLocale,
  useTranslations,
} from 'next-intl';
import Link from 'next/link';
import { useRouter } from 'next/navigation';

import { CloseIcon } from '@components/atoms/iconComponents';

import { Checkbox } from '../customCheckbox';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
  renderAdminCellContent,
} from './Table';

export type TElement =
  | 'text'
  | 'textarea'
  | 'number'
  | 'image'
  | 'bool'
  | 'action'
  | 'date'
  | 'rate'
  | 'enum';

export interface ColumnDef<TData> {
  Header: React.ReactNode;
  Accessor: keyof TData | ((row: TData) => React.ReactNode);
  Type: TElement;
  Options?: ('Edit' | 'Delete' | 'Active')[];
}

export interface DataTableProps<TData> {
  columns: ColumnDef<TData>[];
  actions: string[];
  data: TData[];
  entity: string;
  pageSize: number;
  /** Extra toolbar buttons (e.g. Excel import/export) that need the grid's selection/search. */
  toolbarActions?: (state: {
    selectedIds: (string | number)[];
    search: string;
  }) => React.ReactNode;
}

function isAccessorFunction<TData>(
  accessor: keyof TData | ((row: TData) => React.ReactNode),
): accessor is (row: TData) => React.ReactNode {
  return typeof accessor === 'function';
}

function getCellRawValue<TData>(
  col: ColumnDef<TData>,
  row: TData,
): React.ReactNode {
  const rawValue = isAccessorFunction(col.Accessor)
    ? col.Accessor(row)
    : (row[col.Accessor] as React.ReactNode);

  return React.isValidElement(rawValue)
    ? rawValue
    : ((rawValue as React.ReactNode) ?? '');
}

export function DataTable<TData extends { id: number | string }>({
  columns,
  actions,
  data,
  entity,
  pageSize,
  toolbarActions,
}: DataTableProps<TData>) {
  const [selectedRows, setSelectedRows] = useState<(string | number)[]>([]);
  const [search, setSearch] = useState<string>('');
  const [isAllSelected, setIsAllSelected] = useState(false);
  const skip = useRef(false);
  const t = useTranslations();
  const route = useRouter();
  const locale = useLocale();

  const dataColumns = columns.filter((col) => col.Type !== 'action');
  const actionColumn = columns.find((col) => col.Type === 'action');

  const handleSelectAll = () => {
    if (isAllSelected) {
      setSelectedRows([]);
      setIsAllSelected(false);
    } else {
      setSelectedRows(data.map((item) => item.id));
      setIsAllSelected(true);
    }
  };

  const handleSelectRow = (id: string | number) => {
    setSelectedRows((prev) =>
      prev.includes(id) ? prev.filter((x) => x !== id) : [...prev, id],
    );
  };

  const handleFilterList = (e: ChangeEvent<HTMLInputElement>) => {
    setSearch(e.target.value.trim());
  };

  useEffect(() => {
    const filter = setTimeout(() => {
      if (skip.current) {
        if (search.trim().length > 0) {
          route.push(
            `/${locale}/admin/${entity}?ByConfig=true&page=${1}&pageSize=${pageSize}&q=${search}`,
          );
        } else {
          route.push(
            `/${locale}/admin/${entity}?ByConfig=true&page=${1}&pageSize=${pageSize}`,
          );
        }
      }
      skip.current = true;
    }, 808);
    return () => clearTimeout(filter);
  }, [search]);

  useEffect(() => {
    setIsAllSelected(selectedRows.length === data.length && data.length > 0);
  }, [selectedRows, data]);

  const toolbar = (
    <div className="admin-toolbar">
      <div className="flex flex-wrap items-center gap-2">
        {actions.includes('new') && (
          <Link
            href={`/${locale}/admin/${entity}/new`}
            className="admin-btn admin-btn-primary w-full sm:w-auto"
          >
            {t('general.new')}
          </Link>
        )}
        {toolbarActions?.({ selectedIds: selectedRows, search })}
        {selectedRows.length > 0 && (
          <span className="text-[var(--admin-text-muted)] text-sm">
            {t('admin.selectedCount', { count: selectedRows.length })}
          </span>
        )}
      </div>

      <div className="admin-search">
        <button
          type="button"
          onClick={() => setSearch('')}
          className="top-1/2 z-20 absolute border-[var(--admin-border)] border rounded-full -translate-y-1/2 end-2"
          aria-label={t('general.delete')}
        >
          <CloseIcon config={{ size: 14 }} />
        </button>
        <div className="absolute inset-y-0 flex items-center pointer-events-none ps-3 start-0">
          <svg
            className="w-4 h-4 text-[var(--admin-text-muted)]"
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 20 20"
          >
            <path
              stroke="currentColor"
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth="2"
              d="m19 19-4-4m0-7A7 7 0 1 1 1 8a7 7 0 0 1 14 0Z"
            />
          </svg>
        </div>
        <input
          type="text"
          placeholder={t('general.search')}
          onChange={handleFilterList}
          value={search}
        />
      </div>
    </div>
  );

  const emptyState = (
    <div className="admin-empty">
      <p className="admin-empty-title">{t('admin.noData')}</p>
      <p className="text-sm">{t('admin.noDataHint')}</p>
    </div>
  );

  return (
    <div className="relative w-full">
      {toolbar}

      {/* Mobile card layout */}
      <div className="admin-card-list">
        {data.length === 0 ? (
          emptyState
        ) : (
          data.map((row) => (
            <article key={row.id} className="admin-record-card">
              <div className="flex justify-between items-center gap-3">
                <label className="inline-flex items-center gap-2 text-[var(--admin-text-muted)] text-sm">
                  <Checkbox
                    checked={selectedRows.includes(row.id)}
                    onChange={() => handleSelectRow(row.id)}
                    className="bg-[var(--admin-surface-muted)] border-[var(--admin-border)] rounded-sm focus:ring-primary w-4 h-4 text-primary cursor-pointer"
                  />
                  <span>#{row.id}</span>
                </label>
                {actionColumn
                  ? renderAdminCellContent({
                      itemType: 'action',
                      children: getCellRawValue(actionColumn, row),
                      row: row as Record<string, any>,
                      compact: true,
                    })
                  : null}
              </div>

              <div className="flex flex-col gap-2.5">
                {dataColumns.map((col, colIndex) => {
                  const cellContent = getCellRawValue(col, row);
                  return (
                    <div key={colIndex} className="admin-record-card-row">
                      <span className="admin-record-card-label">
                        {col.Header}
                      </span>
                      <div className="admin-record-card-value">
                        {renderAdminCellContent({
                          itemType: col.Type,
                          children: cellContent,
                          row: row as Record<string, any>,
                          compact: true,
                        })}
                      </div>
                    </div>
                  );
                })}
              </div>
            </article>
          ))
        )}
      </div>

      {/* Desktop / tablet table */}
      <div className="hidden md:block admin-table-scroll">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead className="p-2 w-4">
                <div className="flex justify-center items-center">
                  <Checkbox
                    id="checkbox-all"
                    type="checkbox"
                    checked={isAllSelected}
                    onChange={handleSelectAll}
                    className="bg-[var(--admin-surface-muted)] border-[var(--admin-border)] rounded-sm focus:ring-primary w-4 h-4 text-primary cursor-pointer"
                  />
                </div>
              </TableHead>

              {columns.map((col, idx) => (
                <TableHead key={idx}>{col.Header}</TableHead>
              ))}
            </TableRow>
          </TableHeader>

          <TableBody>
            {data.length > 0 ? (
              data.map((row) => (
                <TableRow key={row.id}>
                  <TableCell>
                    <div className="flex justify-center items-center">
                      <Checkbox
                        checked={selectedRows.includes(row.id)}
                        onChange={() => handleSelectRow(row.id)}
                        className="bg-[var(--admin-surface-muted)] border-[var(--admin-border)] rounded-sm focus:ring-primary w-4 h-4 text-primary cursor-pointer"
                      />
                    </div>
                  </TableCell>

                  {columns.map((col, colIndex) => {
                    const cellContent = getCellRawValue(col, row);

                    return (
                      <TableCell key={colIndex} itemType={col.Type} row={row}>
                        {cellContent}
                      </TableCell>
                    );
                  })}
                </TableRow>
              ))
            ) : (
              <TableRow>
                <TableCell colSpan={columns.length + 1}>
                  {emptyState}
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </div>
    </div>
  );
}
