import {
  Slide,
  toast,
} from 'react-toastify';

import ToastBox from '@components/molecules/toastBox';

export const createCookie = (
  cookieName: string,
  cookieValue: string,
  hourToExpire?: number,
  infinite?: boolean
) => {
  const date = new Date();
  const defaultHour = hourToExpire ?? 8;
  date.setTime(date.getTime() + defaultHour * 60 * 60 * 1000);
  if (infinite) {
    document.cookie = `${cookieName}=${cookieValue};`;
  } else {
    document.cookie = `${cookieName}=${cookieValue}; expires=${date.toUTCString()}; path=/;`;
  }
};
export const formatCurrency = (value: number | string): string => {
  const numericValue = typeof value === "string" ? parseFloat(value) : value;

  return new Intl.NumberFormat("fa-IR", {
    minimumFractionDigits: 0,
  }).format(numericValue);
};
export const toman = (rial: number) => Math.round(rial / 10);
export const getTokens = (Name: string): { val: string; valid: boolean } => {
  const cookies = document.cookie.split(";");
  const cookie = cookies.find((x) => x.includes(`${Name}=`));
  if (cookie) {
    const result = cookie.split("=");
    if (result[1] !== "") {
      return { val: result[1], valid: true };
    } else {
      return { val: "", valid: false };
    }
  }
  return { val: "", valid: false };
};
export const getCookie = (Name: string): string => {
  // Storefront components are server-rendered too; there is no cookie jar there.
  if (typeof document === "undefined") return "";
  const cookies = document.cookie.split(";");
  const cookie = cookies.find((x) => x.includes(`${Name}=`));
  if (cookie) {
    const result = cookie.split("=");
    if (result[1] !== "") {
      return result[1];
    } else {
      return "";
    }
  }
  return "";
};
export const deleteCookie = (name: string) => {
  const cookies = document.cookie.split(";");
  const cookie = cookies.find((x) => x.includes(`${name}=`));
  if (cookie) {
    const cookieName = cookie.split("=")[0];
    document.cookie = `${cookieName}=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/`;
  }
};
export const showErrorToast = (message:string, title = "", time = 0) => {
  return toast(<ToastBox message={message} title={title} />, {
    className: "!rounded-2xl border-state-error7 border-[0.5px] !shadow-toast",
    position: "top-right",
    autoClose: time,
    hideProgressBar: true,
    closeOnClick: false,
    pauseOnHover: false,
    draggable: true,
    progress: undefined,
    theme: "light",
    transition: Slide,
  });
};
export const showSuccessToast = (message:string, title = "", time = 0) => {
  return toast(<ToastBox message={message} title={title} />, {
    className: "!rounded-2xl border-green-900 border-[0.5px] !shadow-toast",
    position: "top-right",
    autoClose: time,
    hideProgressBar: true,
    closeOnClick: false,
    pauseOnHover: false,
    draggable: true,
    progress: undefined,
    theme: "light",
    transition: Slide,
  });
};
