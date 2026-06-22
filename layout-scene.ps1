$BASE = "http://127.0.0.1:7777"
$errors = 0

function F($go, $comp, $field, $val) {
    Start-Sleep -Milliseconds 120
    try {
        $body = "{`"gameObject`":`"$go`",`"component`":`"$comp`",`"field`":`"$field`",`"value`":`"$val`"}"
        $r = Invoke-RestMethod -Uri "$BASE/set-field" -Method POST -Body $body -ContentType "application/json" -TimeoutSec 15
        if ($r.ok -eq $false) { Write-Warning "$go/$field FAIL: $($r.error)"; $script:errors++ }
    } catch { Write-Warning "$go/$field ERR: $_"; $script:errors++ }
}

# Shorthand: set RectTransform anchors + position + size in one go
function Rect($go, $axMin, $ayMin, $axMax, $ayMax, $px, $py, $sx, $sy, $pivX=0.5, $pivY=0.5) {
    F $go "RectTransform" "m_AnchorMin.x" "$axMin"
    F $go "RectTransform" "m_AnchorMin.y" "$ayMin"
    F $go "RectTransform" "m_AnchorMax.x" "$axMax"
    F $go "RectTransform" "m_AnchorMax.y" "$ayMax"
    F $go "RectTransform" "m_AnchoredPosition.x" "$px"
    F $go "RectTransform" "m_AnchoredPosition.y" "$py"
    F $go "RectTransform" "m_SizeDelta.x" "$sx"
    F $go "RectTransform" "m_SizeDelta.y" "$sy"
    F $go "RectTransform" "m_Pivot.x" "$pivX"
    F $go "RectTransform" "m_Pivot.y" "$pivY"
}

function Color4($go, $r, $g, $b, $a=1) {
    F $go "Image" "m_Color.r" "$r"
    F $go "Image" "m_Color.g" "$g"
    F $go "Image" "m_Color.b" "$b"
    F $go "Image" "m_Color.a" "$a"
}

Write-Host "`n=== IntroOverlay: full-screen black ===" -ForegroundColor Cyan
Rect "IntroOverlay" 0 0 1 1 0 0 0 0
Color4 "IntroOverlay" 0 0 0 1

Write-Host "`n=== LoreText: stretch inside overlay ===" -ForegroundColor Cyan
Rect "LoreText" 0.1 0.2 0.9 0.8 0 0 0 0
F "LoreText" "TextMeshProUGUI" "m_fontSize" "28"

Write-Host "`n=== GalaxyUI: full-screen ===" -ForegroundColor Cyan
Rect "GalaxyUI" 0 0 1 1 0 0 0 0

Write-Host "`n=== MapContainer: center 70% ===" -ForegroundColor Cyan
Rect "MapContainer" 0.15 0.1 0.85 0.9 0 0 0 0

Write-Host "`n=== HUD: top bar ===" -ForegroundColor Cyan
Rect "HUD" 0 1 1 1 0 0 0 50 0.5 1

Write-Host "`n=== HUD Labels ===" -ForegroundColor Cyan
Rect "TurnLabel"     0    0 0.12 1 0 0 0 0
Rect "InfluenceLabel" 0.12 0 0.28 1 0 0 0 0
Rect "SanityLabel"   0.28 0 0.44 1 0 0 0 0
Rect "TensionBar"    0.46 0.2 0.70 0.8 0 0 0 0
Rect "TensionLabel"  0.70 0 0.88 1 0 0 0 0
Rect "FS_ToggleBtn"  0.88 0.1 1.0 0.9 0 0 0 0

Write-Host "`n=== EndTurnButton: bottom right ===" -ForegroundColor Cyan
Rect "EndTurnButton" 1 0 1 0 -110 20 180 50 1 0

Write-Host "`n=== SystemInfoPanel: right panel ===" -ForegroundColor Cyan
Rect "SystemInfoPanel" 0.78 0.1 1 0.85 0 0 0 0
Color4 "SystemInfoPanel" 0.1 0.1 0.15 0.92
Rect "SIP_SystemName"  0 0.88 1 1    0 0 0 0
Rect "SIP_SystemType"  0 0.78 1 0.88 0 0 0 0
Rect "SIP_Faction"     0 0.68 1 0.78 0 0 0 0
Rect "SIP_Population"  0 0.58 1 0.68 0 0 0 0
Rect "SIP_SpiceYield"  0 0.48 1 0.58 0 0 0 0
Rect "SIP_Military"    0 0.38 1 0.48 0 0 0 0
Rect "SIP_CharButtons" 0 0.05 1 0.35 0 0 0 0

