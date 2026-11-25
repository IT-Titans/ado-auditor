export interface UserRuleItem {
  id: string;
  userName: string;
  metaType: string;
  affectedUserRulesCounts: SummaryCounts;
}

export interface PatRuleItem {
  id: string;
  name: string;
  user: {
    userId: string;
    userName: string;
  };
  scopes: string[];
  createdAt: string;
  expiresIn: string;
  affectedPatRulesCounts: SummaryCounts;
}

export interface ProjectRuleItem {
  id: string;
  name: string;
  affectedProjectRulesCounts: SummaryCounts;
}

export interface GroupRuleItem {
  id: string;
  name: string;
  affectedGroupRulesCounts: SummaryCounts;
}

export interface BaseRule {
  id: string;
  scope: Scope;
  severity: Severity;
  name: string;
  description: string;
  recommendation: string;
  totalCounts: number;
}

export interface RuleResponse<T> extends BaseApiResult {
  rule: BaseRule & { items: T[] };
}

export interface RulesResponse extends BaseApiResult {
  rules: Array<BaseRule>;
}
