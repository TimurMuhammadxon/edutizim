import { describe, it, expect, beforeEach } from "vitest";
import { useAuthStore } from "./auth";
import { makeJwt } from "@/test/makeJwt";

const ROLE_CLAIM = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
const PHONE_CLAIM = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/mobilephone";

function tokenExpiringIn(seconds: number, overrides: Record<string, unknown> = {}) {
  return makeJwt({
    sub: "user-1",
    email: "a@b.com",
    exp: Math.floor(Date.now() / 1000) + seconds,
    [ROLE_CLAIM]: "OrgAdmin",
    org_id: "org-1",
    given_name: "Timur",
    ...overrides,
  });
}

beforeEach(() => {
  localStorage.clear();
  useAuthStore.setState({ accessToken: null, hasSession: false, user: null });
});

describe("useAuthStore", () => {
  it("setTokens parses the JWT into `user` and flips hasSession on", () => {
    const token = tokenExpiringIn(900);
    useAuthStore.getState().setTokens(token);

    const state = useAuthStore.getState();
    expect(state.accessToken).toBe(token);
    expect(state.hasSession).toBe(true);
    expect(state.user).toMatchObject({
      id: "user-1",
      email: "a@b.com",
      role: "OrgAdmin",
      organizationId: "org-1",
      firstName: "Timur",
    });
  });

  it("clearAuth resets accessToken, hasSession, and user", () => {
    useAuthStore.getState().setTokens(tokenExpiringIn(900));
    useAuthStore.getState().clearAuth();

    const state = useAuthStore.getState();
    expect(state.accessToken).toBeNull();
    expect(state.hasSession).toBe(false);
    expect(state.user).toBeNull();
  });

  it("isAuthenticated is true for a non-expired token and false once it expires", () => {
    useAuthStore.getState().setTokens(tokenExpiringIn(900));
    expect(useAuthStore.getState().isAuthenticated()).toBe(true);

    useAuthStore.getState().setTokens(tokenExpiringIn(-1));
    expect(useAuthStore.getState().isAuthenticated()).toBe(false);
  });

  it("isAuthenticated is false with no token set", () => {
    expect(useAuthStore.getState().isAuthenticated()).toBe(false);
  });

  it("hasRole checks the decoded role against the given list", () => {
    useAuthStore.getState().setTokens(tokenExpiringIn(900));
    expect(useAuthStore.getState().hasRole("OrgAdmin", "Owner")).toBe(true);
    expect(useAuthStore.getState().hasRole("Teacher", "Student")).toBe(false);
  });

  it("hasRole is false when no user is set", () => {
    expect(useAuthStore.getState().hasRole("OrgAdmin")).toBe(false);
  });

  it("parses the phone claim when present instead of email", () => {
    const token = tokenExpiringIn(900, { email: undefined, [PHONE_CLAIM]: "+998901112233" });
    useAuthStore.getState().setTokens(token);
    expect(useAuthStore.getState().user?.phone).toBe("+998901112233");
  });
});
