$BASE = "http://localhost:7777"
function U($path, $body = "{}") {
    try {
        $r = Invoke-RestMethod -Uri "$BASE$path" -Method POST -Body $body -ContentType "application/json" -ErrorAction Stop
        if ($r.ok -eq $false) { Write-Warning "  FAIL  >> $($r.error)" }
        else                   { Write-Host    "  ok    $($r.msg)" }
    } catch { Write-Warning "  ERR   >> $_" }
}

Write-Host "`n=== WIRE MISSING COMPONENTS ===" -ForegroundColor Yellow
# Components that failed first time due to protocol timeouts
U "/add-component" '{"gameObject":"WarSystem","type":"WarSystem"}'
U "/add-component" '{"gameObject":"InfluenceLabel","type":"TextMeshProUGUI"}'
U "/add-component" '{"gameObject":"SanityLabel","type":"TextMeshProUGUI"}'
U "/add-component" '{"gameObject":"SP_SystemName","type":"TextMeshProUGUI"}'
U "/add-component" '{"gameObject":"SP_IsolatedWarning","type":"Image"}'
U "/add-component" '{"gameObject":"RP_Trade","type":"TextMeshProUGUI"}'
U "/add-component" '{"gameObject":"RP_Military","type":"TextMeshProUGUI"}'
U "/add-component" '{"gameObject":"RP_OpenBtn","type":"Image"}'
U "/add-component" '{"gameObject":"RP_CloseBtn","type":"Button"}'
U "/add-component" '{"gameObject":"DU_Portrait","type":"Image"}'
U "/add-component" '{"gameObject":"DU_HistoryScroll","type":"Image"}'
U "/add-component" '{"gameObject":"DU_SendBtn","type":"Button"}'
U "/add-component" '{"gameObject":"DU_CloseBtn","type":"Image"}'

Write-Host "`n=== WIRING ALL REFERENCES ===" -ForegroundColor Yellow

# Bootstrap
U "/set-reference" '{"gameObject":"Bootstrap","component":"GameBootstrap","field":"gameManager","target":"GameManager","targetComponent":"GameManager"}'
U "/set-reference" '{"gameObject":"Bootstrap","component":"GameBootstrap","field":"llmService","target":"LLMService","targetComponent":"LLMService"}'
U "/set-reference" '{"gameObject":"Bootstrap","component":"GameBootstrap","field":"galaxyUI","target":"GalaxyUI","targetComponent":"GalaxyUI"}'
U "/set-reference" '{"gameObject":"Bootstrap","component":"GameBootstrap","field":"dialogueUI","target":"DialogueUI","targetComponent":"DialogueUI"}'
U "/set-reference" '{"gameObject":"Bootstrap","component":"GameBootstrap","field":"factionStatusUI","target":"FactionStatusUI","targetComponent":"FactionStatusUI"}'

# GalaxyUI
U "/set-reference" '{"gameObject":"GalaxyUI","component":"GalaxyUI","field":"_mapContainer","target":"MapContainer","targetComponent":"RectTransform"}'
U "/set-reference" '{"gameObject":"GalaxyUI","component":"GalaxyUI","field":"_turnLabel","target":"TurnLabel","targetComponent":"TextMeshProUGUI"}'
U "/set-reference" '{"gameObject":"GalaxyUI","component":"GalaxyUI","field":"_influenceLabel","target":"InfluenceLabel","targetComponent":"TextMeshProUGUI"}'
U "/set-reference" '{"gameObject":"GalaxyUI","component":"GalaxyUI","field":"_sanityLabel","target":"SanityLabel","targetComponent":"TextMeshProUGUI"}'
U "/set-reference" '{"gameObject":"GalaxyUI","component":"GalaxyUI","field":"_tensionBar","target":"TensionBar","targetComponent":"Slider"}'
U "/set-reference" '{"gameObject":"GalaxyUI","component":"GalaxyUI","field":"_tensionLabel","target":"TensionLabel","targetComponent":"TextMeshProUGUI"}'
U "/set-reference" '{"gameObject":"GalaxyUI","component":"GalaxyUI","field":"_systemInfoPanel","target":"SystemInfoPanel","targetComponent":"SystemInfoPanel"}'
U "/set-reference" '{"gameObject":"GalaxyUI","component":"GalaxyUI","field":"_routePanel","target":"RoutePanel","targetComponent":"RoutePanel"}'
U "/set-reference" '{"gameObject":"GalaxyUI","component":"GalaxyUI","field":"_endTurnButton","target":"EndTurnButton","targetComponent":"Button"}'

