import { jwtDecode } from 'jwt-decode';
import { NextResponse } from 'next/server';

import { serverApiBaseUrl } from '@lib/api';
import {
  ACCESS_COOKIE,
  REFRESH_COOKIE,
  SESSION_FLAG_COOKIE,
  accessCookieOptions,
  refreshCookieOptions,
  sessionFlagCookieOptions,
} from '@lib/auth-cookies';
import { clientIpHeaders } from '@lib/client-ip';
import type { TokenPayload } from '@components/templates/register/type';
import { describeStatus, errorTextOf } from '@lib/apiError';

export async function POST(req: Request) {
  try {
    const body = await req.json();
console.log("http://localhost:8080/api/Identity/login",`${serverApiBaseUrl}/Identity/login`,"============================================================================")
    const response = await fetch(`${serverApiBaseUrl}/Identity/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        ...clientIpHeaders(req.headers),
      },
      body: JSON.stringify(body),
    });

    const raw = await response.text();
    let data: {
      isSuccess?: boolean;
      error?: string;
      data?: {
        accessToken?: string;
        refreshToken?: string;
      };
    };

    try {
      data = JSON.parse(raw) as typeof data;
    } catch {
      console.error(
        `Login upstream non-JSON (${response.status}) from ${serverApiBaseUrl}/Identity/login`,
        raw.slice(0, 300),
      );
      return NextResponse.json(
        {
          isSuccess: false,
          error: describeStatus(response.ok ? 'PARSING_ERROR' : response.status),
        },
        { status: 502 },
      );
    }

    if (!response.ok || !data?.isSuccess || !data.data?.accessToken) {
      return NextResponse.json(
        data?.isSuccess === false
          ? data
          : {
              isSuccess: false,
              error: errorTextOf(data) ?? describeStatus(response.status),
            },
        { status: response.ok ? 400 : response.status }
      );
    }

    const { accessToken, refreshToken } = data.data;
    let role = 'Customer';
    try {
      const decoded = jwtDecode<TokenPayload>(accessToken);
      if (decoded?.role) {
        role = decoded.role;
      }
    } catch {
      // keep default role
    }

    // Never expose tokens to the browser JS — only httpOnly cookies + role for UX.
    const res = NextResponse.json({
      isSuccess: true,
      data: {
        role,
      },
    });

    res.cookies.set(ACCESS_COOKIE, accessToken, accessCookieOptions);
    if (refreshToken) {
      res.cookies.set(REFRESH_COOKIE, refreshToken, refreshCookieOptions);
    }
    res.cookies.set(SESSION_FLAG_COOKIE, '1', sessionFlagCookieOptions);

    return res;
  } catch (error) {
    console.error('Login: no answer from the backend', error);
    return NextResponse.json(
      { isSuccess: false, error: describeStatus(503) },
      { status: 502 }
    );
  }
}
