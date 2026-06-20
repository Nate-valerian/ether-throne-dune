# claude-unity.ps1 — Claude Code helper for talking to the Unity Editor control server
# Usage: .\claude-unity.ps1 <command> [json-body]
#   .\claude-unity.ps1 ping
#   .\claude-unity.ps1 hierarchy
#   .\claude-unity.ps1 logs
#   .\claude-unity.ps1 create   '{"name":"GameManager"}'
#   .\claude-unity.ps1 component '{"gameObject":"GameManager","type":"GameManager"}'
#   .\claude-unity.ps1 active   '{"gameObject":"DialogueUI","active":"false"}'
#   .\claude-unity.ps1 field    '{"gameObject":"Bootstrap","component":"GameBootstrap","field":"gameManager","value":"..."}'
#   .\claude-unity.ps1 ref      '{"gameObject":"Bootstrap","component":"GameBootstrap","field":"gameManager","target":"GameManager","targetComponent":"GameManager"}'
#   .\claude-unity.ps1 prefab   '{"prefab":"Assets/Prefabs/SystemNodePrefab.prefab","parent":"MapContainer"}'
#   .\claude-unity.ps1 play
#   .\claude-unity.ps1 stop
#   .\claude-unity.ps1 save

param(
    [Parameter(Mandatory)][string]$Command,
    [string]$Body = "{}"
)

$BASE = "http://localhost:7777"

$routes = @{
    "ping"      = @{ method = "GET";  path = "/ping" }
    "hierarchy" = @{ method = "GET";  path = "/hierarchy" }
    "logs"      = @{ method = "GET";  path = "/logs" }
    "create"    = @{ method = "POST"; path = "/create" }
    "component" = @{ method = "POST"; path = "/add-component" }
    "active"    = @{ method = "POST"; path = "/set-active" }
    "field"     = @{ method = "POST"; path = "/set-field" }
    "ref"       = @{ method = "POST"; path = "/set-reference" }
    "prefab"    = @{ method = "POST"; path = "/create-prefab-instance" }
    "play"      = @{ method = "POST"; path = "/play" }
    "stop"      = @{ method = "POST"; path = "/stop" }
    "save"      = @{ method = "POST"; path = "/save-scene" }
}

if (-not $routes.ContainsKey($Command)) {
    Write-Error "Unknown command '$Command'. Valid: $($routes.Keys -join ', ')"
    exit 1
}

$r      = $routes[$Command]
$uri    = "$BASE$($r.path)"
$method = $r.method

try {
    if ($method -eq "GET") {
        $response = Invoke-RestMethod -Uri $uri -Method GET
    } else {
        $response = Invoke-RestMethod -Uri $uri -Method POST -Body $Body -ContentType "application/json"
    }
    $response | ConvertTo-Json -Depth 5
} catch {
    Write-Error "Request failed: $_"
    exit 1
}
