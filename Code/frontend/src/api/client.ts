const BASE_URL = import.meta.env.VITE_API_URL ?? '/api';

export class ApiError extends Error {
  status: number;
  code: string | null;

  constructor(status: number, code: string | null, rawBody: string) {
    super(`HTTP ${status}: ${rawBody}`);
    this.status = status;
    this.code = code;
  }
}

export async function apiFetch<T>(
  path: string,
  token: string | null,
  options?: RequestInit,
): Promise<T> {
  const headers: HeadersInit = { 'Content-Type': 'application/json' };
  if (token) headers['Authorization'] = `Bearer ${token}`;

  const res = await fetch(`${BASE_URL}${path}`, { ...options, headers });

  if (!res.ok) {
    const body = await res.text();
    let code: string | null = null;
    try {
      const json = JSON.parse(body);
      code = typeof json.error === 'string' ? json.error : null;
    } catch {}
    throw new ApiError(res.status, code, body);
  }

  const text = await res.text();
  return (text ? JSON.parse(text) : undefined) as T;
}
