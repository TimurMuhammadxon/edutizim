import { create } from "zustand";
import { persist } from "zustand/middleware";
import { type AuthUser } from "@/types";
import { decodeJwt } from "@/lib/jwt";

interface AuthState {
  accessToken: string | null;
  // The refresh token itself lives only in an httpOnly cookie, invisible to JS. This
  // flag just remembers "a session cookie was issued" so route guards / session
  // recovery know a silent refresh is worth attempting, without ever holding the token.
  hasSession: boolean;
  user: AuthUser | null;
  setTokens: (accessToken: string) => void;
  clearAuth: () => void;
  isAuthenticated: () => boolean;
  hasRole: (...roles: string[]) => boolean;
}

function parseUser(token: string): AuthUser | null {
  const payload = decodeJwt(token);
  if (!payload) return null;
  return {
    id: payload.sub,
    email: payload.email,
    phone: payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/mobilephone"],
    role: payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] as AuthUser["role"],
    organizationId: payload.org_id,
    firstName: payload.given_name,
    lastName: payload.family_name,
  };
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      accessToken: null,
      hasSession: false,
      user: null,

      setTokens: (accessToken) => {
        const user = parseUser(accessToken);
        set({ accessToken, hasSession: true, user });
      },

      clearAuth: () => set({ accessToken: null, hasSession: false, user: null }),

      isAuthenticated: () => {
        const { accessToken } = get();
        if (!accessToken) return false;
        const payload = decodeJwt(accessToken);
        if (!payload) return false;
        return payload.exp * 1000 > Date.now();
      },

      hasRole: (...roles) => {
        const { user } = get();
        if (!user) return false;
        return roles.includes(user.role);
      },
    }),
    {
      name: "auth-storage",
      partialize: (state) => ({
        accessToken: state.accessToken,
        hasSession: state.hasSession,
        user: state.user,
      }),
    }
  )
);
