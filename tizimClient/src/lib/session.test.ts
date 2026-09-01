import { describe, it, expect, beforeEach, afterEach, vi } from "vitest";
import { useAuthStore } from "@/store/auth";
import { makeJwt } from "@/test/makeJwt";

const ROLE_CLAIM = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

const mockPost = vi.fn();
vi.mock("axios", () => ({
  default: { create: () => ({ post: mockPost }) },
}));

// Imported after the axios mock so session.ts's module-level `bare` picks it up.
const { refreshSession, ensureValidSession } = await import("./session");

function tokenExpiringIn(seconds: number) {
  return makeJwt({ sub: "user-1", email: "a@b.com", exp: Math.floor(Date.now() / 1000) + seconds, [ROLE_CLAIM]: "OrgAdmin" });
}

beforeEach(() => {
  mockPost.mockReset();
  localStorage.clear();
  useAuthStore.setState({ accessToken: null, hasSession: false, user: null });
});

afterEach(() => {
  delete window.Telegram;
});

describe("ensureValidSession", () => {
  it("returns the current token without a network call when it isn't near expiry", async () => {
    const token = tokenExpiringIn(900);
    useAuthStore.getState().setTokens(token);

    const result = await ensureValidSession();

    expect(result).toBe(token);
    expect(mockPost).not.toHaveBeenCalled();
  });

  it("refreshes via the cookie when the token is expired", async () => {
    useAuthStore.getState().setTokens(tokenExpiringIn(-10));
    const newToken = tokenExpiringIn(900);
    mockPost.mockResolvedValueOnce({ data: { accessToken: newToken } });

    const result = await ensureValidSession();

    expect(result).toBe(newToken);
    expect(mockPost).toHaveBeenCalledTimes(1);
    expect(mockPost).toHaveBeenCalledWith("/auth/refresh");
    expect(useAuthStore.getState().accessToken).toBe(newToken);
  });
});

describe("refreshSession single-flight", () => {
  it("shares one in-flight request across concurrent callers", async () => {
    useAuthStore.getState().setTokens(tokenExpiringIn(-10));
    const newToken = tokenExpiringIn(900);

    let resolvePost!: (v: { data: { accessToken: string } }) => void;
    mockPost.mockReturnValueOnce(new Promise((resolve) => { resolvePost = resolve; }));

    const first = refreshSession();
    const second = refreshSession(); // fires before the first has resolved

    resolvePost({ data: { accessToken: newToken } });
    const [firstResult, secondResult] = await Promise.all([first, second]);

    expect(firstResult).toBe(newToken);
    expect(secondResult).toBe(newToken);
    expect(mockPost).toHaveBeenCalledTimes(1); // not two separate refresh calls
  });

  it("allows a new request once the previous one has settled", async () => {
    useAuthStore.getState().setTokens(tokenExpiringIn(-10));
    mockPost.mockResolvedValueOnce({ data: { accessToken: tokenExpiringIn(900) } });
    await refreshSession();

    mockPost.mockResolvedValueOnce({ data: { accessToken: tokenExpiringIn(900) } });
    await refreshSession();

    expect(mockPost).toHaveBeenCalledTimes(2);
  });
});

describe("session recovery fallback", () => {
  it("falls back to Telegram initData when the refresh-token cookie is invalid", async () => {
    useAuthStore.getState().setTokens(tokenExpiringIn(-10)); // hasSession: true, but refresh will fail
    window.Telegram = { WebApp: { initData: "tg-init-data", ready: () => {} } };
    const newToken = tokenExpiringIn(900);

    mockPost.mockImplementation((url: string) => {
      if (url === "/auth/refresh") return Promise.reject(new Error("expired"));
      if (url === "/auth/telegram") return Promise.resolve({ data: { accessToken: newToken } });
      throw new Error(`unexpected call: ${url}`);
    });

    const result = await refreshSession();

    expect(result).toBe(newToken);
    expect(mockPost).toHaveBeenCalledWith("/auth/refresh");
    expect(mockPost).toHaveBeenCalledWith("/auth/telegram", { initData: "tg-init-data" });
  });

  it("clears auth and returns null when there is no session and no Telegram context", async () => {
    useAuthStore.getState().clearAuth();

    const result = await refreshSession();

    expect(result).toBeNull();
    expect(mockPost).not.toHaveBeenCalled();
    expect(useAuthStore.getState().accessToken).toBeNull();
  });
});
