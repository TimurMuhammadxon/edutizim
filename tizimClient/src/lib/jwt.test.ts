import { describe, it, expect } from "vitest";
import { decodeJwt, getRole, getUserId, getEmail } from "./jwt";
import { makeJwt } from "@/test/makeJwt";

const ROLE_CLAIM = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

describe("decodeJwt", () => {
  it("decodes a well-formed token's payload", () => {
    const token = makeJwt({ sub: "user-1", email: "a@b.com", exp: 1234, [ROLE_CLAIM]: "OrgAdmin" });
    const payload = decodeJwt(token);
    expect(payload?.sub).toBe("user-1");
    expect(payload?.email).toBe("a@b.com");
    expect(payload?.[ROLE_CLAIM]).toBe("OrgAdmin");
  });

  it("returns null for a token with the wrong number of segments", () => {
    expect(decodeJwt("not-a-jwt")).toBeNull();
    expect(decodeJwt("only.two")).toBeNull();
  });

  it("returns null for a token whose payload isn't valid base64/JSON", () => {
    expect(decodeJwt("header.not-valid-base64!!!.signature")).toBeNull();
  });

  it("decodes unicode characters in the payload correctly", () => {
    const token = makeJwt({ sub: "user-1", exp: 1234, given_name: "Тимур", [ROLE_CLAIM]: "OrgAdmin" });
    expect(decodeJwt(token)?.given_name).toBe("Тимур");
  });
});

describe("getRole / getUserId / getEmail", () => {
  const token = makeJwt({ sub: "user-42", email: "x@y.com", exp: 1234, [ROLE_CLAIM]: "Staff" });

  it("extracts each claim", () => {
    expect(getRole(token)).toBe("Staff");
    expect(getUserId(token)).toBe("user-42");
    expect(getEmail(token)).toBe("x@y.com");
  });

  it("returns null for all claims on a garbage token", () => {
    expect(getRole("garbage")).toBeNull();
    expect(getUserId("garbage")).toBeNull();
    expect(getEmail("garbage")).toBeNull();
  });
});
