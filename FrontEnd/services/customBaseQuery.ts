import { browserApiBaseUrl } from '@lib/api';
import {
  forbiddenMessage,
  withAuthRetry,
} from '@lib/client-auth';
import {
  BaseQueryApi,
  FetchArgs,
  fetchBaseQuery,
} from '@reduxjs/toolkit/query';
import { showErrorToast } from '@utils/core';

const rawBaseQuery = fetchBaseQuery({
  baseUrl: browserApiBaseUrl,
  credentials: "include",
});

/**
 * On 401: new tokens from /auth/refresh, then the same request again. When the
 * refresh itself is 401 the user is sent to the login page (lib/client-auth.ts).
 */
export async function baseQueryByToken(
  args: string | FetchArgs,
  api: BaseQueryApi,
  extraOptions: {}
) {
  const result = await withAuthRetry(
    async () => rawBaseQuery(args, api, extraOptions),
    (response) => response.error?.status
  );

  if (result.error?.status === 403) {
    showErrorToast(forbiddenMessage());
  }

  return result;
}
