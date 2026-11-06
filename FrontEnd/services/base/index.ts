import { useEffect } from 'react';

import { createApi } from '@reduxjs/toolkit/query/react';
import { baseQueryByToken } from '@services/customBaseQuery';
import { showErrorToast } from '@utils/core';

import { endpoints } from './endpoints';
import type {
  IBaseQueryResponse,
  IBaseRequest,
  IService,
} from './type';

export const apiService = createApi({
  reducerPath: "apiService",
  baseQuery: baseQueryByToken,
  tagTypes: endpoints.map((endpoint) => endpoint.toLowerCase()),
  endpoints: (builder) => ({
    getInfinite: builder.query<
      IBaseQueryResponse<any>,
      { url: string; body?: IBaseRequest<any>; id: number | string }
    >({
      query: ({ url, body, id }) => ({
        url,
        method: "POST",
        body,
        id,
      }),

      serializeQueryArgs: ({ queryArgs }) => {
        return queryArgs.url;
      },
      merge: (currentCache, newItems) => {
        if (newItems.data.length === 0) return;

        if (currentCache.data && newItems.data) {
          currentCache.data.push(...newItems.data);
        }
      },


      providesTags: (result, error, { url, id }) => {
        const parts = url.split("/");
        const operationIndex = parts?.findIndex(
          (el) => el.toLowerCase() === "select"
        );
        const endpoint =
          parts?.[operationIndex + 1]?.toLowerCase() || "unknown";
        const tag = endpoint.split("4", 1)[0];
        return [{ type: tag, id: id ? id : "All" }];
      },
    }),

    get: builder.query<
      IBaseQueryResponse<any>,
      { url: string; body?: IBaseRequest<any>; id?: number | string; method?: "GET"|"POST"|"DELETE"|"PUT" }
    >({
      query: ({ url, body, method = "GET", id }) => ({
        url,
        method,
        body,
      }),

      providesTags: (result, error, { url, id }) => {
        const parts = url.split("/");

        const operationIndex = parts?.findIndex(
          (el) => el.toLowerCase() === "select"
        );
        const endpoint =
          parts?.[operationIndex + 1]?.toLowerCase() || "unknown";
        const tag = endpoint.split("4", 1)[0];
        return [{ type: tag, id: id ? id : "All" }];
      },
    }),

    getConditionally: builder.mutation<
      IBaseQueryResponse<any>,
      { url: string; body?: IBaseRequest<any>; id?: number | string; method?: "GET"|"POST"|"DELETE"|"PUT" }
    >({
      query: ({ url, body, method = "POST" }) => ({
        url,
        method,
        body,
      }),
    }),

    CUDData: builder.mutation<
      IBaseQueryResponse<any>,
      { url: string; body?: any; id?: number | string ;method?: "GET"|"POST"|"DELETE"|"PUT"}
    >({
      query: ({ url, body,method="POST", id }) => ({
        url,
        method,
        body,
        id: id ? id : "All",
      }),

      invalidatesTags: (result, error, { url, id }) => {
        if (id !== "LIST") {
          if (result?.isSuccess) {
            const parts = url.split("/");
            const operationIndex = parts?.findIndex(
              (el) =>
                el.toLowerCase() === "insert" ||
                el.toLowerCase() === "update" ||
                el.toLowerCase() === "delete"
            );
            const endpoint =
              parts?.[operationIndex + 1]?.toLowerCase() || "unknown";

            const tag = endpoint.split("4", 1)[0];

            return [{ type: tag, id: id ? id : "All" }];
          }
          if (result?.isSuccess) {
            const parts = url.split("/");
            const operationIndex = parts?.findIndex(
              (el) =>
                el.toLowerCase() === "insert" ||
                el.toLowerCase() === "update" ||
                el.toLowerCase() === "delete"
            );
            const endpoint =
              parts?.[operationIndex + 1]?.toLowerCase() || "unknown";

            const tag = endpoint.split("4", 1)[0];

            return [{ type: tag, id: id ? id : "All" }];
          }
        }

        return [];
      },
    }),
    CUDFormData: builder.mutation<
      IBaseQueryResponse<any>,
      { url: string; body: any; id?: number | string }
    >({
      query: ({ url, body, id }) => ({
        url,
        method: "POST",
        body,
        id: id ? id : "All",
      }),

      invalidatesTags: (result, error, { url, id }) => {
        if (result?.isSuccess) {
          const parts = url.split("/");
          const operationIndex = parts?.findIndex(
            (el) =>
              el.toLowerCase() === "insert" ||
              el.toLowerCase() === "update" ||
              el.toLowerCase() === "delete"
          );
          const endpoint =
            parts?.[operationIndex + 1]?.toLowerCase() || "unknown";
          const tag = endpoint.split("4", 1)[0];

          return [{ type: tag, id: id ? id : "All" }];
        }

        return [];
      },
    }),
  }),
});

export const {
  useGetQuery,
  useCUDDataMutation,
  useGetInfiniteQuery,
  useCUDFormDataMutation,
  useGetConditionallyMutation,
} = apiService;

export const useGetInfinite = <T, K = unknown>({
  url,
  body,
  id = "All",
  skip = false,
}: IService<T, K>) => {
  const { data, ...rest } = useGetInfiniteQuery(
    {
      url,
      body,
      id,
    },
    {
      skip,
      refetchOnMountOrArgChange: true,
      selectFromResult: (result) => ({
        ...result,
        data: result.data as IBaseQueryResponse<T> | undefined,
      }),
    }
  );

  return {
    data,
    ...rest,
  };
};

export const useGetData = <T, K = unknown>({
  url,
  body,
  method="GET",
  id = "All",
  skip = false,
  reportFailure = false,
}: IService<T, K>) => {
  const { data, ...rest } = useGetQuery(
    {
      url,
      body,
      method,
      id,
    },
    {
      skip,
      selectFromResult: (result) => ({
        ...result,
        data: result.data as IBaseQueryResponse<T> | undefined,
      }),
    }
  );

  const failure = reportFailure && data?.isSuccess === false ? data.error : null;
  useEffect(() => {
    if (failure) showErrorToast(failure, '', 6000);
  }, [failure]);

  return {
    data,
    ...rest,
  };
};
