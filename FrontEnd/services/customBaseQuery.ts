import { browserApiBaseUrl } from '@lib/api';
import {
  describeRtkError,
  describeStatus,
  errorTextOf,
} from '@lib/apiError';
import { withAuthRetry } from '@lib/client-auth';
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

const ERROR_TOAST_MS = 6000;

function failureText(
  args: string | FetchArgs,
  result: { error?: unknown; data?: unknown },
) {
  const isRead = (typeof args === 'string' ? 'GET' : args.method ?? 'GET').toUpperCase() === 'GET';

  if (result.error) {
    const status = (result.error as { status?: unknown }).status;
    // A background read for a visitor who is not signed in is not a failure they caused.
    if (status === 401 && isRead) return undefined;
    return describeRtkError(result.error);
  }

  // A read that finds nothing also answers isSuccess false; the screen shows that as empty.
  const data = result.data as { isSuccess?: boolean } | undefined;
  if (data?.isSuccess === false && !isRead) {
    return errorTextOf(data) ?? describeStatus(500);
  }
  return undefined;
}

/**
 * On 401: new tokens from /auth/refresh, then the same request again. When the
 * refresh itself is 401 the user is sent to the login page (lib/client-auth.ts).
 *
 * Every failed call is reported here, so no component can fail silently. A component
 * that shows the same text itself causes no second toast: showErrorToast dedupes by text.
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

  const message = failureText(args, result);
  if (message) {
    showErrorToast(message, '', ERROR_TOAST_MS);
  }

  return result;
}
