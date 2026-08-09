'use client';

import { useTranslations } from 'next-intl';

import type { IIconConfig } from '../type';

export const RialIcon = ({ config }: { config?: IIconConfig }) => {
  const t = useTranslations();
  const size = config?.size ?? 26;
  const strokeWidth = config?.strokeWidth ?? 1;
  const stroke = config?.stroke ?? "currentColor";
  const fill = config?.fill ?? "none";
  const className = config?.className ?? "";

  return (
    <svg
      xmlns="http://www.w3.org/2000/svg"
      width={size}
      height={size}
      fill={fill}
      viewBox="0 0 28 28"
      strokeWidth={strokeWidth}
      stroke={stroke}
 
    >
      <circle
        cx="14"
        cy="14"
        r="13"
        fill={fill}
        stroke={stroke}
        strokeWidth="1.2"
              className={`stroke-primary ${className}`}
      />

      <circle
        cx="14"
        cy="14"
        r="11.5"
        fill={fill}
        stroke={stroke}
        strokeWidth="0.3"
              className="stroke-primary"
      />

      <text
        x="14"
        y="14"
        textAnchor="middle"
        dominantBaseline="middle"
        // font-family="IRANSans, Vazirmatn, Tahoma"
        fontSize={12}
        fontWeight={200}
        // fontStyle={"italic"}
        className="font-iranSansLight italic"
        fontFamily={"iranSansLight"}
        // style={{fontStretch:"expanded",fontFamily:"serif"}}
      >
        {t('common.rial')}
      </text>
    </svg>
  );
};
