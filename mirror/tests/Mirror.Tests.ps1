#Requires -Version 7.0
<#
    Everything here runs against git repositories created in a temporary directory. No
    test touches the network: "GitHub" is a local bare repository, and an unreachable
    GitHub is a path that does not exist.

    Run with Pester 5:  Invoke-Pester -Path mirror/tests
#>

BeforeAll {
    Import-Module (Join-Path $PSScriptRoot '..' 'Mirror.psm1') -Force

    $script:DriftScript = Join-Path $PSScriptRoot '..' 'Test-MirrorDrift.ps1'
    $script:PublishScript = Join-Path $PSScriptRoot '..' 'Publish-MirrorSnapshot.ps1'
    $script:Identity = '-c', 'user.name=Internal Developer', '-c', 'user.email=developer@internal.example', '-c', 'core.autocrlf=false', '-c', 'commit.gpgsign=false', '-c', 'tag.gpgsign=false'

    function Invoke-TestGit {
        param([string] $Repository, [Parameter(ValueFromRemainingArguments)] [string[]] $Arguments)

        $output = & git -C $Repository @script:Identity @Arguments 2>&1
        if ($LASTEXITCODE -ne 0) {
            throw "git $($Arguments -join ' ') failed in ${Repository}: $output"
        }

        return ($output -join "`n").Trim()
    }

    function Set-TestFile {
        param([string] $Repository, [string] $Path, [string] $Content)

        $full = Join-Path $Repository $Path
        $null = New-Item -ItemType Directory -Path (Split-Path $full) -Force
        [System.IO.File]::WriteAllText($full, $Content)
    }

    # Commits a release into the internal repository and returns its commit.
    function Add-TestRelease {
        param([string] $Repository, [string] $Version, [hashtable] $Files = @{})

        Set-TestFile $Repository 'backend/Directory.Build.props' "<Project><PropertyGroup><Version>$Version</Version></PropertyGroup></Project>`n"
        foreach ($path in $Files.Keys) {
            Set-TestFile $Repository $path $Files[$path]
        }

        $null = Invoke-TestGit $Repository add --all
        $null = Invoke-TestGit $Repository commit --quiet --message "internal work for $Version"

        return Invoke-TestGit $Repository rev-parse HEAD
    }

    # The internal repository, with two releases in its history.
    function New-InternalRepository {
        $path = Join-Path $TestDrive ("internal-" + [guid]::NewGuid().ToString('N'))
        $null = New-Item -ItemType Directory -Path $path
        $null = & git init --quiet --initial-branch main $path

        $first = Add-TestRelease $path '1.0.0' @{ 'README.md' = "first`n"; 'src/app.txt' = "one`n" }
        $second = Add-TestRelease $path '1.1.0' @{ 'README.md' = "second`n"; 'src/new.txt' = "new`n" }

        return [pscustomobject]@{ Path = $path; First = $first; Second = $second }
    }

    # "GitHub": a bare repository holding one snapshot commit with the tree of a commit
    # of the internal repository, and nothing of its history.
    function New-PublicRepository {
        param([string] $InternalRepository, [string] $Commit, [string] $Tag)

        $path = Join-Path $TestDrive ("public-" + [guid]::NewGuid().ToString('N') + '.git')
        $null = & git init --quiet --bare --initial-branch main $path

        if ($Commit) {
            $tree = Invoke-TestGit $InternalRepository rev-parse "$Commit^{tree}"
            $snapshot = Invoke-TestGit $InternalRepository commit-tree $tree -m 'chore(release): earlier'
            $null = Invoke-TestGit $InternalRepository push --quiet $path "${snapshot}:refs/heads/main"

            if ($Tag) {
                $null = Invoke-TestGit $path tag --annotate --message $Tag $Tag $snapshot
            }
        }

        return $path
    }

    # Somebody commits to GitHub directly.
    function Add-ForeignCommit {
        param([string] $PublicRepository)

        $clone = Join-Path $TestDrive ("clone-" + [guid]::NewGuid().ToString('N'))
        $null = & git clone --quiet $PublicRepository $clone 2>&1
        Set-TestFile $clone 'CONTRIBUTED.md' "made on GitHub`n"
        $null = Invoke-TestGit $clone add --all
        $null = Invoke-TestGit $clone commit --quiet --message 'docs: a change made on GitHub'
        $null = Invoke-TestGit $clone push --quiet origin main

        return Invoke-TestGit $clone rev-parse HEAD
    }

    function Get-PublicState {
        param([string] $PublicRepository)

        return (& git -C $PublicRepository for-each-ref --format='%(refname) %(objectname)') -join "`n"
    }

    function Get-ExceptionName {
        param([scriptblock] $Script)

        try {
            $null = & $Script
            return $null
        }
        catch {
            return $_.Exception.GetType().Name
        }
    }

    $script:Publish = @{ AuthorName = 'Release Publisher'; AuthorEmail = 'publisher@public.example'; RetryCount = 0; RetryDelaySeconds = 0 }
    $script:NoRetry = @{ RetryCount = 0; RetryDelaySeconds = 0 }
}

