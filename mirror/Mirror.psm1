#Requires -Version 7.0
<#
    What the two mirror scripts share: reading the public GitHub repository, comparing
    trees, and deciding what a release has to publish.

    The public repository holds one commit per release. Each of them carries the complete
    tree of what was released and has the previous snapshot as its only parent, so no
    commit of the internal history ever leaves. Because the two histories share no commit,
    "is GitHub up to date" is a question about trees, not about commits.

    Nothing here writes to the repository it is run in. The GitHub side is fetched into a
    temporary bare repository that borrows the local objects through an alternates file,
    which is what lets git compare the two trees without copying either.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

class MirrorRemoteException : System.Exception {
    MirrorRemoteException([string] $message) : base($message) { }
}

class MirrorRefusedException : System.Exception {
    MirrorRefusedException([string] $message) : base($message) { }
}

class MirrorPushException : System.Exception {
    MirrorPushException([string] $message) : base($message) { }
}

function Invoke-MirrorGit {
    <#
    .SYNOPSIS
        Runs git and returns exit code, standard output and standard error separately.
    .DESCRIPTION
        Never prompts: a git that waits for a user name on a build agent hangs the job
        instead of failing it.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)] [string[]] $Arguments,
        [string] $WorkingDirectory,
        [hashtable] $Environment = @{},
        [switch] $AllowFailure
    )

    $startInfo = [System.Diagnostics.ProcessStartInfo]::new('git')
    foreach ($argument in $Arguments) {
        $startInfo.ArgumentList.Add($argument)
    }

    if ($WorkingDirectory) {
        $startInfo.WorkingDirectory = $WorkingDirectory
    }

    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $startInfo.UseShellExecute = $false
    $startInfo.Environment['GIT_TERMINAL_PROMPT'] = '0'
    $startInfo.Environment['LC_ALL'] = 'C'

    # A git hook or a caller may have set these for another repository.
    foreach ($inherited in 'GIT_DIR', 'GIT_WORK_TREE', 'GIT_INDEX_FILE', 'GIT_AUTHOR_NAME', 'GIT_AUTHOR_EMAIL', 'GIT_AUTHOR_DATE', 'GIT_COMMITTER_NAME', 'GIT_COMMITTER_EMAIL', 'GIT_COMMITTER_DATE') {
        $null = $startInfo.Environment.Remove($inherited)
    }

    foreach ($name in $Environment.Keys) {
        $startInfo.Environment[$name] = [string] $Environment[$name]
    }

    $process = [System.Diagnostics.Process]::Start($startInfo)
    try {
        $standardOutput = $process.StandardOutput.ReadToEndAsync()
        $standardError = $process.StandardError.ReadToEndAsync()
        $process.WaitForExit()

        $result = [pscustomobject]@{
            ExitCode = $process.ExitCode
            Output   = $standardOutput.GetAwaiter().GetResult().TrimEnd()
            Error    = $standardError.GetAwaiter().GetResult().TrimEnd()
        }
    }
    finally {
        $process.Dispose()
    }

    if ($result.ExitCode -ne 0 -and -not $AllowFailure) {
        throw "git $($Arguments -join ' ') failed with exit code $($result.ExitCode): $($result.Error)"
    }

    return $result
}

function Resolve-MirrorCommit {
    <#
    .SYNOPSIS
        The commit and tree a revision of the local repository stands for.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)] [string] $RepositoryPath,
        [Parameter(Mandatory)] [string] $Commit
    )

    $resolved = Invoke-MirrorGit -WorkingDirectory $RepositoryPath -Arguments 'rev-parse', '--verify', '--quiet', "$Commit^{commit}" -AllowFailure
    if ($resolved.ExitCode -ne 0) {
        throw "'$Commit' is not a commit in $RepositoryPath."
    }

    $tree = Invoke-MirrorGit -WorkingDirectory $RepositoryPath -Arguments 'rev-parse', '--verify', "$($resolved.Output)^{tree}"

    return [pscustomobject]@{
        Commit = $resolved.Output
        Tree   = $tree.Output
    }
}

