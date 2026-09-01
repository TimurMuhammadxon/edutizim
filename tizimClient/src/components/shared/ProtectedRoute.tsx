import { useEffect, useState } from "react";
import { Navigate } from "react-router-dom";
import { useAuthStore } from "@/store/auth";
import { ensureValidSession } from "@/lib/session";
import { PageLoader } from "@/components/shared/LoadingSpinner";

interface Props {
  children: React.ReactNode;
  roles?: string[];
}

export function ProtectedRoute({ children, roles }: Props) {
  const { isAuthenticated, hasRole } = useAuthStore();

  // A session is recoverable if the access token is still valid, or we believe a
  // refresh-token cookie was issued (the cookie itself is httpOnly, invisible to JS —
  // `hasSession` just remembers that one was set), or we have Telegram initData.
  const authed = isAuthenticated();
  const canRecover =
    authed ||
    useAuthStore.getState().hasSession ||
    !!window.Telegram?.WebApp?.initData;

  // Expired access token but recoverable → refresh now instead of bouncing to /login.
  // Gate rendering until it resolves, so children never mount with a stale token —
  // only matters for this specific case; an already-valid session paints immediately.
  const [recovering, setRecovering] = useState(!authed && canRecover);

  useEffect(() => {
    if (!authed && canRecover) {
      Promise.resolve()
        .then(() => setRecovering(true))
        .then(() => ensureValidSession())
        .finally(() => setRecovering(false));
    }
  }, [authed, canRecover]);

  if (!canRecover) {
    return <Navigate to="/login" replace />;
  }

  if (recovering) {
    return <PageLoader />;
  }

  if (roles && roles.length > 0 && !hasRole(...roles)) {
    return <Navigate to="/home" replace />;
  }

  return <>{children}</>;
}
