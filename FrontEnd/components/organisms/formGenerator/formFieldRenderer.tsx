import type { ReactNode } from 'react';

import { useTranslations } from 'next-intl';

import { Checkbox } from '@components/atoms/defaultElements/customCheckbox';
import ColorField from '@components/atoms/defaultElements/colorField';
import CustomDatePicker
  from '@components/atoms/defaultElements/customDatePicker';
import { Input } from '@components/atoms/defaultElements/customInput';
import { Rate } from '@components/atoms/defaultElements/customRate';
import { Select } from '@components/atoms/defaultElements/customSelect';
import { Textarea } from '@components/atoms/defaultElements/customTextarea';
import { DynamicSelect } from '@components/atoms/defaultElements/dynamicSelect';
import Uploader from '@components/atoms/defaultElements/uploader';
import VideoUploader from '@components/atoms/defaultElements/videoUploader';
import ImagesInput from '@components/molecules/imagesInput';

import { FormFieldRendererProps } from './type';

const FieldShell = ({
  caption,
  help,
  error,
  children,
  wide,
}: {
  caption: string;
  help?: string;
  error?: string;
  children: ReactNode;
  wide?: boolean;
}) => (
  <div className={`admin-field ${wide ? 'md:col-span-2' : ''}`}>
    <div className="flex flex-col sm:flex-row sm:justify-between sm:items-baseline gap-1 sm:gap-2">
      <label className="admin-field-label">{caption}</label>
      {help ? (
        <span className="text-[var(--admin-text-muted)] text-xs leading-relaxed">
          {help}
        </span>
      ) : null}
    </div>
    {children}
    {error ? <p className="admin-field-error">{error}</p> : null}
  </div>
);

const FormFieldRenderer = ({
  field,
  register,
  error,
  setValue,
  getValues,
  defaultValues,
  watch,
  trigger,
}: FormFieldRendererProps) => {
  const t = useTranslations();
  switch (field.Type) {
    case 'text':
    case 'number':
    case 'price':
      return (
        <FieldShell caption={field.Caption} help={field.Help} error={error}>
          <Input
            placeholder={field.PlaceHolder}
            aria-label={field.Name}
            type={field.Type === 'price' ? 'number' : field.Type}
            {...register}
          />
        </FieldShell>
      );
    case 'file':
      return (
        <FieldShell caption={field.Caption} help={field.Help} error={error}>
          <Uploader
            placeHolder={field.PlaceHolder}
            value={watch(field.Name) || ''}
            onChange={(file) => setValue(field.Name, file)}
          />
        </FieldShell>
      );
    case 'color':
      return (
        <FieldShell caption={field.Caption} help={field.Help} error={error}>
          <ColorField
            placeHolder={field.PlaceHolder}
            value={watch(field.Name) || ''}
            onChange={(hex) => setValue(field.Name, hex)}
          />
        </FieldShell>
      );
    case 'video':
      return (
        <FieldShell caption={field.Caption} help={field.Help} error={error}>
          <VideoUploader
            placeHolder={field.PlaceHolder}
            value={watch(field.Name) || ''}
            onChange={(file) => setValue(field.Name, file)}
          />
        </FieldShell>
      );
    case 'textarea':
      return (
        <FieldShell
          caption={field.Caption}
          help={field.Help}
          error={error}
          wide
        >
          <Textarea placeholder={field.PlaceHolder} {...register} />
        </FieldShell>
      );
    case 'date':
      return (
        <FieldShell caption={field.Caption} help={field.Help} error={error}>
          <CustomDatePicker
            defaultValue={getValues(field.Name) || ''}
            onChange={(e) => {
              setValue(field.Name, new Date(e as number).toISOString());
            }}
          />
        </FieldShell>
      );
    case 'checkbox':
      return (
        <FieldShell caption={field.Caption} help={field.Help} error={error}>
          <div className="flex items-center gap-2 bg-[var(--admin-surface-muted)] px-3 py-2.5 border border-[var(--admin-border)] rounded-xl min-h-10">
            <Checkbox {...register} />
          </div>
        </FieldShell>
      );
    case 'select': {
      const options = field.Options?.map((op) => ({
        label: op.label,
        value: op.value,
      })) || [{ label: t('common.noOptionsFound'), value: '' }];
      return (
        <FieldShell caption={field.Caption} help={field.Help} error={error}>
          <Select
            options={options}
            value={watch(field.Name)}
            onChange={(val) => {
              setValue(field.Name, val);
            }}
          />
        </FieldShell>
      );
    }
    case 'rate':
      return (
        <FieldShell caption={field.Caption} help={field.Help} error={error}>
          <Rate
            mode="rate"
            value={watch(field.Name) || ''}
            onChange={(e) => {
              setValue(field.Name, e);
            }}
          />
        </FieldShell>
      );
    case 'dynamicSelect': {
      let temp = new Object();
      if (
        field.FetchConfig?.fetchFilters &&
        field.FetchConfig?.fetchFilters.length > 0
      ) {
        field.FetchConfig?.fetchFilters.map((f) => {
          temp = {
            ...temp,
            [f]:
              watch(f) && String(watch(f)).length > 0
                ? watch(f)
                : defaultValues &&
                    defaultValues[f] &&
                    typeof defaultValues[f] == 'string' &&
                    defaultValues[f]?.length > 0
                  ? defaultValues[f]
                  : 0,
          };
        });
      }
      const config = { api: field.FetchConfig?.api, ...temp };
      return (
        <FieldShell caption={field.Caption} help={field.Help} error={error}>
          <DynamicSelect
            {...register}
            fetchConfig={config}
            value={watch(field.Name) || ''}
            onChange={(e) => {
              setValue(field.Name, e);
            }}
          />
        </FieldShell>
      );
    }
    case 'fileArray':
      return (
        <FieldShell
          caption={field.Caption}
          help={field.Help}
          error={error}
          wide
        >
          <ImagesInput
            onChange={(
              e: { id: number; file: undefined | File; isMain: boolean }[],
            ) => {
              const fileArray = e.map((f) => f.file).filter((f) => f);
              const isMainArray = e.map((f) => f.isMain);
              setValue('Images', fileArray);
              setValue('IsMainImages', isMainArray);
              trigger('Images');
            }}
          />
        </FieldShell>
      );

    default:
      return (
        <FieldShell caption={field.Caption} help={field.Help} error={error}>
          <Input placeholder={field.PlaceHolder} {...register} />
        </FieldShell>
      );
  }
};

export default FormFieldRenderer;
