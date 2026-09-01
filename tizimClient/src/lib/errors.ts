import { isAxiosError } from "axios";

/** Extracts a backend-provided error message from an ASP.NET Core ProblemDetails response. */
export function getApiErrorMessage(error: unknown): string | undefined {
  if (isAxiosError<{ detail?: string; title?: string }>(error)) {
    return error.response?.data?.detail ?? error.response?.data?.title;
  }
  return undefined;
}
