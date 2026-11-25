export interface User {
  id: string;
  userName: string;
  metaType: string;
  isActive: boolean;
  affectedPatRulesCounts: SummaryCounts;
  affectedUserRulesCounts: SummaryCounts;
}

export interface UserResponse extends BaseApiResult {
  users: Array<User>;
}

export interface UserDetailsResponse extends BaseApiResult {
  user: User;
  patsSummary: SummaryCounts;
  projectsSummary?: SummaryCounts;
  groupsSummary?: SummaryCounts;
  pats: Array<Pat<PatUser>>;
  projects?: [];
  groups?: [];
  rules: Array<BaseRule>;
}
