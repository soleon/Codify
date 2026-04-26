# Codify Agent Backlog

This file is the durable handoff for follow-up Codex sessions. Start each new
session by reading `AGENTS.md` and this file, then pick exactly one pending
task unless the user asks for broader work.

## Current Workspace Checkpoint

Completed in the current uncommitted workspace:

- Fixed remaining `ObservableDictionary<TKey, TValue>` dictionary/collection
  divergence risk for rejected reentrant clear, remove, and same-key set paths.
- Confirmed the old backlog task "Fix AdaptiveObservableCollection batch
  notifications" was not done in the current tree, then fixed range add,
  remove, replace, move, reset fallback, constructor validation, and disposal
  unsubscribe behavior.
- Added focused tests for both fixes.

Verification already run for this checkpoint:

```powershell
dotnet test Tests\Codify.System.Tests\Codify.System.Tests.csproj --no-restore
dotnet build Codify.slnx
dotnet test Codify.slnx --no-build
dotnet format Codify.slnx --verify-no-changes --verbosity minimal
dotnet build Codify.slnx -c Release
dotnet test Codify.slnx -c Release --no-build
git diff --check
```

Known status:

- `git diff --check` passed, but printed Git line-ending normalization warnings.
- The workspace has uncommitted source/test changes plus this backlog file.

## Session Starter Prompt

Use this prompt for future sessions:

```text
Read AGENTS.md and AGENT_BACKLOG.md. Pick the next pending task only, implement it with focused tests, keep the change scoped, and update AGENT_BACKLOG.md when complete. Use Codify.slnx for solution-level commands.
```

## Pending Tasks

### 1. Complete Public API XML Documentation

Status: completed

Completion note:

- Completed in the current uncommitted workspace on branch `master`.
- Enabled XML documentation generation and XML documentation warning
  enforcement for packable library projects.
- Added meaningful XML documentation for public and protected APIs in
  `System` and `System.Windows`, and fixed malformed type parameter cref usage.
- Suppressed forced CS1591 noise for test projects when documentation
  generation is enabled from the command line.

Verification:

```powershell
dotnet build Codify.slnx /p:GenerateDocumentationFile=true
# Passed: build succeeded with 0 warnings and 0 errors.

dotnet build Codify.slnx
# Passed: build succeeded with 0 warnings and 0 errors.

dotnet test Codify.slnx --no-build
# Passed: 93 tests passed, 0 failed, 0 skipped.

dotnet format Codify.slnx --verify-no-changes --verbosity minimal
# Passed after running dotnet format Codify.slnx --verbosity minimal to apply whitespace-only formatting.

git diff --check
# Passed; Git may print line-ending normalization warnings.
```

Primary files:

- `Directory.Build.props`
- `System/**/*.cs`
- `System.Windows/**/*.cs`

Problem:

- `AGENTS.md` requires all public-facing types and members to have full XML
  documentation coverage.
- Running `dotnet build Codify.slnx /p:GenerateDocumentationFile=true`
  currently exposes many `CS1591` missing XML documentation warnings in
  packable library projects.
- `AdaptiveObservableCollection` also has malformed cref usage for type
  parameters.

Acceptance criteria:

- Public and protected public-facing APIs in `System` and `System.Windows`
  have meaningful XML documentation.
- XML documentation generation is enabled for packable library projects.
- Missing or malformed docs are enforced in build for packable projects.
- Test projects do not need XML documentation unless intentionally enabled.

Suggested verification:

```powershell
dotnet build Codify.slnx /p:GenerateDocumentationFile=true
dotnet build Codify.slnx
dotnet test Codify.slnx --no-build
```

### 2. Apply `global::System.*` Qualification in `Codify.System`

Status: completed

Completion note:

- Completed in the current uncommitted workspace on branch `master`.
- Removed ambiguous BCL `System.*` usings from `System` production sources and
  qualified BCL type references with `global::System.*`.
- Added a focused source-level regression test that guards production
  `Codify.System` sources against reintroducing unqualified BCL references.

Verification:

