'use server';

import {
  revalidatePath,
  revalidateTag,
} from 'next/cache';
import { unstable_rethrow } from 'next/navigation';

import { describeThrown } from './apiError';
import { authenticatedFetch } from './server-fetch';

export async function fetchDefault(entity:string,formData:FormData) {
  const id = formData.get('id') as string;
  const isDefault = formData.get('isDefault') === 'true';
  try {
    // Refreshes the session on 401 and redirects to login if that fails.
    const data = await authenticatedFetch<{ isSuccess?: boolean; error?: string }>({
      endpoint: `${entity}/set-default`,
      method: 'PUT',
      body: {
        Id: id,
        IsDefault: !isDefault,
      },
    });
    if (!data?.isSuccess) {
      return { ok: false, error: data?.error };
    }
  } catch (error) {
    // Let the login redirect through; any other failure is reported to the caller.
    unstable_rethrow(error);
    return { ok: false, error: describeThrown(error) };
  }
  revalidatePath(`admin/${entity}`);
  revalidateTag(entity);
  return { ok: true };
}
