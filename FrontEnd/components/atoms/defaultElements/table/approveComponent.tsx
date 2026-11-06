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
import { approveComment } from '@lib/comment';

import { Button } from '../customButton';
import { describeStatus } from '@lib/apiError';
import { showErrorToast } from '@utils/core';

interface IProps {
  id: number;
  isApprove: boolean;
}
 
export default function ApproveComponent({ id, isApprove }: IProps) {
 
  const [isPending, startTransition] = useTransition();
  const params = useParams()!;
  const router = useRouter();
  const handleApprove = (formData: FormData) => {
    startTransition(async () => {
     const result = await approveComment(params.field as string, formData);
     if (result.ok) {
      router.refresh();
     } else {
      showErrorToast(result.error ?? describeStatus(500), '', 6000);
     }
    });
  };

  return (
    <form action={handleApprove}>
      <input type="hidden" name="id" value={id} />
      <input type="hidden" name="isApproved" value={String(isApprove)} />
      
      <Button 
        className='admin-icon-btn !bg-[var(--admin-surface-elevated)] !p-0 !w-9 !h-9' 
        type='submit' 
        disabled={isPending}
        aria-label="approve"
      >
        {isPending ? (
          '...'
        ) : isApprove ? (
          <EyeIcon />
        ) : (
          <CloseEyeIcon />
        )}
      </Button>
    </form>
  );
}