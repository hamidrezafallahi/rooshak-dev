/**
 * The backend rate-limits per client IP, but every call from this server comes from
 * the frontend container. Calls made for one visitor's request (BFF proxy, login,
 * token refresh, admin renders) pass that visitor's IP on as X-Forwarded-For; the
 * backend believes it only from the Docker network.
 *
 * X-Real-IP comes from nginx, which sets it to the connecting address and replaces
 * anything the browser sent; in production nothing but nginx can reach this server.
 * (Not X-Forwarded-For: its first entries are whatever the browser sent.) Without it
 * (local dev) nothing is forwarded and the backend counts the call as internal.
 *
 * Storefront ISR fetches must not use this: reading the request headers makes the
 * page dynamic, and a per-visitor header would split Next's fetch cache per IP.
 */
export function clientIpHeaders(
  incoming: Pick<Headers, 'get'>,
): Record<string, string> {
  const ip = incoming.get('x-real-ip')?.trim();
  return ip ? { 'X-Forwarded-For': ip } : {};
}
