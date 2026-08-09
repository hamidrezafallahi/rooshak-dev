"use client";

import React, {
  useEffect,
  useTransition,
} from 'react';

import {
  useLocale,
  useTranslations,
} from 'next-intl';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import {
  RegisterOptions,
  SubmitHandler,
  useForm,
} from 'react-hook-form';

import { Button } from '@components/atoms/defaultElements/customButton';
import { saveEntity } from '@lib/saveEntity';

import FormFieldRenderer from './formFieldRenderer';
import FormHeader from './formHeader';
import {
  FormField,
  FormGeneratorProps,
} from './type';

export default function FormGenerator({
  entityFormConfig,
  defaultValues,
  embedded = false,
}: FormGeneratorProps) {
  const [isPending, startTransition] = useTransition();
  const locale = useLocale();
  const route = useRouter();
  const t = useTranslations();
  const {
    register,
    handleSubmit,
    formState: { errors },
    setValue,
    getValues,
    resetField,
    reset,
    watch,
    trigger,
  } = useForm({ mode: "onBlur", defaultValues });

  let formFields: FormField[] = [];
  let parseError = false;
  if (entityFormConfig?.formFieldsJson) {
    try {
      formFields = JSON.parse(entityFormConfig.formFieldsJson);
    } catch {
      parseError = true;
    }
  }

  if (!entityFormConfig?.formFieldsJson || parseError) {
    return (
      <div className={embedded ? 'flex flex-col gap-4' : 'admin-page'}>
        <div className="admin-panel admin-empty">
          <p className="admin-empty-title">
            {parseError
              ? t("admin.formParseError")
              : t("admin.formLoadError")}
          </p>
          <Link
            href={`/${locale}/admin/${entityFormConfig?.endPoint ?? ""}`}
            className="admin-btn mt-2"
          >
            {t("general.back")}
          </Link>
        </div>
      </div>
    );
  }

  const displayName =
    locale !== "fa"
      ? entityFormConfig.englishDisplayName
      : entityFormConfig.persianDisplayName;
  const isEdit = Boolean(defaultValues?.id);

  const getValidationRules = (field: FormField): RegisterOptions => {
    const rules: RegisterOptions = {};
    if (field.Type === "number" || field.Type === "price") {
      rules.valueAsNumber = true;
    }
    field.Rules?.forEach((r) => {
      if (r.Rule === "required" && r.Condition) {
        rules.required = r.Message || `${field.Caption} is required`;
      }
    });
    return rules;
  };

  const coerceFieldValue = (field: FormField, raw: unknown) => {
    if (field.Type === "checkbox") {
      return Boolean(raw);
    }
    if (field.Type === "number" || field.Type === "price") {
      if (raw === "" || raw === null || raw === undefined) return undefined;
      const n = typeof raw === "number" ? raw : Number(raw);
      return Number.isFinite(n) ? n : undefined;
    }
    if (typeof raw === "string" && raw.length === 0) return undefined;
    return raw;
  };

  const handleAddOrUpdateRecord: SubmitHandler<any> = async (data) => {
    const cleanedData: any = {
      id: data["id"] ? Number(data["id"]) : undefined,
    };
    formFields.forEach((field) => {
      if (field.Type === "json" && data[field.Name]) {
        cleanedData[field.Name] = JSON.stringify(data[field.Name]);
      } else {
        cleanedData[field.Name] = coerceFieldValue(field, data[field.Name]);
      }
    });
    const hasFileArray = formFields.some((field) => field.Type === "fileArray");
    let bodyToSend: any = cleanedData;
    if (hasFileArray) {
      const formData = new FormData();
      for (const key in cleanedData) {
        if (key !== "Images" && key !== "IsMainImages") {
          if (cleanedData[key] instanceof File) {
            formData.append(key, cleanedData[key]);
          } else {
            formData.append(key, cleanedData[key] ?? "");
          }
        } else {
          if (key == "Images") {
            const imagesFiles = cleanedData["Images"] || [];
            imagesFiles.forEach((file: File) => {
              formData.append("Images", file);
            });
          } else {
            const isMainImages = cleanedData["IsMainImages"] || [];
            isMainImages.forEach((isMain: boolean) => {
              formData.append("IsMainImages", isMain.toString());
            });
          }
        }
      }
      bodyToSend = formData;
    } else {
      const hasFile = formFields.some((field) => field.Type === "file");
      if (hasFile) {
        const formData = new FormData();
        for (const key in cleanedData) {
          if (cleanedData[key] instanceof File) {
            formData.append(key, cleanedData[key]);
          } else if (cleanedData[key] !== undefined) {
            formData.append(key, cleanedData[key] ?? "");
          }
        }
        bodyToSend = formData;
      }
    }
    startTransition(() => {
      void (async () => {
        try {
          const res = await saveEntity({
            endPoint: entityFormConfig.endPoint,
            body: bodyToSend,
            method: defaultValues?.id ? "PUT" : "POST",
          });
          if (res?.isSuccess) {
            Object.entries(formFields).forEach(([, v]) => {
              resetField(v.Name, undefined);
              setValue(v.Name, undefined);
            });
            route.push(`/${locale}/admin/${entityFormConfig.endPoint}`);
            route.refresh();
          } else {
            const { showErrorToast } = await import('@utils/core');
            showErrorToast(
              res?.error || t('common.saveFailed'),
              '',
              5000,
            );
          }
        } catch (err) {
          const { showErrorToast } = await import('@utils/core');
          const message =
            err instanceof Error ? err.message : t('common.saveFailed');
          showErrorToast(message, '', 5000);
        }
      })();
    });
  };

  const handleResetButton = () => {
    if (defaultValues) {
      Object.entries(formFields).forEach(([, v]) => {
        resetField(v.Name, defaultValues[v.Name]);
      });
    } else {
      Object.entries(formFields).forEach(([, v]) => {
        resetField(v.Name, undefined);
        setValue(v.Name, undefined);
      });
    }
  };

  useEffect(() => {
    if (defaultValues === undefined) {
      const emptyDefaults: Record<string, unknown> = {};
      formFields.forEach((field) => {
        const emptyValue =
          field.Type === "checkbox"
            ? false
            : field.Type === "number" || field.Type === "price"
              ? undefined
              : "";
        emptyDefaults[field.Name] = emptyValue;
        resetField(field.Name as any, { defaultValue: emptyValue });
        setValue(field.Name as any, emptyValue);
      });
      reset(emptyDefaults);
    } else {
      reset(defaultValues);
    }
  }, [defaultValues]);

  return (
    <div className={embedded ? 'flex flex-col gap-4' : 'admin-page'}>
      <FormHeader
        DisplayName={displayName}
        icon={
          <div
            className="flex justify-center items-center bg-[var(--admin-active)] rounded-xl w-11 h-11 text-primary [&>svg]:w-5 [&>svg]:h-5"
            dangerouslySetInnerHTML={{
              __html: entityFormConfig.entityIconBase64,
            }}
          />
        }
        isEdit={isEdit}
        resetField={handleResetButton}
        backHref={
          embedded
            ? undefined
            : `/${locale}/admin/${entityFormConfig.endPoint}?ByConfig=true`
        }
      />

      <form
        onSubmit={handleSubmit(handleAddOrUpdateRecord)}
        className="admin-panel flex flex-col overflow-hidden"
      >
        <div
          className={`hidden-show-scrollbar gap-4 sm:gap-5 grid grid-cols-1 md:grid-cols-2 p-4 sm:p-5 md:p-6 overflow-y-auto ${
            embedded
              ? 'max-h-[min(70dvh,calc(100dvh-300px))]'
              : 'max-h-[calc(100dvh-240px)] lg:max-h-[calc(100dvh-220px)]'
          }`}
        >
          {formFields
            .filter((field) => field.Type !== "hidden")
            .sort((a, b) => (a.Order || 0) - (b.Order || 0))
            .map((field) => (
              <div
                key={field.Name}
                className={
                  field.Type === "fileArray" || field.Type === "textarea"
                    ? "md:col-span-2 min-w-0"
                    : "min-w-0"
                }
              >
                <FormFieldRenderer
                  field={field}
                  defaultValues={defaultValues}
                  getValues={getValues}
                  watch={watch}
                  setValue={setValue}
                  trigger={trigger}
                  register={register(field.Name, getValidationRules(field))}
                  error={errors[field.Name]?.message as string | undefined}
                />
              </div>
            ))}
        </div>

        <div className="admin-form-actions">
          <Button
            type="button"
            variant="outline"
            className="admin-btn"
            onClick={handleResetButton}
            disabled={isPending}
          >
            {t("general.reset")}
          </Button>
          <Button
            className="admin-btn admin-btn-primary sm:min-w-32"
            type="submit"
            disabled={isPending}
          >
            {isPending ? t("general.saving") : t("general.save")}
          </Button>
        </div>
      </form>
    </div>
  );
}
