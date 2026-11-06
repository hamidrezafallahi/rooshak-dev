import * as React from 'react';

import { useLocale } from 'next-intl';

import { DataResponse } from '@models/base';
import { useGetData } from '@services/base';

import {
  Select,
  SelectProps,
} from '../customSelect';

export interface DynamicSelectProps extends SelectProps {
  fetchConfig: { api: string };
  fetchSize?: number;
}
export interface ResponseSelectOption {
  id: number;
  persianLabel: string;
  englishLabel: string;
}

const DynamicSelect = React.forwardRef<HTMLDivElement, DynamicSelectProps>(
  ({ fetchConfig, fetchSize = 7, ...props }, ref) => {
    const [options, setOptions] = React.useState<SelectProps["options"]>([]);
    const [page, setPage] = React.useState<number>(1);
    const locale = useLocale();
    const prevConfigRef = React.useRef(fetchConfig);
    // browserApiBaseUrl is already '/api'; stored paths must be controller-relative
    // (Users/selectOption). Strip a legacy 'api/' prefix to avoid /api/api/...
    const normalizedApi = String(fetchConfig?.api ?? '')
      .replace(/^\/?api\//i, '')
      .replace(/^\//, '');
    const endpoint = normalizedApi ? `/${normalizedApi}` : '';
    const params = new URLSearchParams();

    params.set("page", String(page));
    params.set("pageSize", String(fetchSize));
    Object.entries(fetchConfig ?? {}).forEach(([k, v]) => {
      if (k !== 'api' && v !== undefined && v !== null) {
        params.set(k, String(v));
      }
    });
    const { data } = useGetData<
      DataResponse<ResponseSelectOption>
    >({
      url: endpoint ? `${endpoint}?${params.toString()}` : '',
      method: 'GET',
      skip: !endpoint,
      reportFailure: true,
    });
    const handleSetPage = () => {
      if (data?.data && data.data.totalPages > page) {
        setPage(page + 1);
      }
    };

    React.useEffect(() => {
      if (data?.isSuccess && data.data) {
        const newOptions = data.data.records.map((opt) => ({
          label: locale == "fa" ? opt.persianLabel : opt.englishLabel,
          value: opt.id,
        }));
        setOptions((prev) => {
          const configChanged =
            JSON.stringify(fetchConfig) !==
            JSON.stringify(prevConfigRef.current);
          prevConfigRef.current = { ...fetchConfig };
          if (configChanged || page === 1) {
            return newOptions;
          } else {
            const prevIds = new Set(prev.map((opt) => opt.value));
            const freshOptions = newOptions.filter(
              (opt) => !prevIds.has(opt.value),
            );
            return [...prev, ...freshOptions];
          }
        });
      }
    }, [data]);
    return (
      <Select
        {...props}
        options={options}
        ref={ref}
        onPageChange={handleSetPage}
      />
    );
  },
);
DynamicSelect.displayName = "dynamicSelect";
export { DynamicSelect };
