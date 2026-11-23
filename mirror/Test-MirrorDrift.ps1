#Requires -Version 7.0
using module ./Mirror.psm1

<#
.SYNOPSIS
    Checks that the public GitHub repository holds exactly the tree of a commit.

.DESCRIPTION
    The public repository is a series of release snapshots and shares no commit with
    this one, so the comparison is between trees. The GitHub side is read without any
    credential, through a shallow fetch into a temporary repository; nothing is written
    to the repository this runs in.

    Exit codes:
      0  GitHub holds the same tree.
      1  GitHub holds a different tree. Both commits and the first differing paths are
         printed.
      2  GitHub could not be read, so nothing is known. This is not drift.
      3  The check could not run at all, for example because the commit does not exist.

    On a build agent an unreachable GitHub completes the step with a warning instead of
    failing it. A red step therefore always means: the public source is not what was
    built.

.PARAMETER Commit
    The commit whose tree GitHub is expected to hold. Defaults to HEAD.

.PARAMETER RemoteUrl
    The public repository.

.PARAMETER Branch
    The branch to compare with. Defaults to the default branch of the remote.

.EXAMPLE
    ./mirror/Test-MirrorDrift.ps1 -Commit origin/main
#>
[CmdletBinding()]
param(
    [string] $Commit = 'HEAD',

    [string] $RemoteUrl = 'https://github.com/IT-Titans/ado-auditor.git',

    [string] $Branch,

    [string] $RepositoryPath = (Join-Path $PSScriptRoot '..'),

    [int] $RetryCount = 2,

    [double] $RetryDelaySeconds = 5
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$onBuildAgent = $env:TF_BUILD -eq 'True'

try {
    $comparison = Compare-MirrorTree -RepositoryPath $RepositoryPath -Commit $Commit -RemoteUrl $RemoteUrl -Branch $Branch -RetryCount $RetryCount -RetryDelaySeconds $RetryDelaySeconds
}
catch {
    Write-Host "The drift check could not run: $($_.Exception.Message)"
    if ($onBuildAgent) {
        Write-Host "##vso[task.logissue type=error]The drift check could not run: $($_.Exception.Message)"
    }

    exit 3
}

switch ($comparison.Status) {
    'Identical' {
        "No drift. $RemoteUrl ($($comparison.RemoteBranch)) holds the tree of the commit that was built."
        "  built commit : $($comparison.LocalCommit)"
        "  GitHub commit: $($comparison.RemoteCommit)"
        "  tree         : $($comparison.LocalTree)"

        exit 0
    }

    'Unreachable' {
        "Not checked. $($comparison.Message)"
        'Whether GitHub holds what was built is unknown. Run this step again.'

        if ($onBuildAgent) {
            Write-Host "##vso[task.logissue type=warning]GitHub could not be read, so the mirror was not checked. This is not a finding of drift. Run the stage again."
            Write-Host '##vso[task.complete result=SucceededWithIssues;]GitHub could not be read'

            exit 0
        }

        exit 2
    }

    'Drift' {
        "DRIFT. $RemoteUrl ($($comparison.RemoteBranch)) does not hold the tree of the commit that was built."
        "  built commit : $($comparison.LocalCommit)  tree $($comparison.LocalTree)"
        "  GitHub commit: $($comparison.RemoteCommit)  tree $($comparison.RemoteTree)"
        ''
        "$($comparison.DifferenceCount) path(s) differ. A: missing on GitHub, D: only on GitHub, M: different content."
        $comparison.Differences | ForEach-Object { "  $_" }
        if ($comparison.DifferenceCount -gt $comparison.Differences.Count) {
            "  ... and $($comparison.DifferenceCount - $comparison.Differences.Count) more"
        }

        ''
        'See docs/development/github-mirror.md for what to do.'

        if ($onBuildAgent) {
            Write-Host "##vso[task.logissue type=error]The public repository does not hold what was built: $($comparison.DifferenceCount) path(s) differ between $($comparison.LocalCommit) and GitHub commit $($comparison.RemoteCommit). See docs/development/github-mirror.md."
        }

        exit 1
    }
}