```powershell
dotnet test Tests\Codify.System.Tests\Codify.System.Tests.csproj --no-restore --filter GlobalSystemQualificationTests
# Red before implementation: failed with 1 convention-test failure for current unqualified sources.
# Green after implementation: passed with 1 test passed, 0 failed, 0 skipped.

dotnet test Tests\Codify.System.Tests\Codify.System.Tests.csproj --no-restore
# Passed: 52 tests passed, 0 failed, 0 skipped.

dotnet build Codify.slnx
# Passed: build succeeded with 0 warnings and 0 errors.

dotnet test Codify.slnx --no-build
# Passed: 94 tests passed, 0 failed, 0 skipped.

dotnet format Codify.slnx --verify-no-changes --verbosity minimal
# Passed: exited with no formatting changes.

git diff --check
# Passed; Git printed line-ending normalization warnings.
```

Primary files:

- `System/Collections/ObjectModel/AdaptiveObservableCollection.cs`
- `System/Collections/ObjectModel/BatchObservableCollection.cs`
- `System/Collections/ObjectModel/ObservableDictionary.cs`
- `System/ComponentModel/ExpandableNotificationObject.cs`
- `System/ComponentModel/NotificationObject.cs`
- `System/Extensions/EnumerableExtensions.cs`
- `System/StaticInstance.cs`

Problem:

- `AGENTS.md` requires BCL `System.*` types to be fully qualified as
  `global::System.*` from inside the `Codify.System` namespace where the
  compiler could otherwise resolve names under `Codify.System`.
- Several files still use unqualified BCL usings and type names.

Acceptance criteria:

- Production files under the `System` project no longer rely on ambiguous
  unqualified BCL `System.*` names.
- Public API signatures remain source- and binary-compatible where practical.
- Existing tests continue to pass.

Suggested verification:

```powershell
dotnet build Codify.slnx
dotnet test Codify.slnx --no-build
```

### 3. Reduce `NotificationObject` Hot-Path Allocations and Boxing

Status: completed

Completion note:

- Completed in the current uncommitted workspace on branch `master`.
- Replaced `object.Equals` in `NotificationObject.SetValue<T>` with
  `EqualityComparer<T>.Default` to avoid boxing value types and to use generic
  equality for `IEquatable<T>` implementations.
- Cached immutable `PropertyChangedEventArgs` instances by property name,
  including the `null` property-name case, so repeated notifications avoid
  per-notification event-args allocation.
- Added focused tests for value-type equality, reference-type equality, null
  reference values, and repeated dependent-property notification ordering and
  event-args reuse.

Verification:

```powershell
dotnet test Tests\Codify.System.Tests\Codify.System.Tests.csproj --no-restore --filter NotificationObjectTests
# Red before implementation: failed 3 tests for object.Equals usage and repeated event-args allocation.
# Green after implementation: passed with 13 tests passed, 0 failed, 0 skipped.

dotnet test Tests\Codify.System.Tests\Codify.System.Tests.csproj --no-restore
# Passed: 56 tests passed, 0 failed, 0 skipped.

dotnet build Codify.slnx
# Passed: build succeeded with 0 warnings and 0 errors.

dotnet test Codify.slnx --no-build
# Passed: 98 tests passed, 0 failed, 0 skipped.

dotnet format Codify.slnx --verify-no-changes --verbosity minimal
# Passed: exited with no formatting changes.

git diff --check
# Passed; Git printed line-ending normalization warnings.
```

Primary file:

- `System/ComponentModel/NotificationObject.cs`

Problem:

- `SetValue` uses `object.Equals`, which boxes value types.
- `OnPropertyChanged` allocates a new `PropertyChangedEventArgs` on every
  notification.
- `AGENTS.md` treats allocation rate and boxing on hot paths as first-class
  performance concerns.

Acceptance criteria:

- `SetValue<T>` compares via `EqualityComparer<T>.Default` or an equivalent
  allocation-conscious path.
- Repeated property notifications avoid unnecessary `PropertyChangedEventArgs`
  allocation, while preserving public behavior.
- Existing dependent-property notification ordering is preserved.
- Tests cover equality behavior for value types, reference types, nulls, and
  dependent property notifications.

Suggested verification:

