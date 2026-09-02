interface LogoMarkProps {
  size?: number;
  variant?: "color" | "reversed";
  className?: string;
}

/** The edutizim symbol alone: one root node + two children, same shapes used in favicon.svg. */
export function LogoMark({ size = 32, variant = "color", className }: LogoMarkProps) {
  const reversed = variant === "reversed";
  return (
    <svg width={size} height={size} viewBox="0 0 100 100" className={className} aria-hidden="true" focusable="false">
      <rect x="12" y="46" width="36" height="36" rx="12" fill={reversed ? "#F8FAFC" : "#22D3EE"} opacity={reversed ? 0.55 : 1} />
      <rect x="52" y="46" width="36" height="36" rx="12" fill={reversed ? "#F8FAFC" : "#155E75"} opacity={reversed ? 0.8 : 1} />
      <rect x="28" y="10" width="44" height="44" rx="14" fill={reversed ? "#F8FAFC" : "#0891B2"} />
    </svg>
  );
}

interface LogoProps {
  height?: number;
  variant?: "color" | "reversed";
  className?: string;
  wordmarkColor?: string;
}

/** Primary horizontal lockup: mark + "edutizim" wordmark, sized off a single `height`. */
export function Logo({ height = 32, variant = "color", className, wordmarkColor }: LogoProps) {
  const color = wordmarkColor ?? (variant === "reversed" ? "#F8FAFC" : "hsl(var(--foreground))");
  return (
    <div
      className={className}
      role="img"
      aria-label="edutizim"
      style={{ display: "inline-flex", alignItems: "center", gap: height * 0.32 }}
    >
      <LogoMark size={height} variant={variant} />
      <span
        aria-hidden="true"
        style={{
          fontFamily: "'Outfit', sans-serif",
          fontWeight: 800,
          fontSize: height * 0.6,
          letterSpacing: "-0.01em",
          color,
          lineHeight: 1,
          whiteSpace: "nowrap",
        }}
      >
        edutizim
      </span>
    </div>
  );
}
