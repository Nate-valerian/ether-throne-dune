$BASE = "http://127.0.0.1:7777"
$errors = 0

function U($path, $body) {
    Start-Sleep -Milliseconds 250
    try {
        $r = Invoke-RestMethod -Uri "$BASE$path" -Method POST -Body $body -ContentType "application/json" -TimeoutSec 20
        if ($r.ok -eq $false) { Write-Warning "FAIL >> $($r.error)"; $script:errors++ }
        else { Write-Host "ok   $($r.msg)" }
    } catch { Write-Warning "ERR  >> $_"; $script:errors++ }
}

Write-Host "`n=== DELETE MISPLACED + DUPLICATES ===" -ForegroundColor Red
# Delete the 3 stray NewObjects at root
U "/delete" '{"gameObject":"NewObject"}'
U "/delete" '{"gameObject":"NewObject"}'
U "/delete" '{"gameObject":"NewObject"}'
# Duplicate RP_Warning will be handled by reparent (first instance stays, second was the retry)

Write-Host "`n=== REPARENT SystemInfoPanel children ===" -ForegroundColor Cyan
U "/reparent" '{"gameObject":"SIP_SystemName","parent":"SystemInfoPanel"}'
U "/reparent" '{"gameObject":"SIP_SystemType","parent":"SystemInfoPanel"}'
U "/reparent" '{"gameObject":"SIP_Faction","parent":"SystemInfoPanel"}'
U "/reparent" '{"gameObject":"SIP_Population","parent":"SystemInfoPanel"}'
U "/reparent" '{"gameObject":"SIP_SpiceYield","parent":"SystemInfoPanel"}'
U "/reparent" '{"gameObject":"SIP_Military","parent":"SystemInfoPanel"}'
U "/reparent" '{"gameObject":"SIP_IsolatedWarning","parent":"SystemInfoPanel"}'
U "/reparent" '{"gameObject":"SIP_CharButtons","parent":"SystemInfoPanel"}'

Write-Host "`n=== REPARENT RoutePanel children ===" -ForegroundColor Cyan
U "/reparent" '{"gameObject":"RP_Header","parent":"RoutePanel"}'
U "/reparent" '{"gameObject":"RP_Status","parent":"RoutePanel"}'
U "/reparent" '{"gameObject":"RP_Cost","parent":"RoutePanel"}'
U "/reparent" '{"gameObject":"RP_Trade","parent":"RoutePanel"}'
U "/reparent" '{"gameObject":"RP_Military","parent":"RoutePanel"}'
U "/reparent" '{"gameObject":"RP_Warning","parent":"RoutePanel"}'
U "/reparent" '{"gameObject":"RP_OpenBtn","parent":"RoutePanel"}'
U "/reparent" '{"gameObject":"RP_CloseBtn","parent":"RoutePanel"}'

Write-Host "`n=== REPARENT FactionStatusUI children ===" -ForegroundColor Cyan
U "/reparent" '{"gameObject":"FS_ListContainer","parent":"FactionStatusUI"}'

Write-Host "`n=== REPARENT FactionOfferUI children ===" -ForegroundColor Cyan
U "/reparent" '{"gameObject":"FO_Title","parent":"FactionOfferUI"}'
U "/reparent" '{"gameObject":"FO_Container","parent":"FactionOfferUI"}'
U "/reparent" '{"gameObject":"FO_CloseBtn","parent":"FactionOfferUI"}'

Write-Host "`n=== REPARENT DialogueUI children ===" -ForegroundColor Cyan
U "/reparent" '{"gameObject":"DU_Portrait","parent":"DialogueUI"}'
U "/reparent" '{"gameObject":"DU_CharName","parent":"DialogueUI"}'
U "/reparent" '{"gameObject":"DU_StageBadge","parent":"DialogueUI"}'
U "/reparent" '{"gameObject":"DU_BondBar","parent":"DialogueUI"}'
U "/reparent" '{"gameObject":"DU_BondLabel","parent":"DialogueUI"}'
U "/reparent" '{"gameObject":"DU_HistoryScroll","parent":"DialogueUI"}'
U "/reparent" '{"gameObject":"DU_InputField","parent":"DialogueUI"}'
U "/reparent" '{"gameObject":"DU_SendBtn","parent":"DialogueUI"}'
U "/reparent" '{"gameObject":"DU_CloseBtn","parent":"DialogueUI"}'
U "/reparent" '{"gameObject":"DU_ThinkingIndicator","parent":"DialogueUI"}'

Write-Host "`n=== SAVE ===" -ForegroundColor Cyan
Start-Sleep -Milliseconds 500
U "/save-scene" '{}'

$color = if ($errors -eq 0) { "Green" } else { "Red" }
Write-Host "`n=== DONE: $errors error(s) ===" -ForegroundColor $color
