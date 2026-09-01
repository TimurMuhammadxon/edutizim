import { describe, it, expect } from "vitest";
import { getApiErrorMessage } from "./errors";

function fakeAxiosError(data: unknown) {
  return { isAxiosError: true, response: { data } };
}

describe("getApiErrorMessage", () => {
  it("prefers `detail` over `title`", () => {
    expect(getApiErrorMessage(fakeAxiosError({ detail: "Specific reason", title: "Generic" }))).toBe(
      "Specific reason"
    );
  });

  it("falls back to `title` when `detail` is missing", () => {
    expect(getApiErrorMessage(fakeAxiosError({ title: "Something went wrong" }))).toBe(
      "Something went wrong"
    );
  });

  it("returns undefined when the axios error has no response body", () => {
    expect(getApiErrorMessage({ isAxiosError: true })).toBeUndefined();
  });

  it("returns undefined for a non-axios error (network failure, thrown string, etc.)", () => {
    expect(getApiErrorMessage(new Error("boom"))).toBeUndefined();
    expect(getApiErrorMessage("plain string")).toBeUndefined();
    expect(getApiErrorMessage(null)).toBeUndefined();
  });
});
