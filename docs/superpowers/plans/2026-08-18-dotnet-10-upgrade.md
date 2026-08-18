# .NET 10 Upgrade Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Upgrade the supported Event Assembler Core release to .NET 10 and publish verified cross-platform release `v2026.08.18`.

**Architecture:** Retarget the two SDK-style projects in the Core dependency graph and update the existing GitHub Actions SDK selection. Preserve runtime behavior by explicitly binding the repository's existing indexed-enumeration helper where new .NET 10 BCL APIs create name collisions.

**Tech Stack:** .NET SDK 10, MSBuild, PowerShell, Git, GitHub Actions, GitHub CLI

## Global Constraints

- `Nintenlord/Nintenlord.csproj` and `Event Assembler/Core/Core.csproj` must target `net10.0`.
- The legacy .NET Framework 4.0 GUI and conversion utility projects must remain unchanged.
- Publishing must remain framework-dependent and cross-platform.
- Existing package references and runtime behavior must remain unchanged.
- The indexed pointer-list enumeration must continue returning the repository's
  existing `Tuple<int,T>` shape.
- GitHub Actions must produce Linux, macOS, and Windows ZIP assets.
- The release tag must be `v2026.08.18`.
- Every commit must include `Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>`.

---

### Task 1: Retarget the Supported Core Projects

**Files:**
- Modify: `Nintenlord/Nintenlord.csproj:3`
- Modify: `Event Assembler/Core/Core.csproj:3`
- Modify: `Event Assembler/Core/Code/Language/EACodeLanguageDisassembler.cs:58`
- Modify: `.github/workflows/ci.yml:26`
- Modify: `README.md:10`

**Interfaces:**
- Consumes: The existing `Core.csproj` project reference to `Nintenlord.csproj`.
- Produces: A Core dependency graph targeting `net10.0`, an unambiguous call
  to the existing indexed-enumeration helper, and CI configured with
  `dotnet-version: 10.0.x`.

- [ ] **Step 1: Run the target-version assertion before editing**

```powershell
$checks = @{
  "Nintenlord\Nintenlord.csproj" = "<TargetFramework>net10.0</TargetFramework>"
  "Event Assembler\Core\Core.csproj" = "<TargetFramework>net10.0</TargetFramework>"
  ".github\workflows\ci.yml" = "dotnet-version: 10.0.x"
}

foreach ($entry in $checks.GetEnumerator()) {
  if (-not (Select-String -Path $entry.Key -SimpleMatch $entry.Value -Quiet)) {
    throw "$($entry.Key) does not contain '$($entry.Value)'"
  }
}
```

Expected: FAIL because the projects and workflow still select .NET 6.

- [ ] **Step 2: Retarget `Nintenlord.csproj`**

Replace:

```xml
<TargetFramework>net6.0</TargetFramework>
```

with:

```xml
<TargetFramework>net10.0</TargetFramework>
```

- [ ] **Step 3: Retarget `Core.csproj`**

Replace:

```xml
<TargetFramework>net6.0</TargetFramework>
```

with:

```xml
<TargetFramework>net10.0</TargetFramework>
```

- [ ] **Step 4: Update the CI SDK**

Replace:

```yaml
          dotnet-version: 6.0.x
```

with:

```yaml
          dotnet-version: 10.0.x
```

- [ ] **Step 5: Verify the .NET 10 compatibility failure**

```powershell
dotnet build "Event Assembler\Core\Core.csproj" -c Release --nologo
```

Expected: FAIL with `CS0121` because .NET 9 and later include
`System.Linq.Enumerable.Index`, which conflicts with the repository's
`CollectionExtensions.Index` when the project is upgraded to .NET 10.

- [ ] **Step 6: Bind the existing indexed-enumeration helper**

Replace:

```csharp
foreach (Tuple<int, Tuple<string, List<Priority>>> tuple in pointerList.SelectMany (x => x).Index ()) {
```

with:

```csharp
foreach (Tuple<int, Tuple<string, List<Priority>>> tuple in Nintenlord.Collections.CollectionExtensions.Index (pointerList.SelectMany (x => x))) {
```

- [ ] **Step 7: Document the runtime requirement**

Insert after the introductory sentence in `README.md`:

```markdown
The cross-platform Core release requires the [.NET 10 runtime](https://dotnet.microsoft.com/download/dotnet/10.0).
```

- [ ] **Step 8: Re-run the target-version assertion**

Run the PowerShell assertion from Step 1.

Expected: PASS with exit code 0 and no output.

- [ ] **Step 9: Check the scoped diff**

```powershell
git -c core.whitespace=cr-at-eol --no-pager diff --check
git --no-pager diff -- "Nintenlord\Nintenlord.csproj" "Event Assembler\Core\Core.csproj" "Event Assembler\Core\Code\Language\EACodeLanguageDisassembler.cs" ".github\workflows\ci.yml" README.md
```

Expected: No whitespace errors; only the two target frameworks, explicit
existing-helper call, CI SDK version, and README requirement change.

### Task 2: Validate .NET 10 Build and Package Metadata

