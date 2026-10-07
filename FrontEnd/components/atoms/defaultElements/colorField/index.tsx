'use client';

import React, { useEffect, useState } from 'react';

const HEX = /^#[0-9a-fA-F]{6}$/;

type Props = {
  /** Current #RRGGBB value (may be empty on a new record). */
  value?: string;
  /** Suggested default; used when the field is empty. */
  placeHolder?: string;
  onChange: (hex: string) => void;
};

/**
 * Admin colour control: native picker + editable hex box + live swatch.
 * The value stays a validated #RRGGBB string, which is exactly what the theme API stores.
 */
export default function ColorField({ value, placeHolder, onChange }: Props) {
  const fallback = HEX.test(placeHolder ?? '') ? (placeHolder as string) : '#000000';
  const current = HEX.test(value ?? '') ? (value as string) : '';
  const [text, setText] = useState(current || fallback);

  // New record: seed the form with the suggested colour so the field is never empty.
  useEffect(() => {
    if (!current) onChange(fallback);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Keep the box in sync when the form is reset / loaded with an existing record.
  useEffect(() => {
    if (current) setText(current);
  }, [current]);

  const commit = (next: string) => {
    const hex = next.startsWith('#') ? next : `#${next}`;
    setText(hex);
    if (HEX.test(hex)) onChange(hex.toLowerCase());
  };

  const valid = HEX.test(text);

  return (
    <div className="flex items-center gap-2" dir="ltr">
      <input
        type="color"
        aria-label="color picker"
        value={valid ? text : fallback}
        onChange={(e) => commit(e.target.value)}
        className="bg-transparent p-0 border border-[var(--admin-border)] rounded-lg w-11 h-10 cursor-pointer"
      />
      <input
        type="text"
        inputMode="text"
        maxLength={7}
        spellCheck={false}
        value={text}
        onChange={(e) => commit(e.target.value.trim())}
        aria-invalid={!valid}
        placeholder={fallback}
        className={`flex-1 bg-[var(--admin-surface-muted)] px-3 py-2 border rounded-xl font-mono text-sm uppercase outline-none ${
          valid ? 'border-[var(--admin-border)]' : 'border-[var(--error-color)]'
        } text-[var(--admin-text)]`}
      />
      <span
        aria-hidden
        className="border border-[var(--admin-border)] rounded-lg w-10 h-10 shrink-0"
        style={{ backgroundColor: valid ? text : 'transparent' }}
      />
    </div>
  );
}