```powershell
dotnet test Tests\Codify.System.Tests\Codify.System.Tests.csproj --no-restore
dotnet build Codify.slnx
dotnet test Codify.slnx --no-build
```

### 4. Add Remaining Public Behavior Test Coverage

Status: completed

Completion note:

- Completed in the current uncommitted workspace on branch `master`.
- Added focused public behavior coverage for `StaticInstance<T>`,
  `BatchObservableCollection<T>`, `WindowViewModel<T>`, command lifecycle
  behavior, and converter edge cases.
- Fixed the `BatchObservableCollection<T>` defect exposed by the new tests:
  nested update handles now suppress notifications until the outermost handle
  is disposed, and disposing the same update handle more than once is
  idempotent.

Verification:

```powershell
dotnet test Tests\Codify.System.Tests\Codify.System.Tests.csproj --no-restore --filter BatchObservableCollectionTests
# Red before implementation: failed 2 tests for nested update notification timing and duplicate reset notifications.
# Green after implementation: passed with 5 tests passed, 0 failed, 0 skipped.

dotnet test Tests\Codify.System.Tests\Codify.System.Tests.csproj --no-restore
# Passed: 60 tests passed, 0 failed, 0 skipped.

dotnet test Tests\Codify.System.Windows.Tests\Codify.System.Windows.Tests.csproj --no-restore
# Passed: 55 tests passed, 0 failed, 0 skipped.

dotnet build Codify.slnx
# Passed: build succeeded with 0 warnings and 0 errors.

dotnet test Codify.slnx --no-build
# Passed: 115 tests passed, 0 failed, 0 skipped.

dotnet format Codify.slnx --verify-no-changes --verbosity minimal
# Passed: exited with no formatting changes.

git diff --check
# Passed; Git printed line-ending normalization warnings.
```

Primary areas:

- `System/StaticInstance.cs`
- `System/Collections/ObjectModel/BatchObservableCollection.cs`
- `System.Windows/Controls/WindowViewModel.cs`
- `System.Windows/Input/*.cs`
- `System.Windows/Data/*.cs`

Problem:

- Focused tests now cover `ObservableDictionary` and
  `AdaptiveObservableCollection`, but other public APIs still have weak or
  missing edge-case coverage.
- Remaining gaps include `StaticInstance`, `WindowViewModel`, nested or
  double-disposed `BatchObservableCollection` updates, and additional WPF
  command/converter lifecycle edge cases.

Acceptance criteria:

- Add focused tests for public behavior and regression-prone edge cases.
- Use STA test execution for WPF view/window behavior where needed.
- Keep tests deterministic and independent of machine-specific state.
- Do not change production behavior unless a test exposes a real defect.

Suggested verification:

```powershell
dotnet test Codify.slnx
dotnet build Codify.slnx --no-restore
```

### 5. Decide and Document SDK Pinning Policy

Status: completed

Completion note:

- Completed in the current uncommitted workspace on branch `master`.
- Kept SDK selection intentionally floating rather than adding `global.json`,
  because this library targets the current stable frameworks and should pick up
  compatible stable SDK, tooling, and analyzer fixes without a repository
  update.
- Documented the policy in `README.md` and added a focused repository-policy
  test that guards the no-`global.json` decision and README policy text.

Verification:

```powershell
dotnet --info
# Passed: SDK 10.0.202 is installed and no global.json file was found.

dotnet test Tests\Codify.System.Tests\Codify.System.Tests.csproj --no-restore --filter SdkPolicyTests
# Red before documentation: failed because README.md did not contain the floating SDK policy.
# Green after documentation: passed with 1 test passed, 0 failed, 0 skipped.

dotnet build Codify.slnx
# Passed: build succeeded with 0 warnings and 0 errors.

dotnet test Codify.slnx --no-build
# Passed: Codify.System.Tests had 61 tests passed, and Codify.System.Windows.Tests had 55 tests passed.

dotnet format Codify.slnx --verify-no-changes --verbosity minimal
# Passed: exited with no formatting changes.

git diff --check
# Passed; Git printed line-ending normalization warnings for AGENT_BACKLOG.md and README.md.
```

