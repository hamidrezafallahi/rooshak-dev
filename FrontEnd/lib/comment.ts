'use server';

import {
  revalidatePath,
  revalidateTag,
} from 'next/cache';
import { unstable_rethrow } from 'next/navigation';

import { authenticatedFetch } from './server-fetch';

export async function approveComment(field:string,formData: FormData) {
  const id = formData.get('id') as string;
  const isApprove = formData.get('isApproved') === 'true';
  try {
    // Refreshes the session on 401 and redirects to login if that fails.
    const data = await authenticatedFetch<{ isSuccess?: boolean }>({
      endpoint: `${field}/approve`,
      method: 'PUT',
      body: {
        Id: parseInt(id),
        IsApprove: !isApprove,
      },
    });
    if (!data?.isSuccess) {
      return false;
    }
  } catch (error) {
    // Let the login redirect through; any other failure is just "not saved".
    unstable_rethrow(error);
    return false;
  }
  revalidatePath('/fa/admin/comments');
  revalidatePath('/en/admin/comments');
  revalidateTag('comments');
  return true
}
