# The public repository on GitHub

ADO Auditor is developed in Azure DevOps and published at
[github.com/IT-Titans/ado-auditor](https://github.com/IT-Titans/ado-auditor). The service
is licensed under the AGPL, which gives everyone who uses it over the network the right to
the source of the version that is running. The public repository therefore has to hold
exactly what is deployed to production. This page describes how that is kept true.

## How it works

The public repository is not a copy of the internal history. It holds **one commit per
release**. Each of these snapshot commits carries the complete tree of the release and has
the previous snapshot as its only parent. No internal commit, author or branch is
transferred. Because the two histories share no commit, the two sides are compared by their
**tree**: the public repository is up to date when the tree at the tip of its default
branch is the tree of the commit that was deployed.

Two scripts in [`mirror/`](../../mirror) do the work. Both read GitHub without any
credential, through a shallow fetch into a temporary repository, and neither writes to the
repository it is run in.

| Script | What it does |
| --- | --- |
| `Publish-MirrorSnapshot.ps1` | Commits the tree of a released commit on top of the GitHub branch as `chore(release): <version>` and tags it `v<version>`. The version is read from `backend/Directory.Build.props` at that commit. |
| `Test-MirrorDrift.ps1` | Compares the tree of a commit with the tree on GitHub and fails when they differ, naming both commits and the first differing paths. |

The publish script

- does nothing when GitHub already holds the tree and the tag,
- never forces a push, and pushes branch and tag together or not at all,
- **refuses** when the tip of the GitHub branch holds a tree that is not the tree of any
  commit in the history being released. Such a tip is something that never existed
  internally, usually a change somebody made on GitHub directly, and publishing over it
  would bury it. See [Contributions that arrive on GitHub](#contributions-that-arrive-on-github),
- publishes without a tag, and warns, when `v<version>` already exists on GitHub with other
  content. That happens when something was deployed without raising the version. The source
  is still published, because that is what the licence asks for; the tag is never moved.

## In the pipeline

The last stage of `azure-pipelines.yml`, **Publish to GitHub**, runs for builds of `main`
after the production deployment has succeeded. It publishes the release and then runs the
drift check.

The stage comes after the deployment and nothing depends on it. **A red stage does not
mean that the deployment failed, and it cannot hold back or undo one.** By the time it
runs, the new version is live. What the colours mean:

| Result | Meaning | What to do |
| --- | --- | --- |
| Green | GitHub holds the source of what is running. | Nothing. |
| Red, drift, after the warning "not published, no deploy key" | The release is live and its source is **not** on GitHub, because the pipeline has no credential. | Publish by hand (below), or set the credential up, then re-run the stage. |
| Red, drift, in any other case | GitHub does not hold what was deployed. The log names both commits and the paths. | Find out why from the publish step above it, publish, re-run the stage. |
| Red, "refused" | GitHub holds a change that the internal history does not know. | See [Contributions that arrive on GitHub](#contributions-that-arrive-on-github), then re-run. |
| Red, "push was rejected" | The key has no write access, a ruleset blocks the push, or GitHub's host key changed. | Fix the cause, re-run the stage. |
| Orange, "GitHub could not be read" | GitHub was unreachable. Nothing is known. This is not a finding of drift. | Re-run the stage. |
| Orange, "published with a warning" | Published, but without a tag because the version was not raised. | Raise the version with the next release. |

"Re-run the stage" means **Rerun failed jobs** on the stage *Publish to GitHub* of that
run. It does not build or deploy anything again.

The drift check only runs when the pipeline runs. A change made on GitHub between two
releases is found by the next release, not before.

## One-time setup by an operator

Until this is done the publish step is skipped with a warning, and the stage is red after
every release that was not published by hand. That is intended: it is the signal that the
source of the running version is missing.

The credential is an SSH **deploy key** of the GitHub repository. It can push to this one
repository and nothing else, and it belongs to no person. It never appears in this
repository, in a work item or in a chat: it goes from the machine that creates it straight
into GitHub and into a secret pipeline variable.

1. On your own machine, create a key pair without a passphrase:
   `ssh-keygen -t ed25519 -C "ado-auditor mirror" -f mirror-deploy-key -N ""`
2. On GitHub, open the repository's **Settings > Deploy keys > Add deploy key**. Paste the
   content of `mirror-deploy-key.pub`, name it `Azure DevOps mirror`, and tick
   **Allow write access**.
3. In Azure DevOps, open the pipeline `ado-exporter`, **Edit > Variables**, and add:

   | Variable | Value | Secret |
   | --- | --- | --- |
   | `mirrorDeployKeyBase64` | The private key file, base64 encoded on one line | **Yes** ("Keep this value secret") |
   | `mirrorAuthorName` | The name the snapshot commits carry | No |
   | `mirrorAuthorEmail` | The address the snapshot commits carry | No |

   To get the encoded key onto the clipboard without it touching a file or a terminal log:
   `[Convert]::ToBase64String([IO.File]::ReadAllBytes('mirror-deploy-key')) | Set-Clipboard`
4. Delete `mirror-deploy-key` and `mirror-deploy-key.pub` from your machine and clear the
   clipboard.
5. If a ruleset protects the default branch or tags on GitHub, make sure the deploy key may
   push to the branch and create `v*` tags. A rule that only forbids force pushes and
   deletions does not get in the way; the publish step does neither.
6. Re-run the stage *Publish to GitHub* of the latest run on `main`. It should end green
   with "GitHub already holds this tree".

A secret variable is used, and not a secure file or a service connection, because Azure
Pipelines checks the secure files and service connections a pipeline names before a run
starts. Naming one that does not exist yet would stop every run, the deployment included.
A variable that is not set simply arrives empty, so the pipeline keeps working before the
setup and after the key is removed.

To rotate the key, repeat the steps and delete the old deploy key on GitHub. The host keys
of github.com are pinned in `mirror/github-known-hosts`; they are published at
`https://api.github.com/meta` (`ssh_keys`). If GitHub changes them, the push fails until
that file is updated.

## Publishing by hand

Someone with write access to the GitHub repository can publish from a clone of this
repository that has `main` up to date:

```powershell
# What would be published? Reads only.
./mirror/Publish-MirrorSnapshot.ps1 -Commit origin/main -DryRun

# Is GitHub up to date?
./mirror/Test-MirrorDrift.ps1 -Commit origin/main
```

The script pushes with a deploy key only. To publish with your own access instead, do what
it does: commit the tree of `origin/main` on top of the GitHub branch and tag it.

```powershell
git fetch https://github.com/IT-Titans/ado-auditor.git main
$version = '1.4.0'   # the version in backend/Directory.Build.props on origin/main
$commit = git commit-tree 'origin/main^{tree}' -p FETCH_HEAD -m "chore(release): $version"
git tag --annotate --message "ADO Auditor $version" "mirror-v$version" $commit
git push --atomic https://github.com/IT-Titans/ado-auditor.git "${commit}:refs/heads/main" "refs/tags/mirror-v${version}:refs/tags/v$version"
git tag --delete "mirror-v$version"
```

Then re-run the stage, or run the drift check, to confirm.

## Contributions that arrive on GitHub

A pull request on GitHub is not merged on GitHub. The public branch only ever receives
release snapshots, so a merge there would be overwritten in content by the next release and
is refused by the publish step before that happens.

A contribution comes in like this:

1. Review the pull request on GitHub. A first contribution needs a signed
   [Contributor Licence Agreement](../../CLA.md), see [CONTRIBUTING.md](../../CONTRIBUTING.md).
2. Take the commits over into a branch of the internal repository, with the contributor
   as the author:

   ```powershell
   git fetch https://github.com/IT-Titans/ado-auditor.git pull/<number>/head
   git switch --create contrib/<number>-<short-description> origin/dev
   git cherry-pick <snapshot commit the pull request starts from>..FETCH_HEAD
   ```

   The pull request is based on a snapshot, and a snapshot has the same tree as the
   release it stands for. The commits therefore apply to `dev` the way a change based on
   that release would, conflicts with newer work included.
3. Open a pull request against `dev` in Azure DevOps. From here on it is a normal change:
   review, build validation, merge.
4. Close the pull request on GitHub with a comment that says it was taken over and will be
   part of the next release. With that release its content appears in the public
   repository, in the snapshot commit.

If somebody did push to the public branch directly, the publish step refuses and the stage
turns red. Either take the change over as above and release it, or decide against it and
revert it on GitHub. After the revert the tip holds the tree of the last snapshot again,
which the publish step recognises, and publishing continues. Do not force-push the public
branch.
