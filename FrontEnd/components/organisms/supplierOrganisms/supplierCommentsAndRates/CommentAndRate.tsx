"use client";
import React, {
  useEffect,
  useRef,
  useState,
} from 'react';

import {
  useLocale,
  useTranslations,
} from 'next-intl';
import Link from 'next/link';
import {
  usePathname,
} from 'next/navigation';

import { Button } from '@components/atoms/defaultElements/customButton';
import { Rate } from '@components/atoms/defaultElements/customRate';
import { Textarea } from '@components/atoms/defaultElements/customTextarea';
import { SpinnerIcon } from '@components/atoms/iconComponents';
import { EnumTargetType } from '@models/comment';
import { useGetConditionallyMutation } from '@services/base';
import {
  getCookie,
  showErrorToast,
} from '@utils/core';

interface TCommentAndRate {
  TargetType: EnumTargetType;
  TargetId: string;
}
export default function CommentAndRate({ ...props }: TCommentAndRate) {
  const { TargetId, TargetType } = props;
  const pathname = usePathname();
  const locale = useLocale();
  const [value, setValue] = useState(0);
  const [isAuthenticated, setIsAuthenticated] = useState<boolean | null>(null);
  const t = useTranslations();
  const [mutateRate] = useGetConditionallyMutation();
  const [mutateCommand, { isLoading }] = useGetConditionallyMutation();
  const textAreaRef = useRef<HTMLTextAreaElement>(null);
  const handleSetRate = async (e: number) => {
    const res = await mutateRate({
      url: "/Rates",
      body: {
        targetId: TargetId,
        targetType: TargetType,
        value: e,
      },
      method: "POST",
    });
    if (res) {
      console.log(res);
    }
    setValue(e);
  };

  const handleSubmitComment = async () => {
    const comment = textAreaRef.current?.value.trim();
    if (comment?.length === 0) {
      showErrorToast(t('product.pleaseEnterComment'));
      textAreaRef.current?.focus();
      return;
    } else {
      const res = await mutateCommand({
        url: "/Comments",
        body: {
          targetId: TargetId,
          targetType: TargetType,
          content: comment,
        },
        method: "POST",
      });
      if (res.data) {
        console.log(res);
      }
    }
  };
  const redirectUrl = encodeURIComponent(pathname);
  useEffect(() => {
    setIsAuthenticated(Boolean(getCookie("candySession")));
  }, []);
  return (
    <div className="flex flex-col gap-2 bg-store-muted hover:shadow-md p-6 rounded-2xl w-5/6 md:w-1/2 text-right transition">
      {isAuthenticated ? (
        <>
          <div className="flex justify-between w-full">
            <div>{t("comments.yourRate")}</div>
            <Rate mode="rate" value={value} onChange={handleSetRate} />
          </div>
          <Textarea
            ref={textAreaRef}
            className="p-3 border rounded-xl w-full text-sm resize-none"
            placeholder={t('product.writeCommentPlaceholder')}
          />
          <Button
            onClick={handleSubmitComment}
            disabled={isLoading}
            className="bg-primary mt-2 px-6 py-2 rounded-xl text-primary-foreground"
          >
            {isLoading ? <SpinnerIcon /> : <span> {t("general.save")}</span>}
          </Button>
        </>
      ) : (
        <Link
          className="bg-primary p-2 px-3 rounded-lg text-primary-foreground"
          href={`/${locale}/register?redirect=${redirectUrl}`}
        >
          {t("general.loginFirst")}
        </Link>
      )}
    </div>
  );
}