Write-Host "`n=== RoutePanel: small popup center-right ===" -ForegroundColor Cyan
Rect "RoutePanel" 0.55 0.3 0.78 0.72 0 0 0 0
Color4 "RoutePanel" 0.08 0.1 0.14 0.95
Rect "RP_Header"  0 0.85 1 1    0 0 0 0
Rect "RP_Status"  0 0.72 1 0.85 0 0 0 0
Rect "RP_Cost"    0 0.59 1 0.72 0 0 0 0
Rect "RP_Trade"   0 0.46 1 0.59 0 0 0 0
Rect "RP_Military" 0 0.33 1 0.46 0 0 0 0
Rect "RP_Warning" 0 0.2  1 0.33 0 0 0 0
Rect "RP_OpenBtn"  0    0.03 0.45 0.17 0 0 0 0
Rect "RP_CloseBtn" 0.55 0.03 1    0.17 0 0 0 0

Write-Host "`n=== FactionStatusUI: left overlay ===" -ForegroundColor Cyan
Rect "FactionStatusUI" 0 0.3 0.22 0.92 0 0 0 0
Color4 "FactionStatusUI" 0.08 0.1 0.14 0.95
Rect "FS_ListContainer" 0 0 1 1 0 0 0 0

Write-Host "`n=== FactionOfferUI: center popup ===" -ForegroundColor Cyan
Rect "FactionOfferUI" 0.2 0.2 0.8 0.8 0 0 0 0
Color4 "FactionOfferUI" 0.06 0.07 0.1 0.97
Rect "FO_Title"     0 0.85 1 1    0 0 0 0
Rect "FO_Container" 0 0.12 1 0.85 0 0 0 0
Rect "FO_CloseBtn"  0.35 0.01 0.65 0.11 0 0 0 0

Write-Host "`n=== DialogueUI: center panel ===" -ForegroundColor Cyan
Rect "DialogueUI" 0.1 0.05 0.9 0.95 0 0 0 0
Color4 "DialogueUI" 0.05 0.05 0.08 0.97
Rect "DU_Portrait"    0 0.82 0.18 1    0 0 0 0
Rect "DU_CharName"    0.2 0.88 0.7  1    0 0 0 0
Rect "DU_StageBadge"  0.7 0.88 1.0  1    0 0 0 0
Rect "DU_BondBar"     0.2 0.82 0.75 0.88 0 0 0 0
Rect "DU_BondLabel"   0.76 0.82 1.0  0.88 0 0 0 0
Rect "DU_HistoryScroll" 0 0.12 1 0.82 0 0 0 0
Rect "DU_HistoryContainer" 0 0 1 1  0 0 0 0
Rect "DU_InputField"  0 0.04 0.82 0.12 0 0 0 0
Rect "DU_SendBtn"    0.83 0.04 1    0.12 0 0 0 0
Rect "DU_CloseBtn"   0.83 0.86 1    0.96 0 0 0 0
Rect "DU_ThinkingIndicator" 0.35 0.01 0.65 0.04 0 0 0 0
Rect "DU_StageBadge" 0.7 0.88 1.0 1.0 0 0 0 0

# Background dark panel on DialogueUI root
F "DialogueUI" "Image" "m_Color.r" "0.05"
F "DialogueUI" "Image" "m_Color.g" "0.05"
F "DialogueUI" "Image" "m_Color.b" "0.08"
F "DialogueUI" "Image" "m_Color.a" "0.97"

Write-Host "`n=== Save ===" -ForegroundColor Cyan
Start-Sleep -Milliseconds 500
$r = Invoke-RestMethod -Uri "$BASE/save-scene" -Method POST -Body '{}' -ContentType "application/json" -TimeoutSec 20
Write-Host "Scene: $($r.msg)"

$col = if ($errors -eq 0) { "Green" } else { "Yellow" }
Write-Host "`n=== DONE: $errors error(s) ===" -ForegroundColor $col
