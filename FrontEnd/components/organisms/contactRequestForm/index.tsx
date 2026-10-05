'use client';

import React, { useState } from 'react';

import { useTranslations } from 'next-intl';
import { SubmitHandler, useForm } from 'react-hook-form';

import { Button } from '@components/atoms/defaultElements/customButton';
import { Input } from '@components/atoms/defaultElements/customInput';
import { Textarea } from '@components/atoms/defaultElements/customTextarea';
import { Label } from '@components/atoms/defaultElements/label';
import { cn } from '@lib/utils';
import { useCUDDataMutation } from '@services/base';
import { showErrorToast } from '@utils/core';

import {
  ContactRequestFormProps,
  ContactRequestFormValues,
} from './type';

const EMPTY_VALUES: ContactRequestFormValues = {
  firstName: '',
  lastName: '',
  phoneNumber: '',
  email: '',
  address: '',
  preferredContactTime: '',
  message: '',
  website: '',
};

const FIELD_CLASS = 'bg-white text-gray-900';
const ERROR_CLASS = 'border-red-500 focus-visible:ring-red-300';

/** Persian / Arabic digits -> Latin, and drop spaces and dashes. */
function normalizePhone(value: string) {
  return value
    .replace(/[۰-۹]/g, (d) => String(d.charCodeAt(0) - 0x06f0))
    .replace(/[٠-٩]/g, (d) => String(d.charCodeAt(0) - 0x0660))
    .replace(/[\s-]/g, '');
}

const PHONE_REGEX = /^(\+98|0098|0)?9\d{9}$/;
const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

function emptyToUndefined(value: string) {
  const trimmed = value.trim();
  return trimmed.length > 0 ? trimmed : undefined;
}

