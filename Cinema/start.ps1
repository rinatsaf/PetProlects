param(
    [string]$Action = "up"
)

$ProjectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location -LiteralPath $ProjectRoot

function Start-All {
    Write-Host "=== Starting Docker Compose ===" -ForegroundColor Cyan
    docker compose up -d

    Write-Host "=== Waiting for nginx... ===" -ForegroundColor Cyan
    Start-Sleep -Seconds 3

    Write-Host "=== Checking nginx availability ===" -ForegroundColor Cyan
    $retries = 10
    do {
        try {
            $req = Invoke-WebRequest -Uri "http://localhost/api/genres" -TimeoutSec 2 -ErrorAction Stop
            if ($req.StatusCode -in 200, 201, 204) { break }
        } catch {
            $retries--
            if ($retries -eq 0) {
                Write-Host "nginx not responding after 10 attempts" -ForegroundColor Yellow
                break
            }
            Start-Sleep -Seconds 1
        }
    } while ($retries -gt 0)

    Write-Host "=== Starting xtunnel ===" -ForegroundColor Cyan
    Write-Host "URL will appear below (look for tunnel4.com):" -ForegroundColor Yellow
    & "C:\tools\xtunnel\xtunnel.exe" http 80 --tunnel-host tunnel4.com
}

function Stop-All {
    Write-Host "=== Stopping xtunnel (if running) ===" -ForegroundColor Cyan
    Stop-Process -Name "xtunnel" -Force -ErrorAction SilentlyContinue

    Write-Host "=== Stopping Docker Compose ===" -ForegroundColor Cyan
    docker compose down
}

function Status-All {
    Write-Host "=== Docker Compose status ===" -ForegroundColor Cyan
    docker compose ps

    Write-Host "=== xtunnel process ===" -ForegroundColor Cyan
    Get-Process -Name "xtunnel" -ErrorAction SilentlyContinue | Format-Table Id, ProcessName, StartTime
}

switch ($Action.ToLower()) {
    "up" { Start-All }
    "down" { Stop-All }
    default {
        Write-Host "Usage: .\start.ps1 [up|down]" -ForegroundColor White
        Write-Host "  up   - Start Docker + xtunnel (default)" -ForegroundColor Green
        Write-Host "  down - Stop xtunnel + Docker" -ForegroundColor Red
    }
}