function New-MirrorWorkspace {
    <#
    .SYNOPSIS
        Creates the temporary bare repository the GitHub side is fetched into.
    .DESCRIPTION
        Its alternates file points at the object store of the local repository, so
        every local commit and tree can be read from it while nothing is written to the
        local repository. Works for a linked worktree as well.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)] [string] $RepositoryPath
    )

    $repository = (Resolve-Path -LiteralPath $RepositoryPath).ProviderPath
    $objects = (Invoke-MirrorGit -WorkingDirectory $repository -Arguments 'rev-parse', '--git-path', 'objects').Output
    $objects = [System.IO.Path]::GetFullPath($objects, $repository)

    $workspace = Join-Path ([System.IO.Path]::GetTempPath()) ("mirror-" + [System.Guid]::NewGuid().ToString('N'))
    $null = New-Item -ItemType Directory -Path $workspace

    $null = Invoke-MirrorGit -Arguments 'init', '--quiet', '--bare', $workspace
    $alternates = Join-Path $workspace 'objects' 'info' 'alternates'
    [System.IO.File]::WriteAllText($alternates, $objects.Replace('\', '/') + "`n")

    # Borrow the objects, not the branches. git would otherwise offer the tips of the
    # local repository to the remote and walk their history when it checks a fetch.
    $null = Invoke-MirrorGit -WorkingDirectory $workspace -Arguments 'config', 'core.alternateRefsPrefixes', 'refs/mirror/none/'

    # A build agent often has a shallow clone. The workspace has to know where that
    # history stops, or git reports the missing parents as a broken fetch, which would
    # look like an unreachable GitHub.
    $shallow = (Invoke-MirrorGit -WorkingDirectory $repository -Arguments 'rev-parse', '--git-path', 'shallow').Output
    $shallow = [System.IO.Path]::GetFullPath($shallow, $repository)
    if (Test-Path -LiteralPath $shallow) {
        Copy-Item -LiteralPath $shallow -Destination (Join-Path $workspace 'shallow')
    }

    return $workspace
}

function Remove-MirrorWorkspace {
    [CmdletBinding()]
    param(
        [string] $Workspace
    )

    if ($Workspace -and (Test-Path -LiteralPath $Workspace)) {
        # git marks its object files read-only, which -Force gets past.
        Remove-Item -LiteralPath $Workspace -Recurse -Force -ErrorAction SilentlyContinue
    }
}

function Invoke-MirrorRemote {
    <#
    .SYNOPSIS
        Runs a git command that talks to the remote and retries it when it fails.
    .DESCRIPTION
        Throws a MirrorRemoteException once every attempt has failed. That exception is
        what tells "GitHub could not be reached" apart from "GitHub holds something
        else": a comparison is only ever made after this has succeeded.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)] [string[]] $Arguments,
        [Parameter(Mandatory)] [string] $Workspace,
        [Parameter(Mandatory)] [string] $RemoteUrl,
        [int] $RetryCount = 2,
        [double] $RetryDelaySeconds = 5
    )

    # Gives up on a connection that has stalled instead of waiting for it forever.
    $transport = '-c', 'http.lowSpeedLimit=1000', '-c', 'http.lowSpeedTime=30'

    $attempt = 0
    while ($true) {
        $attempt++
        $result = Invoke-MirrorGit -WorkingDirectory $Workspace -Arguments ($transport + $Arguments) -AllowFailure
        if ($result.ExitCode -eq 0) {
            return $result
        }

        if ($attempt -gt $RetryCount) {
            throw [MirrorRemoteException]::new("$RemoteUrl could not be read after $attempt attempt(s): $($result.Error)")
        }

        Start-Sleep -Seconds $RetryDelaySeconds
    }
}

