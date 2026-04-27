# Codify

Codify is a small set of reusable .NET libraries for application infrastructure and WPF applications.

## Packages

| Package | Target framework | Purpose |
| --- | --- | --- |
| `Codify.System` | `net10.0` | Collection, component model, extension, and utility types. |
| `Codify.System.Windows` | `net10.0-windows10.0.17763.0` | WPF command, converter, control, and view-model helpers. |

## Installation

```powershell
dotnet add package Codify.System
dotnet add package Codify.System.Windows
```

`Codify.System.Windows` references `Codify.System`, so WPF applications usually only need the Windows package.

## Common Library Example

Use `NotificationObject` as a base for models or view models that need efficient `INotifyPropertyChanged` support.

```csharp
using Codify.System.ComponentModel;

public sealed class CustomerViewModel : NotificationObject
{
    private string name = string.Empty;

    public string Name
    {
        get => name;
        set => SetValue(ref name, value);
    }
}
```

## WPF Example

Use command helpers to expose synchronous or asynchronous actions through `ICommand`.

```csharp
using Codify.System.Windows.Input;

public sealed class ShellViewModel
{
    public ShellViewModel()
    {
        SaveCommand = new ActionCommand(Save, CanSave);
    }

    public ActionCommand SaveCommand { get; }

    private bool CanSave() => true;

    private void Save()
    {
        // Save application state.
    }
}
```

## Notes

- `Codify.System.Windows` requires Windows because it targets WPF.
- The packages intentionally target only the latest LTS .NET line used by this repository. Older target frameworks are not preserved when the package moves to a newer LTS.
- `Codify.System.Windows` explicitly targets the Windows 10 1809 platform baseline instead of the SDK's implicit Windows 7 fallback.
- The repository intentionally does not include a `global.json` SDK pin. SDK selection floats to the latest installed stable .NET SDK that can build `net10.0` and `net10.0-windows10.0.17763.0`, so package builds pick up supported SDK, tooling, and analyzer fixes without a repository update. Target frameworks and package versions remain explicit to preserve package compatibility.
- Test-only transitive package freshness: Direct test package references are kept on their latest stable versions. Outdated test-only transitive packages are left to the direct test packages that own them when those direct packages are current, non-vulnerable, and non-deprecated. The repository does not add explicit references to `Microsoft.Testing.Platform`, `Microsoft.Bcl.AsyncInterfaces`, `Newtonsoft.Json`, or related telemetry packages solely to silence transitive freshness reports.
