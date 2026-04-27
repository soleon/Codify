# Codify Agent Instructions

Codify is a generic WPF and .NET library. These instructions apply to all AI agents performing code reviews, code changes, tests, documentation updates, dependency updates, or repository maintenance in this repository.

## Core Priorities

1. Preserve correctness, public API compatibility, and expected runtime behavior.
2. Maximize performance and minimize memory consumption as the first engineering priority after correctness.
3. Use the latest stable C# language, .NET runtime, WPF platform, SDK, tooling, and library features when they improve correctness, performance, memory use, maintainability, or testability.
4. Use the latest publicly stable version of dependent libraries, technical stacks, and platforms when compatible with the repository's supported target frameworks and public API.
5. Do not use preview, experimental, deprecated, unsupported, or unstable APIs unless explicitly requested and documented.
6. Apply best practices for the relevant .NET, WPF, packaging, testing, and library-design context whenever possible.
7. Keep code testable by preferring small units, explicit dependencies, deterministic behavior, and clear public contracts.
8. Add or update meaningful test coverage whenever practical for changed behavior, bug fixes, public APIs, and edge cases.

## Dependency and Platform Policy

- Prefer current stable NuGet packages and supported Microsoft platforms.
- Target only the latest LTS .NET runtime and WPF platform supported by the repository. Do not multi-target older .NET versions, add compatibility target frameworks, or add package-validation baselines solely to preserve consumers on previous LTS releases.
- When the latest LTS target changes, dropping older target frameworks is intentional for this repository and is not, by itself, a compatibility violation.
- Public API compatibility means preserving the API surface for the repository's current target framework line unless an intentional break is documented; it does not require preserving older target frameworks.
- Verify compatibility within the current latest-LTS target before changing package versions, target frameworks, SDK versions, or tooling.
- Avoid adding dependencies unless they clearly improve correctness, performance, maintainability, testability, or platform support.
- Do not introduce large dependencies for small utilities.
- When a dependency cannot be updated to the latest stable version, document the compatibility reason.

## Performance and Memory Policy

- Treat allocation rate, steady-state memory use, startup cost, UI responsiveness, and throughput as first-class design constraints.
- Prefer allocation-conscious APIs, efficient data structures, and predictable algorithms.
- Avoid unnecessary LINQ, reflection, boxing, string allocations, repeated enumeration, closure captures, and async overhead on hot paths.
- Use spans, memory pooling, caching, compiled expressions, source generators, or specialized collections only when they are appropriate, measurable, and maintainable.
- For performance-sensitive changes, add benchmarks or explain why benchmark coverage is not practical.
- Do not trade away correctness, public API compatibility, or maintainability for speculative micro-optimizations.

## WPF and .NET Library Policy

- Keep UI-thread work minimal and avoid blocking the dispatcher.
- Preserve WPF binding behavior, dependency property semantics, resource lookup behavior, and design-time usability.
- Prefer nullable-safe, analyzer-friendly, idiomatic modern C#.
- Keep public APIs stable, documented where useful, and suitable for reuse by external consumers.
- All public-facing types and members must have full XML documentation coverage.
- Avoid global state and hidden side effects unless they are required by platform constraints.
- Because the product library namespaces sit under `Codify.System`, the `System` and `System.Windows` projects must fully qualify BCL `System.*` types as `global::System.*` regardless of whether a name collision currently exists. This is enforced as a static guard test and is intentional defense-in-depth so that introducing a future `Codify.System.*` type cannot silently hijack a BCL reference. Test projects under `Codify.System.Tests` and `Codify.System.Windows.Tests` only need the same qualification when the compiler would otherwise resolve the reference under `Codify.System`.

## Testing Policy

- Add or update tests for behavior changes, bug fixes, regressions, public API changes, and meaningful edge cases.
- Prefer focused unit tests for library logic.
- Use integration or WPF-specific tests when unit tests cannot cover the behavior effectively.
- Keep tests deterministic and independent of machine-specific state whenever possible.
- Do not reduce or remove test coverage without documenting the reason.

## Code Review Policy

When reviewing changes, prioritize findings in this order:

1. correctness bugs
2. performance or memory regressions
3. public API compatibility breaks
4. WPF threading, binding, resource, or lifecycle risks
5. missing or weak tests
6. dependency, target framework, SDK, or tooling risks
7. maintainability and best-practice issues

Do not prefix review findings with bracketed severity labels such as `[P0]`, `[P1]`, `[P2]`, or `[P3]`.

## Tooling Notes

- Do not run `dotnet build`, `dotnet test`, `dotnet pack`, or `dotnet format` concurrently against the same solution or projects; shared `obj/` outputs can lock and cause transient MSBuild/CSC failures.
- When intentionally deleting a tracked file that already has staged edits, `git rm` may refuse the deletion; after confirming the deletion is intended, use `git rm -f -- <path>`.

## Required Final Checklist

Before claiming work is complete, agents must verify or explicitly state why they could not verify:

- the solution builds
- relevant tests pass
- analyzers, formatting, or style checks pass where available
- dependency and platform choices use stable supported versions
- performance and memory impact were considered
- new or changed behavior has appropriate test coverage where practical
- public API compatibility was preserved or any intentional break was documented
