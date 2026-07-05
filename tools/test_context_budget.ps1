param(
    [int]$MaxDefaultFiles = 120
)

$ErrorActionPreference = "Stop"

$root = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $root

$failures = New-Object System.Collections.Generic.List[string]

function Add-Failure {
    param([string]$Message)
    $failures.Add($Message) | Out-Null
}

function Write-Check {
    param(
        [string]$Name,
        [bool]$Passed,
        [string]$Detail
    )

    $status = if ($Passed) { "PASS" } else { "FAIL" }
    Write-Output ("[{0}] {1} - {2}" -f $status, $Name, $Detail)
}

$rg = Get-Command rg -ErrorAction SilentlyContinue
if (-not $rg) {
    Add-Failure "rg is not available"
    Write-Check "rg available" $false "install or expose ripgrep before measuring context budget"
} else {
    Write-Check "rg available" $true $rg.Source

    $defaultFiles = @(rg --files)
    $defaultCount = $defaultFiles.Count
    $countPassed = $defaultCount -le $MaxDefaultFiles
    Write-Check "default file budget" $countPassed ("{0} files, limit {1}" -f $defaultCount, $MaxDefaultFiles)
    if (-not $countPassed) {
        Add-Failure ("default rg file count is {0}, expected <= {1}" -f $defaultCount, $MaxDefaultFiles)
    }

    $forbiddenPattern = "dnSpy|decompiled|__pycache__|\.pyc$|\.json$|bin\\|obj\\|logs\\"
    $forbiddenFiles = @($defaultFiles | Select-String -Pattern $forbiddenPattern)
    $forbiddenPassed = $forbiddenFiles.Count -eq 0
    Write-Check "heavy paths excluded" $forbiddenPassed ("{0} forbidden default paths found" -f $forbiddenFiles.Count)
    if (-not $forbiddenPassed) {
        Add-Failure "heavy paths are visible in default rg output"
        $forbiddenFiles | Select-Object -First 20 | ForEach-Object {
            Write-Output ("  " + $_.Line)
        }
    }
}

$entryFiles = @(
    "AGENTS.md",
    "docs/codex_entry.md",
    "docs/project_status.md",
    "docs/README.md",
    ".rgignore",
    "tools/context_inventory.ps1"
)

foreach ($entryFile in $entryFiles) {
    $exists = Test-Path $entryFile
    $detail = if ($exists) { "exists" } else { "missing" }
    Write-Check ("entry file " + $entryFile) $exists $detail
    if (-not $exists) {
        Add-Failure ("missing " + $entryFile)
    }
}

try {
    $inventoryOutput = @(powershell -ExecutionPolicy Bypass -File tools/context_inventory.ps1)
    $inventoryPassed = $LASTEXITCODE -eq 0 -and $inventoryOutput.Count -gt 0
    Write-Check "context inventory" $inventoryPassed ("{0} output lines" -f $inventoryOutput.Count)
    if (-not $inventoryPassed) {
        Add-Failure "context_inventory.ps1 did not run cleanly"
    }
} catch {
    Write-Check "context inventory" $false $_.Exception.Message
    Add-Failure "context_inventory.ps1 threw an exception"
}

Write-Output ""
if ($failures.Count -eq 0) {
    Write-Output "Context smoke test passed."
    exit 0
}

Write-Output "Context smoke test failed:"
foreach ($failure in $failures) {
    Write-Output ("- " + $failure)
}
exit 1

