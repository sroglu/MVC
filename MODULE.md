# MVC

## Purpose
Lightweight Model-View-Controller framework for Unity. A `ControllerBase` holds a `Model` and a `ViewBase`, can be **page** type (one screen-level singleton) or **instance** type (per-object), and broadcasts named actions to peer controllers via `Redirect`. `ViewManager` tracks registered page views and instantiates instance views from prefabs.

Designed to be **input-source-agnostic**: the assembly does not reference `Unity.InputSystem` or any specific input package. Pointer enter/exit detection uses Unity's `EventSystem` (`IPointerEnterHandler`/`IPointerExitHandler`); games wire their own input layer to controllers (see *Game-specific input* below).

## Assembly

| Assembly | Path | Depends On |
|---|---|---|
| `mehmetsrl.MVC` | `mehmetsrl.MVC.asmdef` | (none) — Unity built-in only |

## Key Classes

- **`ControllerBase`** — Abstract base controller with `Redirect(actionName, …)` action broadcast and `OnActionRedirected` override.
- **`Controller<V, M>`** — Typed controller; `View` (V) and `Model` (M) auto-resolved (page view from `ViewManager`'s pre-registered list, or new instance view from prefab list).
- **`ControllerType`** — `Page` (uses pre-designed page view, hidden by default) or `Instance` (instantiated at runtime).
- **`IController`** / **`IModel`** — Interfaces.
- **`ViewBase`** — Abstract `MonoBehaviour` view; exposes `Show`/`Hide`, `IsPointerOn`, `OnCreate`/`OnRemove`/`OnStateChanged` hooks.
- **`View<M>`** — Generic typed view bound to a model.
- **`ModelBase`** / **`Model<T>`** — Auto-registered models with cloneable description data; supports single + array models.
- **`ViewManager`** — Singleton MonoBehaviour managing the page view roster and the instance prefab list. Tracks page-view history (`ShowPageView` / `ShowLastPageView`).
- **`EmptyModel`**, **`EmptyView`**, **`EmptyData`** — No-op implementations for controllers that don't need all three pieces.

## Quick Start

```csharp
using mehmetsrl.MVC;
using mehmetsrl.MVC.core;

public class InventoryModel : Model<InventoryData> { public InventoryModel(InventoryData d) : base(d) { } }

public class InventoryView : View<InventoryModel>
{
    public override void UpdateView() { /* refresh UI */ }
}

public class InventoryController : Controller<InventoryView, InventoryModel>
{
    public InventoryController(InventoryModel model)
        : base(ControllerType.Page, model) { }

    public void OnItemClicked(int id) => Redirect("ItemSelected");

    protected override void OnActionRedirected(IController src, string action, EventArgs data)
    {
        if (action == "InventoryRefresh") View.UpdateView();
    }
}
```

In your scene, drop a `ViewManager` MonoBehaviour, populate its `pageViews[]` (pre-designed page roots) and `instancePrefabs[]` (instantiable view prefabs). Call `ViewManager.ShowPageView<InventoryView>()` to switch screens.

## Game-specific input

The MVC assembly intentionally has **no input dependency**. To wire input:

- **Pointer hover/enter/exit** — already handled by `EventSystem` (`IsPointerOn` is set by `IPointerEnterHandler`/`IPointerExitHandler` callbacks on `ViewBase`). No setup required beyond an `EventSystem` in the scene.
- **Custom actions** (click, hold, swipe, etc.) — add an input handler in your game project (e.g. `Assets/GameSpecific/Input/`) that:
  1. Owns the InputSystem `.inputactions` asset and generated `Inputs.cs`
  2. On callback, calls into your controller's public method (e.g. `inventoryController.OnHoldGesture()`)
- This separation lets you swap input backends (InputSystem, Touch, Gamepad-only, mock for tests) without touching MVC.

A previous version of this module embedded an `Inputs.inputactions` asset and a static `ViewBase.ActionPerformed` event coupled to `InputAction.CallbackContext` — those have been removed. Game projects that previously relied on them should keep their own `Inputs.inputactions` under `Assets/GameSpecific/Input/`.

## File Structure
```
(submodule root)
mehmetsrl.MVC.asmdef
MODULE.md
ViewManager.cs
Core/
  Controller.cs        (ControllerBase, Controller<V,M>, ControllerType, IController)
  Model.cs             (IModel, ModelBase, Model<T>)
  View.cs              (IView, ViewBase, View<M>)
EmptyComponent/
  EmptyData.cs
  EmptyModel.cs
  EmptyView.cs
Tests/                 (EditMode unit tests — mehmetsrl.MVC.Tests)
Samples/               (Counter sample, autoReferenced=false — see Samples/MODULE.md)
```

## Known Limitations / Future Work

- `Redirect(actionName, controllerName)` is **string-based**. Type-safe overloads (`Redirect<TController>(action)` or delegate-based) would catch typos at compile time. Not blocking — current API works.
- `ControllerBase.RedirectToAction` is a **static delegate** chaining all controllers — global mutable state. Switching to per-`ViewManager` (or DI'd) event bus would isolate test scopes and allow multiple MVC contexts.
- `ModelBase.modelDictionary` is a **static** registry — same caveat. Consider per-context model registry.

## Downstream Dependents

No assemblies in this Infrastructural project depend on MVC — it is consumed by separate game projects (e.g. `StrategyGame`, `TileMatch`) that copy the source locally today; future work is to switch them to this submodule reference.
