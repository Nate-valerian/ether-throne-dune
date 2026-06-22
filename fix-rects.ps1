$BASE = "http://127.0.0.1:7777"
$errors = 0

function U($path, $body) {
    Start-Sleep -Milliseconds 150
    try {
        $r = Invoke-RestMethod -Uri "$BASE$path" -Method POST -Body $body -ContentType "application/json" -TimeoutSec 20
        if ($r.ok -eq $false) { Write-Warning "FAIL: $($r.error)"; $script:errors++ }
        else { Write-Host "ok   $($r.msg)" }
    } catch { Write-Warning "ERR: $_"; $script:errors++ }
}

function RT($go) { U "/add-component" "{`"gameObject`":`"$go`",`"type`":`"RectTransform`"}" }
function Img($go) { U "/add-component" "{`"gameObject`":`"$go`",`"type`":`"Image`"}" }

Write-Host "`n=== Add RectTransform to all UI elements ===" -ForegroundColor Cyan
RT "GalaxyUI"
RT "HUD"
RT "TurnLabel"
RT "InfluenceLabel"
RT "SanityLabel"
RT "TensionBar"
RT "TensionLabel"
RT "FS_ToggleBtn"
RT "EndTurnButton"
RT "SystemInfoPanel"
RT "SIP_SystemName"
RT "SIP_SystemType"
RT "SIP_Faction"
RT "SIP_Population"
RT "SIP_SpiceYield"
RT "SIP_Military"
RT "SIP_IsolatedWarning"
RT "SIP_CharButtons"
RT "RoutePanel"
RT "RP_Header"
RT "RP_Status"
RT "RP_Cost"
RT "RP_Trade"
RT "RP_Military"
RT "RP_Warning"
RT "RP_OpenBtn"
RT "RP_CloseBtn"
RT "FactionStatusUI"
RT "FS_ListContainer"
RT "FactionOfferUI"
RT "FO_Title"
RT "FO_Container"
RT "FO_CloseBtn"
RT "DialogueUI"
RT "DU_Portrait"
RT "DU_CharName"
RT "DU_StageBadge"
RT "DU_BondBar"
RT "DU_BondLabel"
RT "DU_HistoryScroll"
RT "DU_InputField"
RT "DU_SendBtn"
RT "DU_CloseBtn"
RT "DU_ThinkingIndicator"
RT "DU_StageBadge"

Write-Host "`n=== Add Image backgrounds to panels ===" -ForegroundColor Cyan
Img "GalaxyUI"
Img "SystemInfoPanel"
Img "RoutePanel"
Img "FactionStatusUI"
Img "FactionOfferUI"
Img "DialogueUI"
Img "EndTurnButton"
Img "FS_ToggleBtn"
Img "RP_OpenBtn"
Img "RP_CloseBtn"
Img "FO_CloseBtn"
Img "DU_SendBtn"
Img "DU_CloseBtn"

Write-Host "`n=== Save ===" -ForegroundColor Cyan
Start-Sleep -Milliseconds 500
$r = Invoke-RestMethod -Uri "$BASE/save-scene" -Method POST -Body '{}' -ContentType "application/json" -TimeoutSec 20
Write-Host "Scene: $($r.msg)"

$col = if ($errors -eq 0) { "Green" } else { "Yellow" }
Write-Host "`n=== DONE: $errors error(s) ===" -ForegroundColor $col
