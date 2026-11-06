import { cache } from 'react';

// When the API is down every section fails for the same reason; the reader needs it once.
const shownThisRender = cache(() => new Set<string>());

/** Inline explanation of a page or section that could not load. Renders nothing without a message. */
export default function ApiErrorNotice({
  message,
  className = '',
}: {
  message?: string | null;
  className?: string;
}) {
  if (!message) return null;

  const shown = shownThisRender();
  if (shown.has(message)) return null;
  shown.add(message);

  return (
    <p
      role="alert"
      className={`px-4 py-3 border border-[var(--error-color)] text-[var(--error-color)] text-sm text-start leading-7 whitespace-pre-line ${className}`}
    >
      {message}
    </p>
  );
}
