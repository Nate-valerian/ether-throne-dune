# Comprehensive setup script for AetherThrone with local LLM implementation
# This script handles all necessary setup to run the project without Claude API

Write-Host "🌟 Setting up AetherThrone with Local LLM Implementation" -ForegroundColor Green
Write-Host ""

# Check prerequisites
Write-Host "🔍 Checking prerequisites..." -ForegroundColor Cyan

# Check if Python is available
if (!(Get-Command python -ErrorAction SilentlyContinue)) {
    Write-Error "❌ Python is not installed or not in PATH. Please install Python 3.8+."
    exit 1
}

# Check if Git is available (for potential future use)
$gitAvailable = Get-Command git -ErrorAction SilentlyContinue
if (!$gitAvailable) {
    Write-Host "⚠️  Git is not installed - this is optional but recommended" -ForegroundColor Yellow
}

Write-Host "✅ Python is available" -ForegroundColor Green

# Install required Python packages
Write-Host ""
Write-Host "📦 Installing Python packages..." -ForegroundColor Cyan

$req_file = "Backend\requirements.txt"
if (Test-Path $req_file) {
    Write-Host "Installing packages from requirements.txt..." -ForegroundColor White
    python -m pip install -r $req_file
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "There may have been issues installing Python packages"
    } else {
        Write-Host "✅ Python packages installed successfully" -ForegroundColor Green
    }
} else {
    Write-Error "❌ requirements.txt not found in Backend directory"
    exit 1
}

# Ensure Backend directory exists
if (!(Test-Path "Backend\memory")) {
    New-Item -ItemType Directory -Path "Backend\memory" -Force | Out-Null
    Write-Host "✅ Created Backend\memory directory" -ForegroundColor Green
}

# Create/update .env file
$env_file = "Backend\.env"
if (!(Test-Path $env_file)) {
    "# Local backend doesn't require API keys" | Out-File -FilePath $env_file -Encoding UTF8
    Write-Host "✅ Created Backend\.env file" -ForegroundColor Green
} else {
    # Check if it already has Anthropic key, if so, warn about local vs API
    $env_content = Get-Content $env_file -ErrorAction SilentlyContinue
    if ($env_content -match "ANTHROPIC_API_KEY") {
        Write-Host "ℹ️  Found Anthropic API key in .env - the local backend will ignore it" -ForegroundColor Cyan
    }
}

# Create Assets/Scripts/AI directory if it doesn't exist
if (!(Test-Path "Assets\Scripts\AI")) {
    New-Item -ItemType Directory -Path "Assets\Scripts\AI" -Force | Out-Null
    Write-Host "✅ Created Assets\Scripts\AI directory" -ForegroundColor Green
}

# Check if LocalLLMService.cs exists
if (!(Test-Path "Assets\Scripts\AI\LocalLLMService.cs")) {
    Write-Host "⚠️  LocalLLMService.cs not found, you may need to create it manually" -ForegroundColor Yellow
} else {
    Write-Host "✅ LocalLLMService.cs is in place" -ForegroundColor Green
}

# Create Prefabs directory if it doesn't exist
if (!(Test-Path "Assets\Prefabs")) {
    New-Item -ItemType Directory -Path "Assets\Prefabs" -Force | Out-Null
    Write-Host "✅ Created Assets\Prefabs directory" -ForegroundColor Green
}

# Create Resources directory if it doesn't exist
if (!(Test-Path "Assets\Resources")) {
    New-Item -ItemType Directory -Path "Assets\Resources" -Force | Out-Null
    Write-Host "✅ Created Assets\Resources directory" -ForegroundColor Green
}

# Create Portraits subdirectory
if (!(Test-Path "Assets\Resources\Portraits")) {
    New-Item -ItemType Directory -Path "Assets\Resources\Portraits" -Force | Out-Null
    Write-Host "✅ Created Assets\Resources\Portraits directory" -ForegroundColor Green
}

Write-Host ""
Write-Host "🔧 Configuration complete!" -ForegroundColor Green
Write-Host ""

Write-Host "📋 Summary of changes made:" -ForegroundColor Cyan
Write-Host "   • Installed Python packages from Backend/requirements.txt" -ForegroundColor White
Write-Host "   • Created Backend/memory directory for conversation history" -ForegroundColor White
Write-Host "   • Ensured .env file exists in Backend directory" -ForegroundColor White
Write-Host "   • Created necessary directories in Assets folder" -ForegroundColor White
Write-Host ""

Write-Host "🎮 To run the project:" -ForegroundColor Green
Write-Host "   1. Start the local backend: .\run-local-backend.ps1" -ForegroundColor White
Write-Host "   2. Open the project in Unity" -ForegroundColor White
Write-Host "   3. Build the scene: .\build-scene-full.ps1" -ForegroundColor White
Write-Host "   4. Press Play in Unity" -ForegroundColor White
Write-Host ""

Write-Host "💡 Note: The game will now use local LLM simulation instead of Claude API" -ForegroundColor Yellow
Write-Host "   Responses will be generated locally without requiring internet connection" -ForegroundColor Yellow