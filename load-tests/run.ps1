# MomVibe Load Test Runner — PowerShell
# Runs the k6 load test with sensible defaults.
# Prerequisites: winget install k6

param(
    [string]$BaseUrl    = "http://localhost:5038",
    [int]   $PeakAnon   = 600,
    [int]   $PeakAuth   = 280,
    [int]   $PeakTrader = 120,
    [string]$UsersFile  = "",
    [switch]$Html                    # Pass -Html to generate an HTML report
)

$k6Args = @(
    "run",
    "--env", "BASE_URL=$BaseUrl",
    "--env", "PEAK_ANON=$PeakAnon",
    "--env", "PEAK_AUTH=$PeakAuth",
    "--env", "PEAK_TRADER=$PeakTrader"
)

if ($UsersFile) {
    # k6 open() resolves paths relative to the script file, not the working directory.
    # GetUnresolvedProviderPathFromPSPath uses PowerShell's own location (not the .NET
    # process directory which is C:\WINDOWS\system32 in Windows PowerShell).
    $absoluteUsersFile = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($UsersFile)
    $k6Args += "--env", "USERS_FILE=$absoluteUsersFile"
}

if ($Html) {
    # k6 v2 removed --out html; use web-dashboard (live browser UI at http://localhost:5665)
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $reportsDir = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath("load-tests\reports")
    New-Item -ItemType Directory -Force $reportsDir | Out-Null
    $reportPath = Join-Path $reportsDir "k6_${timestamp}.json"
    $k6Args += "--out", "json=$reportPath"
    $k6Args += "--out", "web-dashboard"
    Write-Host "Live dashboard : http://localhost:5665"
    Write-Host "JSON report    : $reportPath"
}

$k6Args += "load-tests\k6\load-test.js"

Write-Host ""
Write-Host "=== MomVibe Load Test ===" -ForegroundColor Cyan
Write-Host "Target  : $BaseUrl"
Write-Host "Peak VUs: anon=$PeakAnon  auth=$PeakAuth  trader=$PeakTrader  total=$($PeakAnon+$PeakAuth+$PeakTrader)"
if ($UsersFile) { Write-Host "Users   : $UsersFile" }
Write-Host ""

& k6 @k6Args

# ── Example invocations ────────────────────────────────────────────────────────
# Local dev, anonymous only (fast smoke test):
#   .\load-tests\run.ps1 -PeakAnon 50 -PeakAuth 0 -PeakTrader 0
#
# Full 1000-VU test with real accounts:
#   .\load-tests\run.ps1 -PeakAnon 600 -PeakAuth 280 -PeakTrader 120 -UsersFile .\load-tests\k6\test-users.json -Html
#
# Against production:
#   .\load-tests\run.ps1 -BaseUrl https://api.momvibe.bg -UsersFile .\prod-test-users.json
