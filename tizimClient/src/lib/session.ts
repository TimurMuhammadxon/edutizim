import axios from "axios";
import { useAuthStore } from "@/store/auth";
import { decodeJwt } from "@/lib/jwt";

// Bare axios (no interceptors) to avoid recursion during refresh. withCredentials so the
// httpOnly refresh-token cookie is sent.
const bare = axios.create({ baseURL: "/api", timeout: 20000, withCredentials: true });

interface Tokens {
  accessToken: string;
}

// Time before real expiry at which we treat the access token as stale (ms).
const EXPIRY_SKEW_MS = 10_000;

function getInitData(): string | null {
  return window.Telegram?.WebApp?.initData || null;
}

function accessTokenValid(): string | null {
  const token = useAuthStore.getState().accessToken;
  if (!token) return null;
  const payload = decodeJwt(token);
  if (!payload) return null;
  return payload.exp * 1000 > Date.now() + EXPIRY_SKEW_MS ? token : null;
}

// Recover a session: try the refresh-token cookie, then fall back to Telegram initData.
// Returns a fresh access token, or null if the session cannot be recovered.
async function recover(): Promise<string | null> {
  const { hasSession, setTokens, clearAuth } = useAuthStore.getState();

  if (hasSession) {
    try {
      const { data } = await bare.post<Tokens>("/auth/refresh");
      setTokens(data.accessToken);
      return data.accessToken;
    } catch {
      // fall through to Telegram re-login
    }
  }

  const initData = getInitData();
  if (initData) {
    try {
      const { data } = await bare.post<Tokens>("/auth/telegram", { initData });
      setTokens(data.accessToken);
      return data.accessToken;
    } catch {
      // fall through
    }
  }

  clearAuth();
  return null;
}

// Single-flight: concurrent callers share one in-flight recovery.
let inFlight: Promise<string | null> | null = null;

export function refreshSession(): Promise<string | null> {
  if (!inFlight) {
    inFlight = recover().finally(() => {
      inFlight = null;
    });
  }
  return inFlight;
}

// Ensure a usable access token exists; refresh/re-login only when needed.
export function ensureValidSession(): Promise<string | null> {
  const valid = accessTokenValid();
  if (valid) return Promise.resolve(valid);
  return refreshSession();
}