Primary files:

- `global.json` if pinning is chosen
- `README.md` or `AGENT_BACKLOG.md` if floating SDK is intentional

Problem:

- The repository targets current stable .NET, but there is no `global.json`.
- SDK selection floats to whatever compatible SDK is installed on the machine.
- This may be intentional, but the compatibility reason is not documented.

Acceptance criteria:

- Decide whether to pin the SDK with `global.json` or intentionally keep it
  floating.
- If pinned, use a stable supported SDK compatible with the target frameworks.
- If floating, document why that is acceptable for this library.

Suggested verification:

```powershell
dotnet --info
dotnet build Codify.slnx
dotnet test Codify.slnx --no-build
```

### 6. Review Transitive Test Package Freshness

Status: completed

Completion note:

- Completed in the current uncommitted workspace on branch `master`.
- Verified that direct test package references have no available stable updates
  from the configured NuGet sources.
- Left outdated test-only transitive packages unresolved because they are owned
  by current direct test packages and the package graph is clean for vulnerable
  and deprecated packages.
- Documented the policy in `README.md` and added a focused repository-policy
  test that guards against adding explicit references to those transitives only
  to silence freshness reports.

Verification:

```powershell
dotnet list Codify.slnx package --outdated
# Passed: all projects had no package updates from the configured sources.

dotnet list Codify.slnx package --include-transitive --outdated
# Passed: direct packages had no updates; test projects still report outdated
# test-only transitives including Microsoft.ApplicationInsights,
# Microsoft.Bcl.AsyncInterfaces, Microsoft.Testing.* packages, and
# Newtonsoft.Json.

dotnet list Codify.slnx package --vulnerable
# Passed: all projects had no vulnerable packages.

dotnet list Codify.slnx package --deprecated
# Passed: all projects had no deprecated packages.

dotnet test Tests\Codify.System.Tests\Codify.System.Tests.csproj --no-restore --filter RepositoryDocumentsTestOnlyTransitivePackageFreshnessPolicy
# Red before documentation: failed because README.md did not contain the
# test-only transitive package freshness policy.
# Green after documentation: passed with 1 test passed, 0 failed, 0 skipped.

dotnet test Tests\Codify.System.Tests\Codify.System.Tests.csproj --no-restore --filter SdkPolicyTests
# Passed: 2 tests passed, 0 failed, 0 skipped.

dotnet build Codify.slnx
# Passed: build succeeded with 0 warnings and 0 errors.

dotnet test Codify.slnx --no-build
# Passed: Codify.System.Tests had 62 tests passed, and
# Codify.System.Windows.Tests had 55 tests passed.

dotnet format Codify.slnx --verify-no-changes --verbosity minimal
# Passed: exited with no formatting changes.

git diff --check
# Passed; Git may print line-ending normalization warnings.
```

Primary files:

- `Directory.Build.props`
- test project files under `Tests/**`
- optional package management files if central package management is adopted

Problem:

- Direct package references are current, non-vulnerable, and non-deprecated.
- `dotnet list Codify.slnx package --include-transitive --outdated` reports
  older test-only transitive packages such as `Microsoft.Testing.Platform`,
  `Microsoft.Bcl.AsyncInterfaces`, and `Newtonsoft.Json`.
- If these are intentionally controlled by direct test dependencies, the reason
  should be documented.

Acceptance criteria:

- Verify whether any direct dependency update can safely lift outdated
  transitives.
- Avoid adding explicit transitive package references unless there is a clear
  compatibility, security, or tooling reason.
- Document any intentionally unresolved transitive freshness gap.

Suggested verification:

```powershell
dotnet list Codify.slnx package --outdated
dotnet list Codify.slnx package --include-transitive --outdated
dotnet list Codify.slnx package --vulnerable
dotnet list Codify.slnx package --deprecated
dotnet build Codify.slnx
dotnet test Codify.slnx --no-build
```

## Completion Protocol

When a task is finished:

- Change its status from `pending` to `completed`.
- Add a short note with the branch or commit that completed it.
- List exact verification commands and results.
- Leave unrelated tasks untouched.
