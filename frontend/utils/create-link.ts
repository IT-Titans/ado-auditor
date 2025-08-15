export const linkTemplates = {
  global: {
    home: () => createLink("/"),
    audit: () => createLink("/audit"),
    expiredAudit: () => createLink("/audit?expired=true"),
  },
  audit: {
    overview: (auditId: string) => createLink("/audit/%s", auditId),
    expired: () => createLink("/audit/expired"),
    pats: {
      overview: (auditId: string) => createLink("/audit/%s/pats", auditId),
      pat: (auditId: string, patId: string) =>
        createLink("/audit/%s/pats/%s", auditId, patId),
    },
    users: {
      overview: (auditId: string) => createLink("/audit/%s/users", auditId),
      user: (auditId: string, userId: string) =>
        createLink("/audit/%s/users/%s", auditId, userId),
    },
    rules: {
      overview: (auditId: string, scope: Scope) =>
        createLink("/audit/%s/rules/%s", auditId, scope),
      rule: (auditId: string, scope: Scope, ruleId: string) =>
        createLink("/audit/%s/rules/%s/%s", auditId, scope, ruleId),
    },
  },
};

/**
 * Simple function for replacing an link template string with dynamic link parts.
 *
 * @param template - The link template with optional `%s` as placeholder
 * @param args - The option substitution variables of the placeholders, when placeholder are in the template string.
 *
 * @returns The link with the replacements or the passed template.
 */
export function createLink(template: string, ...args: string[]) {
  // TODO: Improve so that edge cases are intercepted, e.g. placeholder is not %s
  if (args.length === 0) return template;
  let link = template;

  for (const arg of args) {
    link = link.replace(/%[s]/, arg.trim());
  }

  return link;
}
