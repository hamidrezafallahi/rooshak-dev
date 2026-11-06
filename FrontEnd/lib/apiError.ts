/**
 * The sentence a user reads when an API call fails. The backend's `error` text always wins;
 * the texts here only cover answers that never came from it (offline, proxy error pages),
 * and none of them shows a status code.
 */
const COPY = {
  fa: {
    offline: 'ارتباط با سرور برقرار نشد؛ اتصال اینترنت خود را بررسی کنید و دوباره تلاش کنید.',
    timeout: 'پاسخ سرور بیش از حد طول کشید؛ چند لحظه بعد دوباره تلاش کنید.',
    unreadable: 'پاسخ نامعتبری از سرور دریافت شد؛ چند لحظه بعد دوباره تلاش کنید.',
    login: 'برای انجام این کار ابتدا وارد حساب کاربری خود شوید.',
    forbidden: 'شما دسترسی لازم برای انجام این کار را ندارید.',
    notFound: 'مورد درخواستی روی سرور پیدا نشد.',
    tooLarge: 'حجم داده یا فایل ارسال‌شده بیشتر از سقف مجاز سرور است.',
    rateLimited: 'تعداد درخواست‌ها بیش از حد مجاز است؛ کمی صبر کنید و دوباره تلاش کنید.',
    unavailable: 'سرور موقتاً در دسترس نیست؛ چند لحظه بعد دوباره تلاش کنید.',
    server: 'سرور در انجام این درخواست با مشکل روبه‌رو شد؛ چند لحظه بعد دوباره تلاش کنید.',
    invalid: 'سرور این درخواست را نپذیرفت؛ اطلاعات واردشده را بررسی کنید.',
    tracking: 'کد پیگیری',
  },
  en: {
    offline: 'Could not reach the server. Check your connection and try again.',
    timeout: 'The server took too long to answer. Please try again shortly.',
    unreadable: 'The server sent an unreadable answer. Please try again shortly.',
    login: 'Please sign in to do this.',
    forbidden: 'You do not have permission to do this.',
    notFound: 'The requested item was not found on the server.',
    tooLarge: 'The data or file you sent is larger than the server allows.',
    rateLimited: 'Too many requests. Please wait a moment and try again.',
    unavailable: 'The server is temporarily unavailable. Please try again shortly.',
    server: 'The server could not complete this request. Please try again shortly.',
    invalid: 'The server refused this request. Please check what you entered.',
    tracking: 'Tracking code',
  },
};

// Runs outside React as well (no next-intl hooks), so the language comes from the URL.
function copy() {
  const isEn =
    typeof window !== 'undefined' &&
    /^\/en(\/|$)/.test(window.location.pathname);
  return isEn ? COPY.en : COPY.fa;
}

/** One field the backend refused. `message` is ready to show under the input. */
export interface FieldError {
  field: string;
  label?: string;
  message: string;
}

/** Every API answer: success or the failure shape of the backend's ApiErrorBody. */
export type ApiResult<T = any> = {
  isSuccess: boolean;
  data: T;
  error: string | null;
  errorCode?: string;
  fieldErrors?: FieldError[];
};

const DIGEST_PREFIX = 'API_ERROR:';

/**
 * A failure with a text written for the user. Next.js swaps the message of a server error
 * for a generic one in production but keeps its digest, so when this is thrown while
 * rendering, the explanation travels there and error.tsx reads it with messageFromDigest().
 */
export class ApiRequestError extends Error {
  readonly digest: string;

  constructor(message: string) {
    super(message);
    this.name = 'ApiRequestError';
    this.digest = `${DIGEST_PREFIX}${encodeURIComponent(message)}`;
  }
}

/** The explanation an ApiRequestError carried to the error boundary, if this was one. */
export function messageFromDigest(digest: string | undefined): string | undefined {
  if (!digest?.startsWith(DIGEST_PREFIX)) return undefined;
  try {
    return decodeURIComponent(digest.slice(DIGEST_PREFIX.length));
  } catch {
    return undefined;
  }
}