function Get-MirrorRemoteHead {
    <#
    .SYNOPSIS
        Fetches the tip of the remote's default branch, or of the branch named, into the
        workspace and returns its commit and tree.
    .DESCRIPTION
        A shallow fetch of one commit is enough: only the tree at the tip matters.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)] [string] $Workspace,
        [Parameter(Mandatory)] [string] $RemoteUrl,
        [string] $Branch,
        [int] $RetryCount = 2,
        [double] $RetryDelaySeconds = 5
    )

    $retry = @{ Workspace = $Workspace; RemoteUrl = $RemoteUrl; RetryCount = $RetryCount; RetryDelaySeconds = $RetryDelaySeconds }

    if (-not $Branch) {
        $head = Invoke-MirrorRemote @retry -Arguments 'ls-remote', '--symref', $RemoteUrl, 'HEAD'
        $symref = $head.Output -split "`n" | Where-Object { $_ -match '^ref: refs/heads/(?<branch>\S+)\s+HEAD$' } | Select-Object -First 1
        if (-not $symref) {
            throw [MirrorRemoteException]::new("$RemoteUrl does not name a default branch. Is the repository empty?")
        }

        $null = $symref -match '^ref: refs/heads/(?<branch>\S+)\s+HEAD$'
        $Branch = $Matches['branch']
    }

    $null = Invoke-MirrorRemote @retry -Arguments 'fetch', '--quiet', '--no-tags', '--depth', '1', $RemoteUrl, "+refs/heads/${Branch}:refs/mirror/head"

    return [pscustomobject]@{
        Branch = $Branch
        Commit = (Invoke-MirrorGit -WorkingDirectory $Workspace -Arguments 'rev-parse', '--verify', 'refs/mirror/head^{commit}').Output
        Tree   = (Invoke-MirrorGit -WorkingDirectory $Workspace -Arguments 'rev-parse', '--verify', 'refs/mirror/head^{tree}').Output
    }
}

function Get-MirrorRemoteTag {
    <#
    .SYNOPSIS
        The commit and tree a tag of the remote points at, or $null when the remote has
        no such tag.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)] [string] $Workspace,
        [Parameter(Mandatory)] [string] $RemoteUrl,
        [Parameter(Mandatory)] [string] $Tag,
        [int] $RetryCount = 2,
        [double] $RetryDelaySeconds = 5
    )

    $retry = @{ Workspace = $Workspace; RemoteUrl = $RemoteUrl; RetryCount = $RetryCount; RetryDelaySeconds = $RetryDelaySeconds }

    $listed = Invoke-MirrorRemote @retry -Arguments 'ls-remote', $RemoteUrl, "refs/tags/$Tag"
    if (-not $listed.Output) {
        return $null
    }

    $null = Invoke-MirrorRemote @retry -Arguments 'fetch', '--quiet', '--no-tags', '--depth', '1', $RemoteUrl, "+refs/tags/${Tag}:refs/mirror/tag"

    return [pscustomobject]@{
        Tag    = $Tag
        Commit = (Invoke-MirrorGit -WorkingDirectory $Workspace -Arguments 'rev-parse', '--verify', 'refs/mirror/tag^{commit}').Output
        Tree   = (Invoke-MirrorGit -WorkingDirectory $Workspace -Arguments 'rev-parse', '--verify', 'refs/mirror/tag^{tree}').Output
    }
}

