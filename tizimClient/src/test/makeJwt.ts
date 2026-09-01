/** Builds a syntactically valid (unsigned) JWT string for tests — decodeJwt only ever reads the payload. */
export function makeJwt(payload: Record<string, unknown>): string {
  const base64url = (obj: unknown) => {
    // btoa only accepts Latin1; re-encode the UTF-8 JSON string into a Latin1-safe
    // binary string first (mirrors decodeJwt's inverse percent-decoding trick).
    const utf8Binary = unescape(encodeURIComponent(JSON.stringify(obj)));
    return btoa(utf8Binary).replace(/\+/g, "-").replace(/\//g, "_").replace(/=+$/, "");
  };

  return [base64url({ alg: "none", typ: "JWT" }), base64url(payload), "signature"].join(".");
}
