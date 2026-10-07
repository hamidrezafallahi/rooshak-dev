'use client';

import React, { useState } from 'react';

import { useLocale, useTranslations } from 'next-intl';
import Link from 'next/link';

import HeroMedia from '@components/organisms/heroMedia';

import { LoginForm } from './login';
import { SignUpForm } from './signUp';

type Props = {
  imageSrc: string;
  videoSrc?: string;
  mediaAlt: string;
  playLabel: string;
  pauseLabel: string;
};

/**
 * Standalone sign-in / sign-up screen (no header/footer). On desktop it is split in
 * two halves: hero media on one side, the form on the other; switching between the
 * two forms swaps the sides.
 */
function Register({ imageSrc, videoSrc, mediaAlt, playLabel, pauseLabel }: Props) {
  const [isLogin, setIsLogin] = useState(true);
  const t = useTranslations('register');
  const tHeader = useTranslations('header');
  const tBrand = useTranslations('brand');
  const locale = useLocale();

  return (
    <div className="lg:relative lg:overflow-hidden min-h-screen">
      {/* Desktop: both halves are absolutely placed and slide past each other. */}
      <aside
        className={`hidden lg:block lg:absolute lg:inset-y-0 lg:start-0 lg:w-1/2 bg-black overflow-hidden lg:transition-transform lg:duration-[800ms] lg:ease-[cubic-bezier(0.65,0,0.35,1)] ${
          isLogin ? '' : 'ltr:lg:translate-x-full rtl:lg:-translate-x-full'
        }`}
      >
        <HeroMedia
          desktop={{ image: imageSrc, video: videoSrc }}
          alt={mediaAlt}
          playLabel={playLabel}
          pauseLabel={pauseLabel}
        />
        <Link
          href={`/${locale}`}
          className="top-8 start-8 z-10 absolute text-2xl text-white ltr:uppercase ltr:tracking-[0.3em]"
        >
          {tBrand('name')}
        </Link>
      </aside>

      <main
        className={`flex flex-col justify-center items-center gap-6 px-4 sm:px-10 py-10 min-h-screen lg:absolute lg:inset-y-0 lg:end-0 lg:w-1/2 lg:min-h-0 lg:transition-transform lg:duration-[800ms] lg:ease-[cubic-bezier(0.65,0,0.35,1)] ${
          isLogin ? '' : 'ltr:lg:-translate-x-full rtl:lg:translate-x-full'
        }`}
      >
        <Link
          href={`/${locale}`}
          className="lg:hidden text-2xl ltr:uppercase ltr:tracking-[0.3em]"
        >
          {tBrand('name')}
        </Link>

        <div
          key={isLogin ? 'login' : 'signup'}
          className="flex flex-col items-center gap-6 w-full max-w-md text-center animate-formIn"
        >
          <h1 className="font-normal text-3xl sm:text-4xl">
            {isLogin ? t('enter') : t('signUp')}
          </h1>
          {isLogin ? (
            <LoginForm
              setIsLogin={setIsLogin}
              className="shadow-none p-0 border-0 max-w-none"
            />
          ) : (
            <SignUpForm
              setIsLogin={setIsLogin}
              className="shadow-none p-0 border-0 max-w-none"
            />
          )}
          <Link
            href={`/${locale}`}
            className="text-store-subtle hover:text-store-text text-sm transition-colors"
          >
            {tHeader('landing page')}
          </Link>
        </div>
      </main>
    </div>
  );
}

export default Register;
