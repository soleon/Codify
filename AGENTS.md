# Codify Agent Instructions

Codify is a personal, generic WPF and .NET library. It is not published for general public consumption, has no external consumers, and does not maintain backward compatibility. These instructions apply to all AI agents performing code reviews, code changes, tests, documentation updates, dependency updates, or repository maintenance in this repository.

## Core Priorities

1. Preserve correctness and expected runtime behavior.
2. Maximize performance and minimize memory consumption as the first engineering priority after correctness.
3. Always use the latest stable C# language version, .NET runtime, WPF platform, SDK, tooling, and library features. When the latest stable release of a technology is on a non-LTS track and an LTS track exists, target the latest LTS release of that track instead.
4. Adopt newly stabilized language features, BCL APIs, and framework capabilities as soon as they ship; do not retain older patterns once a cleaner modern equivalent is available.
5. Do not use preview, experimental, deprecated, unsupported, or unstable APIs unless explicitly requested and documented.
6. Apply best practices for the relevant .NET, WPF, packaging, testing, and library-design context whenever possible.
7. Keep code testable by preferring small units, explicit dependencies, deterministic behavior, and clear contracts.
8. Add or update meaningful test coverage whenever practical for changed behavior, bug fixes, APIs, and edge cases.

## Dependency and Platform Policy

- Always use the latest stable version of every NuGet package, .NET runtime, WPF platform, SDK, and tool.
- When a technology has an LTS release track and the latest stable release is not on that track, target the latest LTS release instead. This applies to .NET runtime/SDK in particular: prefer the latest LTS .NET over a newer non-LTS .NET.
- Single-target the chosen runtime. Do not multi-target older versions, add compatibility target frameworks, or add package-validation baselines.
- Backward compatibility is not a requirement. Breaking changes to public APIs, target frameworks, SDK versions, or any other surface are acceptable and do not require migration paths, deprecation cycles, or documented justification.
- When a newer stable (or newer LTS, where applicable) release of any dependency or platform is available, updating to it is the default expectation, not an opt-in.
- Avoid adding dependencies unless they clearly improve correctness, performance, maintainability, testability, or platform support.
- Do not introduce large dependencies for small utilities.
- When a dependency cannot be updated to the latest stable (or latest LTS) version, document the technical reason blocking the update.

## Performance and Memory Policy

- Treat allocation rate, steady-state memory use, startup cost, UI responsiveness, and throughput as first-class design constraints.
- Prefer allocation-conscious APIs, efficient data structures, and predictable algorithms.
- Avoid unnecessary LINQ, reflection, boxing, string allocations, repeated enumeration, closure captures, and async overhead on hot paths.
- Use spans, memory pooling, caching, compiled expressions, source generators, or specialized collections only when they are appropriate, measurable, and maintainable.
- For performance-sensitive changes, add benchmarks or explain why benchmark coverage is not practical.
- Do not trade away correctness or maintainability for speculative micro-optimizations.

## WPF and .NET Library Policy

- Keep UI-thread work minimal and avoid blocking the dispatcher.
- Preserve WPF binding behavior, dependency property semantics, resource lookup behavior, and design-time usability.
- Prefer nullable-safe, analyzer-friendly, idiomatic modern C#. Use the latest C# language version supported by the chosen .NET SDK.
- Public APIs may change freely. Reshape, rename, or remove them whenever doing so produces a cleaner or more idiomatic surface; there are no external consumers to protect.
- All public-facing types and members must have full XML documentation coverage.
- Avoid global state and hidden side effects unless they are required by platform constraints.
- Because the product library namespaces sit under `Codify.System`, the `System` and `System.Windows` projects must fully qualify BCL `System.*` types as `global::System.*` regardless of whether a name collision currently exists. This is enforced as a static guard test and is intentional defense-in-depth so that introducing a future `Codify.System.*` type cannot silently hijack a BCL reference. Test projects under `Codify.System.Tests` and `Codify.System.Windows.Tests` only need the same qualification when the compiler would otherwise resolve the reference under `Codify.System`.

## Testing Policy

- Add or update tests for behavior changes, bug fixes, regressions, API changes, and meaningful edge cases.
- Prefer focused unit tests for library logic.
- Use integration or WPF-specific tests when unit tests cannot cover the behavior effectively.
- Keep tests deterministic and independent of machine-specific state whenever possible.
- Do not reduce or remove test coverage without documenting the reason.

## Code Review Policy

When reviewing changes, prioritize findings in this order:

1. correctness bugs
2. performance or memory regressions
3. WPF threading, binding, resource, or lifecycle risks
4. missing or weak tests
5. failure to use the latest stable (or latest LTS, where an LTS track exists) version of a relevant runtime, SDK, language feature, or dependency
6. dependency, target framework, SDK, or tooling risks
7. maintainability and best-practice issues

Do not flag the absence of backward compatibility, deprecation paths, or migration shims as a finding; they are not required in this repository.

Do not prefix review findings with bracketed severity labels such as `[P0]`, `[P1]`, `[P2]`, or `[P3]`.

## Tooling Notes

- Do not run `dotnet build`, `dotnet test`, `dotnet pack`, or `dotnet format` concurrently against the same solution or projects; shared `obj/` outputs can lock and cause transient MSBuild/CSC failures.
- When intentionally deleting a tracked file that already has staged edits, `git rm` may refuse the deletion; after confirming the deletion is intended, use `git rm -f -- <path>`.

## Required Final Checklist

Before claiming work is complete, agents must verify or explicitly state why they could not verify:

- the solution builds
- relevant tests pass
- analyzers, formatting, or style checks pass where available
- the latest stable (or latest LTS, where an LTS track exists) versions of dependencies, runtimes, SDKs, and language features are in use
- performance and memory impact were considered
- new or changed behavior has appropriate test coverage where practical