function Compare-MirrorTree {
    <#
    .SYNOPSIS
        Compares the tree of a local commit with the tree at the tip of the public
        repository.
    .OUTPUTS
        An object whose Status is 'Identical', 'Drift' or 'Unreachable'. 'Unreachable'
        means no comparison was made, which is not the same as a difference.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)] [string] $RepositoryPath,
        [Parameter(Mandatory)] [string] $Commit,
        [Parameter(Mandatory)] [string] $RemoteUrl,
        [string] $Branch,
        [int] $MaximumDifferences = 20,
        [int] $RetryCount = 2,
        [double] $RetryDelaySeconds = 5
    )

    $local = Resolve-MirrorCommit -RepositoryPath $RepositoryPath -Commit $Commit
    $workspace = New-MirrorWorkspace -RepositoryPath $RepositoryPath

    try {
        try {
            $remote = Get-MirrorRemoteHead -Workspace $workspace -RemoteUrl $RemoteUrl -Branch $Branch -RetryCount $RetryCount -RetryDelaySeconds $RetryDelaySeconds
        }
        catch [MirrorRemoteException] {
            return [pscustomobject]@{
                Status          = 'Unreachable'
                LocalCommit     = $local.Commit
                LocalTree       = $local.Tree
                RemoteBranch    = $Branch
                RemoteCommit    = $null
                RemoteTree      = $null
                Differences     = @()
                DifferenceCount = 0
                Message         = $_.Exception.Message
            }
        }

        $differences = @()
        if ($remote.Tree -ne $local.Tree) {
            # From GitHub to the local commit: A is missing on GitHub, D exists only on
            # GitHub, M differs.
            $listed = Invoke-MirrorGit -WorkingDirectory $workspace -Arguments 'diff-tree', '-r', '--name-status', $remote.Tree, $local.Tree
            $differences = @($listed.Output -split "`n" | Where-Object { $_ })
        }

        return [pscustomobject]@{
            Status          = if ($remote.Tree -eq $local.Tree) { 'Identical' } else { 'Drift' }
            LocalCommit     = $local.Commit
            LocalTree       = $local.Tree
            RemoteBranch    = $remote.Branch
            RemoteCommit    = $remote.Commit
            RemoteTree      = $remote.Tree
            Differences     = @($differences | Select-Object -First $MaximumDifferences)
            DifferenceCount = $differences.Count
            Message         = $null
        }
    }
    finally {
        Remove-MirrorWorkspace -Workspace $workspace
    }
}

function Get-MirrorReleaseVersion {
    <#
    .SYNOPSIS
        The version a commit declares in backend/Directory.Build.props.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)] [string] $RepositoryPath,
        [Parameter(Mandatory)] [string] $Commit
    )

    $properties = Invoke-MirrorGit -WorkingDirectory $RepositoryPath -Arguments 'show', "${Commit}:backend/Directory.Build.props" -AllowFailure
    if ($properties.ExitCode -ne 0) {
        throw "backend/Directory.Build.props cannot be read at ${Commit}: $($properties.Error)"
    }

    if ($properties.Output -notmatch '<Version>\s*(?<version>\d+\.\d+\.\d+)\s*</Version>') {
        throw "backend/Directory.Build.props at $Commit holds no <Version> of the form 1.2.3."
    }

    return $Matches['version']
}

function Get-MirrorPublishPlan {
    <#
    .SYNOPSIS
        Decides what has to be published. Pure: it looks at nothing but its arguments.
    .PARAMETER KnownTrees
        The trees of the released commit and of every commit before it. A GitHub tip
        whose tree is among them is an earlier release of this history and can be
        continued. Any other tip holds something that never was in this history, most
        likely a change made on GitHub directly, and publishing over it would bury it.
    .PARAMETER TagState
        'Missing' when GitHub has no tag for the version, 'SameTree' when its tag
        points at the tree being released, 'OtherTree' when it points at different
        content.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)] [string] $LocalTree,
        [Parameter(Mandatory)] [string] $RemoteTree,
        [Parameter(Mandatory)] [AllowEmptyCollection()] [string[]] $KnownTrees,
        [Parameter(Mandatory)] [string] $Version,
        [Parameter(Mandatory)] [ValidateSet('Missing', 'SameTree', 'OtherTree')] [string] $TagState,
        [bool] $IsShallow
    )

    $plan = {
        param($action, $reason, $warning)
        [pscustomobject]@{ Action = $action; Reason = $reason; Warning = $warning }
    }

    if ($RemoteTree -eq $LocalTree) {
        switch ($TagState) {
            'Missing' { return & $plan 'TagOnly' "GitHub already holds this tree but has no tag v$Version." $null }
            'SameTree' { return & $plan 'None' "GitHub already holds this tree and the tag v$Version." $null }
            default { return & $plan 'None' 'GitHub already holds this tree.' "The tag v$Version on GitHub points at different content than what was released as $Version. Tags are never moved; the next release needs a new version." }
        }
    }

    if ($KnownTrees -notcontains $RemoteTree) {
        $reason = "The tip of the GitHub branch holds a tree that is not the tree of any commit in the history being released. Somebody changed the public repository directly, or it was published from somewhere else. Publishing would bury that change, so nothing was pushed."
        if ($IsShallow) {
            $reason += ' The local repository is a shallow clone, so an older release may simply not have been looked at. Fetch the full history and run this again.'
        }

        return & $plan 'Refuse' $reason $null
    }

    if ($TagState -eq 'Missing') {
        return & $plan 'Snapshot' "GitHub holds an earlier release. Publishing $Version on top of it." $null
    }

    return & $plan 'SnapshotWithoutTag' "GitHub holds an earlier release. Publishing the released tree on top of it." "The tag v$Version already exists on GitHub, so this content is published without a tag. What was deployed differs from what was first released as $Version; the next release needs a new version."
}