# SystemInfoPanel
U "/set-reference" '{"gameObject":"SystemInfoPanel","component":"SystemInfoPanel","field":"_systemName","target":"SP_SystemName","targetComponent":"TextMeshProUGUI"}'
U "/set-reference" '{"gameObject":"SystemInfoPanel","component":"SystemInfoPanel","field":"_systemType","target":"SP_SystemType","targetComponent":"TextMeshProUGUI"}'
U "/set-reference" '{"gameObject":"SystemInfoPanel","component":"SystemInfoPanel","field":"_controllingFaction","target":"SP_Faction","targetComponent":"TextMeshProUGUI"}'
U "/set-reference" '{"gameObject":"SystemInfoPanel","component":"SystemInfoPanel","field":"_population","target":"SP_Population","targetComponent":"TextMeshProUGUI"}'
U "/set-reference" '{"gameObject":"SystemInfoPanel","component":"SystemInfoPanel","field":"_spiceYield","target":"SP_SpiceYield","targetComponent":"TextMeshProUGUI"}'
U "/set-reference" '{"gameObject":"SystemInfoPanel","component":"SystemInfoPanel","field":"_militaryStrength","target":"SP_Military","targetComponent":"TextMeshProUGUI"}'
U "/set-reference" '{"gameObject":"SystemInfoPanel","component":"SystemInfoPanel","field":"_isolatedWarning","target":"SP_IsolatedWarning","targetComponent":""}'
U "/set-reference" '{"gameObject":"SystemInfoPanel","component":"SystemInfoPanel","field":"_characterButtonContainer","target":"SP_CharButtons","targetComponent":"Transform"}'
U "/set-reference" '{"gameObject":"SystemInfoPanel","component":"SystemInfoPanel","field":"_dialogueUI","target":"DialogueUI","targetComponent":"DialogueUI"}'

# RoutePanel
U "/set-reference" '{"gameObject":"RoutePanel","component":"RoutePanel","field":"_routeHeader","target":"RP_Header","targetComponent":"TextMeshProUGUI"}'
U "/set-reference" '{"gameObject":"RoutePanel","component":"RoutePanel","field":"_statusLabel","target":"RP_Status","targetComponent":"TextMeshProUGUI"}'
U "/set-reference" '{"gameObject":"RoutePanel","component":"RoutePanel","field":"_costLabel","target":"RP_Cost","targetComponent":"TextMeshProUGUI"}'
U "/set-reference" '{"gameObject":"RoutePanel","component":"RoutePanel","field":"_tradeValue","target":"RP_Trade","targetComponent":"TextMeshProUGUI"}'
U "/set-reference" '{"gameObject":"RoutePanel","component":"RoutePanel","field":"_militaryValue","target":"RP_Military","targetComponent":"TextMeshProUGUI"}'
U "/set-reference" '{"gameObject":"RoutePanel","component":"RoutePanel","field":"_influenceWarning","target":"RP_InfluenceWarn","targetComponent":"TextMeshProUGUI"}'
U "/set-reference" '{"gameObject":"RoutePanel","component":"RoutePanel","field":"_openButton","target":"RP_OpenBtn","targetComponent":"Button"}'
U "/set-reference" '{"gameObject":"RoutePanel","component":"RoutePanel","field":"_closeButton","target":"RP_CloseBtn","targetComponent":"Button"}'

# FactionStatusUI
U "/set-reference" '{"gameObject":"FactionStatusUI","component":"FactionStatusUI","field":"_listContainer","target":"FS_List","targetComponent":"Transform"}'
U "/set-reference" '{"gameObject":"FactionStatusUI","component":"FactionStatusUI","field":"_toggleButton","target":"FS_ToggleBtn","targetComponent":"Button"}'

# FactionOfferUI
U "/set-reference" '{"gameObject":"FactionOfferUI","component":"FactionOfferUI","field":"_offerContainer","target":"FO_OfferContainer","targetComponent":"Transform"}'
U "/set-reference" '{"gameObject":"FactionOfferUI","component":"FactionOfferUI","field":"_closeButton","target":"FO_CloseBtn","targetComponent":"Button"}'

# DialogueUI
U "/set-reference" '{"gameObject":"DialogueUI","component":"DialogueUI","field":"_portrait","target":"DU_Portrait","targetComponent":"Image"}'
U "/set-reference" '{"gameObject":"DialogueUI","component":"DialogueUI","field":"_characterName","target":"DU_CharName","targetComponent":"TextMeshProUGUI"}'
U "/set-reference" '{"gameObject":"DialogueUI","component":"DialogueUI","field":"_stageBadge","target":"DU_StageBadge","targetComponent":"TextMeshProUGUI"}'
U "/set-reference" '{"gameObject":"DialogueUI","component":"DialogueUI","field":"_bondBar","target":"DU_BondBar","targetComponent":"Slider"}'
U "/set-reference" '{"gameObject":"DialogueUI","component":"DialogueUI","field":"_bondLabel","target":"DU_BondLabel","targetComponent":"TextMeshProUGUI"}'
U "/set-reference" '{"gameObject":"DialogueUI","component":"DialogueUI","field":"_historyContainer","target":"DU_HistoryContainer","targetComponent":"Transform"}'
U "/set-reference" '{"gameObject":"DialogueUI","component":"DialogueUI","field":"_historyScroll","target":"DU_HistoryScroll","targetComponent":"ScrollRect"}'
U "/set-reference" '{"gameObject":"DialogueUI","component":"DialogueUI","field":"_inputField","target":"DU_InputField","targetComponent":"TMP_InputField"}'
U "/set-reference" '{"gameObject":"DialogueUI","component":"DialogueUI","field":"_sendButton","target":"DU_SendBtn","targetComponent":"Button"}'
U "/set-reference" '{"gameObject":"DialogueUI","component":"DialogueUI","field":"_closeButton","target":"DU_CloseBtn","targetComponent":"Button"}'
U "/set-reference" '{"gameObject":"DialogueUI","component":"DialogueUI","field":"_thinkingIndicator","target":"DU_Thinking","targetComponent":""}'

Write-Host "`n=== SAVE ===" -ForegroundColor Cyan
U "/save-scene" '{}'
Write-Host "`n=== DONE ===" -ForegroundColor Green
