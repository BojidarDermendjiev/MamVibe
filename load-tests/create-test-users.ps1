# Creates the 5 load-test accounts via the register API.
# Requires the backend running in Development mode (Turnstile is auto-bypassed there).
#
# Usage (from the project root):
#   .\load-tests\create-test-users.ps1
#   .\load-tests\create-test-users.ps1 -BaseUrl http://localhost:5038

param(
    [string]$BaseUrl = "http://localhost:5038"
)

$users = @(
    @{ email = "loadtest1@test.com"; password = "LoadTest@123"; displayName = "Load Tester 1"; profileType = 1 }
    @{ email = "loadtest2@test.com"; password = "LoadTest@123"; displayName = "Load Tester 2"; profileType = 0 }
    @{ email = "loadtest3@test.com"; password = "LoadTest@123"; displayName = "Load Tester 3"; profileType = 1 }
    @{ email = "loadtest4@test.com"; password = "LoadTest@123"; displayName = "Load Tester 4"; profileType = 0 }
    @{ email = "loadtest5@test.com"; password = "LoadTest@123"; displayName = "Load Tester 5"; profileType = 1 }
)

Write-Host ""
Write-Host "=== Creating Load Test Accounts ===" -ForegroundColor Cyan
Write-Host "Target: $BaseUrl"
Write-Host ""

$created = 0
$skipped = 0
$failed  = 0

foreach ($u in $users) {
    $body = @{
        email           = $u.email
        password        = $u.password
        confirmPassword = $u.password
        displayName     = $u.displayName
        profileType     = $u.profileType
    } | ConvertTo-Json

    try {
        $null = Invoke-WebRequest `
            -Uri "$BaseUrl/api/v1/auth/register" `
            -Method POST `
            -ContentType "application/json" `
            -Body $body `
            -UseBasicParsing `
            -ErrorAction Stop

        Write-Host "  [OK]     $($u.email)" -ForegroundColor Green
        $created++
    }
    catch {
        $status = $_.Exception.Response.StatusCode.value__
        $msg    = $_.ErrorDetails.Message

        if ($status -eq 409 -or $msg -match "already|exists|taken") {
            Write-Host "  [EXISTS] $($u.email) (already exists, skipping)" -ForegroundColor Yellow
            $skipped++
        }
        elseif ($status -eq 400) {
            Write-Host "  [SKIP]   $($u.email) - 400: $msg" -ForegroundColor Yellow
            $skipped++
        }
        else {
            Write-Host "  [FAIL]   $($u.email) - HTTP $status : $($_.Exception.Message)" -ForegroundColor Red
            $failed++
        }
    }
}

Write-Host ""
Write-Host "Done - created: $created  |  already existed: $skipped  |  failed: $failed" -ForegroundColor Cyan

if ($failed -gt 0) {
    Write-Host ""
    Write-Host "Tip: Make sure the backend is running in Development mode." -ForegroundColor Yellow
    Write-Host "     Turnstile is only bypassed in Development - it blocks calls in Production." -ForegroundColor Yellow
}
