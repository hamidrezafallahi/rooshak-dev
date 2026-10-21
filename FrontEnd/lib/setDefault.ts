'use server';

import {
  revalidatePath,
  revalidateTag,
} from 'next/cache';
import { unstable_rethrow } from 'next/navigation';

import { authenticatedFetch } from './server-fetch';

export async function fetchDefault(entity:string,formData:FormData) {
  const id = formData.get('id') as string;
  const isDefault = formData.get('isDefault') === 'true';
  try {
    // Refreshes the session on 401 and redirects to login if that fails.
    const data = await authenticatedFetch<{ isSuccess?: boolean }>({
      endpoint: `${entity}/set-default`,
      method: 'PUT',
      body: {
        Id: id,
        IsDefault: !isDefault,
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
  revalidatePath(`admin/${entity}`);
  revalidateTag(entity);
  return true
}
