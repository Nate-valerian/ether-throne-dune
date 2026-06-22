$BASE = "http://127.0.0.1:7777"
$errors = 0

function U($path, $body) {
    Start-Sleep -Milliseconds 150  # give Unity's main thread time between calls
    try {
        $r = Invoke-RestMethod -Uri "$BASE$path" -Method POST -Body $body -ContentType "application/json" -TimeoutSec 15
        if ($r.ok -eq $false) { Write-Warning "  FAIL >> $($r.error)"; $script:errors++ }
        else                   { Write-Host    "  ok   $($r.msg)" }
    } catch { Write-Warning "  ERR  $path >> $_"; $script:errors++ }
}

Write-Host "`n=== FIX: MISSING OBJECTS ===" -ForegroundColor Cyan

# Relationships child of GameManager (failed first time)
U "/create" '{"name":"Relationships","parent":"GameManager"}'
U "/add-component" '{"gameObject":"Relationships","type":"RelationshipSystem"}'

# NavigatorController failed to add (protocol violation)
U "/add-component" '{"gameObject":"Navigator","type":"NavigatorController"}'

# Canvas component failed
U "/add-component" '{"gameObject":"Canvas","type":"Canvas"}'

# TextMeshProUGUI failures (intermittent)
U "/add-component" '{"gameObject":"InfluenceLabel","type":"TextMeshProUGUI"}'
U "/add-component" '{"gameObject":"SanityLabel","type":"TextMeshProUGUI"}'
U "/add-component" '{"gameObject":"TensionBar","type":"Slider"}'
U "/add-component" '{"gameObject":"SIP_SystemType","type":"TextMeshProUGUI"}'
U "/add-component" '{"gameObject":"SIP_SpiceYield","type":"TextMeshProUGUI"}'
U "/add-component" '{"gameObject":"RP_Header","type":"TextMeshProUGUI"}'
U "/add-component" '{"gameObject":"RP_Cost","type":"TextMeshProUGUI"}'
U "/add-component" '{"gameObject":"FO_Title","type":"TextMeshProUGUI"}'
U "/add-component" '{"gameObject":"RP_OpenBtn","type":"Button"}'

# DU_StageBadge and RP_Warning didn't get created
U "/create" '{"name":"DU_StageBadge","parent":"DialogueUI"}'
U "/add-component" '{"gameObject":"DU_StageBadge","type":"TextMeshProUGUI"}'
U "/create" '{"name":"RP_Warning","parent":"RoutePanel"}'
U "/add-component" '{"gameObject":"RP_Warning","type":"TextMeshProUGUI"}'

Write-Host "`n=== FIX: CANVAS RENDER MODE ===" -ForegroundColor Cyan
# Internal serialized field name for Canvas.renderMode is m_RenderMode
U "/set-field" '{"gameObject":"Canvas","component":"Canvas","field":"m_RenderMode","value":"0"}'

Write-Host "`n=== FIX: WIRE FAILURES ===" -ForegroundColor Yellow

# GameBootstrap refs that failed
U "/set-reference" '{"gameObject":"Bootstrap","component":"GameBootstrap","field":"galaxyUI","target":"GalaxyUI","targetComponent":"GalaxyUI"}'
U "/set-reference" '{"gameObject":"Bootstrap","component":"GameBootstrap","field":"factionStatusUI","target":"FactionStatusUI","targetComponent":"FactionStatusUI"}'

# GalaxyUI._mapContainer — use RectTransform (UI objects have RectTransform, not Transform)
U "/set-reference" '{"gameObject":"GalaxyUI","component":"GalaxyUI","field":"_mapContainer","target":"MapContainer","targetComponent":"RectTransform"}'
U "/set-reference" '{"gameObject":"GalaxyUI","component":"GalaxyUI","field":"_routePanel","target":"RoutePanel","targetComponent":"RoutePanel"}'

# SystemInfoPanel — Transform fields need RectTransform on UI objects
U "/set-reference" '{"gameObject":"SystemInfoPanel","component":"SystemInfoPanel","field":"_systemType","target":"SIP_SystemType","targetComponent":"TextMeshProUGUI"}'
U "/set-reference" '{"gameObject":"SystemInfoPanel","component":"SystemInfoPanel","field":"_spiceYield","target":"SIP_SpiceYield","targetComponent":"TextMeshProUGUI"}'
U "/set-reference" '{"gameObject":"SystemInfoPanel","component":"SystemInfoPanel","field":"_characterButtonContainer","target":"SIP_CharButtons","targetComponent":"RectTransform"}'

# RoutePanel
U "/set-reference" '{"gameObject":"RoutePanel","component":"RoutePanel","field":"_routeHeader","target":"RP_Header","targetComponent":"TextMeshProUGUI"}'
U "/set-reference" '{"gameObject":"RoutePanel","component":"RoutePanel","field":"_costLabel","target":"RP_Cost","targetComponent":"TextMeshProUGUI"}'
U "/set-reference" '{"gameObject":"RoutePanel","component":"RoutePanel","field":"_openButton","target":"RP_OpenBtn","targetComponent":"Button"}'
U "/set-reference" '{"gameObject":"RoutePanel","component":"RoutePanel","field":"_influenceWarning","target":"RP_Warning","targetComponent":"TextMeshProUGUI"}'

# FactionStatusUI — listContainer is Transform -> use RectTransform
U "/set-reference" '{"gameObject":"FactionStatusUI","component":"FactionStatusUI","field":"_listContainer","target":"FS_ListContainer","targetComponent":"RectTransform"}'

# FactionOfferUI — offerContainer is Transform -> use RectTransform
U "/set-reference" '{"gameObject":"FactionOfferUI","component":"FactionOfferUI","field":"_offerContainer","target":"FO_Container","targetComponent":"RectTransform"}'

# DialogueUI — stageBadge (was missing), historyContainer (Transform -> RectTransform)
U "/set-reference" '{"gameObject":"DialogueUI","component":"DialogueUI","field":"_stageBadge","target":"DU_StageBadge","targetComponent":"TextMeshProUGUI"}'
U "/set-reference" '{"gameObject":"DialogueUI","component":"DialogueUI","field":"_historyContainer","target":"DU_HistoryContainer","targetComponent":"RectTransform"}'

Write-Host "`n=== SAVE ===" -ForegroundColor Cyan
Start-Sleep -Milliseconds 500
U "/save-scene" '{}'

$color = if ($errors -eq 0) { "Green" } else { "Red" }
Write-Host "`n=== DONE: $errors error(s) ===" -ForegroundColor $color