export default function ContactRequestForm({
  namespace = 'cooperationPage',
  className,
}: ContactRequestFormProps) {
  const t = useTranslations(namespace);
  const [submitRequest, { isLoading }] = useCUDDataMutation();
  const [isSubmitted, setIsSubmitted] = useState(false);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<ContactRequestFormValues>({
    mode: 'onBlur',
    defaultValues: EMPTY_VALUES,
  });

  const onSubmit: SubmitHandler<ContactRequestFormValues> = async (values) => {
    try {
      const res = await submitRequest({
        url: '/contactRequests',
        method: 'POST',
        // "LIST" skips RTK tag invalidation — nothing on the storefront caches this entity.
        id: 'LIST',
        body: {
          firstName: values.firstName.trim(),
          lastName: values.lastName.trim(),
          phoneNumber: normalizePhone(values.phoneNumber),
          email: emptyToUndefined(values.email),
          address: emptyToUndefined(values.address),
          preferredContactTime: emptyToUndefined(values.preferredContactTime),
          message: emptyToUndefined(values.message),
          website: emptyToUndefined(values.website),
        },
      }).unwrap();

      if (res?.isSuccess) {
        reset(EMPTY_VALUES);
        setIsSubmitted(true);
      } else {
        showErrorToast(res?.error || t('errors.submitFailed'), '', 5000);
      }
    } catch {
      showErrorToast(t('errors.connectionError'), '', 5000);
    }
  };

  if (isSubmitted) {
    return (
      <div
        role="status"
        className={cn(
          'flex flex-col items-center gap-3 p-6 sm:p-8 text-center store-panel',
          className,
        )}
      >
        <h2 className="font-bold text-lg sm:text-xl">{t('successTitle')}</h2>
        <p className="text-[var(--store-text-muted)] text-sm sm:text-base">
          {t('successText')}
        </p>
        <button
          type="button"
          className="mt-2 store-btn"
          onClick={() => setIsSubmitted(false)}
        >
          {t('sendAnother')}
        </button>
      </div>
    );
  }

  return (
    <form
      noValidate
      onSubmit={handleSubmit(onSubmit)}
      className={cn('flex flex-col gap-5 p-5 sm:p-8 store-panel', className)}
    >
      <div className="gap-5 grid grid-cols-1 sm:grid-cols-2">
        <div>
          <Label htmlFor="cr-firstName">{t('fields.firstName')} *</Label>
          <Input
            id="cr-firstName"
            autoComplete="given-name"
            maxLength={100}
            aria-invalid={Boolean(errors.firstName)}
            className={cn('mt-1.5', FIELD_CLASS, errors.firstName && ERROR_CLASS)}
            {...register('firstName', {
              required: t('errors.firstNameRequired'),
              validate: (v) => v.trim().length >= 2 || t('errors.minTwoChars'),
            })}
          />
          {errors.firstName && (
            <p className="mt-1 text-red-600 text-sm">{errors.firstName.message}</p>
          )}
        </div>

        <div>
          <Label htmlFor="cr-lastName">{t('fields.lastName')} *</Label>
          <Input
            id="cr-lastName"
            autoComplete="family-name"
            maxLength={100}
            aria-invalid={Boolean(errors.lastName)}
            className={cn('mt-1.5', FIELD_CLASS, errors.lastName && ERROR_CLASS)}
            {...register('lastName', {
              required: t('errors.lastNameRequired'),
              validate: (v) => v.trim().length >= 2 || t('errors.minTwoChars'),
            })}
          />
          {errors.lastName && (
            <p className="mt-1 text-red-600 text-sm">{errors.lastName.message}</p>
          )}
        </div>

        <div>
          <Label htmlFor="cr-phoneNumber">{t('fields.phoneNumber')} *</Label>
          <Input
            id="cr-phoneNumber"
            type="tel"
            inputMode="tel"
            dir="ltr"
            autoComplete="tel"
            maxLength={20}
            placeholder="09123456789"
            aria-invalid={Boolean(errors.phoneNumber)}
            className={cn('mt-1.5', FIELD_CLASS, errors.phoneNumber && ERROR_CLASS)}
            {...register('phoneNumber', {
              required: t('errors.phoneRequired'),
              validate: (v) =>
                PHONE_REGEX.test(normalizePhone(v)) || t('errors.invalidPhone'),
            })}
          />
          {errors.phoneNumber && (
            <p className="mt-1 text-red-600 text-sm">{errors.phoneNumber.message}</p>
          )}
        </div>

        <div>
          <Label htmlFor="cr-email">{t('fields.email')}</Label>
          <Input
            id="cr-email"
            type="email"
            dir="ltr"
            autoComplete="email"
            maxLength={200}
            placeholder="name@example.com"
            aria-invalid={Boolean(errors.email)}
            className={cn('mt-1.5', FIELD_CLASS, errors.email && ERROR_CLASS)}
            {...register('email', {
              validate: (v) =>
                v.trim().length === 0 ||
                EMAIL_REGEX.test(v.trim()) ||
                t('errors.invalidEmail'),
            })}
          />
          {errors.email && (
            <p className="mt-1 text-red-600 text-sm">{errors.email.message}</p>
          )}
        </div>
      </div>

      <div>
        <Label htmlFor="cr-address">{t('fields.address')} *</Label>
        <Textarea
          id="cr-address"
          rows={2}
          autoComplete="street-address"
          maxLength={500}
          aria-invalid={Boolean(errors.address)}
          className={cn('mt-1.5', FIELD_CLASS, errors.address && ERROR_CLASS)}
          {...register('address', {
            validate: (v) => v.trim().length > 0 || t('errors.addressRequired'),
          })}
        />
        {errors.address && (
          <p className="mt-1 text-red-600 text-sm">{errors.address.message}</p>
        )}
      </div>

      <div>
        <Label htmlFor="cr-preferredContactTime">
          {t('fields.preferredContactTime')}
        </Label>
        <Input
          id="cr-preferredContactTime"
          maxLength={200}
          placeholder={t('fields.preferredContactTimePlaceholder')}
          className={cn('mt-1.5', FIELD_CLASS)}
          {...register('preferredContactTime')}
        />
      </div>

      <div>
        <Label htmlFor="cr-message">{t('fields.message')}</Label>
        <Textarea
          id="cr-message"
          rows={4}
          maxLength={2000}
          placeholder={t('fields.messagePlaceholder')}
          className={cn('mt-1.5', FIELD_CLASS)}
          {...register('message')}
        />
      </div>

      {/* Honeypot: hidden from people and assistive tech; bots tend to fill it. */}
      <div aria-hidden className="hidden">
        <label htmlFor="cr-website">Website</label>
        <input
          id="cr-website"
          type="text"
          tabIndex={-1}
          autoComplete="off"
          {...register('website')}
        />
      </div>

      <div className="flex sm:flex-row flex-col sm:justify-between sm:items-center gap-3">
        <p className="text-[var(--store-text-muted)] text-xs">
          {t('privacyNote')}
        </p>
        <Button
          type="submit"
          disabled={isLoading}
          className="bg-primary disabled:bg-gray-400 sm:min-w-40 text-white"
        >
          {isLoading ? t('submitting') : t('submit')}
        </Button>
      </div>
    </form>
  );
}
