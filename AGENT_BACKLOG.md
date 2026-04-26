# Codify Agent Backlog

This file is the durable handoff for follow-up Codex sessions. Start each new session by reading `AGENTS.md` and this file, then pick exactly one backlog item unless the user asks for a broader change.

## Current Checkpoint

Branch:

```text
codex/observable-dictionary-checkpoint
```

Completed in this checkpoint:

- Fixed `ObservableDictionary<TKey, TValue>` dictionary/collection divergence.
- Added `Tests/Codify.System.Tests` with focused `ObservableDictionary` coverage.
- Migrated `Codify.sln` to `Codify.slnx`.

Verification already run for this checkpoint:

```powershell
dotnet test Codify.slnx
dotnet build Codify.slnx --no-restore
dotnet format Codify.slnx --verify-no-changes --include System/Collections/ObjectModel/ObservableDictionary.cs Tests/Codify.System.Tests/Collections/ObjectModel/ObservableDictionaryTests.cs --verbosity minimal
dotnet list Codify.slnx package --outdated
```

Known pre-existing verification gap:

```powershell
dotnet format Codify.slnx --verify-no-changes --verbosity minimal
```

fails on whitespace in `System.Windows/Data/BooleanConverter.cs` lines 22-27. That failure predates the `ObservableDictionary` fix.

## Session Starter Prompt

Use this prompt for future sessions:

```text
Read AGENTS.md and AGENT_BACKLOG.md. Pick the next unchecked task only, implement it with focused tests, keep the change scoped, and update AGENT_BACKLOG.md when complete. Use Codify.slnx for solution-level commands.
```

## Remaining Tasks

### 1. Fix AdaptiveObservableCollection batch notifications

Status: completed

Completion note:

- Completed in commit `72e2916a9a38217cfaf63df92f86c46c2bbcc7ac`.
- Verification for this task was not re-run in this session; task 3 is the only implementation target for this session.

Primary files:

- `System/Collections/ObjectModel/AdaptiveObservableCollection.cs`

Problem:

- Multi-item Add inserts each item at the same index and reverses order.
- Multi-item Remove increments the old index while the target collection shrinks, so it can skip items and remove the wrong target item.
- Replace and Move should be reviewed for multi-item notifications from derived or custom observable collections.

Acceptance criteria:

- Multi-item Add preserves the source order at `NewStartingIndex`.
- Multi-item Remove removes exactly the items represented by `OldItems`.
- Replace handles all replaced items consistently.
- Move handles moved ranges consistently or documents and guards unsupported range moves.
- Constructor validates `sourceCollection` and `converter`.
- Tests cover single-item and multi-item Add, Remove, Replace, Reset, Move, constructor validation, and disposal unsubscribe behavior.

Suggested verification:

```powershell
dotnet test Codify.slnx
dotnet build Codify.slnx --no-restore
```

### 2. Fix generic command null-parameter behavior

Status: completed

Completion note:

- Completed on branch `codex/observable-dictionary-checkpoint`.
- Red check: `dotnet test .\Tests\Codify.System.Windows.Tests\Codify.System.Windows.Tests.csproj` failed before production edits with 7 failing command tests covering generic null handling and invalid parameter messages.
- Verification: `dotnet test .\Tests\Codify.System.Windows.Tests\Codify.System.Windows.Tests.csproj` passed with 14/14 tests; it emitted the existing `Codify.System.Windows` package readme warning during package generation.
- Verification: `dotnet test .\Codify.slnx` passed with 32/32 tests.
- Verification: `dotnet build .\Codify.slnx --no-restore` succeeded with 0 warnings and 0 errors.
- Verification: `dotnet format .\Codify.slnx --verify-no-changes --include .\System.Windows\Input\Command.cs .\System.Windows\Input\ActionCommand.cs .\System.Windows\Input\AsyncActionCommand.cs .\Tests\Codify.System.Windows.Tests\Input\ActionCommandTests.cs .\Tests\Codify.System.Windows.Tests\Input\AsyncActionCommandTests.cs --verbosity minimal` exited 0.

Primary files:

- `System.Windows/Input/Command.cs`
- `System.Windows/Input/SyncCommand.cs`
- `System.Windows/Input/AsyncCommand.cs`
- `System.Windows/Input/ActionCommand.cs`
- `System.Windows/Input/AsyncActionCommand.cs`

Problem:

- Generic commands can report `CanExecute(null) == true`, then `Execute(null)` can crash while formatting `param.GetType()`.
- WPF commonly invokes commands with null parameters.

