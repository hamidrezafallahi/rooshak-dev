'use client';
import * as React from 'react';
import { createPortal } from 'react-dom';

import { useTranslations } from 'next-intl';
import { useRenderPosition } from 'react-persian-range-picker';

import { cn } from '@/lib/utils';

export interface SelectProps {
  options: { label: string; value: string | number }[];
  placeholder?: string;
  className?: string;
  dropdownClassName?: string;
  maxHeight?: string;
  value?: string | number;
  onChange?: ( value: string | number ) => void;
  ref?: React.RefCallback<any>;
  onPageChange?: () => void;
}
const Select = React.forwardRef<HTMLDivElement, SelectProps>(
  (
    {
      className,
      placeholder,
      dropdownClassName,
      maxHeight = "200px",
      options,
      value,
      onChange,
      onPageChange,
      ...props
    },
    ref,
  ) => {
    const t = useTranslations();
    const [isOpen, setIsOpen] = React.useState(false);
    const [selectedValue, setSelectedValue] = React.useState(value);
    const buttonRef = React.useRef<HTMLElement>(null);
    const popupRef = React.useRef<HTMLDivElement>(null);

    const selectedOption = React.useMemo(
      () => options?.find((opt) => opt.value === selectedValue),
      [options, selectedValue],
    );
    useRenderPosition({
      buttonRef: buttonRef as React.RefObject<HTMLElement>,
      popupRef: popupRef,
      setIsOpen: setIsOpen,
      isOpen: isOpen,
    });
    const handleScroll = React.useCallback(
      (e: React.UIEvent<HTMLDivElement>) => {
        const target = e.currentTarget;
        if (target.clientHeight + target.scrollTop >= target.scrollHeight) {
          onPageChange?.();
        }
      },
      [onPageChange],
    );
    const displayText =
      selectedOption?.label || placeholder || t('common.pleaseSelect');

    return (
      <div
        className={cn("relative w-full", className)}
        ref={ref}
        {...props}
      >
        <button
          ref={buttonRef as React.RefObject<HTMLButtonElement>}
          type="button"
          className="flex justify-between items-center bg-white hover:bg-gray-50 px-3 py-2 border border-input rounded-md focus-visible:ring-2 focus-visible:ring-ring w-full h-10 text-sm transition-colors cursor-pointer"
          onClick={() => setIsOpen(!isOpen)}
          tabIndex={0}
          onKeyDown={(e) => e.key === "Enter" && setIsOpen(!isOpen)}
        >
          <span>{displayText}</span>
          <svg
            className={`w-4 h-4 transition-transform ${isOpen ? "rotate-180" : ""}`}
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M19 9l-7 7-7-7"
            />
          </svg>
        </button>

        {isOpen &&
          createPortal(
            <div
              ref={popupRef}
              style={{
                position: "absolute",
                overflowY: "scroll",
                overflowX: "hidden",
                zIndex: 1050,
                background: "#fff",
                maxHeight,
                width: buttonRef.current?.clientWidth,
              }}
              className={cn(
                "bg-white shadow-lg mt-1 border border-input rounded-md w-fit",
                dropdownClassName,
              )}
              onScroll={handleScroll}
            >
              {options.map((opt) => (
                <div
                  key={String(opt.value)}
                  className="hover:bg-gray-100 px-3 py-2 text-sm transition-colors cursor-pointer"
                  onClick={(e) => {
                    setSelectedValue(opt.value);
                    onChange?.(opt.value);
                   
                    setIsOpen(false);
                  }}
                >
                  {opt.label}
                </div>
              ))}
            </div>,

            document.body,
          )}
      </div>
    );
  },
);

Select.displayName = "Select";
export { Select };
