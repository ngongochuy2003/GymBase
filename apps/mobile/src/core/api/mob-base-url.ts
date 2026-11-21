import { resolveApiOrigin } from '../config/api-origin';

/** Địa chỉ gốc của API mobile: `<origin>/api/v1/mob`. */
export function resolveMobApiBaseUrl(): string {
  return `${resolveApiOrigin()}/api/v1/mob`;
}
