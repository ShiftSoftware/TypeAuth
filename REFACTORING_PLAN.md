# TypeAuth Core – Refactoring Plan

> **Goal:** Improve code quality, maintainability, and extensibility of `TypeAuth.Core` while maintaining **100% backwards compatibility** with all existing public APIs, JSON formats, and behaviors.

---

## Current State Summary

### What works well
- Clean public API surface (`TypeAuthContext`, builder, typed action classes).
- Flexible permission model (static actions, dynamic per-ID actions, text/decimal values, wildcards).
- Solid test coverage across multiple scenarios (wildcards, multi-tree merging, dynamic actions, access tree generation with reduce/preserve).

### Areas for improvement

#### 1. Code duplication in Action classes
- `Action` and `DynamicAction` hierarchies are parallel but share no code beyond `ActionBase`. Each action type is duplicated: `TextAction`/`DynamicTextAction`, `DecimalAction`/`DynamicDecimalAction`, etc.
- `TextAction` and `DynamicTextAction` have identical properties (`MaximumAccess`, `MinimumAccess`, `Comparer`, `Merger`) with identical validation logic — copy-pasted.

#### 2. `TypeAuthContextHelper` is a god class
- `GenerateActionTree`, `PopulateActionBank`, `ExpandDynamicActions`, `LocateActionInBank`, and `Can` are all in one 194-line class with mixed concerns (reflection-based tree building, JSON traversal, access evaluation).

#### 3. `AccessTreeGenerator` mixes concerns
- ~375 lines of static extension methods handling: toggle/set access, generate access tree, reduce, preserve, flatten, traverse, and find inaccessible actions — all in one file.

#### 4. `AccessTreeNode` parsing is fragile
- Constructor uses `GetType()` checks against `JValue`, `JArray`, `JObject` — no error handling for unexpected types.

#### 5. Commented-out debug code
- Multiple `Console.WriteLine` blocks commented out across `TypeAuthContext`, `TypeAuthContextHelper`, and tests.

#### 6. Minor inconsistencies
- `ActionBankItem` constructor parameter `acessValue` (typo).
- `SelfRererenceKey` constant has a typo ("Rererence" → should be "Reference") but **cannot be changed** (breaking change for serialized access trees).
- `Extensions.cs` is an empty file.
- `DynamicActionTree` class exists but is unused.

#### 7. No interface on `TypeAuthContext`
- `ITypeAuthService` exists but `TypeAuthContext` doesn't implement it. The interface mirrors the context's methods but the relationship isn't formalized in core.

#### 8. `ActionTreeNode` uses `HashSet` without `Equals`/`GetHashCode`
- `ActionTreeNode` uses `HashSet<ActionTreeNode>` for `ActionTreeItems` but doesn't override equality — effectively behaves as a reference-equality set (same as `List` but with `HashSet` overhead).

---

## Phased Plan

### Phase 1: Housekeeping & Safety Net
**Goal:** Clean up without changing any logic. Zero risk.

- [x] **1.1** Remove commented-out `Console.WriteLine` debug blocks from `TypeAuthContext.cs`, `TypeAuthContextHelper.cs`, and `ActionTreeNode.cs`.
- [x] **1.2** Remove the empty `Extensions.cs` file.
- [x] **1.3** Fix the `acessValue` parameter typo in `ActionBankItem` constructor (internal class — not a breaking change).
- [x] **1.4** Add `// Note:` comment on `SelfRererenceKey` explaining why the typo is preserved.
- [x] **1.5** All 62 tests pass ✅

### Phase 2: Extract shared Text/Decimal logic (reduce duplication)
**Goal:** Reduce copy-paste between `Action`/`DynamicAction` text property sets.

- [ ] **2.1** Introduce an internal interface `ITextAccessProperties` with `MaximumAccess`, `MinimumAccess`, `Comparer`, `Merger` — implemented by both `TextAction` and `DynamicTextAction`.
- [ ] **2.2** Refactor `TypeAuthContext.GetTextAccessValue` and `TypeAuthContextHelper.PopulateActionBank` to use the interface instead of casting to both `TextAction` and `DynamicTextAction` separately.
- [ ] **2.3** Refactor `AccessTreeGenerator` text-handling code to use the interface.
- [ ] **2.4** Run all tests → green.

### Phase 3: Break up `TypeAuthContextHelper`
**Goal:** Single-responsibility classes behind the same internal API.

- [ ] **3.1** Extract `ActionTreeBuilder` — responsible for `GenerateActionTree` (reflection-based tree generation from Action Tree types).
- [ ] **3.2** Extract `ActionBankPopulator` — responsible for `PopulateActionBank` and `ExpandDynamicActions` (JSON → ActionBank population).
- [ ] **3.3** Keep `LocateActionInBank` and `Can` on `TypeAuthContextHelper` (or move to a slim evaluator class).
- [ ] **3.4** `TypeAuthContextHelper` becomes a façade that composes the above.
- [ ] **3.5** Run all tests → green.

### Phase 4: Break up `AccessTreeGenerator`
**Goal:** Separate tree generation from access manipulation.

- [ ] **4.1** Group the public access manipulation methods (`ToggleAccess`, `SetAccessValue`) — consider whether they belong on a builder/editor class rather than extension methods on `TypeAuthContext`.
- [ ] **4.2** Extract the `TraverseActionTree` recursive logic into a dedicated internal class.
- [ ] **4.3** Extract `FindInAccessibleActionsOn` into its own utility or extension class.
- [ ] **4.4** Run all tests → green.

### Phase 5: Harden `AccessTreeNode` parsing
**Goal:** More robust JSON → internal model conversion.

- [ ] **5.1** Add defensive handling for unexpected JSON token types in `AccessTreeNode`.
- [ ] **5.2** Consider adding unit tests for malformed access trees (null, empty, wrong types).
- [ ] **5.3** Run all tests → green.

### Phase 6: `ActionTreeNode` collection choice
**Goal:** Use the right collection type.

- [ ] **6.1** Evaluate whether `HashSet<ActionTreeNode>` should become `List<ActionTreeNode>` (since no custom equality is defined and order matters for serialization) — or properly implement `Equals`/`GetHashCode` if set semantics are needed.
- [ ] **6.2** Run all tests → green.

### Phase 7: Documentation & XML comments
**Goal:** Improve discoverability for consumers.

- [ ] **7.1** Add XML doc comments to all public API members that lack them.
- [ ] **7.2** Review and update the existing XML comments for accuracy.

---

## Progress Tracker

| Phase | Status | Notes |
|-------|--------|-------|
| 1 – Housekeeping | ✅ Complete | All 62 tests pass. Removed debug comments, empty file, fixed internal typo. |
| 2 – Text/Decimal dedup | ⬜ Not started | |
| 3 – Split ContextHelper | ⬜ Not started | |
| 4 – Split AccessTreeGen | ⬜ Not started | |
| 5 – Harden parsing | ⬜ Not started | |
| 6 – Collection choice | ⬜ Not started | |
| 7 – Documentation | ⬜ Not started | |

---

## Rules of Engagement

1. **One phase at a time.** Complete and verify before moving on.
2. **Tests are the contract.** Every phase ends with all existing tests passing.
3. **No public API changes.** Method signatures, class names, namespaces, and JSON shapes are frozen.
4. **Internal changes only need to preserve behavior**, not specific implementation structure.
5. **Update this file** after each phase is done.