Describe 'Compare-MirrorTree' {
    BeforeEach {
        $internal = New-InternalRepository
    }

    It 'reports identical trees although the two commits differ' {
        $public = New-PublicRepository $internal.Path $internal.Second

        $comparison = Compare-MirrorTree -RepositoryPath $internal.Path -Commit $internal.Second -RemoteUrl $public @NoRetry

        $comparison.Status | Should -Be 'Identical'
        $comparison.RemoteCommit | Should -Not -Be $comparison.LocalCommit
        $comparison.RemoteTree | Should -Be $comparison.LocalTree
        $comparison.RemoteBranch | Should -Be 'main'
        $comparison.Differences | Should -BeNullOrEmpty
    }

    It 'reports drift and names both commits and the differing paths' {
        $public = New-PublicRepository $internal.Path $internal.First

        $comparison = Compare-MirrorTree -RepositoryPath $internal.Path -Commit $internal.Second -RemoteUrl $public @NoRetry

        $comparison.Status | Should -Be 'Drift'
        $comparison.LocalCommit | Should -Be $internal.Second
        $comparison.RemoteCommit | Should -Not -BeNullOrEmpty
        $comparison.DifferenceCount | Should -Be 3
        $comparison.Differences | Should -Contain "M`tREADME.md"
        $comparison.Differences | Should -Contain "M`tbackend/Directory.Build.props"
        $comparison.Differences | Should -Contain "A`tsrc/new.txt"
    }

    It 'marks a file that exists only on GitHub as D' {
        $public = New-PublicRepository $internal.Path $internal.Second

        $comparison = Compare-MirrorTree -RepositoryPath $internal.Path -Commit $internal.First -RemoteUrl $public @NoRetry

        $comparison.Differences | Should -Contain "D`tsrc/new.txt"
    }

    It 'lists only the first paths and still counts all of them' {
        $public = New-PublicRepository $internal.Path $internal.First

        $comparison = Compare-MirrorTree -RepositoryPath $internal.Path -Commit $internal.Second -RemoteUrl $public -MaximumDifferences 1 @NoRetry

        $comparison.Differences.Count | Should -Be 1
        $comparison.DifferenceCount | Should -Be 3
    }

    It 'reports an unreachable remote as unreachable, not as drift' {
        $missing = Join-Path $TestDrive 'no-such-repository.git'

        $comparison = Compare-MirrorTree -RepositoryPath $internal.Path -Commit $internal.Second -RemoteUrl $missing @NoRetry

        $comparison.Status | Should -Be 'Unreachable'
        $comparison.RemoteCommit | Should -BeNullOrEmpty
        $comparison.Message | Should -Match 'could not be read after 1 attempt'
    }

    It 'reports an empty remote as unreachable' {
        $public = New-PublicRepository

        $comparison = Compare-MirrorTree -RepositoryPath $internal.Path -Commit $internal.Second -RemoteUrl $public @NoRetry

        $comparison.Status | Should -Be 'Unreachable'
        $comparison.Message | Should -Match 'default branch'
    }

    It 'reports a branch the remote does not have as unreachable' {
        $public = New-PublicRepository $internal.Path $internal.Second

        $comparison = Compare-MirrorTree -RepositoryPath $internal.Path -Commit $internal.Second -RemoteUrl $public -Branch 'release' @NoRetry

        $comparison.Status | Should -Be 'Unreachable'
    }

    It 'retries before it gives up' {
        $missing = Join-Path $TestDrive 'no-such-repository.git'

        $comparison = Compare-MirrorTree -RepositoryPath $internal.Path -Commit $internal.Second -RemoteUrl $missing -RetryCount 2 -RetryDelaySeconds 0

        $comparison.Message | Should -Match 'after 3 attempt'
    }

    It 'compares from a shallow clone, which is what a build agent usually has' {
        # A shallow clone lacks the parents of its tip. That must not be mistaken for a
        # fetch that failed, or drift would be reported as an unreachable GitHub.
        $behind = New-PublicRepository $internal.Path $internal.First
        $upToDate = New-PublicRepository $internal.Path $internal.Second
        $shallow = Join-Path $TestDrive ("shallow-" + [guid]::NewGuid().ToString('N'))
        $null = & git clone --quiet --depth 1 ("file:///" + $internal.Path.Replace('\', '/').TrimStart('/')) $shallow 2>&1

        (Compare-MirrorTree -RepositoryPath $shallow -Commit 'HEAD' -RemoteUrl $behind @NoRetry).Status | Should -Be 'Drift'
        (Compare-MirrorTree -RepositoryPath $shallow -Commit 'HEAD' -RemoteUrl $upToDate @NoRetry).Status | Should -Be 'Identical'
    }

    It 'writes nothing to the repository it runs in' {
        $public = New-PublicRepository $internal.Path $internal.First
        $before = (& git -C $internal.Path for-each-ref) + (& git -C $internal.Path count-objects -v)

        $null = Compare-MirrorTree -RepositoryPath $internal.Path -Commit $internal.Second -RemoteUrl $public @NoRetry

        $after = (& git -C $internal.Path for-each-ref) + (& git -C $internal.Path count-objects -v)
        $after | Should -Be $before
        Join-Path $internal.Path '.git' 'FETCH_HEAD' | Should -Not -Exist
    }

    It 'works from a linked worktree' {
        $public = New-PublicRepository $internal.Path $internal.Second
        $worktree = Join-Path $TestDrive ("worktree-" + [guid]::NewGuid().ToString('N'))
        $null = Invoke-TestGit $internal.Path worktree add --quiet --detach $worktree $internal.Second

        $comparison = Compare-MirrorTree -RepositoryPath $worktree -Commit 'HEAD' -RemoteUrl $public @NoRetry

        $comparison.Status | Should -Be 'Identical'
    }

    It 'leaves no temporary repository behind' {
        $public = New-PublicRepository $internal.Path $internal.First
        $before = @(Get-ChildItem ([System.IO.Path]::GetTempPath()) -Directory -Filter 'mirror-*').Count

        $null = Compare-MirrorTree -RepositoryPath $internal.Path -Commit $internal.Second -RemoteUrl $public @NoRetry

        @(Get-ChildItem ([System.IO.Path]::GetTempPath()) -Directory -Filter 'mirror-*').Count | Should -Be $before
    }

    It 'fails for a commit that does not exist' {
        $public = New-PublicRepository $internal.Path $internal.Second

        { Compare-MirrorTree -RepositoryPath $internal.Path -Commit 'no-such-commit' -RemoteUrl $public @NoRetry } | Should -Throw '*is not a commit*'
    }
}

Describe 'Get-MirrorReleaseVersion' {
    It 'reads the version of the commit, not of the working tree' {
        $internal = New-InternalRepository

        Get-MirrorReleaseVersion -RepositoryPath $internal.Path -Commit $internal.First | Should -Be '1.0.0'
        Get-MirrorReleaseVersion -RepositoryPath $internal.Path -Commit $internal.Second | Should -Be '1.1.0'
    }

    It 'fails when the commit declares no version' {
        $internal = New-InternalRepository
        Set-TestFile $internal.Path 'backend/Directory.Build.props' "<Project />`n"
        $null = Invoke-TestGit $internal.Path commit --quiet --all --message 'drop the version'

        { Get-MirrorReleaseVersion -RepositoryPath $internal.Path -Commit 'HEAD' } | Should -Throw '*holds no <Version>*'
    }
}

Describe 'Get-MirrorPublishPlan' {
    BeforeAll {
        $script:Plan = @{ LocalTree = 'tree-new'; KnownTrees = @('tree-new', 'tree-old'); Version = '1.1.0' }
    }

    It 'does nothing when GitHub holds the tree and the tag' {
        $plan = Get-MirrorPublishPlan @Plan -RemoteTree 'tree-new' -TagState 'SameTree'

        $plan.Action | Should -Be 'None'
        $plan.Warning | Should -BeNullOrEmpty
    }

    It 'adds only the tag when GitHub holds the tree without it' {
        (Get-MirrorPublishPlan @Plan -RemoteTree 'tree-new' -TagState 'Missing').Action | Should -Be 'TagOnly'
    }

    It 'does not move a tag that points elsewhere' {
        $plan = Get-MirrorPublishPlan @Plan -RemoteTree 'tree-new' -TagState 'OtherTree'

        $plan.Action | Should -Be 'None'
        $plan.Warning | Should -Match 'never moved'
    }

    It 'publishes a snapshot with a tag on top of an earlier release' {
        (Get-MirrorPublishPlan @Plan -RemoteTree 'tree-old' -TagState 'Missing').Action | Should -Be 'Snapshot'
    }

    It 'publishes without a tag when the version was released before with other content' {
        $plan = Get-MirrorPublishPlan @Plan -RemoteTree 'tree-old' -TagState 'OtherTree'

        $plan.Action | Should -Be 'SnapshotWithoutTag'
        $plan.Warning | Should -Match 'needs a new version'
    }

    It 'refuses when GitHub holds a tree this history never had' {
        $plan = Get-MirrorPublishPlan @Plan -RemoteTree 'tree-foreign' -TagState 'Missing'

        $plan.Action | Should -Be 'Refuse'
        $plan.Reason | Should -Match 'nothing was pushed'
        $plan.Reason | Should -Not -Match 'shallow'
    }

    It 'says so when a shallow clone may be the reason for refusing' {
        $plan = Get-MirrorPublishPlan @Plan -RemoteTree 'tree-foreign' -TagState 'Missing' -IsShallow $true

        $plan.Action | Should -Be 'Refuse'
        $plan.Reason | Should -Match 'shallow clone'
    }
}

Describe 'Publish-MirrorSnapshot' {
    BeforeEach {
        $internal = New-InternalRepository
    }

    Context 'when GitHub holds the previous release' {
        BeforeEach {
            $public = New-PublicRepository $internal.Path $internal.First 'v1.0.0'
            $previous = Invoke-TestGit $public rev-parse main
            $published = Publish-MirrorSnapshot -RepositoryPath $internal.Path -Commit $internal.Second -RemoteUrl $public @Publish
        }

        It 'pushes one commit with the released tree on top of the previous snapshot' {
            $published.Action | Should -Be 'Snapshot'
            $published.Pushed | Should -BeTrue
            Invoke-TestGit $public rev-parse main | Should -Be $published.PublishedCommit
            Invoke-TestGit $public rev-parse 'main^{tree}' | Should -Be (Invoke-TestGit $internal.Path rev-parse "$($internal.Second)^{tree}")
            Invoke-TestGit $public rev-parse 'main^' | Should -Be $previous
            Invoke-TestGit $public rev-list --count main | Should -Be '2'
        }

        It 'names the commit after the version read from the released commit' {
            Invoke-TestGit $public log -1 --format=%s main | Should -Be 'chore(release): 1.1.0'
        }

        It 'uses the configured identity as author and committer' {
            Invoke-TestGit $public log -1 '--format=%an <%ae> / %cn <%ce>' main | Should -Be 'Release Publisher <publisher@public.example> / Release Publisher <publisher@public.example>'
        }

        It 'tags the snapshot with an annotated tag' {
            Invoke-TestGit $public rev-parse 'v1.1.0^{commit}' | Should -Be $published.PublishedCommit
            Invoke-TestGit $public cat-file -t v1.1.0 | Should -Be 'tag'
            Invoke-TestGit $public for-each-ref '--format=%(contents:subject) / %(taggername)' refs/tags/v1.1.0 | Should -Be 'ADO Auditor 1.1.0 / Release Publisher'
        }

        It 'transfers no commit of the internal history' {
            foreach ($internalCommit in $internal.First, $internal.Second) {
                & git -C $public cat-file -e "$internalCommit^{commit}" 2>$null
                $LASTEXITCODE | Should -Not -Be 0
            }

            Invoke-TestGit $public log --format=%s main | Should -Not -Match 'internal work'
        }

        It 'leaves GitHub identical to the release' {
            (Compare-MirrorTree -RepositoryPath $internal.Path -Commit $internal.Second -RemoteUrl $public @NoRetry).Status | Should -Be 'Identical'
        }

        It 'does nothing when run a second time' {
            $state = Get-PublicState $public

            $again = Publish-MirrorSnapshot -RepositoryPath $internal.Path -Commit $internal.Second -RemoteUrl $public @Publish

            $again.Action | Should -Be 'None'
            $again.Pushed | Should -BeFalse
            Get-PublicState $public | Should -Be $state
        }
    }

    It 'adds only the missing tag when GitHub already holds the tree' {
        $public = New-PublicRepository $internal.Path $internal.Second
        $tip = Invoke-TestGit $public rev-parse main

        $published = Publish-MirrorSnapshot -RepositoryPath $internal.Path -Commit $internal.Second -RemoteUrl $public @Publish

        $published.Action | Should -Be 'TagOnly'
        Invoke-TestGit $public rev-parse main | Should -Be $tip
        Invoke-TestGit $public rev-parse 'v1.1.0^{commit}' | Should -Be $tip
    }

    It 'refuses when somebody committed to GitHub directly, and pushes nothing' {
        $public = New-PublicRepository $internal.Path $internal.First 'v1.0.0'
        $null = Add-ForeignCommit $public
        $state = Get-PublicState $public

        Get-ExceptionName { Publish-MirrorSnapshot -RepositoryPath $internal.Path -Commit $internal.Second -RemoteUrl $public @Publish } | Should -Be 'MirrorRefusedException'

        Get-PublicState $public | Should -Be $state
    }

    It 'continues once the change made on GitHub has been taken over internally' {
        $public = New-PublicRepository $internal.Path $internal.Second 'v1.1.0'
        $null = Add-ForeignCommit $public

        # The same content arrives in the internal history, followed by the next release.
        Set-TestFile $internal.Path 'CONTRIBUTED.md' "made on GitHub`n"
        $null = Invoke-TestGit $internal.Path add --all
        $null = Invoke-TestGit $internal.Path commit --quiet --message 'docs: take over the change made on GitHub'
        $release = Add-TestRelease $internal.Path '1.2.0' @{ 'README.md' = "third`n" }

        $published = Publish-MirrorSnapshot -RepositoryPath $internal.Path -Commit $release -RemoteUrl $public @Publish

        $published.Action | Should -Be 'Snapshot'
        Invoke-TestGit $public rev-list --count main | Should -Be '3'
        Invoke-TestGit $public rev-parse 'v1.2.0^{commit}' | Should -Be $published.PublishedCommit
    }

    It 'continues once the change made on GitHub has been reverted there' {
        # The revert puts the tree of the last snapshot back at the tip, and that tree is
        # one this history knows.
        $public = New-PublicRepository $internal.Path $internal.Second 'v1.1.0'
        $null = Add-ForeignCommit $public
        $clone = Join-Path $TestDrive ("revert-" + [guid]::NewGuid().ToString('N'))
        $null = & git clone --quiet $public $clone 2>&1
        $null = Invoke-TestGit $clone revert --no-edit HEAD
        $null = Invoke-TestGit $clone push --quiet origin main
        $release = Add-TestRelease $internal.Path '1.2.0' @{ 'README.md' = "third`n" }

        $published = Publish-MirrorSnapshot -RepositoryPath $internal.Path -Commit $release -RemoteUrl $public @Publish

        $published.Action | Should -Be 'Snapshot'
        Invoke-TestGit $public rev-list --count main | Should -Be '4'
    }

    It 'does not publish an older release over a newer one' {
        $public = New-PublicRepository $internal.Path $internal.Second 'v1.1.0'
        $state = Get-PublicState $public

        Get-ExceptionName { Publish-MirrorSnapshot -RepositoryPath $internal.Path -Commit $internal.First -RemoteUrl $public @Publish } | Should -Be 'MirrorRefusedException'

        Get-PublicState $public | Should -Be $state
    }

    It 'publishes without a tag and warns when the version was not raised' {
        $public = New-PublicRepository $internal.Path $internal.Second 'v1.1.0'
        $tagBefore = Invoke-TestGit $public rev-parse v1.1.0
        $hotfix = Add-TestRelease $internal.Path '1.1.0' @{ 'src/app.txt' = "fixed`n" }

        $published = Publish-MirrorSnapshot -RepositoryPath $internal.Path -Commit $hotfix -RemoteUrl $public @Publish

        $published.Action | Should -Be 'SnapshotWithoutTag'
        $published.Warning | Should -Match 'already exists on GitHub'
        Invoke-TestGit $public rev-parse v1.1.0 | Should -Be $tagBefore
        Invoke-TestGit $public rev-parse 'main^{tree}' | Should -Be (Invoke-TestGit $internal.Path rev-parse "$hotfix^{tree}")
        Invoke-TestGit $public log -1 --format=%s main | Should -Be 'chore(release): 1.1.0, content changed without a new version'
    }

    It 'pushes nothing in a dry run and still says what it would do' {
        $public = New-PublicRepository $internal.Path $internal.First 'v1.0.0'
        $state = Get-PublicState $public

        $published = Publish-MirrorSnapshot -RepositoryPath $internal.Path -Commit $internal.Second -RemoteUrl $public -DryRun @Publish

        $published.Action | Should -Be 'Snapshot'
        $published.Pushed | Should -BeFalse
        Get-PublicState $public | Should -Be $state
    }

    It 'reports an unreachable GitHub as such' {
        $missing = Join-Path $TestDrive 'no-such-repository.git'

        Get-ExceptionName { Publish-MirrorSnapshot -RepositoryPath $internal.Path -Commit $internal.Second -RemoteUrl $missing @Publish } | Should -Be 'MirrorRemoteException'
    }

    It 'never forces: a push that GitHub rejects fails and changes nothing' {
        # Reads from one repository and pushes to another whose branch has moved on, which
        # is what a second publisher finishing in between looks like.
        $public = New-PublicRepository $internal.Path $internal.First 'v1.0.0'
        $moved = New-PublicRepository $internal.Path $internal.First 'v1.0.0'
        $null = Add-ForeignCommit $moved
        $state = Get-PublicState $moved

        Get-ExceptionName { Publish-MirrorSnapshot -RepositoryPath $internal.Path -Commit $internal.Second -RemoteUrl $public -PushUrl $moved @Publish } | Should -Be 'MirrorPushException'

        Get-PublicState $moved | Should -Be $state
    }

    It 'pushes branch and tag together or not at all' {
        # The tag already exists at the push target, so the tag is rejected. The branch
        # must then not move either.
        $public = New-PublicRepository $internal.Path $internal.First 'v1.0.0'
        $target = New-PublicRepository $internal.Path $internal.First 'v1.1.0'
        $state = Get-PublicState $target

        Get-ExceptionName { Publish-MirrorSnapshot -RepositoryPath $internal.Path -Commit $internal.Second -RemoteUrl $public -PushUrl $target @Publish } | Should -Be 'MirrorPushException'

        Get-PublicState $target | Should -Be $state
    }

    It 'says that a shallow clone may be why it refuses' {
        $public = New-PublicRepository $internal.Path $internal.First 'v1.0.0'
        $shallow = Join-Path $TestDrive ("shallow-" + [guid]::NewGuid().ToString('N'))
        $null = & git clone --quiet --depth 1 ("file:///" + $internal.Path.Replace('\', '/').TrimStart('/')) $shallow 2>&1

        $message = try { $null = Publish-MirrorSnapshot -RepositoryPath $shallow -Commit 'HEAD' -RemoteUrl $public @Publish } catch { $_.Exception.Message }

        $message | Should -Match 'shallow clone'
    }

    It 'fails before touching GitHub when the commit declares no version' {
        $public = New-PublicRepository $internal.Path $internal.First 'v1.0.0'
        Set-TestFile $internal.Path 'backend/Directory.Build.props' "<Project />`n"
        $null = Invoke-TestGit $internal.Path commit --quiet --all --message 'drop the version'
        $state = Get-PublicState $public

        { Publish-MirrorSnapshot -RepositoryPath $internal.Path -Commit 'HEAD' -RemoteUrl $public @Publish } | Should -Throw '*holds no <Version>*'

        Get-PublicState $public | Should -Be $state
    }
}

Describe 'New-MirrorSshEnvironment' {
    BeforeEach {
        $workspace = Join-Path $TestDrive ("workspace-" + [guid]::NewGuid().ToString('N'))
        $null = New-Item -ItemType Directory -Path $workspace
        $knownHosts = Join-Path $PSScriptRoot '..' 'github-known-hosts'
        $key = "-----BEGIN OPENSSH PRIVATE KEY-----`r`nbm90IGEgcmVhbCBrZXk=`r`n-----END OPENSSH PRIVATE KEY-----`r`n"
        $encoded = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($key))
    }

    It 'writes the key into the workspace with unix line endings' {
        $null = New-MirrorSshEnvironment -Workspace $workspace -DeployKeyBase64 $encoded -KnownHostsPath $knownHosts

        [System.IO.File]::ReadAllText((Join-Path $workspace 'deploy-key')) | Should -Be "-----BEGIN OPENSSH PRIVATE KEY-----`nbm90IGEgcmVhbCBrZXk=`n-----END OPENSSH PRIVATE KEY-----`n"
    }

    It 'makes git use that key only and refuse a host it does not know' {
        $environment = New-MirrorSshEnvironment -Workspace $workspace -DeployKeyBase64 $encoded -KnownHostsPath $knownHosts

        $environment.GIT_SSH_COMMAND | Should -Match "-i '.*deploy-key'"
        $environment.GIT_SSH_COMMAND | Should -Match 'IdentitiesOnly=yes'
        $environment.GIT_SSH_COMMAND | Should -Match 'StrictHostKeyChecking=yes'
        $environment.GIT_SSH_COMMAND | Should -Match "UserKnownHostsFile='.*github-known-hosts'"
        $environment.GIT_SSH_COMMAND | Should -Match 'BatchMode=yes'
    }

    It 'removes the key together with the workspace' {
        $null = New-MirrorSshEnvironment -Workspace $workspace -DeployKeyBase64 $encoded -KnownHostsPath $knownHosts

        Remove-MirrorWorkspace -Workspace $workspace

        $workspace | Should -Not -Exist
    }

    It 'rejects a value that is not base64 without repeating it' {
        $secret = 'this is !!! not base64'

        $message = try { $null = New-MirrorSshEnvironment -Workspace $workspace -DeployKeyBase64 $secret -KnownHostsPath $knownHosts } catch { $_.Exception.Message }

        $message | Should -Be 'The deploy key is not valid base64.'
    }

    It 'rejects a value that is not a private key without repeating it' {
        $secret = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes('ssh-ed25519 AAAA public-not-private'))

        $message = try { $null = New-MirrorSshEnvironment -Workspace $workspace -DeployKeyBase64 $secret -KnownHostsPath $knownHosts } catch { $_.Exception.Message }

        $message | Should -Be 'The deploy key does not decode to a private key.'
        Join-Path $workspace 'deploy-key' | Should -Not -Exist
    }

    It 'pins the three host keys GitHub publishes' {
        $lines = Get-Content $knownHosts

        $lines.Count | Should -Be 3
        $lines | ForEach-Object { $_ | Should -Match '^github\.com (ssh-ed25519|ecdsa-sha2-nistp256|ssh-rsa) [A-Za-z0-9+/=]+$' }
    }
}

Describe 'Test-MirrorDrift.ps1' {
    BeforeAll {
        $script:Internal = New-InternalRepository
        $script:UpToDate = New-PublicRepository $Internal.Path $Internal.Second
        $script:Behind = New-PublicRepository $Internal.Path $Internal.First
        $script:Missing = Join-Path $TestDrive 'no-such-repository.git'

        function Invoke-DriftScript {
            param([string] $RemoteUrl, [string] $Commit = $Internal.Second, [string] $TfBuild = '')

            $env:TF_BUILD = $TfBuild
            try {
                $output = & pwsh -NoProfile -File $DriftScript -RepositoryPath $Internal.Path -Commit $Commit -RemoteUrl $RemoteUrl -RetryCount 0 -RetryDelaySeconds 0 2>&1
                return [pscustomobject]@{ ExitCode = $LASTEXITCODE; Output = ($output -join "`n") }
            }
            finally {
                $env:TF_BUILD = $null
            }
        }
    }

    It 'exits with 0 when GitHub holds the tree' {
        $run = Invoke-DriftScript $UpToDate

        $run.ExitCode | Should -Be 0
        $run.Output | Should -Match 'No drift'
    }

    It 'exits with 1 on drift and names both commits and the paths' {
        $run = Invoke-DriftScript $Behind

        $run.ExitCode | Should -Be 1
        $run.Output | Should -Match 'DRIFT'
        $run.Output | Should -Match $Internal.Second
        $run.Output | Should -Match (Invoke-TestGit $Behind rev-parse main)
        $run.Output | Should -Match 'src/new\.txt'
    }

    It 'exits with 2 when GitHub cannot be read' {
        $run = Invoke-DriftScript $Missing

        $run.ExitCode | Should -Be 2
        $run.Output | Should -Match 'Not checked'
        $run.Output | Should -Not -Match 'DRIFT'
    }

    It 'exits with 3 when the commit does not exist' {
        (Invoke-DriftScript $UpToDate -Commit 'no-such-commit').ExitCode | Should -Be 3
    }

    It 'fails the step on a build agent when there is drift' {
        $run = Invoke-DriftScript $Behind -TfBuild 'True'

        $run.ExitCode | Should -Be 1
        $run.Output | Should -Match '##vso\[task\.logissue type=error\]'
    }

    It 'completes the step with a warning on a build agent when GitHub cannot be read' {
        $run = Invoke-DriftScript $Missing -TfBuild 'True'

        $run.ExitCode | Should -Be 0
        $run.Output | Should -Match '##vso\[task\.logissue type=warning\]'
        $run.Output | Should -Match '##vso\[task\.complete result=SucceededWithIssues;\]'
        $run.Output | Should -Not -Match 'type=error'
    }
}

Describe 'Publish-MirrorSnapshot.ps1' {
    BeforeAll {
        $script:Internal = New-InternalRepository

        function Invoke-PublishScript {
            param([string] $RemoteUrl, [string] $Commit = $Internal.Second, [string] $DeployKey = '', [string] $TfBuild = '', [switch] $DryRun)

            $env:TF_BUILD = $TfBuild
            $env:MIRROR_DEPLOY_KEY_BASE64 = $DeployKey
            $env:MIRROR_AUTHOR_NAME = 'Release Publisher'
            $env:MIRROR_AUTHOR_EMAIL = 'publisher@public.example'
            try {
                $arguments = @('-RepositoryPath', $Internal.Path, '-Commit', $Commit, '-RemoteUrl', $RemoteUrl, '-PushUrl', $RemoteUrl, '-RetryCount', '0', '-RetryDelaySeconds', '0')
                if ($DryRun) {
                    $arguments += '-DryRun'
                }

                $output = & pwsh -NoProfile -File $PublishScript @arguments 2>&1
                return [pscustomobject]@{ ExitCode = $LASTEXITCODE; Output = ($output -join "`n") }
            }
            finally {
                $env:TF_BUILD = $null
                $env:MIRROR_DEPLOY_KEY_BASE64 = $null
                $env:MIRROR_AUTHOR_NAME = $null
                $env:MIRROR_AUTHOR_EMAIL = $null
            }
        }
    }

    It 'skips with a warning and without failing when no deploy key is configured' {
        $public = New-PublicRepository $Internal.Path $Internal.First 'v1.0.0'
        $state = Get-PublicState $public

        $run = Invoke-PublishScript $public -TfBuild 'True'

        $run.ExitCode | Should -Be 0
        $run.Output | Should -Match 'NOT published'
        $run.Output | Should -Match '##vso\[task\.logissue type=warning\]'
        $run.Output | Should -Match '##vso\[task\.complete result=SucceededWithIssues;\]'
        Get-PublicState $public | Should -Be $state
    }

    It 'treats a pipeline variable that was never defined as not configured' {
        $public = New-PublicRepository $Internal.Path $Internal.First 'v1.0.0'

        $run = Invoke-PublishScript $public -DeployKey '$(mirrorDeployKeyBase64)'

        $run.ExitCode | Should -Be 0
        $run.Output | Should -Match 'no deploy key is configured'
    }

    It 'says in a dry run what it would publish, without a key' {
        $public = New-PublicRepository $Internal.Path $Internal.First 'v1.0.0'
        $state = Get-PublicState $public

        $run = Invoke-PublishScript $public -DryRun

        $run.ExitCode | Should -Be 0
        $run.Output | Should -Match 'Publishing 1\.1\.0 on top'
        $run.Output | Should -Match 'dry run: nothing was pushed'
        Get-PublicState $public | Should -Be $state
    }

    It 'exits with 4 when it refuses' {
        $public = New-PublicRepository $Internal.Path $Internal.First 'v1.0.0'
        $null = Add-ForeignCommit $public

        $run = Invoke-PublishScript $public -DryRun

        $run.ExitCode | Should -Be 4
        $run.Output | Should -Match 'NOT published'
    }

    It 'exits with 2 when GitHub cannot be read' {
        (Invoke-PublishScript (Join-Path $TestDrive 'no-such-repository.git') -DryRun).ExitCode | Should -Be 2
    }

    It 'rejects a deploy key that is not one, without printing it' {
        $public = New-PublicRepository $Internal.Path $Internal.First 'v1.0.0'
        $secret = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes('definitely-a-secret-value'))

        $run = Invoke-PublishScript $public -DeployKey $secret

        $run.ExitCode | Should -Be 3
        $run.Output | Should -Not -Match 'definitely-a-secret-value'
        $run.Output | Should -Not -Match ([regex]::Escape($secret))
    }
}

Describe 'azure-pipelines.yml' {
    BeforeAll {
        $script:Pipeline = Get-Content (Join-Path $PSScriptRoot '..' '..' 'azure-pipelines.yml') -Raw
        $script:Stages = [regex]::Matches($Pipeline, '(?m)^- stage: (?<name>\w+)') | ForEach-Object { $_.Groups['name'].Value }
        $script:MirrorStage = ($Pipeline -split '(?m)^- stage: ' | Where-Object { $_ -match '^mirror\b' })
    }

    It 'has the mirror stage as its last stage' {
        $Stages[-1] | Should -Be 'mirror'
    }

    It 'lets no stage depend on the mirror stage, so it cannot block anything' {
        $Pipeline | Should -Not -Match '(?m)^\s*dependsOn:\s*mirror\b'
        $Pipeline | Should -Not -Match '(?m)^\s*-\s*mirror\s*$'
    }

    It 'runs the mirror stage after the production deployment and only for main' {
        $MirrorStage | Should -Match '(?m)^  dependsOn: deploy_prod$'
        $MirrorStage | Should -Match "(?m)^  condition: and\(succeeded\(\), eq\(variables\['Build\.SourceBranch'\], 'refs/heads/main'\)\)$"
    }

    It 'runs the drift check even when publishing failed' {
        $MirrorStage | Should -Match '(?s)Test-MirrorDrift\.ps1'
        $MirrorStage | Should -Match '(?s)condition: succeededOrFailed\(\).*Test-MirrorDrift\.ps1'
    }

    It 'hands the deploy key to the publish step through the environment only' {
        $MirrorStage | Should -Match 'MIRROR_DEPLOY_KEY_BASE64: \$\(mirrorDeployKeyBase64\)'
        ([regex]::Matches($Pipeline, 'mirrorDeployKeyBase64')).Count | Should -Be 1
    }
}
