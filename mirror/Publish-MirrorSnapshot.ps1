#Requires -Version 7.0
using module ./Mirror.psm1

<#
.SYNOPSIS
    Publishes a released commit to the public GitHub repository as one snapshot commit.

.DESCRIPTION
    Takes the tree of the commit, commits it on top of the GitHub branch as
    "chore(release): <version>" and tags it "v<version>". The version comes from
    backend/Directory.Build.props at that commit. The snapshot commit has the previous
    snapshot as its only parent, so no commit of this repository's history is
    transferred.

    Does nothing when GitHub already holds the tree. Never forces a push. Refuses when
    the tip of the GitHub branch holds a tree that never existed in the history being
    released, because that is a change made on GitHub that publishing would bury.

    The credential is an SSH deploy key of the GitHub repository, handed in base64
    encoded through the environment variable MIRROR_DEPLOY_KEY_BASE64. Without it the
    script prints a warning and ends successfully without publishing: the drift check
    that follows is what reports the consequence.

    Exit codes:
      0  Published, nothing to do, or skipped because no credential is configured.
      2  GitHub could not be read.
      3  The publish could not run at all, for example because the version is missing.
      4  Refused: GitHub holds something this history does not know.
      5  The push was rejected.

.PARAMETER DryRun
    Decides and prints what would be published, without a credential and without
    pushing.

.EXAMPLE
    ./mirror/Publish-MirrorSnapshot.ps1 -Commit origin/main -DryRun
#>
[CmdletBinding()]
param(
    [string] $Commit = 'HEAD',

    [string] $RemoteUrl = 'https://github.com/IT-Titans/ado-auditor.git',

    [string] $PushUrl = 'git@github.com:IT-Titans/ado-auditor.git',

    [string] $Branch,

    [string] $AuthorName = $env:MIRROR_AUTHOR_NAME,

    [string] $AuthorEmail = $env:MIRROR_AUTHOR_EMAIL,

    [string] $DeployKeyBase64 = $env:MIRROR_DEPLOY_KEY_BASE64,

    [string] $KnownHostsPath = (Join-Path $PSScriptRoot 'github-known-hosts'),

    [string] $RepositoryPath = (Join-Path $PSScriptRoot '..'),

    [switch] $DryRun,

    [int] $RetryCount = 2,

    [double] $RetryDelaySeconds = 5
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$onBuildAgent = $env:TF_BUILD -eq 'True'

function Write-Issue([string] $type, [string] $message) {
    Write-Host $message
    if ($onBuildAgent) {
        Write-Host "##vso[task.logissue type=$type]$message"
    }
}

# A pipeline variable that is not defined arrives as its own name in macro syntax,
# for example "$(mirrorAuthorName)". That means "not set", the same as an empty value.
function Get-Configured([string] $value) {
    if ([string]::IsNullOrWhiteSpace($value) -or $value -match '^\$\(.*\)$') {
        return $null
    }

    return $value
}

$AuthorName = Get-Configured $AuthorName
$AuthorEmail = Get-Configured $AuthorEmail
$DeployKeyBase64 = Get-Configured $DeployKeyBase64

if (-not $DryRun -and -not $DeployKeyBase64) {
    Write-Issue 'warning' 'The release was NOT published to GitHub: no deploy key is configured (pipeline variable mirrorDeployKeyBase64). Publish it by hand or set the key up, see docs/development/github-mirror.md. The drift check that follows fails until GitHub holds this release.'
    if ($onBuildAgent) {
        Write-Host '##vso[task.complete result=SucceededWithIssues;]Not published, no deploy key'
    }

    exit 0
}

if ($DryRun) {
    # A dry run pushes nothing, so the identity only has to be well-formed.
    if (-not $AuthorName) { $AuthorName = 'dry run' }
    if (-not $AuthorEmail) { $AuthorEmail = 'dry-run@invalid' }
}

if (-not $AuthorName -or -not $AuthorEmail) {
    Write-Issue 'error' 'The release was NOT published to GitHub: the identity of the snapshot commit is not configured (pipeline variables mirrorAuthorName and mirrorAuthorEmail).'

    exit 3
}

$workspace = $null
try {
    $workspace = New-MirrorWorkspace -RepositoryPath $RepositoryPath

    $pushEnvironment = @{}
    if (-not $DryRun) {
        $pushEnvironment = New-MirrorSshEnvironment -Workspace $workspace -DeployKeyBase64 $DeployKeyBase64 -KnownHostsPath $KnownHostsPath
    }

    $published = Publish-MirrorSnapshot `
        -RepositoryPath $RepositoryPath `
        -Commit $Commit `
        -RemoteUrl $RemoteUrl `
        -PushUrl $PushUrl `
        -Branch $Branch `
        -AuthorName $AuthorName `
        -AuthorEmail $AuthorEmail `
        -PushEnvironment $pushEnvironment `
        -Workspace $workspace `
        -DryRun:$DryRun `
        -RetryCount $RetryCount `
        -RetryDelaySeconds $RetryDelaySeconds

    $published.Reason
    "  version         : $($published.Version)"
    "  released commit : $($published.LocalCommit)  tree $($published.LocalTree)"
    "  GitHub before   : $($published.PreviousCommit) ($($published.Branch))"

    switch ($published.Action) {
        'None' { '  nothing was pushed' }
        'TagOnly' { "  tag             : $($published.Tag) on $($published.PublishedCommit)" }
        'Snapshot' { "  snapshot commit : $($published.PublishedCommit), tag $($published.Tag)" }
        'SnapshotWithoutTag' { "  snapshot commit : $($published.PublishedCommit), no tag" }
    }

    if ($DryRun -and $published.Action -ne 'None') {
        '  dry run: nothing was pushed'
    }

    if ($published.Warning) {
        Write-Issue 'warning' $published.Warning
        if ($onBuildAgent) {
            Write-Host '##vso[task.complete result=SucceededWithIssues;]Published with a warning'
        }
    }

    exit 0
}
catch [MirrorRemoteException] {
    Write-Issue 'error' "The release was NOT published to GitHub: $($_.Exception.Message)"

    exit 2
}
catch [MirrorRefusedException] {
    Write-Issue 'error' "The release was NOT published to GitHub. $($_.Exception.Message) See docs/development/github-mirror.md."

    exit 4
}
catch [MirrorPushException] {
    Write-Issue 'error' "The release was NOT published to GitHub. $($_.Exception.Message)"

    exit 5
}
catch {
    Write-Issue 'error' "The release was NOT published to GitHub: $($_.Exception.Message)"

    exit 3
}
finally {
    # Removes the deploy key together with everything else that was fetched.
    Remove-MirrorWorkspace -Workspace $workspace
}
