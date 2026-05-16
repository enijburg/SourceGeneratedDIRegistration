# SourceGeneratedDIRegistration

Companion sample code for the blog post
**"Source generators in practice: auto-registering plugins at compile time"**
(`2026-05-16-source-generated-di.md`).

The repository contains a tiny, self-contained `MyApp` that demonstrates how a
C# incremental source generator can auto-register plugin implementations into
`Microsoft.Extensions.DependencyInjection` at **build time** — no reflection,
no manual `AddSingleton` calls, AOT/trim friendly.

## Layout

```
src/
  MyApp/                        # the consumer library: interfaces + sample plugins
  MyApp.SourceGeneration/       # the IIncrementalGenerator (netstandard2.0)
test/
  MyApp.Tests/                  # unit tests for the generator + end-to-end DI tests
```

## What's demonstrated

* A `public static partial class ServiceCollectionExtensions` whose
  `static partial void Register…Plugins(IServiceCollection)` stubs are filled
  in by the generator.
* A two-stage incremental pipeline (cheap syntactic filter → semantic check).
* Deterministic, sorted output so generated files are diff-friendly.
* Unit tests that run the generator against in-memory source and assert on the
  emitted `.g.cs` files, plus an end-to-end test that resolves the plugins from
  a real `ServiceCollection`.

## Run

```pwsh
dotnet test
```

Adding a new plugin is "drop a class that implements one of the plugin
interfaces, then build". The generator picks it up on the next compile.
