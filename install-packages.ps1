# PowerShell script to install required Unity packages for AetherThrone
# This script installs TextMeshPro and other necessary packages

Write-Host "📦 Installing required Unity packages for AetherThrone" -ForegroundColor Green
Write-Host ""

# Check if Unity project exists
if (!(Test-Path "Assets")) {
    Write-Error "❌ This doesn't appear to be a Unity project directory. Please run this script from the project root."
    exit 1
}

Write-Host "📋 Current packages in manifest.json:" -ForegroundColor Cyan
$manifestPath = "Packages\manifest.json"
if (Test-Path $manifestPath) {
    $manifest = Get-Content $manifestPath | ConvertFrom-Json
    $manifest.dependencies.PSObject.Properties | ForEach-Object {
        Write-Host "   $($_.Name): $($_.Value)"
    }
} else {
    Write-Host "   No manifest.json found, creating default one..." -ForegroundColor Yellow
    
    $defaultManifest = @{
        dependencies = @{
            "com.unity.textmeshpro" = "3.0.9"
            "com.unity.ugui" = "2.0.0"
            "com.unity.modules.ui" = "1.0.0"
            "com.unity.modules.unitywebrequest" = "1.0.0"
            "com.unity.modules.jsonserialize" = "1.0.0"
            "com.unity.modules.screencapture" = "1.0.0"
        }
    }
    
    $defaultManifest | ConvertTo-Json -Depth 10 | Out-File -FilePath $manifestPath -Encoding UTF8
    Write-Host "✅ Created default manifest.json with required packages" -ForegroundColor Green
}

# Check for TextMeshPro specifically
if (Test-Path $manifestPath) {
    $manifest = Get-Content $manifestPath | ConvertFrom-Json
    if ($manifest.dependencies."com.unity.textmeshpro") {
        Write-Host "✅ TextMeshPro is already in manifest.json" -ForegroundColor Green
    } else {
        Write-Host "➕ Adding TextMeshPro to manifest.json..." -ForegroundColor Yellow
        $manifest.dependencies."com.unity.textmeshpro" = "3.0.9"
        $manifest | ConvertTo-Json -Depth 10 | Out-File -FilePath $manifestPath -Encoding UTF8
        Write-Host "✅ Added TextMeshPro to manifest.json" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "💡 To complete package installation:" -ForegroundColor Cyan
Write-Host "   1. Open the project in Unity Hub" -ForegroundColor White
Write-Host "   2. Unity will automatically resolve packages from manifest.json" -ForegroundColor White
Write-Host "   3. If prompted, import TMP Essential Resources when opening the project" -ForegroundColor White
Write-Host ""

Write-Host "🎉 Required packages setup complete!" -ForegroundColor Green
Write-Host "   The project now includes TextMeshPro and other essential packages." -ForegroundColor White