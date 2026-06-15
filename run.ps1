param(
    [switch]$Infrastructure,
    [switch]$Api,
    [switch]$Matching,
    [switch]$All,
    [string]$Mode = "RabbitMQ"
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSCommandPath

function Start-Service($Name, $Project, $Port) {
    Write-Host ">>> Starting $Name (port $Port)..." -ForegroundColor Green
    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = "pwsh.exe"
    $psi.Arguments = "-NoExit -NoProfile -Command `"`$env:InfrastructureMode='$Mode'; dotnet run --project '$Project'`""
    $psi.WorkingDirectory = $RepoRoot
    $psi.UseShellExecute = $true
    $psi.WindowStyle = [System.Diagnostics.ProcessWindowStyle]::Normal
    [System.Diagnostics.Process]::Start($psi) | Out-Null
}

if ($Infrastructure -or $All) {
    Write-Host ">>> Starting Docker infrastructure..." -ForegroundColor Green
    Push-Location $RepoRoot
    docker compose up -d
    Pop-Location
    Write-Host ">>> Infrastructure ready — Seq UI at http://localhost:8081" -ForegroundColor Green
}

if ($Api -or $All) {
    Start-Service "Applications API" "src/Services/WorkPac.Recruitment.Applications.Api" 5001
}

if ($Matching -or $All) {
    Start-Sleep -Seconds 2
    Start-Service "Matching Service" "src/Services/WorkPac.Recruitment.Matching.Service" 5002
}

if ($All) {
    Write-Host ""
    Write-Host ">>> Both services started in new windows. Mode: $Mode" -ForegroundColor Green
    Write-Host ">>> Applications API: http://localhost:5001/swagger" -ForegroundColor Cyan
}

if (-not $Infrastructure -and -not $Api -and -not $Matching -and -not $All) {
    Write-Host @"

Usage: .\run.ps1 [options]

Options:
  -Infrastructure   Start Docker infrastructure (SQL, RabbitMQ, Azurite, Seq)
  -Api              Start Applications API in a new terminal
  -Matching         Start Matching Service in a new terminal
  -All              Start infrastructure + both services (default mode: RabbitMQ)
  -Mode <name>      Infrastructure mode: Local, RabbitMQ (default), or Azure

Examples:
  .\run.ps1 -All                    # Full end-to-end with RabbitMQ
  .\run.ps1 -Api -Mode Local        # API only, in-memory (no Docker)
  .\run.ps1 -Infrastructure         # Just start Docker containers
"@
}