Acceptance criteria:

- Null command parameters never produce `NullReferenceException`.
- `CanExecute` and `Execute` agree for null and invalid parameter cases.
- Invalid non-null parameter errors remain deterministic and useful.
- Async command exceptions are not silently lost beyond normal `ICommand.Execute` constraints.
- Tests cover non-generic commands, generic commands, null parameters, invalid parameter types, disabled commands, and async command behavior.

Suggested verification:

```powershell
dotnet test Codify.slnx
dotnet build Codify.slnx --no-restore
```

### 3. Make ExpandableNotificationObject async hooks explicit

Status: completed

Completion note:

- Completed in this session on branch `master`.
- Chosen behavior: explicit fire-and-forget for async hooks, with faulted tasks observed through protected `OnAsyncHookException(Exception)`.
- Red check: `dotnet test .\Tests\Codify.System.Tests\Codify.System.Tests.csproj` failed before production edits with CS0115 because `OnAsyncHookException(Exception)` did not exist yet.
- Verification: `dotnet test .\Tests\Codify.System.Tests\Codify.System.Tests.csproj` passed with 23/23 tests; it emitted the existing `Codify.System` package readme warning during package generation.
- Verification: `dotnet format .\Codify.slnx --verify-no-changes --include .\System\ComponentModel\ExpandableNotificationObject.cs .\Tests\Codify.System.Tests\ComponentModel\ExpandableNotificationObjectTests.cs --verbosity minimal` exited 0.
- Verification: `dotnet test .\Codify.slnx` passed with 37/37 tests; it emitted the existing `Codify.System` package readme warning during package generation.
- Verification: `dotnet build .\Codify.slnx --no-restore` succeeded with 0 warnings and 0 errors.
- Known unrelated gap confirmed: `dotnet format .\Codify.slnx --verify-no-changes --verbosity minimal` still fails on pre-existing whitespace in `System.Windows\Data\BooleanConverter.cs` lines 22-27.

Primary file:

- `System/ComponentModel/ExpandableNotificationObject.cs`

Problem:

- `OnExpansionChangedAsync` and `OnSelectionChangedAsync` return `Task`, but callers do not await or observe those tasks.
- Exceptions can be lost or raised outside the property-change flow.

Acceptance criteria:

- Choose and document one compatible behavior:
  - explicit fire-and-forget with exception observation, or
  - new awaitable API while preserving existing public API behavior.
- Synchronous hooks still fire when the property value changes.
- Async hooks do not run when the value is unchanged.
- Tests cover hook ordering, unchanged values, and exception behavior.

Suggested verification:

```powershell
dotnet test Codify.slnx
dotnet build Codify.slnx --no-restore
```

### 4. Add broader public behavior test coverage

Status: completed

Completion note:

- Completed in this session on branch `master`.
- Added focused public behavior tests for `NotificationObject`, `BatchObservableCollection`, `EnumerationExtensions`, WPF data converters, and `ViewModel<T>`.
- Verification: `dotnet test .\Tests\Codify.System.Tests\Codify.System.Tests.csproj` passed with 37/37 tests; it emitted the existing `Codify.System` package readme warning during package generation.
- Verification: `dotnet test .\Tests\Codify.System.Windows.Tests\Codify.System.Windows.Tests.csproj` passed with 42/42 tests; it emitted the existing `Codify.System.Windows` package readme warning during package generation.
- Verification: `dotnet test .\Codify.slnx` passed with 79/79 tests.
- Verification: `dotnet build .\Codify.slnx --no-restore` succeeded with 0 warnings and 0 errors.
- Verification: `dotnet format .\Codify.slnx --verify-no-changes --include .\Tests\Codify.System.Tests\ComponentModel\NotificationObjectTests.cs .\Tests\Codify.System.Tests\Collections\ObjectModel\BatchObservableCollectionTests.cs .\Tests\Codify.System.Tests\Extensions\EnumerationExtensionsTests.cs .\Tests\Codify.System.Windows.Tests\Data\BooleanConverterTests.cs .\Tests\Codify.System.Windows.Tests\Data\EqualityConverterTests.cs .\Tests\Codify.System.Windows.Tests\Data\MultiBooleanConverterTests.cs .\Tests\Codify.System.Windows.Tests\Data\VisibilityConverterTests.cs .\Tests\Codify.System.Windows.Tests\Controls\ViewModelTests.cs --verbosity minimal` exited 0.

Primary areas:

