# Where the code lives

ADO Auditor is developed in this repository,
[github.com/IT-Titans/ado-auditor](https://github.com/IT-Titans/ado-auditor). Issues, pull
requests and reviews happen here. The service is licensed under the AGPL, which gives
everyone who uses it over the network the right to the source of the version that is
running: that is the branch `main`, and every release carries a tag.

## Branches and checks

| Branch | Holds | Deployed to |
| --- | --- | --- |
| `dev` | The current work. Every change enters here through a pull request. | The development environment |
| `main` | The released version. It only changes through a pull request from `dev`. | Production |

A pull request is checked by GitHub Actions ([`ci.yml`](../../.github/workflows/ci.yml)):
the backend tests with the coverage minimum, type check, lint and tests of the frontend,
and a build of both images. The rulesets of `dev` and `main` require these checks. The
workflow needs no secret and also runs for pull requests from forks.

Pull requests into `dev` are merged by squash, and their title has to be a
[Conventional Commit](https://www.conventionalcommits.org/), because it becomes the
commit message. A release is merged from `dev` into `main` with a merge commit, see
[Releases](releases.md).

## Deployment

Building the images and deploying them is done by Azure Pipelines
([`azure-pipelines.yml`](../../azure-pipelines.yml)), in the Azure DevOps organisation of
IT Titans GmbH. The pipeline reads this repository and runs for every commit that reaches
`dev` or `main` and touches `backend/` or `frontend/`. It does not run for pull requests,
so nothing that has not been merged by a maintainer gets near the registry or Azure. Its
runs are visible to IT Titans staff only; the result shows up here as a commit status.

## Security fixes

A fix for a vulnerability must not be readable before it is deployed. It is prepared in
a [repository security advisory](https://docs.github.com/en/code-security/security-advisories/working-with-repository-security-advisories/about-repository-security-advisories)
with a temporary private fork, not in a public pull request. GitHub does not run checks
in such a fork. Run the checks from [Backend Development](backend.md) and
[Frontend Development](frontend.md) on your machine before the advisory is merged, and
release right after.

## History before the move

Up to version 1.5.0, ADO Auditor was developed in an internal repository in Azure DevOps,
and this repository received one commit per release. The oldest commits here are those
snapshots: each holds the complete tree of a release and says nothing about how it came
about.

The history behind them, with its commits, pull requests and work items, stays in that
internal repository, which is kept read-only: project *ADO Exporter*, repository
`ado-exporter`, in the Azure DevOps organisation of IT Titans GmbH. It carries the tags
`v1.0.0` to `v1.5.0`. If you work at IT Titans and need to know why a line that predates
the move looks the way it does, `git blame` there is where to look. The internal history
is not published, because it contains material that must not be public.
