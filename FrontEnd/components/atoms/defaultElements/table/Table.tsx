'use client';

import React, {
  Key,
  ReactNode,
} from 'react';

import moment from 'moment-jalaali';

import MediaImage from '@components/atoms/MediaImage';
import { CloseIcon } from '@components/atoms/iconComponents';
import { TickIcon } from '@components/atoms/iconComponents/icons/tickIcon';

import { Rate } from '../customRate';
import { TElement } from './';
import ActiveComponent from './activeComponent';
import ApproveComponent from './approveComponent';
import DefaultComponent from './defaultComponent';
import DeleteComponent from './deleteComponent';
import EditComponent from './editComponent';

interface TableProps extends React.ThHTMLAttributes<HTMLTableCellElement> {
  children: ReactNode;
  key?: Key;
  itemId?: number;
  itemType?: TElement;
}
interface IRow extends TableProps {
  row?: Record<string, any>;
}

export function renderAdminCellContent({
  itemType,
  children,
  row,
  compact = false,
}: {
  itemType?: TElement;
  children: ReactNode;
  row?: Record<string, any>;
  compact?: boolean;
}): ReactNode {
  switch (itemType) {
    case 'image':
      return typeof children === 'string' && children.trim().length > 0 ? (
        <div className="inline-flex justify-center items-center">
          <div
            className={`relative shadow-sm border border-[var(--admin-border)] rounded-xl overflow-hidden ${
              compact ? 'w-9 h-9' : 'w-11 h-11'
            }`}
          >
            <MediaImage
              src={children}
              alt="thumbnail"
              fill
              className="object-cover"
            />
          </div>
        </div>
      ) : (
        <span className="text-[var(--admin-text-muted)]">—</span>
      );
    case 'bool':
      return (
        <div className="inline-flex justify-center items-center h-8">
          {children ? (
            <TickIcon config={{ stroke: 'var(--success-color)' }} />
          ) : (
            <CloseIcon config={{ className: 'stroke-[var(--error-color)]' }} />
          )}
        </div>
      );
    case 'action':
      return (
        <div className="flex flex-wrap justify-end sm:justify-center items-center gap-1.5">
          {Array.isArray(children)
            ? children.map((item: string, index: number) => {
                switch (item.toLowerCase()) {
                  case 'edit':
                    return <EditComponent key={index} id={row!.id} />;
                  case 'delete':
                    return <DeleteComponent key={index} id={row!.id} />;
                  case 'active':
                    return (
                      <ActiveComponent
                        key={index}
                        id={row!.id}
                        checked={row!.isActive}
                      />
                    );
                  case 'approve':
                    return (
                      <ApproveComponent
                        key={index}
                        id={row!.id}
                        isApprove={row!.isApproved}
                      />
                    );
                  case 'default':
                    return (
                      <DefaultComponent
                        key={index}
                        id={row!.id}
                        isDefault={row!.isDefault}
                      />
                    );
                  default:
                    return null;
                }
              })
            : null}
        </div>
      );
    case 'date': {
      const dateValue =
        typeof children === 'string' ? new Date(children) : children;
      const formatted = moment(dateValue as string).format(
        'HH:mm - jYYYY/jMM/jDD',
      );
      return isNaN(dateValue as any) ? '-' : formatted;
    }
    case 'rate':
      return <Rate mode="display" value={Number(children as string)} />;
    default:
      return children === null || children === undefined || children === ''
        ? '—'
        : children;
  }
}

export function Table({ children }: TableProps) {
  return <table className="admin-table">{children}</table>;
}

export function TableHeader({ children }: TableProps) {
  return <thead>{children}</thead>;
}

export function TableBody({ children }: TableProps) {
  return <tbody>{children}</tbody>;
}

export function TableRow({ children }: TableProps) {
  return <tr>{children}</tr>;
}

export function TableHead({ children, ...props }: TableProps) {
  return (
    <th scope="col" {...props}>
      {children}
    </th>
  );
}

export function TableCell({ children, ...props }: IRow) {
  const { itemType, row } = props;
  const content = renderAdminCellContent({
    itemType: itemType as TElement,
    children,
    row,
  });

  // `max-width` on a <td> is ignored by the table layout, so a long value used to
  // widen its column until the whole table scrolled sideways on desktop. The limit
  // lives on an inner block instead; the full text stays available in `title`.
  const colSpan = (props as { colSpan?: number }).colSpan;
  const title = typeof children === 'string' || typeof children === 'number'
    ? String(children)
    : undefined;

  switch (itemType as TElement) {
    case 'image':
    case 'bool':
    case 'action':
      return <td colSpan={colSpan}>{content}</td>;
    case 'date':
      return (
        <td colSpan={colSpan} className="text-[var(--admin-text)] whitespace-nowrap">
          {content}
        </td>
      );
    case 'rate':
      return (
        <td colSpan={colSpan} title={title} className="text-[var(--admin-text)]">
          <div className="max-w-24 truncate">{content}</div>
        </td>
      );
    default:
      return (
        <td colSpan={colSpan} title={title} className="text-[var(--admin-text)]">
          {typeof content === 'string' || typeof content === 'number' ? (
            <div className="admin-cell-text">{content}</div>
          ) : (
            content
          )}
        </td>
      );
  }
}