- `NotificationObject`
- `BatchObservableCollection`
- `EnumerableExtensions`
- WPF converters in `System.Windows/Data`
- WPF view-model helpers in `System.Windows/Controls`

Problem:

- The repo now has focused `ObservableDictionary` tests, but most public APIs remain untested.

Acceptance criteria:

- Add focused unit tests for library logic.
- Add WPF-specific tests only where behavior cannot be covered without WPF types.
- Cover edge cases and regression-prone behavior.
- Keep tests deterministic and independent of machine-specific state.

Suggested verification:

```powershell
dotnet test Codify.slnx
dotnet build Codify.slnx --no-restore
```

### 5. Enable nullable and analyzer posture

Status: pending

Primary files:

- `System/System.csproj`
- `System.Windows/System.Windows.csproj`
- new `Directory.Build.props` if useful
- new `.editorconfig` if useful

Problem:

- Nullable reference types are not enabled for production projects.
- Repo-wide analyzer/style posture is not defined.
- Prior exploratory checks reported nullable warnings and analyzer warnings.

Acceptance criteria:

- Decide whether to enable nullable/analyzers in one step or staged commits.
- Fix warnings without breaking public API compatibility.
- Add suppressions only with clear justification.
- Keep WPF interface nullability compatible with platform contracts.

Suggested verification:

```powershell
dotnet build Codify.slnx --no-incremental
dotnet build Codify.slnx -warnaserror --no-incremental
dotnet test Codify.slnx
```

### 6. Review target framework policy

Status: pending

Primary files:

- `System/System.csproj`
- `System.Windows/System.Windows.csproj`
- `Tests/Codify.System.Tests/Codify.System.Tests.csproj`

Problem:

- Projects target `net8.0` and `net8.0-windows`.
- `AGENTS.md` asks agents to prefer latest stable Microsoft platforms when compatible, while also preserving public API compatibility.

Acceptance criteria:

- Decide whether to stay on `net8.0`, move to `net10.0`, or multi-target.
- Document compatibility reasoning if not moving to the latest stable target.
- Verify package generation and WPF build behavior after any target change.

Suggested verification:

```powershell
dotnet build Codify.slnx
dotnet test Codify.slnx
dotnet pack System/System.csproj -c Release
dotnet pack System.Windows/System.Windows.csproj -c Release
```

### 7. Clean repository-local IDE artifacts

Status: pending

Primary files:

- `.gitignore`
- tracked `.vs/**` files

Problem:

- `.vs` files are tracked, and `.gitignore` does not ignore `.vs/`.
- These are machine-local Visual Studio artifacts.

Acceptance criteria:

- Add `.vs/` to `.gitignore`.
- Remove tracked `.vs/**` files from source control.
- Do not delete unrelated user files outside the repo.

Suggested verification:

```powershell
git status --short
git ls-files .vs
dotnet build Codify.slnx --no-restore
```

### 8. Fix formatting and establish style checks

Status: pending

Primary files:

- `System.Windows/Data/BooleanConverter.cs`
- optional `.editorconfig`

Problem:

- Full solution formatting verification fails on `BooleanConverter.cs` whitespace.

Acceptance criteria:

- `dotnet format Codify.slnx --verify-no-changes --verbosity minimal` passes.
- Formatting behavior is either default SDK style or documented in `.editorconfig`.
- No behavior changes are bundled unless separately tested.

Suggested verification:

```powershell
dotnet format Codify.slnx --verify-no-changes --verbosity minimal
dotnet test Codify.slnx
dotnet build Codify.slnx --no-restore
```

### 9. Improve package metadata and pack workflow

Status: pending

Primary files:

- `System/System.csproj`
- `System.Windows/System.Windows.csproj`
- `README.md`

Problem:

- Build emits NuGet package readme warnings.
- Prior exploratory `dotnet pack Codify.sln -c Release` failed before Release outputs existed, while Release build generated packages through `GeneratePackageOnBuild`.

Acceptance criteria:

- Add package readme metadata or document why it is intentionally omitted.
- Verify normal pack commands work cleanly for both packages.
- Avoid packaging the test project.

Suggested verification:

```powershell
dotnet pack System/System.csproj -c Release
dotnet pack System.Windows/System.Windows.csproj -c Release
dotnet build Codify.slnx -c Release
```

## Completion Protocol

When a task is finished:

- Change its status from `pending` to `completed`.
- Add a short note with the branch or commit that completed it.
- List exact verification commands and results.
- Leave unrelated tasks untouched.
