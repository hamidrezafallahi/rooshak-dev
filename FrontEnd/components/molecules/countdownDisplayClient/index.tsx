"use client";
import React, {
  useEffect,
  useState,
} from 'react';

import { useTranslations } from 'next-intl';

interface CountdownDisplayClientProps {
  targetTime: number;
}

export default function CountdownDisplayClient({ targetTime }: CountdownDisplayClientProps) {
  const t = useTranslations('countdown');
  // Time-dependent: stay null until mounted so the server HTML (prerendered, hence
  // stale) and the first client render are identical — no hydration mismatch.
  const [timeLeft, setTimeLeft] = useState<ReturnType<typeof calculateTimeLeft> | null>(null);

  useEffect(() => {
    setTimeLeft(calculateTimeLeft(targetTime));
    const timer = setInterval(() => {
      setTimeLeft(calculateTimeLeft(targetTime));
    }, 1000);
    return () => clearInterval(timer);
  }, [targetTime]);

  const isExpired =
    timeLeft !== null &&
    timeLeft.hours === 0 &&
    timeLeft.minutes === 0 &&
    timeLeft.seconds === 0;

  if (isExpired)
    return (
      <p className="bg-gray-950 bg-opacity-80 p-1 rounded font-semibold text-yellow-200">
        {t('expired')}
      </p>
    );

  return (
    <div className="flex items-center gap-1 text-center">
      <TimeBox value={timeLeft?.hours} label={t('hours')} />
      <span className="font-bold">:</span>
      <TimeBox value={timeLeft?.minutes} label={t('minutes')} />
      <span className="font-bold">:</span>
      <TimeBox value={timeLeft?.seconds} label={t('seconds')} />
    </div>
  );
}

function TimeBox({ value, label }: { value?: number; label: string }) {
  return (
    <div className="flex flex-col items-center">
      <div className="bg-store-surface bg-opacity-70 p-1 rounded-lg min-w-[30px] font-extrabold text-rose-600">
        {value === undefined ? "--" : value.toString().padStart(2, "0")}
      </div>
      <span className="opacity-80 mt-1 text-xs">{label}</span>
    </div>
  );
}

function calculateTimeLeft(target: number) {
  const now = new Date().getTime();
  const diff = Math.max(0, target - now);
  return {
    hours: Math.floor((diff / (1000 * 60 * 60)) % 24),
    minutes: Math.floor((diff / (1000 * 60)) % 60),
    seconds: Math.floor((diff / 1000) % 60),
  };
}
