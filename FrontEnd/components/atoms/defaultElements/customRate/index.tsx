'use client';
import React, { useState } from 'react';

import { useTranslations } from 'next-intl';

import { StarFantasyIcon } from '@components/atoms/iconComponents';
import { cn } from '@lib/utils';

import { Button } from '../customButton';

interface RateProps {
  value: number;
  max?: number;
  mode?: 'display' | 'rate';
  onChange?: (rate: number) => void;
  showValue?: boolean;
  className?: string;
}

export const Rate: React.FC<RateProps> = ({
  value,
  max = 5,
  mode = 'display',
  onChange,
  showValue = false,
  className,
}) => {
  const t = useTranslations();
  const [hoverValue, setHoverValue] = useState<number | null>(null);

  const activeValue = hoverValue ?? value;

  const stars = Array.from({ length: max }).map((_, index) => {
    const rateValue = index + 1;
    const full = activeValue >= index + 1;
    const half = activeValue >= index + 0.5 && activeValue < index + 1;
    const type: 'full' | 'half' | 'empty' = full
      ? 'full'
      : half
        ? 'half'
        : 'empty';

    if (mode === 'display') {
      return (
        <span key={rateValue} className="inline-flex w-6 h-6" aria-hidden>
          <StarFantasyIcon config={{ type }} />
        </span>
      );
    }

    return (
      <Button
        key={rateValue}
        type="button"
        variant="ghost"
        size="icon"
        className="hover:bg-transparent m-0 p-0 w-6 h-6"
        aria-label={t('common.ratingAria', { value: rateValue, max })}
        onMouseEnter={() => setHoverValue(rateValue)}
        onMouseLeave={() => setHoverValue(null)}
        onClick={() => onChange?.(rateValue)}
      >
        <StarFantasyIcon config={{ type }} />
      </Button>
    );
  });

  return (
    <div
      className={cn('flex flex-row-reverse items-center gap-1', className)}
      role={mode === 'display' ? 'img' : 'group'}
      aria-label={t('common.ratingAria', { value, max })}
    >
      {stars}

      {showValue && (
        <span className="ml-1 text-gray-600 text-xs">{value.toFixed(1)}</span>
      )}
    </div>
  );
};
