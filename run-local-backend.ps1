# PowerShell script to run the local backend for AetherThrone
# This script starts the local LLM backend without requiring Anthropic API

Write-Host "🚀 Starting AetherThrone Local Backend Server" -ForegroundColor Green
Write-Host "   Using local LLM simulation instead of Claude API" -ForegroundColor Yellow
Write-Host ""

# Check if Python is available
if (!(Get-Command python -ErrorAction SilentlyContinue)) {
    Write-Error "❌ Python is not installed or not in PATH. Please install Python 3.8+."
    exit 1
}

# Check if required packages are installed
Write-Host "📦 Checking for required Python packages..." -ForegroundColor Cyan

$missing_packages = @()
$req_file = "Backend\requirements.txt"

if (Test-Path $req_file) {
    $packages = Get-Content $req_file | Where-Object { $_ -notmatch "^#" -and $_ -ne "" }
    foreach ($pkg in $packages) {
        if ($pkg -ne "anthropic") {  # Skip anthropic since we're not using it
            try {
                python -c "import $($pkg.split('[')[0])" 2>$null
                if ($LASTEXITCODE -ne 0) {
                    $missing_packages += $pkg
                }
            } catch {
                $missing_packages += $pkg
            }
        }
    }
} else {
    Write-Error "❌ requirements.txt not found in Backend directory"
    exit 1
}

if ($missing_packages.Count -gt 0) {
    Write-Host "📦 Installing missing packages: $($missing_packages -join ", ")" -ForegroundColor Yellow
    $missing_list = $missing_packages -join " "
    python -m pip install $missing_list
}

# Check if .env file exists, if not create a basic one
$env_file = "Backend\.env"
if (!(Test-Path $env_file)) {
    Write-Host "📝 Creating .env file..." -ForegroundColor Cyan
    "# Local backend doesn't require API keys" | Out-File -FilePath $env_file -Encoding UTF8
}

# Start the local backend
Write-Host "🎮 Starting local backend server..." -ForegroundColor Green
Write-Host "   Server will be available at http://127.0.0.1:8000" -ForegroundColor Cyan
Write-Host "   Press Ctrl+C to stop the server" -ForegroundColor Yellow
Write-Host ""

# Change to Backend directory and start the server
Push-Location -Path "Backend"
try {
    python main_local.py
} finally {
    Pop-Location
}