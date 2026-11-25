export interface Pat<TUser = User> {
  id: string;
  name: string;
  user?: TUser | null;
  scopes: Array<string>;
  createdAt: string;
  expiresIn: string;
  affectedPatRulesCounts: SummaryCounts;
}

export interface PatResponse extends BaseApiResult {
  pats: Array<Pat<PatUser>>;
}

export interface PatDetailResponse extends BaseApiResult {
  pat: Pat<PatUser>;
  rules: Array<BaseRule>;
}

export interface PatUser {
  userId: string;
  userName: string;
}
