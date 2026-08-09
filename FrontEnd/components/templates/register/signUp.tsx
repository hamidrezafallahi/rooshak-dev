"use client";
import React, {
  useCallback,
  useState,
} from 'react';

import { useTranslations } from 'next-intl';

import { Button } from '@components/atoms/defaultElements/customButton';
import { Input } from '@components/atoms/defaultElements/customInput';
import { Label } from '@components/atoms/defaultElements/label';
import Uploader from '@components/atoms/defaultElements/uploader';
import { apiBaseUrl } from '@lib/api';
import { cn } from '@lib/utils';
import {
  showErrorToast,
  showSuccessToast,
} from '@utils/core';

import {
  IProps,
  ISignup,
} from './type';

export function SignUpForm({ className, setIsLogin, ...props }: IProps) {
  const [signup, setSignup] = useState<ISignup>({
    email: "",
    fullName: "",
    password: "",
    phoneNumber: "",
    image: null,
  });

  const [errors, setErrors] = useState<Record<string, string>>({});
  const t = useTranslations();
  const validateField = useCallback(
    (field: string, value: string) => {
      let fieldError = "";

      switch (field) {
        case "fullName":
          if (!value.trim()) fieldError = t("register.fullNameRequired");
          else if (value.trim().length < 2)
            fieldError = t("register.minTwoChars");
          break;

        case "email":
          const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
          if (!value.trim()) fieldError = t("register.emailRequired");
          else if (!emailRegex.test(value))
            fieldError = t("register.invalidEmail");
          break;

        case "phoneNumber":
          const phoneRegex = /^09[0-9]{9}$/;
          if (!value.trim()) fieldError = t("register.phoneRequired");
          else if (!phoneRegex.test(value))
            fieldError = t("register.invalidPhone");
          break;

        case "password":
          if (!value.trim()) fieldError = t("register.passwordRequired");
          else if (value.length < 8)
            fieldError = t("register.passwordMinLength");
          break;
      }

      setErrors((prev) => {
        const newErrors = { ...prev, [field]: fieldError };
        console.log("🔥 SET ERRORS:", fieldError, newErrors);
        return newErrors;
      });
    },
    [t],
  );
  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setSignup((prev) => {
      const newSignup = { ...prev, [name]: value };
      return newSignup;
    });
    validateField(name, value);
  };
  const validateForm = useCallback(() => {
    const newErrors: Record<string, string> = {};

    if (!signup.fullName.trim())
      newErrors.fullName = t("register.fullNameRequired");
    else if (signup.fullName.trim().length < 2)
      newErrors.fullName = t("register.minTwoCharsShort");

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!signup.email.trim()) newErrors.email = t("register.emailRequired");
    else if (!emailRegex.test(signup.email))
      newErrors.email = t("register.invalidEmail");

    const phoneRegex = /^09[0-9]{9}$/;
    if (!signup.phoneNumber.trim())
      newErrors.phoneNumber = t("register.phoneRequired");
    else if (!phoneRegex.test(signup.phoneNumber))
      newErrors.phoneNumber = t("register.invalidPhone");

    if (!signup.password.trim())
      newErrors.password = t("register.passwordRequired");
    else if (signup.password.length < 8)
      newErrors.password = t("register.passwordMinLength");

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  }, [signup, t]);

const handleRegister = async () => {
  if (!validateForm()) return;

  const formData = new FormData();
  if (signup.image) formData.append("UserImageFile", signup.image);
  formData.append("Email", signup.email);
  formData.append("FullName", signup.fullName);
  formData.append("Password", signup.password);
  formData.append("PhoneNumber", signup.phoneNumber);

  try {
    // ✅ Fetch دستی - بدون RTK Query
    const response = await fetch(`${apiBaseUrl}/Identity/Register`, {
      method: 'POST',
      body: formData,  // browser خودش Content-Type: multipart/form-data set می‌کنه
    });

    const res = await response.json();
    
    if (res.isSuccess) {
      showSuccessToast(t("register.success"));
      setIsLogin(true);
    } else {
      showErrorToast(res.error || t("register.registerError"));
    }
  } catch (error) {
    console.error(error);
    showErrorToast(t("register.connectionError"));
  }
};

  const isFormValid =
    Object.values(errors).every((error) => !error) &&
    signup.fullName.trim() &&
    signup.email.trim() &&
    signup.phoneNumber.trim() &&
    signup.password.trim();
  return (
    <div className={cn("...", className)} {...props}>
      <form
        className="space-y-6 w-full"
        onSubmit={(e) => {
          e.preventDefault();
          handleRegister();
        }}
      >
        {/* Uploader */}
        <div className="bg-red-500 mx-auto rounded-full w-20 h-20 overflow-hidden">
          <Uploader
            className="!min-h-0 !h-full border-0 rounded-full"
            onChange={(file) => setSignup((prev) => ({ ...prev, image: file }))}
          />
        </div>
        {/* FullName */}
        <div>
          <Label htmlFor="fullName">{t("register.fullName")}</Label>
          <Input
            id="fullName"
            name="fullName"
            value={signup.fullName}
            onChange={handleChange}
            className={cn(
              "w-full ...",
              errors.fullName &&
                "border-red-500 focus:border-red-500 ring-1 ring-red-200",
            )}
          />
          {errors.fullName && (
            <p className="mt-1 text-red-600 text-sm">{errors.fullName}</p>
          )}
        </div>

        {/* Email */}
        <div>
          <Label htmlFor="email">{t("register.email")}</Label>
          <Input
            id="email"
            name="email"
            value={signup.email}
            onChange={handleChange}
            className={cn(
              "w-full ...",
              errors.email &&
                "border-red-500 focus:border-red-500 ring-1 ring-red-200",
            )}
          />
          {errors.email && (
            <p className="mt-1 text-red-600 text-sm">{errors.email}</p>
          )}
        </div>

        {/* Phone */}
        <div>
          <Label htmlFor="phoneNumber">{t("register.phoneNumber")}</Label>
          <Input
            id="phoneNumber"
            name="phoneNumber"
            value={signup.phoneNumber}
            onChange={handleChange}
            className={cn(
              "w-full ...",
              errors.phoneNumber &&
                "border-red-500 focus:border-red-500 ring-1 ring-red-200",
            )}
          />
          {errors.phoneNumber && (
            <p className="mt-1 text-red-600 text-sm">{errors.phoneNumber}</p>
          )}
        </div>

        {/* Password */}
        <div>
          <Label htmlFor="password">{t("register.password")}</Label>
          <Input
            id="password"
            type="password"
            name="password"
            value={signup.password}
            onChange={handleChange}
            className={cn(
              "w-full ...",
              errors.password &&
                "border-red-500 focus:border-red-500 ring-1 ring-red-200",
            )}
          />
          {errors.password && (
            <p className="mt-1 text-red-600 text-sm">{errors.password}</p>
          )}
        </div>

        <Button
  type="submit"
  disabled={!isFormValid }
  className="bg-primary disabled:bg-gray-400 w-full text-white"
>
  {t("register.register")}
</Button>
      </form>
    </div>
  );
}
