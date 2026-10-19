[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$ProjectPath,
    [string]$RevitInstallDir = (Join-Path $env:ProgramFiles 'Autodesk\Revit 2024'),
    [Parameter(Mandatory = $true)][string]$OutputDirectory,
    [ValidatePattern('^\d+\.\d+\.\d+$')][string]$Version = '1.0.0'
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (-not (Test-Path -LiteralPath $ProjectPath -PathType Leaf)) {
    throw "Project not found: $ProjectPath"
}
$resolvedProject = (Resolve-Path -LiteralPath $ProjectPath).ProviderPath
if ([IO.Path]::GetExtension($resolvedProject) -ne '.csproj') {
    throw 'ProjectPath must identify a C# project.'
}
foreach ($apiName in @('RevitAPI.dll', 'RevitAPIUI.dll')) {
    $apiPath = Join-Path $RevitInstallDir $apiName
    if (-not (Test-Path -LiteralPath $apiPath -PathType Leaf)) {
        throw "Missing $apiName in RevitInstallDir. Supply a Revit 2024 installation."
    }
    $apiVersion = [Reflection.AssemblyName]::GetAssemblyName($apiPath).Version
    if ($apiVersion.Major -ne 24) {
        throw "This helper supports Revit 2024 only; $apiName has version $apiVersion."
    }
}
$resolvedInstall = (Resolve-Path -LiteralPath $RevitInstallDir).ProviderPath
$targetingPack = Join-Path ${env:ProgramFiles(x86)} 'Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\mscorlib.dll'
if (-not (Test-Path -LiteralPath $targetingPack -PathType Leaf)) {
    throw '.NET Framework 4.8 targeting pack missing. Install its developer pack first.'
}
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw '.NET SDK not found. Install an SDK with MSBuild support.'
}
$resolvedOutput = [IO.Path]::GetFullPath($OutputDirectory)
if ($resolvedOutput.Contains(';') -or $resolvedInstall.Contains(';')) {
    throw 'MSBuild property paths must not contain semicolons.'
}
New-Item -ItemType Directory -Path $resolvedOutput -Force | Out-Null
$buildArgs = @(
    'build', $resolvedProject, '--configuration', 'Release', '--nologo',
    '--output', $resolvedOutput,
    "-p:RevitInstallDir=$resolvedInstall", "-p:Version=$Version",
    "-p:AssemblyVersion=$Version.0", "-p:FileVersion=$Version.0"
)
& dotnet @buildArgs
if ($LASTEXITCODE -ne 0) { throw "Compilation failed (exit $LASTEXITCODE)." }
$dlls = @(Get-ChildItem -LiteralPath $resolvedOutput -Filter '*.dll' -File)
if ($dlls.Count -eq 0) { throw 'Compilation returned no DLL in the output folder.' }
foreach ($apiName in @('RevitAPI.dll', 'RevitAPIUI.dll')) {
    if (Test-Path -LiteralPath (Join-Path $resolvedOutput $apiName)) {
        throw "Autodesk binary found in output: $apiName. Use a clean output folder and Copy Local=false."
    }
}
foreach ($dll in $dlls) {
    $hash = (Get-FileHash -LiteralPath $dll.FullName -Algorithm SHA256).Hash
    Write-Output "DLL: $($dll.FullName)"
    Write-Output "SHA256: $hash"
}
Write-Output 'Compilation passed. Revit runtime validation has not been run by this script.'
