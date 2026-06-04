param()

$inputJson = [Console]::In.ReadToEnd()
try { $toolInput = $inputJson | ConvertFrom-Json } catch { exit 0 }

$cmd = $toolInput.command
if (-not $cmd) { exit 0 }

$startPatterns = 'dotnet run', 'npm run dev', 'docker compose up', 'docker-compose up'
$isStartCommand = $startPatterns | Where-Object { $cmd -match [regex]::Escape($_) }
if (-not $isStartCommand) { exit 0 }

Write-Host ""
Write-Host "=== Pre-Start QA Gate ==="
Write-Host "Running unit tests before starting the project..."
Write-Host ""

Set-Location "C:\WORK_PLACE\MamVibe"

dotnet test "backend/tests/MomVibe.UnitTests" --verbosity minimal --nologo 2>&1
$unitResult = $LASTEXITCODE

if ($unitResult -ne 0) {
    Write-Host ""
    Write-Host "BLOCKED: Unit tests FAILED. Fix the tests before starting the project."
    Write-Host "Tip: Run /test-qa for the full report."
    exit 2
}

Write-Host ""
Write-Host "Unit tests passed. Starting project..."
Write-Host "==========================="
Write-Host ""
exit 0
