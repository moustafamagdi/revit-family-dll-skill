# Build and execution handoff

## Compatibility

The supplied starter/helper support Revit 2024 only. Compilation needs Windows,
a .NET SDK with MSBuild, the .NET Framework 4.8 developer/targeting pack, and a
legitimate local Revit 2024 installation with `RevitAPI.dll` and `RevitAPIUI.dll`.
Execution needs a compatible family template and Add-in Manager for that release.

Revit 2024 uses .NET Framework 4.8. Revit 2025 moved to .NET 8 and needs a separate
compatible build and dependency review. For other releases, check their SDK and
official Autodesk guidance. Changing a year in a filename is not compatibility.
See Autodesk's [migration guide](https://help.autodesk.com/view/RVT/2025/ENU/?caas=caas/blog/thebuildingcoder.typepad.com/blog/2024/04/migrating-from-net-48-to-net-core-8.html).

Reference target API assemblies with Copy Local disabled
(`<Private>false</Private>`). Do not redistribute Autodesk API binaries,
templates, or Add-in Manager. Users provide them separately.

Implement `IExternalCommand` with `[Transaction(TransactionMode.Manual)]`. Minimize
runtime dependencies; document any necessary dependency's loading and licensing.
Embed catalogue/notes resources rather than resolving them beside the DLL:
Add-in Manager may load a temporary copy.

## Starter build

Copy `assets/starter` to a new working folder. Invoke the skill's helper against
that copy (PowerShell, with the current directory set to the skill directory):

```powershell
& .\scripts\build.ps1 `
  -ProjectPath 'D:\FamilyWork\Demo\FamilyGenerator.csproj' `
  -RevitInstallDir 'C:\Program Files\Autodesk\Revit 2024' `
  -OutputDirectory 'D:\FamilyWork\Demo\delivery' `
  -Version '1.0.0'
```

The helper compiles only. It does not install or execute the DLL. Output:
`FamilyGenerator_Revit2024.dll`, command
`RevitFamilyGenerator.BuildDemoFamily`.

Use a user-selected localized template or resolve from
`Application.FamilyTemplatePath`; do not require an English filename on every
installation. Hosting/category may require a different RFT. The demo asks for a
Generic Model template explicitly and validates its category.

## Add-in Manager handoff

1. Open the matching Revit release and compatible Add-in Manager, **Manual Mode**.
2. Load the delivered DLL and select its exact full namespace/class command.
3. Run and choose template/output when prompted.
4. Inspect the generated RFAs and the Desktop log reported by the command.

Use version/MVID to distinguish stale loads from new builds. Do not promise that
any Add-in Manager can hot-reload every assembly. If a restart or filename change
is necessary, explain the observed reason.

Deliver DLL, necessary non-Autodesk dependencies, editable project/source,
embedded inputs, source/assumption notes, and concise instructions. State the
exact build target and untested behavior. Keep prior runtime evidence attached
to the exact binary it describes.