**Files:**
- Read: `Event Assembler/Core/Core.csproj`
- Generated outside repository: `$env:TEMP\event-assembler-net10`

**Interfaces:**
- Consumes: The `net10.0` project graph from Task 1.
- Produces: A successful Release publish whose dependency metadata targets `.NETCoreApp,Version=v10.0`.

- [ ] **Step 1: Remove only the prior validation output**

```powershell
$publishDir = Join-Path $env:TEMP "event-assembler-net10"
if (Test-Path $publishDir) {
  Remove-Item -LiteralPath $publishDir -Recurse -Force
}
```

Expected: `$publishDir` is absent before publishing.

- [ ] **Step 2: Restore the Core project**

```powershell
dotnet restore "Event Assembler\Core\Core.csproj" --nologo
```

Expected: Exit code 0; both Core and Nintenlord restore successfully.

- [ ] **Step 3: Build the Core project**

```powershell
dotnet build "Event Assembler\Core\Core.csproj" -c Release --no-restore --nologo
```

Expected: `Build succeeded.` and `0 Error(s)`. Existing legacy-source warnings are allowed.

- [ ] **Step 4: Publish the Core project**

```powershell
dotnet publish "Event Assembler\Core\Core.csproj" -c Release --no-restore --nologo -o $publishDir
```

Expected: Exit code 0 and `$publishDir\Core.dll` exists.

- [ ] **Step 5: Assert the published target framework**

```powershell
$deps = Get-Content (Join-Path $publishDir "Core.deps.json") -Raw | ConvertFrom-Json
if ($deps.runtimeTarget.name -ne ".NETCoreApp,Version=v10.0") {
  throw "Unexpected runtime target: $($deps.runtimeTarget.name)"
}
```

Expected: PASS with exit code 0.

- [ ] **Step 6: Confirm the repository remains free of generated files**

```powershell
git status --short
```

Expected: Only the seven intended upgrade and documentation files are
modified or untracked.

### Task 3: Commit and Push the Upgrade

**Files:**
- Commit: `Nintenlord/Nintenlord.csproj`
- Commit: `Event Assembler/Core/Core.csproj`
- Commit: `Event Assembler/Core/Code/Language/EACodeLanguageDisassembler.cs`
- Commit: `.github/workflows/ci.yml`
- Commit: `README.md`
- Commit: `docs/superpowers/plans/2026-08-18-dotnet-10-upgrade.md`
- Commit: `docs/superpowers/specs/2026-08-18-dotnet-10-upgrade-design.md`

**Interfaces:**
- Consumes: The locally validated Task 1 changes.
- Produces: An upgrade commit on `dotnet-10-upgrade`, fast-forwarded to
  `master`, and reachable from `origin/master`.

- [ ] **Step 1: Stage the scoped files**

```powershell
git add -- "Nintenlord\Nintenlord.csproj" "Event Assembler\Core\Core.csproj" "Event Assembler\Core\Code\Language\EACodeLanguageDisassembler.cs" ".github\workflows\ci.yml" README.md "docs\superpowers\plans\2026-08-18-dotnet-10-upgrade.md" "docs\superpowers\specs\2026-08-18-dotnet-10-upgrade-design.md"
git -c core.whitespace=cr-at-eol --no-pager diff --cached --check
git --no-pager diff --cached --stat
```

Expected: Seven staged files and no whitespace errors.

- [ ] **Step 2: Commit the upgrade**