function New-MirrorSshEnvironment {
    <#
    .SYNOPSIS
        Turns a base64 encoded private key into what git needs to push over SSH with it.
    .DESCRIPTION
        The key is written into the workspace, which is removed when the publish ends,
        and is readable by the current user only. The host key of the remote is pinned:
        an unknown host is refused instead of being trusted on first use, because a
        build agent sees every host for the first time.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)] [string] $Workspace,
        [Parameter(Mandatory)] [string] $DeployKeyBase64,
        [Parameter(Mandatory)] [string] $KnownHostsPath
    )

    try {
        $key = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($DeployKeyBase64.Trim()))
    }
    catch {
        # The value is a secret. It is not repeated, not even in part.
        throw 'The deploy key is not valid base64.'
    }

    if ($key -notmatch '^-----BEGIN [A-Z ]*PRIVATE KEY-----') {
        throw 'The deploy key does not decode to a private key.'
    }

    if (-not (Test-Path -LiteralPath $KnownHostsPath)) {
        throw "The known hosts file $KnownHostsPath does not exist."
    }

    $keyPath = Join-Path $Workspace 'deploy-key'
    [System.IO.File]::WriteAllText($keyPath, $key.Replace("`r`n", "`n").TrimEnd() + "`n")
    if (-not $IsWindows) {
        [System.IO.File]::SetUnixFileMode($keyPath, [System.IO.UnixFileMode]::UserRead -bor [System.IO.UnixFileMode]::UserWrite)
    }

    $quote = { param($path) "'" + $path.Replace('\', '/') + "'" }
    $knownHosts = (Resolve-Path -LiteralPath $KnownHostsPath).ProviderPath

    return @{
        GIT_SSH_COMMAND = "ssh -i $(& $quote $keyPath) -o IdentitiesOnly=yes -o StrictHostKeyChecking=yes -o UserKnownHostsFile=$(& $quote $knownHosts) -o BatchMode=yes"
    }
}

function Publish-MirrorSnapshot {
    <#
    .SYNOPSIS
        Publishes the tree of a released commit as one commit on top of the public
        repository's branch, and tags it with the version.
    .DESCRIPTION
        Fetching always uses RemoteUrl, which needs no credential for a public
        repository. Only the push uses PushUrl and the credential in PushEnvironment.

        Nothing is ever forced. If the branch moved between the fetch and the push the
        push is rejected and reported; running the publish again continues from the new
        tip.
    .OUTPUTS
        An object describing what was decided and done.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)] [string] $RepositoryPath,
        [Parameter(Mandatory)] [string] $Commit,
        [Parameter(Mandatory)] [string] $RemoteUrl,
        [string] $PushUrl,
        [string] $Branch,
        [Parameter(Mandatory)] [string] $AuthorName,
        [Parameter(Mandatory)] [string] $AuthorEmail,
        [hashtable] $PushEnvironment = @{},
        [string] $Workspace,
        [switch] $DryRun,
        [int] $RetryCount = 2,
        [double] $RetryDelaySeconds = 5
    )

    if (-not $PushUrl) {
        $PushUrl = $RemoteUrl
    }

    $retry = @{ RetryCount = $RetryCount; RetryDelaySeconds = $RetryDelaySeconds }
    $local = Resolve-MirrorCommit -RepositoryPath $RepositoryPath -Commit $Commit
    $version = Get-MirrorReleaseVersion -RepositoryPath $RepositoryPath -Commit $local.Commit
    $tag = "v$version"

    $ownsWorkspace = -not $Workspace
    if ($ownsWorkspace) {
        $Workspace = New-MirrorWorkspace -RepositoryPath $RepositoryPath
    }

    try {
        $remote = Get-MirrorRemoteHead -Workspace $Workspace -RemoteUrl $RemoteUrl -Branch $Branch @retry
        $remoteTag = Get-MirrorRemoteTag -Workspace $Workspace -RemoteUrl $RemoteUrl -Tag $tag @retry

        $tagState = if (-not $remoteTag) { 'Missing' } elseif ($remoteTag.Tree -eq $local.Tree) { 'SameTree' } else { 'OtherTree' }
        $knownTrees = @((Invoke-MirrorGit -WorkingDirectory $RepositoryPath -Arguments 'log', '--format=%T', $local.Commit).Output -split "`n" | Where-Object { $_ } | Select-Object -Unique)
        $isShallow = (Invoke-MirrorGit -WorkingDirectory $RepositoryPath -Arguments 'rev-parse', '--is-shallow-repository').Output -eq 'true'

        $plan = Get-MirrorPublishPlan -LocalTree $local.Tree -RemoteTree $remote.Tree -KnownTrees $knownTrees -Version $version -TagState $tagState -IsShallow $isShallow

        $result = [pscustomobject]@{
            Action          = $plan.Action
            Reason          = $plan.Reason
            Warning         = $plan.Warning
            Version         = $version
            Tag             = $tag
            Branch          = $remote.Branch
            LocalCommit     = $local.Commit
            LocalTree       = $local.Tree
            PreviousCommit  = $remote.Commit
            PublishedCommit = $null
            Pushed          = $false
        }

        if ($plan.Action -eq 'Refuse') {
            throw [MirrorRefusedException]::new($plan.Reason)
        }

        if ($plan.Action -eq 'None') {
            return $result
        }

        $identity = '-c', "user.name=$AuthorName", '-c', "user.email=$AuthorEmail"
        $references = @()

        if ($plan.Action -eq 'TagOnly') {
            $snapshot = $remote.Commit
        }
        else {
            $message = if ($plan.Action -eq 'Snapshot') { "chore(release): $version" } else { "chore(release): $version, content changed without a new version" }
            $snapshot = (Invoke-MirrorGit -WorkingDirectory $Workspace -Arguments ($identity + @('commit-tree', $local.Tree, '-p', $remote.Commit, '-m', $message))).Output
            $references += "${snapshot}:refs/heads/$($remote.Branch)"
        }

        if ($plan.Action -ne 'SnapshotWithoutTag') {
            $null = Invoke-MirrorGit -WorkingDirectory $Workspace -Arguments ($identity + @('tag', '--annotate', '--message', "ADO Auditor $version", $tag, $snapshot))
            $references += "refs/tags/${tag}:refs/tags/$tag"
        }

        $result.PublishedCommit = $snapshot

        if ($DryRun) {
            return $result
        }

        # No leading plus and no force option: a push that would discard anything on
        # GitHub is rejected by GitHub. Atomic, so branch and tag arrive together or
        # not at all.
        $push = Invoke-MirrorGit -WorkingDirectory $Workspace -Arguments (@('push', '--quiet', '--atomic', $PushUrl) + $references) -Environment $PushEnvironment -AllowFailure
        if ($push.ExitCode -ne 0) {
            throw [MirrorPushException]::new("Pushing to $PushUrl failed: $($push.Error)")
        }

        $result.Pushed = $true

        return $result
    }
    finally {
        if ($ownsWorkspace) {
            Remove-MirrorWorkspace -Workspace $Workspace
        }
    }
}

Export-ModuleMember -Function Invoke-MirrorGit, Resolve-MirrorCommit, New-MirrorWorkspace, Remove-MirrorWorkspace, Get-MirrorRemoteHead, Get-MirrorRemoteTag, Compare-MirrorTree, Get-MirrorReleaseVersion, Get-MirrorPublishPlan, New-MirrorSshEnvironment, Publish-MirrorSnapshot
