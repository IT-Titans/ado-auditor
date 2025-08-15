export type AuditCategory = {
  scope: Scope;
  totalCount: number;
  criticalCount: number;
  warningCount: number;
  okCount: number;
  items: Array<AuditCategorySummary>;
};

export type AuditCategorySummary = {
  severity: Severity;
  affectedCount: number;
  totalCount: number;
  text: string;
};
export interface AuditData extends BaseApiResult {
  summaries: Array<AuditCategory>;
  totalsCount?: number;
  criticalCount?: number;
  warningCount?: number;
  okCount?: number;
}
