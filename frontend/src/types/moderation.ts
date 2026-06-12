// Mirror the backend enums by name — JSON serialiser emits string names for the enum
// fields returned by /me/moderation-status and /admin/users/:id/moderation.

export type ModerationLevel = 'None' | 'Warned' | 'Restricted' | 'Suspended' | 'Banned';

export type ModerationReason =
  | 'Unspecified'
  | 'Spam'
  | 'Scam'
  | 'Harassment'
  | 'FakeListing'
  | 'Inappropriate'
  | 'PaymentFraud'
  | 'RuleViolation'
  | 'MultiAccount'
  | 'AutomatedAbuse'
  | 'FailedLoginBurst'
  | 'ManualReview'
  | 'AppealRejected'
  | 'Other';

export interface UserModerationStatus {
  level: ModerationLevel;
  reason: ModerationReason;
  publicReason: string | null;
  startedAt: string | null;
  expiresAt: string | null;
  activeModerationLogId: string | null;
  canAppeal: boolean;
}

export interface UserModerationLogEntry {
  id: string;
  adminId: string;
  adminDisplayName: string;
  previousLevel: ModerationLevel;
  newLevel: ModerationLevel;
  reason: ModerationReason;
  publicReason: string;
  internalNote: string | null;
  expiresAt: string | null;
  relatedReportId: string | null;
  relatedAppealId: string | null;
  createdAt: string;
}

export interface UserModerationDetail {
  userId: string;
  email: string;
  displayName: string;
  current: UserModerationStatus;
  history: UserModerationLogEntry[];
  openReportCount: number;
  unacknowledgedSignalCount: number;
  totalScore: number;
}

export interface ModerationActionRequest {
  newLevel: ModerationLevel;
  reason: ModerationReason;
  publicReason: string;
  internalNote?: string | null;
  durationMinutes?: number | null;
  relatedReportId?: string | null;
  relatedAppealId?: string | null;
}

/**
 * Shape of the 403 response body when the moderation middleware short-circuits a request.
 */
export interface ModerationForbiddenEnvelope {
  error: string;
  moderation: {
    level: ModerationLevel;
    reason: ModerationReason;
    publicReason: string;
    expiresAt: string | null;
  };
}

export const MODERATION_REASONS: ModerationReason[] = [
  'Spam', 'Scam', 'Harassment', 'FakeListing', 'Inappropriate',
  'PaymentFraud', 'RuleViolation', 'MultiAccount', 'AutomatedAbuse', 'Other',
];

export const MODERATION_LEVELS: ModerationLevel[] = ['Warned', 'Restricted', 'Suspended', 'Banned'];

export type ReportTargetType = 'User' | 'Item' | 'MessageThread' | 'Message';
export type ReportStatus = 'Pending' | 'UnderReview' | 'Resolved' | 'Dismissed' | 'Duplicate';

export interface SubmitReportRequest {
  targetType: ReportTargetType;
  targetId: string;
  reason: ModerationReason;
  description: string;
}

export interface AbuseReportSummary {
  id: string;
  reporterId: string;
  targetType: ReportTargetType;
  targetId: string;
  targetUserId: string;
  reason: ModerationReason;
  status: ReportStatus;
  createdAt: string;
}

export interface AbuseReportDetail {
  id: string;
  reporterId: string;
  targetType: ReportTargetType;
  targetId: string;
  targetUserId: string;
  reason: ModerationReason;
  description: string;
  status: ReportStatus;
  resolvedByAdminId: string | null;
  resolvedAt: string | null;
  resolutionNote: string | null;
  resultingModerationLogId: string | null;
  createdAt: string;
}

export interface PagedReportResult {
  items: AbuseReportSummary[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface ResolveReportRequest {
  status: ReportStatus;
  resolutionNote?: string | null;
  moderationAction?: ModerationActionRequest | null;
}

export type AppealStatus = 'Pending' | 'UnderReview' | 'Approved' | 'Rejected';

export interface SubmitAppealRequest {
  moderationLogId: string;
  statement: string;
}

export interface Appeal {
  id: string;
  userId: string;
  moderationLogId: string;
  userStatement: string;
  status: AppealStatus;
  adminId: string | null;
  adminDecisionNote: string | null;
  decidedAt: string | null;
  createdAt: string;
}

export interface AppealSummary {
  id: string;
  userId: string;
  moderationLogId: string;
  status: AppealStatus;
  createdAt: string;
}

export interface PagedAppealResult {
  items: AppealSummary[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface DecideAppealRequest {
  status: 'Approved' | 'Rejected';
  decisionNote?: string | null;
}
