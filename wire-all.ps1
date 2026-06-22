$BASE = "http://127.0.0.1:7777"
$errors = 0

function Wire($body) {
    Start-Sleep -Milliseconds 200
    try {
        $r = Invoke-RestMethod -Uri "$BASE/set-reference" -Method POST -Body $body -ContentType "application/json" -TimeoutSec 20
        if ($r.ok -eq $false) { Write-Warning "FAIL: $($r.error)"; $script:errors++ }
        else { Write-Host "ok   $($r.msg)" }
    } catch { Write-Warning "ERR: $_"; $script:errors++ }
}

Write-Host "`n=== Bootstrap ===" -ForegroundColor Cyan
Wire '{"gameObject":"Bootstrap","component":"GameBootstrap","field":"gameManager","target":"GameManager","targetComponent":"GameManager"}'
Wire '{"gameObject":"Bootstrap","component":"GameBootstrap","field":"llmService","target":"LLMService","targetComponent":"LLMService"}'
Wire '{"gameObject":"Bootstrap","component":"GameBootstrap","field":"galaxyUI","target":"GalaxyUI","targetComponent":"GalaxyUI"}'
Wire '{"gameObject":"Bootstrap","component":"GameBootstrap","field":"dialogueUI","target":"DialogueUI","targetComponent":"DialogueUI"}'
Wire '{"gameObject":"Bootstrap","component":"GameBootstrap","field":"factionStatusUI","target":"FactionStatusUI","targetComponent":"FactionStatusUI"}'
Wire '{"gameObject":"Bootstrap","component":"GameBootstrap","field":"introSequence","target":"Bootstrap","targetComponent":"IntroSequence"}'

Write-Host "`n=== IntroSequence ===" -ForegroundColor Cyan
Wire '{"gameObject":"Bootstrap","component":"IntroSequence","field":"_overlay","target":"IntroOverlay","targetComponent":"CanvasGroup"}'
Wire '{"gameObject":"Bootstrap","component":"IntroSequence","field":"_loreText","target":"LoreText","targetComponent":"TextMeshProUGUI"}'
Wire '{"gameObject":"Bootstrap","component":"IntroSequence","field":"_dialogueUI","target":"DialogueUI","targetComponent":"DialogueUI"}'

Write-Host "`n=== GalaxyUI ===" -ForegroundColor Cyan
Wire '{"gameObject":"GalaxyUI","component":"GalaxyUI","field":"_mapContainer","target":"MapContainer","targetComponent":"RectTransform"}'
Wire '{"gameObject":"GalaxyUI","component":"GalaxyUI","field":"_turnLabel","target":"TurnLabel","targetComponent":"TextMeshProUGUI"}'
Wire '{"gameObject":"GalaxyUI","component":"GalaxyUI","field":"_influenceLabel","target":"InfluenceLabel","targetComponent":"TextMeshProUGUI"}'
Wire '{"gameObject":"GalaxyUI","component":"GalaxyUI","field":"_sanityLabel","target":"SanityLabel","targetComponent":"TextMeshProUGUI"}'
Wire '{"gameObject":"GalaxyUI","component":"GalaxyUI","field":"_tensionBar","target":"TensionBar","targetComponent":"Slider"}'
Wire '{"gameObject":"GalaxyUI","component":"GalaxyUI","field":"_tensionLabel","target":"TensionLabel","targetComponent":"TextMeshProUGUI"}'
Wire '{"gameObject":"GalaxyUI","component":"GalaxyUI","field":"_systemInfoPanel","target":"SystemInfoPanel","targetComponent":"SystemInfoPanel"}'
Wire '{"gameObject":"GalaxyUI","component":"GalaxyUI","field":"_routePanel","target":"RoutePanel","targetComponent":"RoutePanel"}'
Wire '{"gameObject":"GalaxyUI","component":"GalaxyUI","field":"_endTurnButton","target":"EndTurnButton","targetComponent":"Button"}'

Write-Host "`n=== SystemInfoPanel ===" -ForegroundColor Cyan
Wire '{"gameObject":"SystemInfoPanel","component":"SystemInfoPanel","field":"_systemName","target":"SIP_SystemName","targetComponent":"TextMeshProUGUI"}'
Wire '{"gameObject":"SystemInfoPanel","component":"SystemInfoPanel","field":"_systemType","target":"SIP_SystemType","targetComponent":"TextMeshProUGUI"}'
Wire '{"gameObject":"SystemInfoPanel","component":"SystemInfoPanel","field":"_controllingFaction","target":"SIP_Faction","targetComponent":"TextMeshProUGUI"}'
Wire '{"gameObject":"SystemInfoPanel","component":"SystemInfoPanel","field":"_population","target":"SIP_Population","targetComponent":"TextMeshProUGUI"}'
Wire '{"gameObject":"SystemInfoPanel","component":"SystemInfoPanel","field":"_spiceYield","target":"SIP_SpiceYield","targetComponent":"TextMeshProUGUI"}'
Wire '{"gameObject":"SystemInfoPanel","component":"SystemInfoPanel","field":"_militaryStrength","target":"SIP_Military","targetComponent":"TextMeshProUGUI"}'
Wire '{"gameObject":"SystemInfoPanel","component":"SystemInfoPanel","field":"_isolatedWarning","target":"SIP_IsolatedWarning","targetComponent":""}'
Wire '{"gameObject":"SystemInfoPanel","component":"SystemInfoPanel","field":"_characterButtonContainer","target":"SIP_CharButtons","targetComponent":"RectTransform"}'
Wire '{"gameObject":"SystemInfoPanel","component":"SystemInfoPanel","field":"_dialogueUI","target":"DialogueUI","targetComponent":"DialogueUI"}'

