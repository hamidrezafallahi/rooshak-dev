"use client";

import { useLocale, useTranslations } from 'next-intl';
import { useRouter } from 'next/navigation';

import {
  ColumnDef,
  DataTable,
} from '@components/atoms/defaultElements/table';
import CustomPagination from '@components/molecules/pagination';
import ExcelToolbar from '@components/organisms/excelToolbar';

type AdminListProps = {
  list?: {
    records?: Record<string, unknown>[];
    actionsJson?: string;
    columnsJson?: string;
    totalCount?: number;
  };
  entity: string;
};

function AdminList({ list, entity }: AdminListProps) {
  const t = useTranslations();
  const route = useRouter();
  const locale = useLocale();

  if (!list?.columnsJson || !list?.actionsJson) {
    return (
      <div className="admin-page">
        <div className="admin-panel admin-empty">
          <p className="admin-empty-title">{t('admin.listLoadError')}</p>
        </div>
      </div>
    );
  }

  let columns: ColumnDef<Record<string, unknown>>[] = [];
  let actions: string[] = [];

  try {
    columns = JSON.parse(list.columnsJson);
    actions = JSON.parse(list.actionsJson);
  } catch {
    return (
      <div className="admin-page">
        <div className="admin-panel admin-empty">
          <p className="admin-empty-title">{t('admin.listLoadError')}</p>
        </div>
      </div>
    );
  }

  const { records = [], totalCount = 0 } = list;
  const optionalColumns: ColumnDef<Record<string, unknown>>[] = [
    ...columns,
    {
      Header: t('general.actions'),
      Accessor: 'options',
      Type: 'action',
    },
  ];

  const optionalRecords = records.map((r) => ({
    ...r,
    options: actions,
  }));

  const handleChangePage = (page: number, pageSize: number) => {
    route.push(
      `/${locale}/admin/${entity}?ByConfig=true&page=${page}&pageSize=${pageSize}`,
    );
  };

  const recordHeight = 64;
  const deviceHeight =
    typeof window !== 'undefined' ? window.innerHeight : 800;
  const chromeOffset =
    typeof window !== 'undefined' && window.innerWidth < 1024 ? 280 : 220;
  const recordsPerPage = Math.max(
    5,
    Math.floor((deviceHeight - chromeOffset) / recordHeight),
  );

  return (
    <div className="admin-page">
      <header className="admin-page-header">
        <div>
          <h1 className="admin-page-title">
            {t('admin.listTitle', { entity })}
          </h1>
          <p className="admin-page-subtitle">{t('admin.listSubtitle')}</p>
        </div>
        <span className="admin-badge admin-badge-success">
          {t('admin.totalRecords', { count: totalCount })}
        </span>
      </header>

      <div className="admin-panel overflow-hidden">
        <DataTable
          actions={actions}
          columns={optionalColumns}
          data={optionalRecords as unknown as Array<Record<string, unknown> & { id: string | number }>}
          entity={entity}
          pageSize={recordsPerPage}
          toolbarActions={({ selectedIds, search }) => (
            <ExcelToolbar entity={entity} selectedIds={selectedIds} search={search} />
          )}
        />
        <div className="admin-pagination-bar">
          <CustomPagination
            total={totalCount}
            showSizeChanger={true}
            showTitle={true}
            pageSize={recordsPerPage}
            onChange={handleChangePage}
          />
        </div>
      </div>
    </div>
  );
}

export default AdminList;
