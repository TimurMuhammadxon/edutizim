import type { GroupMembershipStatus, GroupStudentDto, DayOfWeek, PaymentMethod } from "@/types";

export const MEMBERSHIP_STATUS_LABELS: Record<GroupMembershipStatus, string> = {
  Trial: "Sinov darsida", Active: "Faol", Frozen: "Muzlatilgan", Left: "Ketgan",
};

export function membershipBadgeVariant(s: GroupStudentDto): "success" | "destructive" | "warning" | "secondary" | "outline" {
  if (s.status === "Active") return s.isDebtor ? "destructive" : "success";
  if (s.status === "Frozen") return "warning";
  if (s.status === "Trial") return "secondary";
  return "outline";
}

export const DAYS: DayOfWeek[] = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"];
export const DAY_LABELS: Record<DayOfWeek, string> = {
  Monday: "Dush", Tuesday: "Sesh", Wednesday: "Chor", Thursday: "Pay",
  Friday: "Jum", Saturday: "Shan", Sunday: "Yak",
};
export const ODD_DAYS: DayOfWeek[] = ["Monday", "Wednesday", "Friday"];
export const EVEN_DAYS: DayOfWeek[] = ["Tuesday", "Thursday", "Saturday"];

export function withSeconds(time: string): string {
  return time.length === 5 ? `${time}:00` : time;
}

export function addHours(time: string, hours: number): string {
  const [h, m] = time.split(":").map(Number);
  const total = ((h * 60 + m + hours * 60) % (24 * 60) + 24 * 60) % (24 * 60);
  const hh = String(Math.floor(total / 60)).padStart(2, "0");
  const mm = String(total % 60).padStart(2, "0");
  return `${hh}:${mm}`;
}

export const PAYMENT_METHOD_LABELS: Record<PaymentMethod, string> = {
  Cash: "Naqd", Card: "Karta", Click: "Click", Payme: "Payme", BankTransfer: "Bank o'tkazmasi", Other: "Boshqa",
};
export const PAYMENT_METHODS: PaymentMethod[] = ["Cash", "Card", "Click", "Payme", "BankTransfer", "Other"];

const MONTH_LABELS = [
  "Yanvar", "Fevral", "Mart", "Aprel", "May", "Iyun",
  "Iyul", "Avgust", "Sentyabr", "Oktyabr", "Noyabr", "Dekabr",
];

/** Formats an ISO date string (e.g. a `forMonth` value) as "Sentyabr 2026". */
export function formatMonthLabel(isoDate: string): string {
  const [year, month] = isoDate.split("-").map(Number);
  return `${MONTH_LABELS[month - 1]} ${year}`;
}

/** Formats a raw Uzbek phone number ("+998902000013") as "+998 90 200 00 13". Falls back to the raw value if it doesn't match. */
export function formatPhone(phone: string): string {
  const digits = phone.replace(/\D/g, "");
  if (digits.length === 12 && digits.startsWith("998")) {
    const local = digits.slice(3);
    return `+998 ${local.slice(0, 2)} ${local.slice(2, 5)} ${local.slice(5, 7)} ${local.slice(7, 9)}`;
  }
  return phone;
}