Write-Host "`n=== RoutePanel ===" -ForegroundColor Cyan
Wire '{"gameObject":"RoutePanel","component":"RoutePanel","field":"_routeHeader","target":"RP_Header","targetComponent":"TextMeshProUGUI"}'
Wire '{"gameObject":"RoutePanel","component":"RoutePanel","field":"_statusLabel","target":"RP_Status","targetComponent":"TextMeshProUGUI"}'
Wire '{"gameObject":"RoutePanel","component":"RoutePanel","field":"_costLabel","target":"RP_Cost","targetComponent":"TextMeshProUGUI"}'
Wire '{"gameObject":"RoutePanel","component":"RoutePanel","field":"_tradeValue","target":"RP_Trade","targetComponent":"TextMeshProUGUI"}'
Wire '{"gameObject":"RoutePanel","component":"RoutePanel","field":"_militaryValue","target":"RP_Military","targetComponent":"TextMeshProUGUI"}'
Wire '{"gameObject":"RoutePanel","component":"RoutePanel","field":"_influenceWarning","target":"RP_Warning","targetComponent":"TextMeshProUGUI"}'
Wire '{"gameObject":"RoutePanel","component":"RoutePanel","field":"_openButton","target":"RP_OpenBtn","targetComponent":"Button"}'
Wire '{"gameObject":"RoutePanel","component":"RoutePanel","field":"_closeButton","target":"RP_CloseBtn","targetComponent":"Button"}'

Write-Host "`n=== FactionStatusUI ===" -ForegroundColor Cyan
Wire '{"gameObject":"FactionStatusUI","component":"FactionStatusUI","field":"_listContainer","target":"FS_ListContainer","targetComponent":"RectTransform"}'
Wire '{"gameObject":"FactionStatusUI","component":"FactionStatusUI","field":"_toggleButton","target":"FS_ToggleBtn","targetComponent":"Button"}'

Write-Host "`n=== FactionOfferUI ===" -ForegroundColor Cyan
Wire '{"gameObject":"FactionOfferUI","component":"FactionOfferUI","field":"_offerContainer","target":"FO_Container","targetComponent":"RectTransform"}'
Wire '{"gameObject":"FactionOfferUI","component":"FactionOfferUI","field":"_closeButton","target":"FO_CloseBtn","targetComponent":"Button"}'

Write-Host "`n=== DialogueUI ===" -ForegroundColor Cyan
Wire '{"gameObject":"DialogueUI","component":"DialogueUI","field":"_portrait","target":"DU_Portrait","targetComponent":"Image"}'
Wire '{"gameObject":"DialogueUI","component":"DialogueUI","field":"_characterName","target":"DU_CharName","targetComponent":"TextMeshProUGUI"}'
Wire '{"gameObject":"DialogueUI","component":"DialogueUI","field":"_stageBadge","target":"DU_StageBadge","targetComponent":"TextMeshProUGUI"}'
Wire '{"gameObject":"DialogueUI","component":"DialogueUI","field":"_bondBar","target":"DU_BondBar","targetComponent":"Slider"}'
Wire '{"gameObject":"DialogueUI","component":"DialogueUI","field":"_bondLabel","target":"DU_BondLabel","targetComponent":"TextMeshProUGUI"}'
Wire '{"gameObject":"DialogueUI","component":"DialogueUI","field":"_historyContainer","target":"DU_HistoryContainer","targetComponent":"RectTransform"}'
Wire '{"gameObject":"DialogueUI","component":"DialogueUI","field":"_historyScroll","target":"DU_HistoryScroll","targetComponent":"ScrollRect"}'
Wire '{"gameObject":"DialogueUI","component":"DialogueUI","field":"_inputField","target":"DU_InputField","targetComponent":"TMP_InputField"}'
Wire '{"gameObject":"DialogueUI","component":"DialogueUI","field":"_sendButton","target":"DU_SendBtn","targetComponent":"Button"}'
Wire '{"gameObject":"DialogueUI","component":"DialogueUI","field":"_closeButton","target":"DU_CloseBtn","targetComponent":"Button"}'
Wire '{"gameObject":"DialogueUI","component":"DialogueUI","field":"_thinkingIndicator","target":"DU_ThinkingIndicator","targetComponent":""}'

Write-Host "`n=== Save ===" -ForegroundColor Cyan
Start-Sleep -Milliseconds 400
$r = Invoke-RestMethod -Uri "$BASE/save-scene" -Method POST -Body '{}' -ContentType "application/json" -TimeoutSec 20
Write-Host $r.msg

$color = if ($errors -eq 0) { "Green" } else { "Red" }
Write-Host "`n=== DONE: $errors error(s) ===" -ForegroundColor $color