```powershell
git commit -m "Upgrade Event Assembler Core to .NET 10" -m "Retarget the supported Core project graph to net10.0, disambiguate its indexed enumeration from new .NET 10 BCL APIs, build release artifacts with the .NET 10 SDK, and document the runtime requirement." -m "Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

Expected: A new commit on `dotnet-10-upgrade`.

- [ ] **Step 3: Fast-forward and push `master`**

```powershell
$mainRepo = "C:\Projects\laqieer\FEBuilderGBA\tools\Event-Assembler"
git -C $mainRepo merge --ff-only dotnet-10-upgrade
git -C $mainRepo push origin master
```

Expected: `origin/master` advances to the upgrade commit.

- [ ] **Step 4: Verify the remote commit**

```powershell
$sha = git rev-parse HEAD
$mainRepo = "C:\Projects\laqieer\FEBuilderGBA\tools\Event-Assembler"
$masterSha = git -C $mainRepo rev-parse master
$remoteSha = git -C $mainRepo ls-remote origin refs/heads/master | ForEach-Object { ($_ -split "\s+")[0] }
if ($masterSha -ne $sha) {
  throw "master is $masterSha, expected $sha"
}
if ($remoteSha -ne $sha) {
  throw "origin/master is $remoteSha, expected $sha"
}
```

Expected: PASS with exit code 0.

- [ ] **Step 5: Wait for the `master` CI run**

```powershell
$sha = git rev-parse HEAD
$deadline = (Get-Date).AddMinutes(2)
do {
  $runs = @(gh run list --workflow ci.yml --branch master --event push --limit 5 --json databaseId,headSha | ConvertFrom-Json)
  $run = $runs | Where-Object headSha -eq $sha | Select-Object -First 1
  if (-not $run) {
    Start-Sleep -Seconds 5
  }
} until ($run -or (Get-Date) -ge $deadline)
if (-not $run) {
  throw "No master CI run appeared for $sha"
}
gh run watch $run.databaseId --exit-status
```

Expected: The Linux, Windows, and macOS build matrix succeeds.

### Task 4: Create and Verify the GitHub Release

**Files:**
- Remote tag: `v2026.08.18`
- GitHub release: `v2026.08.18`
- Release assets: `Event-Assembler-Linux.zip`, `Event-Assembler-macOS.zip`, `Event-Assembler-Windows.zip`

**Interfaces:**
- Consumes: The successful pushed commit and `master` CI run from Task 3.
- Produces: A published GitHub release at the exact upgrade commit with three verified assets.

- [ ] **Step 1: Create and push the annotated release tag**

```powershell
$sha = git rev-parse HEAD
git tag -a v2026.08.18 $sha -m "v2026.08.18"
git push origin refs/tags/v2026.08.18
```

Expected: GitHub receives the tag and starts the tag workflow. No release is
published until the build and release jobs succeed.

- [ ] **Step 2: Wait for the tag workflow**

```powershell
$sha = git rev-parse HEAD
$deadline = (Get-Date).AddMinutes(2)
do {
  $runs = @(gh run list --workflow ci.yml --branch v2026.08.18 --event push --limit 5 --json databaseId,headSha | ConvertFrom-Json)
  $run = $runs | Where-Object headSha -eq $sha | Select-Object -First 1
  if (-not $run) {
    Start-Sleep -Seconds 5
  }
} until ($run -or (Get-Date) -ge $deadline)
if (-not $run) {
  throw "No tag CI run appeared for $sha"
}
gh run watch $run.databaseId --exit-status
```

Expected: The build matrix and release job succeed.

- [ ] **Step 3: Verify tag and release assets**

```powershell
$sha = git rev-parse HEAD
$tagSha = git ls-remote origin "refs/tags/v2026.08.18^{}" | ForEach-Object { ($_ -split "\s+")[0] }
if ($tagSha -ne $sha) {
  throw "Release tag is $tagSha, expected $sha"
}

$release = gh release view v2026.08.18 --json tagName,isDraft,isPrerelease,assets,url | ConvertFrom-Json
$expectedAssets = @(
  "Event-Assembler-Linux.zip"
  "Event-Assembler-macOS.zip"
  "Event-Assembler-Windows.zip"
)
$actualAssets = @($release.assets.name | Sort-Object)

if ($release.tagName -ne "v2026.08.18" -or $release.isDraft -or $release.isPrerelease) {
  throw "Release publication state is incorrect"
}
if (Compare-Object ($expectedAssets | Sort-Object) $actualAssets) {
  throw "Unexpected release assets: $($actualAssets -join ', ')"
}
if (@($release.assets | Where-Object size -le 0).Count -ne 0) {
  throw "One or more release assets are empty"
}

$release.url
```

Expected: PASS and print the published release URL.

- [ ] **Step 4: Inspect every released ZIP**

```powershell
$downloadDir = Join-Path $env:TEMP "event-assembler-v2026.08.18"
if (Test-Path -LiteralPath $downloadDir) {
  Remove-Item -LiteralPath $downloadDir -Recurse -Force
}
New-Item -ItemType Directory -Path $downloadDir | Out-Null

gh release download v2026.08.18 --pattern "Event-Assembler-*.zip" --dir $downloadDir
$zips = @(Get-ChildItem -LiteralPath $downloadDir -Filter "*.zip" -File)
if ($zips.Count -ne 3) {
  throw "Downloaded $($zips.Count) release ZIPs, expected 3"
}

foreach ($zip in $zips) {
  $extractDir = Join-Path $downloadDir $zip.BaseName
  Expand-Archive -LiteralPath $zip.FullName -DestinationPath $extractDir -Force

  foreach ($relativePath in @(
    "Core\Core.dll"
    "Core\Core.deps.json"
    "README.md"
    "Language Raws"
    "EA Standard Library"
  )) {
    if (-not (Test-Path -LiteralPath (Join-Path $extractDir $relativePath))) {
      throw "$($zip.Name) is missing $relativePath"
    }
  }

  $deps = Get-Content (Join-Path $extractDir "Core\Core.deps.json") -Raw | ConvertFrom-Json
  if ($deps.runtimeTarget.name -ne ".NETCoreApp,Version=v10.0") {
    throw "$($zip.Name) targets $($deps.runtimeTarget.name)"
  }
}
```

Expected: PASS for all three platform archives.

- [ ] **Step 5: Confirm the final repository state**

```powershell
$mainRepo = "C:\Projects\laqieer\FEBuilderGBA\tools\Event-Assembler"
git status --short --branch
git -C $mainRepo status --short --branch
```

Expected: The worktree branch is clean and `master` is aligned with
`origin/master`.
