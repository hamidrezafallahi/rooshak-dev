"use client";

import React, { useState } from 'react';

import { useTranslations } from 'next-intl';
import {
  useRouter,
  useSearchParams,
} from 'next/navigation';
import {
  shallowEqual,
  useDispatch,
} from 'react-redux';

import { Button } from '@components/atoms/defaultElements/customButton';
import { Checkbox } from '@components/atoms/defaultElements/customCheckbox';
import { Input } from '@components/atoms/defaultElements/customInput';
import { Label } from '@components/atoms/defaultElements/label';
import { browserAuthBaseUrl } from '@lib/api';
import { cn } from '@lib/utils';
import { SynchronousResponse } from '@models/product';
import { useGetConditionallyMutation } from '@services/base';
import { IBaseQueryResponse } from '@services/base/type';
import { synchronousCart } from '@slice/shoppingCartSlice';
import { useAppSelector } from '@store/index';
import { showErrorToast } from '@utils/core';

import {
  ILogin,
  ILoginResponse,
  IProps,
} from './type';

export function LoginForm({
  className,
  setIsLogin,
  ...props
}: IProps) {
  const [login, setLogin] = useState<ILogin>({
    EmailOrPhone: "",
    Password: "",
  });

  const [isLoading, setIsLoading] = useState(false);

  const t = useTranslations();

  const { ShoppingCart, locale } = useAppSelector(
    (state) => ({
      ShoppingCart: state.withPersist.ShoppingCart,
      locale: state.withPersist.config.locale,
    }),
    shallowEqual,
  );

  const route = useRouter();

  const [syncCart] = useGetConditionallyMutation();

  const dispatch = useDispatch();

  const searchParams = useSearchParams();

  const redirectUrl =
    searchParams.get("redirect") || "/";

  const handleLogin = async () => {
    try {
       setIsLoading(true);
      const res = await fetch(`${browserAuthBaseUrl}/login`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
        body: JSON.stringify(login),
      });


      const data: IBaseQueryResponse<ILoginResponse> =
        await res.json();

      if (!data.isSuccess) {
        showErrorToast(data.error);
        return;
      }

      const role = data.data.role ?? 'Customer';

      const syncCartResponse: IBaseQueryResponse<SynchronousResponse> =
        await syncCart({
          url:`/Carts/sync`,
          body: {
            clientItems: ShoppingCart?.products?.length
              ? ShoppingCart.products.map((i) => ({
                  productId: i.id,
                  productOfferId: i.productOfferId,
                  quantity: i.quantity,
                }))
              : undefined,
          },
        }).unwrap();

      if (
        syncCartResponse.isSuccess &&
        syncCartResponse.data.items.length > 0
      ) {
        dispatch(
          synchronousCart(syncCartResponse.data),
        );
      }

      if (role === "Customer") {
        route.push(
          decodeURIComponent(redirectUrl),
        );
      } else {
        route.replace(`/${locale}/admin`);
      }
    } catch (error) {
      console.error("Login error:", error);

      showErrorToast(
        typeof error === "string"
          ? error
          : "Login failed",
      );
    } finally {
      setIsLoading(false);
    }
  };

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement>,
  ) => {
    const { name, value } = e.target;

    setLogin((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  return (
    <div
      className={cn(
        "bg-store-surface shadow-sm p-4 sm:p-6 md:p-8 border border-store-border rounded-lg w-full max-w-sm",
        className,
      )}
      {...props}
    >
      <form
        className="space-y-6"
        onSubmit={(e) => {
          e.preventDefault();
          handleLogin();
        }}
      >
        <h5 className="font-medium text-store-text dark:text-white text-xl">
          {t("register.enterEmail")}
        </h5>

        <div>
          <Label
            htmlFor="email"
            className="block mb-2 font-medium text-store-text text-sm  "
          >
            {t("register.email")}
          </Label>

          <Input
            id="email"
            name="EmailOrPhone"
            placeholder={t(
              "register.emailPlaceHolder",
            )}
            onChange={handleChange}
            required
            className="block bg-store-muted p-2.5 border border-store-border focus:border-store-strong rounded-lg w-full text-store-text text-sm"
          />
        </div>

        <div>
          <Label
            htmlFor="password"
            className="block mb-2 font-medium text-store-text dark:text-white text-sm"
          >
            {t("register.password")}
          </Label>

          <Input
            id="password"
            type="password"
            placeholder="••••••••"
            name="Password"
            onChange={handleChange}
            required
            className="block bg-store-muted p-2.5 border border-store-border focus:border-store-strong rounded-lg w-full text-store-text text-sm"
          />
        </div>

        <div className="flex flex-col justify-between items-start gap-4">
          <div className="flex items-center">
            <Checkbox
              id="remember"
              className="text-store-text"
            />

            <Label
              htmlFor="remember"
              className="ms-2 font-medium text-store-text dark:text-gray-300 text-sm"
            >
              {t("register.rememberMe")}
            </Label>
          </div>

          <a
            href="#"
            className="text-store-text dark:text-primary text-sm hover:underline"
          >
            {t("register.forgotPassword")}
          </a>
        </div>

        <Button
          type="submit"
          disabled={isLoading}
          className="bg-primary hover:bg-primary/90 disabled:opacity-50 px-5 py-2.5 rounded-lg w-full font-medium text-primary-foreground text-sm text-center"
        >
          {isLoading
            ? t("common.loading")
            : t("register.enter")}
        </Button>

        <div className="font-medium text-store-subtle dark:text-gray-300 text-sm text-center">
          {t("register.dotHaveAnyAccount")}{" "}
          <button
            type="button"
            onClick={() => setIsLogin(false)}
            className="text-store-text dark:text-primary hover:underline"
          >
            {t("register.signUp")}
          </button>
        </div>
      </form>
    </div>
  );
}