/** A status number, or RTK Query's FETCH_ERROR / TIMEOUT_ERROR / PARSING_ERROR. */
export function describeStatus(status: unknown): string {
  const text = copy();
  if (status === 'FETCH_ERROR' || status === 0) return text.offline;
  if (status === 'TIMEOUT_ERROR' || status === 408 || status === 504) return text.timeout;
  if (status === 'PARSING_ERROR') return text.unreadable;
  if (status === 401) return text.login;
  if (status === 403) return text.forbidden;
  if (status === 404) return text.notFound;
  if (status === 413) return text.tooLarge;
  if (status === 429) return text.rateLimited;
  if (status === 502 || status === 503) return text.unavailable;
  if (typeof status === 'number' && status >= 500) return text.server;
  return text.invalid;
}

/**
 * The explanation inside an API answer, if it carries one. The backend sends `detail`
 * (the technical cause) only to admins, so it can be shown whenever it is there.
 */
export function errorTextOf(body: unknown): string | undefined {
  if (!body || typeof body !== 'object') return undefined;
  const { error, message, detail, errorCode, traceId } = body as Record<string, unknown>;
  const text =
    (typeof error === 'string' && error.trim() && error) ||
    (typeof message === 'string' && message.trim() && message) ||
    undefined;
  if (!text) return undefined;
  if (typeof detail === 'string' && detail.trim()) return `${text}\n${detail}`;
  if (errorCode === 'INTERNAL_ERROR' && typeof traceId === 'string' && traceId) {
    return `${text}\n${copy().tracking}: ${traceId}`;
  }
  return text;
}

/**
 * Reads a fetch Response as an API result and never throws. The failure shape is the same
 * whether the backend answered, or nginx answered with a page of its own.
 */
export async function readApiResult<T = any>(response: Response): Promise<ApiResult<T>> {
  const failed = (error: string): ApiResult<T> => ({
    isSuccess: false,
    data: null as T,
    error,
  });

  let text: string;
  try {
    text = await response.text();
  } catch {
    return failed(describeStatus('FETCH_ERROR'));
  }

  if (response.status === 204) {
    return { isSuccess: true, data: null as T, error: null };
  }

  let body: ApiResult<T>;
  try {
    body = JSON.parse(text) as ApiResult<T>;
  } catch {
    return failed(describeStatus(response.ok ? 'PARSING_ERROR' : response.status));
  }

  if (!response.ok && body?.isSuccess !== true) {
    return {
      ...body,
      isSuccess: false,
      error: errorTextOf(body) ?? describeStatus(response.status),
    };
  }

  return body?.isSuccess === false
    ? { ...body, error: errorTextOf(body) ?? describeStatus(500) }
    : body;
}

/** For the error object of RTK Query (`res.error`, or what `.unwrap()` rejects with). */
export function describeRtkError(error: unknown): string {
  const failure = (error ?? {}) as {
    status?: unknown;
    originalStatus?: unknown;
    data?: unknown;
  };
  return (
    errorTextOf(failure.data) ??
    describeStatus(
      failure.status === 'PARSING_ERROR' && failure.originalStatus !== 200
        ? failure.originalStatus
        : failure.status,
    )
  );
}

/**
 * For `catch (error)`. Browser and framework messages ("Failed to fetch", "Unexpected
 * token <") say nothing to a user, so they are named by what happened instead.
 */
export function describeThrown(error: unknown): string {
  if (error instanceof ApiRequestError) return error.message;
  if (error && typeof error === 'object' && 'status' in error) return describeRtkError(error);
  if (error instanceof Error) {
    if (/[؀-ۿ]/.test(error.message)) return error.message;
    if (error.name === 'AbortError' || error.name === 'TimeoutError') {
      return describeStatus('TIMEOUT_ERROR');
    }
    if (error instanceof SyntaxError) return describeStatus('PARSING_ERROR');
    if (error instanceof TypeError) return describeStatus('FETCH_ERROR');
  }
  return describeStatus(500);
}
