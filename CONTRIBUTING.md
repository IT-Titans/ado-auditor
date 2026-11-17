# Contributing to ADO Auditor

Thank you for taking an interest in the project. ADO Auditor is developed by
IT Titans GmbH and published under the GNU Affero General Public License v3.0
or later. Contributions are welcome.

## Before you start

**Open an issue first** for anything beyond an obvious fix. A short discussion
saves you from building something we cannot merge, and it lets us tell you
whether someone is already working on it.

Security problems are the exception: please do **not** open a public issue for
those. See [SECURITY.md](./SECURITY.md).

## Contributor Licence Agreement

ADO Auditor is dual-licensed. The public licence is the AGPL; IT Titans GmbH
also sells commercial licences to organisations whose circumstances the AGPL
does not fit. To keep that possible, we need the right to license your
contribution under both.

**Before your first pull request can be merged, you need to sign the
[Contributor Licence Agreement](./CLA.md).** You keep the copyright in your
work. The agreement grants us a licence broad enough to relicense it; it does
not take your rights away.

This applies to everyone outside IT Titans GmbH. Work created by IT-Titans
employees in the course of their duties is already covered.

## Making a change

1. Fork the repository and create a branch from `main`.
2. Make your change, with tests.
3. Run the full test suite and the formatter.
4. Open a pull request against `main` describing what changes and why.

### Commit messages

[Conventional Commits](https://www.conventionalcommits.org/). The type prefix
drives our release notes, so it matters:

```
feat: add rule for stale service connections
fix: handle empty organization name in audit report
docs: explain the PAT scopes required for a full audit
```

### Tests

New behaviour needs tests, and a bug fix needs a test that fails before it and
passes after. The backend uses NUnit, the frontend uses Vitest.

```sh
# backend
dotnet test backend/AdoExport.Api.sln

# frontend
pnpm --dir frontend vitest run
```

Run the whole suite, not just the project you touched. A test run that reports
"No test is available" for any project is a failure, even when the exit code is
zero: it means a project was skipped rather than passed.

We are working towards 90% line coverage and will not accept changes that move
it backwards.

### Code style

```sh
dotnet format backend/AdoExport.Api.sln --verify-no-changes
pnpm --dir frontend lint
```

Source code, comments, commit messages and documentation are written in
English.

## What we are unlikely to merge

- Changes that add a dependency under a licence incompatible with the AGPL, or
  one that restricts commercial use
- Reformatting unrelated files, which buries the actual change in noise
- New audit rules without a description and a recommended action; a finding the
  user cannot act on is not worth reporting

## Questions

Open a discussion or write to us at https://it-titans-gmbh.com/kontakt/.
