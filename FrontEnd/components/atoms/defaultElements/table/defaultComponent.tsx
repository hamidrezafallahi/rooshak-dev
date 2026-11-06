'use client';

import React, { useTransition } from 'react';

import {
  useParams,
  useRouter,
} from 'next/navigation';

import {
  CloseEyeIcon,
  EyeIcon,
} from '@components/atoms/iconComponents';
import { fetchDefault } from '@lib/setDefault';

import { Button } from '../customButton';
import { describeStatus } from '@lib/apiError';
import { showErrorToast } from '@utils/core';

interface IProps {
  id: number;
  isDefault: boolean;
}
 
export default function DefaultComponent({ id, isDefault }: IProps) {
 
  const [isPending, startTransition] = useTransition();
  const params = useParams()!;
  const router = useRouter();
  const handleDefault = (formData: FormData) => {
    startTransition(async () => {
     const result = await fetchDefault(params.field as string, formData);
     if (result.ok) {
      router.refresh();
     } else {
      showErrorToast(result.error ?? describeStatus(500), '', 6000);
     }
    });
  };

  return (
    <form action={handleDefault}>
      <input type="hidden" name="id" value={id} />
      <input type="hidden" name="isDefault" value={String(isDefault)} />
      
      <Button 
        className='admin-icon-btn !bg-[var(--admin-surface-elevated)] !p-0 !w-9 !h-9' 
        type='submit' 
        disabled={isPending}
        aria-label="default"
      >
        {isPending ? (
          '...'
        ) : isDefault ? (
          <EyeIcon />
        ) : (
          <CloseEyeIcon />
        )}
      </Button>
    </form>
  );
}