# Codify

Codify is a small set of reusable .NET libraries for application infrastructure and WPF applications.

## Packages

| Package | Target framework | Purpose |
| --- | --- | --- |
| `Codify.System` | `net10.0` | Collection, component model, extension, and utility types. |
| `Codify.System.Windows` | `net10.0-windows` | WPF command, converter, control, and view-model helpers. |

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
        set => SetProperty(ref name, value);
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

- Public APIs are intended to remain compatible across patch releases.
- `Codify.System.Windows` requires Windows because it targets WPF.
- Packages are built and validated with the current stable .NET SDK for the target frameworks listed above.
