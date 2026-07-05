param(
    [switch]$IncludeGenerated,
    [switch]$IncludeBuildArtifacts
)

$ErrorActionPreference = "Stop"

$root = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $root

$excludePatterns = @(
    "tools/dnSpy-netframework/*",
    "tools/dnSpy-netframework.zip",
    "logs/*"
)

if (-not $IncludeGenerated) {
    $excludePatterns += @(
        "generated/decompiled/*",
        "generated/static_knowledge/*.json"
    )
}

if (-not $IncludeBuildArtifacts) {
    $excludePatterns += @(
        "src/**/bin/*",
        "src/**/obj/*",
        "mod/**/Assemblies/*.dll",
        "mod/**/Assemblies/*.pdb",
        "*/__pycache__/*",
        "*.pyc"
    )
}

function Test-Excluded {
    param([string]$Path)

    $normalized = $Path.Replace("\", "/")
    foreach ($pattern in $excludePatterns) {
        if ($normalized -like $pattern) {
            return $true
        }
    }
    return $false
}

$files = Get-ChildItem -Recurse -File |
    ForEach-Object {
        $relative = Resolve-Path -Relative $_.FullName
        if ($relative.StartsWith(".\")) {
            $relative = $relative.Substring(2)
        }
        $relative = $relative.Replace("\", "/")
        [PSCustomObject]@{
            Path = $relative
            Length = $_.Length
        }
    } |
    Where-Object { -not (Test-Excluded $_.Path) } |
    Sort-Object Path

$groups = $files | Group-Object {
    if ($_.Path -notlike "*/*") {
        "(root)"
    } else {
        ($_.Path -split "/")[0]
    }
}

Write-Output "Project inventory from $root"
Write-Output ""

foreach ($group in $groups) {
    $bytes = ($group.Group | Measure-Object Length -Sum).Sum
    Write-Output ("{0}/  files={1}  bytes={2}" -f $group.Name, $group.Count, $bytes)
}

Write-Output ""
Write-Output "Important entry files:"
@(
    "AGENTS.md",
    "docs/codex_entry.md",
    "docs/project_status.md",
    "docs/README.md",
    "src/tryDicing/Initializer.cs",
    "src/tryDicing/tryDicing.csproj"
) | Where-Object { Test-Path $_ } | ForEach-Object { Write-Output $_ }

Write-Output ""
Write-Output "Use -IncludeGenerated or -IncludeBuildArtifacts for deeper inventories."
