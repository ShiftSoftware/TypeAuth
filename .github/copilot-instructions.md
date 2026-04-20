# TypeAuth – Copilot Instructions

## Project Overview

TypeAuth is a **type-safe authorization library** for .NET. It lets developers define permissions as static C# fields on classes ("Action Trees"), serialize/deserialize granted permissions as JSON ("Access Trees"), and check access at runtime through `TypeAuthContext`.

### Key Projects (in-scope for refactoring)

| Project | Target | Role |
|---|---|---|
| `TypeAuth.Core` | .NET Standard 2.0 | Core library – actions, context, access tree generation |
| `TypeAuth.Core.Tests` | .NET 10 | MSTest-based test suite |
| `TypeAuth.Shared` | .NET Standard 2.0 | Sample/shared Action Tree definitions used by tests |

### Out-of-scope (do not modify unless explicitly asked)

`TypeAuth.AspNetCore`, `TypeAuth.Blazor`, `TypeAuth.AspNetCore.Sample`, `TypeAuth.AspNetCore.Tests`

---

## Architecture & Concepts

### Action Trees
- Plain C# classes decorated with `[ActionTree]` attribute.
- Contain `public static readonly` fields of action types (`BooleanAction`, `ReadAction`, `ReadWriteAction`, `ReadWriteDeleteAction`, `TextAction`, `DecimalAction`, and their `Dynamic*` counterparts).
- Can nest via inner classes (each also marked `[ActionTree]`).

### Access Trees
- JSON strings representing granted permissions, structured to mirror the Action Tree hierarchy.
- Leaf values are either a JSON array of `Access` enum values (`["r","w","d","m"]`), a text/decimal value, or a nested object (for dynamic per-ID access).

### TypeAuthContext
- Central runtime class. Constructed with one or more Access Tree JSON strings + one or more Action Tree `Type`s.
- `TypeAuthContextHelper` does the heavy lifting: generates an in-memory `ActionTreeNode` graph from Action Tree types, populates an `ActionBank` (flat list of resolved actions + their granted access), and evaluates `Can`/`AccessValue` queries.
- `AccessTreeGenerator` (extension methods on `TypeAuthContext`) handles generating/reducing/preserving access trees.

### Action Type Hierarchy
```
ActionBase
├── Action (static)
│   ├── BooleanAction
│   ├── ReadAction
│   ├── ReadWriteAction
│   ├── ReadWriteDeleteAction
│   ├── TextAction
│   │   └── DecimalAction
│   └── (each has a Dynamic* sibling under DynamicAction)
└── DynamicAction (per-ID)
    ├── DynamicBooleanAction
    ├── DynamicReadAction
    ├── DynamicReadWriteAction
    ├── DynamicReadWriteDeleteAction
    ├── DynamicTextAction
    │   └── DynamicDecimalAction
    └── Expand() method for populating items
```

### Access Enum
`Read = 1`, `Write = 2`, `Delete = 3`, `Maximum = 4` — serialized as `"r"`, `"w"`, `"d"`, `"m"`.

---

## Critical Constraints

1. **100% Backwards Compatibility** — All existing public APIs, JSON formats, and behaviors must remain unchanged. Any refactoring must preserve the current signatures, serialization shapes, and test outcomes.
2. **All existing tests must continue to pass** after every change. Run the test suite (`TypeAuth.Core.Tests`) to verify.
3. **.NET Standard 2.0** — `TypeAuth.Core` and `TypeAuth.Shared` must remain compatible. No APIs above .NET Standard 2.0 unless behind `#if` guards. `LangVersion 10.0` is used, so C# 10 syntax is fine.
4. **Newtonsoft.Json** — The core library uses Newtonsoft.Json (not System.Text.Json) for serialization. Tests may use either. Do not switch serializers in Core without explicit approval.
5. **No new NuGet dependencies** in Core unless explicitly approved.

---

## Coding Conventions

- Namespace pattern: `ShiftSoftware.TypeAuth.Core`, `ShiftSoftware.TypeAuth.Core.Actions`, `ShiftSoftware.TypeAuth.Shared.ActionTrees`.
- `internal` for implementation details (`TypeAuthContextHelper`, `ActionBankItem`, `AccessTreeNode`).
- `public` for API surface (`TypeAuthContext`, `TypeAuthContextBuilder`, action classes, `ActionTreeNode`, `AccessTreeGenerator`, `ITypeAuthService`).
- Use `this.` prefix for instance member access (matches existing style).
- Tabs for indentation in `.csproj` files, spaces (4) in `.cs` files.
- Add XML doc comments on public API and Internal API.
- Tests use MSTest (`[TestClass]`, `[TestMethod]`).

---

## Workflow Rules

- **Read before edit** — always read a file before modifying it.
- **Build after changes** — run `run_build` to verify compilation.
- **Run tests** — after any logic change, run the test suite to verify no regressions.
- **Incremental changes** — make small, focused changes. Don't combine unrelated refactoring in one step.
- **Update the plan** — after completing a phase, update the master plan at `.shift/repos/typeauth/REFACTORING_PLAN.md` (in the `.shift` repo) to reflect progress.
- **Strong emphasis on context management** — Create per-module detail files as iterations grow, and apply new methods to keep things tidy and keep the master plan (`.shift/repos/typeauth/REFACTORING_PLAN.md`) manageable as complexity increases.

