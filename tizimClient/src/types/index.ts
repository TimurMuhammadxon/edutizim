export type Role = "Owner" | "SuperAdmin" | "OrgAdmin" | "Teacher" | "Student" | "Staff";

export interface AuthUser {
  id: string;
  email?: string;
  phone?: string;
  role: Role;
  organizationId?: string;
  firstName?: string;
  lastName?: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
}

export interface RegisterResponse {
  id: string;
  email: string;
  role: Role;
  organizationId: string;
  organizationName: string;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

// Groups
export interface GroupDto {
  id: string;
  branchId: string;
  name: string;
  description?: string;
  price: number;
  teacherId?: string;
  teacherName?: string;
  roomId?: string;
  roomName?: string;
  studentCount: number;
  isActive: boolean;
  createdAt: string;
}

export type GroupMembershipStatus = "Trial" | "Active" | "Frozen" | "Left";

export interface GroupStudentDto {
  studentId: string;
  fullName: string;
  phone: string;
  status: GroupMembershipStatus;
  balance: number;
  nextPaymentDueDate?: string;
  effectivePrice: number;
  isDebtor: boolean;
  discountedPrice?: number;
  discountStartDate?: string;
  discountEndDate?: string;
}

export type PaymentMethod = "Cash" | "Card" | "Click" | "Payme" | "BankTransfer" | "Other";

export interface PaymentDto {
  id: string;
  groupId: string;
  groupName: string;
  studentId: string;
  studentFullName: string;
  amount: number;
  paidAt: string;
  forMonth: string;
  method: PaymentMethod;
  note?: string;
  createdAt: string;
}

export interface DebtorDto {
  studentId: string;
  studentFullName: string;
  studentPhone: string;
  groupId: string;
  groupName: string;
  effectivePrice: number;
  balance: number;
  nextPaymentDueDate: string;
  daysOverdue: number;
}

export interface PaymentsSummaryDto {
  totalAmount: number;
  count: number;
}

export interface PeriodDebtMonthDto {
  month: string;
  expected: number;
  paid: number;
  shortfall: number;
}

export interface PeriodDebtDto {
  studentId: string;
  studentFullName: string;
  studentPhone: string;
  groupId: string;
  groupName: string;
  amountOwedInPeriod: number;
  months: PeriodDebtMonthDto[];
}

export type DayOfWeek = "Sunday" | "Monday" | "Tuesday" | "Wednesday" | "Thursday" | "Friday" | "Saturday";

export interface GroupScheduleSlotDto {
  dayOfWeek: DayOfWeek;
  startTime: string;
  endTime: string;
}

export type AttendanceStatus = "Present" | "Absent";

export interface AttendanceStudentRowDto {
  studentId: string;
  fullName: string;
  marks: Record<string, AttendanceStatus>;
}

export interface GroupAttendanceDto {
  lessonDates: string[];
  students: AttendanceStudentRowDto[];
}

export interface GroupDetailsDto {
  id: string;
  branchId: string;
  name: string;
  description?: string;
  price: number;
  teacherId?: string;
  teacherName?: string;
  roomId?: string;
  roomName?: string;
  roomCapacity?: number;
  isActive: boolean;
  createdAt: string;
  students: GroupStudentDto[];
  schedule: GroupScheduleSlotDto[];
}

// Organizations
export interface BranchDto {
  id: string;
  name: string;
  address?: string;
  isActive: boolean;
  createdAt: string;
}

export interface RoomDto {
  id: string;
  branchId: string;
  name: string;
  capacity: number;
  isActive: boolean;
  createdAt: string;
}

// CRM
export type ClientSource = "Instagram" | "Telegram" | "Website" | "Referral" | "WalkIn" | "Call" | "Other";
export type LeadStage = "New" | "Contacted" | "TrialScheduled" | "Negotiation" | "Converted" | "Lost";
export type CrmTaskStatus = "Pending" | "Done" | "Cancelled";

export interface LeadDto {
  id: string;
  branchId: string;
  fullName: string;
  phone: string;
  email?: string;
  source: ClientSource;
  stage: LeadStage;
  assignedManagerId?: string;
  notes?: string;
  lostReason?: string;
  createdAt: string;
}

export interface StudentDto {
  id: string;
  branchId: string;
  leadId?: string;
  userId?: string;
  fullName: string;
  phone: string;
  email?: string;
  isActive: boolean;
  notes?: string;
  createdAt: string;
}

export interface StudentGroupMembershipDto {
  groupId: string;
  groupName: string;
  teacherName?: string;
  status: GroupMembershipStatus;
  joinedAt: string;
  activatedAt?: string;
  effectivePrice: number;
  balance: number;
  nextPaymentDueDate?: string;
  presentCount: number;
  absentCount: number;
}

export interface StudentDetailsDto extends StudentDto {
  branchName: string;
  leadFullName?: string;
  startedAt?: string;
  totalBalance: number;
  groups: StudentGroupMembershipDto[];
}

export interface StudentAttendanceGroupDto {
  groupId: string;
  groupName: string;
  lessonDates: string[];
  marks: Record<string, AttendanceStatus>;
  presentCount: number;
  absentCount: number;
}

export interface StudentAttendanceDto {
  groups: StudentAttendanceGroupDto[];
}

// Organization members (Staff/Teacher)
export interface MemberDto {
  id: string;
  fullName?: string;
  phone?: string;
  role: Role;
  isActive: boolean;
  createdAt: string;
}

export interface CrmTaskDto {
  id: string;
  title: string;
  description?: string;
  dueAt: string;
  status: CrmTaskStatus;
  assignedToUserId: string;
  leadId?: string;
  leadFullName?: string;
  createdAt: string;
  completedAt?: string;
}
