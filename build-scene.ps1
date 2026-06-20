$BASE = "http://localhost:7777"

function U($path, $body = "{}") {
    $method = if ($body -eq "{}" -and ($path -eq "/hierarchy" -or $path -eq "/logs")) { "GET" } else { "POST" }
    try {
        $r = Invoke-RestMethod -Uri "$BASE$path" -Method $method -Body $body -ContentType "application/json" -ErrorAction Stop
        if ($r.ok -eq $false) { Write-Warning "  FAIL  $path  >> $($r.error)" }
        else                   { Write-Host    "  ok    $($r.msg)" }
    } catch { Write-Warning "  ERR   $path  >> $_" }
}

Write-Host "`n=== GAME SYSTEMS ===" -ForegroundColor Cyan
U "/create"        '{"name":"GameManager"}'
U "/add-component" '{"gameObject":"GameManager","type":"GameManager"}'
U "/create"        '{"name":"GalaxyMap","parent":"GameManager"}'
U "/add-component" '{"gameObject":"GalaxyMap","type":"GalaxyMap"}'
U "/create"        '{"name":"FactionManager","parent":"GameManager"}'
U "/add-component" '{"gameObject":"FactionManager","type":"FactionManager"}'
U "/create"        '{"name":"Navigator","parent":"GameManager"}'
U "/add-component" '{"gameObject":"Navigator","type":"NavigatorController"}'
U "/create"        '{"name":"WarSystem","parent":"GameManager"}'
U "/add-component" '{"gameObject":"WarSystem","type":"WarSystem"}'
U "/create"        '{"name":"Relationships","parent":"GameManager"}'
U "/add-component" '{"gameObject":"Relationships","type":"RelationshipSystem"}'
U "/create"        '{"name":"LLMService"}'
U "/add-component" '{"gameObject":"LLMService","type":"LLMService"}'
U "/create"        '{"name":"Bootstrap"}'
U "/add-component" '{"gameObject":"Bootstrap","type":"GameBootstrap"}'

Write-Host "`n=== CANVAS ===" -ForegroundColor Cyan
U "/create"        '{"name":"Canvas"}'
U "/add-component" '{"gameObject":"Canvas","type":"Canvas"}'
U "/add-component" '{"gameObject":"Canvas","type":"CanvasScaler"}'
U "/add-component" '{"gameObject":"Canvas","type":"GraphicRaycaster"}'
U "/create"        '{"name":"EventSystem"}'
U "/add-component" '{"gameObject":"EventSystem","type":"EventSystem"}'
U "/add-component" '{"gameObject":"EventSystem","type":"StandaloneInputModule"}'

Write-Host "`n=== GALAXY UI ===" -ForegroundColor Cyan
U "/create"        '{"name":"GalaxyUI","parent":"Canvas"}'
U "/add-component" '{"gameObject":"GalaxyUI","type":"GalaxyUI"}'
U "/add-component" '{"gameObject":"GalaxyUI","type":"Image"}'
U "/create"        '{"name":"MapContainer","parent":"GalaxyUI"}'
U "/add-component" '{"gameObject":"MapContainer","type":"Image"}'   # gives MapContainer a RectTransform
U "/create"        '{"name":"HUD","parent":"GalaxyUI"}'
U "/create"        '{"name":"TurnLabel","parent":"HUD"}'
U "/add-component" '{"gameObject":"TurnLabel","type":"TextMeshProUGUI"}'
U "/create"        '{"name":"InfluenceLabel","parent":"HUD"}'
U "/add-component" '{"gameObject":"InfluenceLabel","type":"TextMeshProUGUI"}'
U "/create"        '{"name":"SanityLabel","parent":"HUD"}'
U "/add-component" '{"gameObject":"SanityLabel","type":"TextMeshProUGUI"}'
U "/create"        '{"name":"TensionBar","parent":"HUD"}'
U "/add-component" '{"gameObject":"TensionBar","type":"Slider"}'
U "/create"        '{"name":"TensionLabel","parent":"HUD"}'
U "/add-component" '{"gameObject":"TensionLabel","type":"TextMeshProUGUI"}'
U "/create"        '{"name":"EndTurnButton","parent":"GalaxyUI"}'
U "/add-component" '{"gameObject":"EndTurnButton","type":"Button"}'
U "/add-component" '{"gameObject":"EndTurnButton","type":"Image"}'

Write-Host "`n=== SYSTEM INFO PANEL ===" -ForegroundColor Cyan
U "/create"        '{"name":"SystemInfoPanel","parent":"Canvas"}'
U "/add-component" '{"gameObject":"SystemInfoPanel","type":"SystemInfoPanel"}'
U "/add-component" '{"gameObject":"SystemInfoPanel","type":"Image"}'
U "/create"        '{"name":"SP_SystemName","parent":"SystemInfoPanel"}'
U "/add-component" '{"gameObject":"SP_SystemName","type":"TextMeshProUGUI"}'
U "/create"        '{"name":"SP_SystemType","parent":"SystemInfoPanel"}'
U "/add-component" '{"gameObject":"SP_SystemType","type":"TextMeshProUGUI"}'
U "/create"        '{"name":"SP_Faction","parent":"SystemInfoPanel"}'
U "/add-component" '{"gameObject":"SP_Faction","type":"TextMeshProUGUI"}'
U "/create"        '{"name":"SP_Population","parent":"SystemInfoPanel"}'
U "/add-component" '{"gameObject":"SP_Population","type":"TextMeshProUGUI"}'
U "/create"        '{"name":"SP_SpiceYield","parent":"SystemInfoPanel"}'
U "/add-component" '{"gameObject":"SP_SpiceYield","type":"TextMeshProUGUI"}'
U "/create"        '{"name":"SP_Military","parent":"SystemInfoPanel"}'
U "/add-component" '{"gameObject":"SP_Military","type":"TextMeshProUGUI"}'
U "/create"        '{"name":"SP_IsolatedWarning","parent":"SystemInfoPanel"}'
U "/create"        '{"name":"SP_CharButtons","parent":"SystemInfoPanel"}'
U "/set-active"    '{"gameObject":"SystemInfoPanel","active":"false"}'

Write-Host "`n=== ROUTE PANEL ===" -ForegroundColor Cyan
U "/create"        '{"name":"RoutePanel","parent":"Canvas"}'
U "/add-component" '{"gameObject":"RoutePanel","type":"RoutePanel"}'
U "/add-component" '{"gameObject":"RoutePanel","type":"Image"}'
U "/create"        '{"name":"RP_Header","parent":"RoutePanel"}'
U "/add-component" '{"gameObject":"RP_Header","type":"TextMeshProUGUI"}'
U "/create"        '{"name":"RP_Status","parent":"RoutePanel"}'
U "/add-component" '{"gameObject":"RP_Status","type":"TextMeshProUGUI"}'
U "/create"        '{"name":"RP_Cost","parent":"RoutePanel"}'
U "/add-component" '{"gameObject":"RP_Cost","type":"TextMeshProUGUI"}'
U "/create"        '{"name":"RP_Trade","parent":"RoutePanel"}'
U "/add-component" '{"gameObject":"RP_Trade","type":"TextMeshProUGUI"}'
U "/create"        '{"name":"RP_Military","parent":"RoutePanel"}'
U "/add-component" '{"gameObject":"RP_Military","type":"TextMeshProUGUI"}'
U "/create"        '{"name":"RP_InfluenceWarn","parent":"RoutePanel"}'
U "/add-component" '{"gameObject":"RP_InfluenceWarn","type":"TextMeshProUGUI"}'
U "/create"        '{"name":"RP_OpenBtn","parent":"RoutePanel"}'
U "/add-component" '{"gameObject":"RP_OpenBtn","type":"Button"}'
U "/add-component" '{"gameObject":"RP_OpenBtn","type":"Image"}'
U "/create"        '{"name":"RP_CloseBtn","parent":"RoutePanel"}'
U "/add-component" '{"gameObject":"RP_CloseBtn","type":"Button"}'
U "/add-component" '{"gameObject":"RP_CloseBtn","type":"Image"}'
U "/set-active"    '{"gameObject":"RoutePanel","active":"false"}'

Write-Host "`n=== FACTION STATUS UI ===" -ForegroundColor Cyan
U "/create"        '{"name":"FactionStatusUI","parent":"Canvas"}'
U "/add-component" '{"gameObject":"FactionStatusUI","type":"FactionStatusUI"}'
U "/add-component" '{"gameObject":"FactionStatusUI","type":"Image"}'
U "/create"        '{"name":"FS_List","parent":"FactionStatusUI"}'
U "/create"        '{"name":"FS_ToggleBtn","parent":"Canvas"}'
U "/add-component" '{"gameObject":"FS_ToggleBtn","type":"Button"}'
U "/add-component" '{"gameObject":"FS_ToggleBtn","type":"Image"}'
U "/set-active"    '{"gameObject":"FactionStatusUI","active":"false"}'

Write-Host "`n=== FACTION OFFER UI ===" -ForegroundColor Cyan
U "/create"        '{"name":"FactionOfferUI","parent":"Canvas"}'
U "/add-component" '{"gameObject":"FactionOfferUI","type":"FactionOfferUI"}'
U "/add-component" '{"gameObject":"FactionOfferUI","type":"Image"}'
U "/create"        '{"name":"FO_Title","parent":"FactionOfferUI"}'
U "/add-component" '{"gameObject":"FO_Title","type":"TextMeshProUGUI"}'
U "/create"        '{"name":"FO_OfferContainer","parent":"FactionOfferUI"}'
U "/create"        '{"name":"FO_CloseBtn","parent":"FactionOfferUI"}'
U "/add-component" '{"gameObject":"FO_CloseBtn","type":"Button"}'
U "/add-component" '{"gameObject":"FO_CloseBtn","type":"Image"}'
U "/set-active"    '{"gameObject":"FactionOfferUI","active":"false"}'

Write-Host "`n=== DIALOGUE UI ===" -ForegroundColor Cyan
U "/create"        '{"name":"DialogueUI","parent":"Canvas"}'
U "/add-component" '{"gameObject":"DialogueUI","type":"DialogueUI"}'
U "/add-component" '{"gameObject":"DialogueUI","type":"Image"}'
U "/create"        '{"name":"DU_Portrait","parent":"DialogueUI"}'
U "/add-component" '{"gameObject":"DU_Portrait","type":"Image"}'
U "/create"        '{"name":"DU_CharName","parent":"DialogueUI"}'
U "/add-component" '{"gameObject":"DU_CharName","type":"TextMeshProUGUI"}'
U "/create"        '{"name":"DU_StageBadge","parent":"DialogueUI"}'
U "/add-component" '{"gameObject":"DU_StageBadge","type":"TextMeshProUGUI"}'
U "/create"        '{"name":"DU_BondBar","parent":"DialogueUI"}'
U "/add-component" '{"gameObject":"DU_BondBar","type":"Slider"}'
U "/create"        '{"name":"DU_BondLabel","parent":"DialogueUI"}'
U "/add-component" '{"gameObject":"DU_BondLabel","type":"TextMeshProUGUI"}'
U "/create"        '{"name":"DU_HistoryScroll","parent":"DialogueUI"}'
U "/add-component" '{"gameObject":"DU_HistoryScroll","type":"ScrollRect"}'
U "/add-component" '{"gameObject":"DU_HistoryScroll","type":"Image"}'
U "/create"        '{"name":"DU_HistoryContainer","parent":"DU_HistoryScroll"}'
U "/add-component" '{"gameObject":"DU_HistoryContainer","type":"VerticalLayoutGroup"}'
U "/add-component" '{"gameObject":"DU_HistoryContainer","type":"ContentSizeFitter"}'
U "/create"        '{"name":"DU_InputField","parent":"DialogueUI"}'
U "/add-component" '{"gameObject":"DU_InputField","type":"TMP_InputField"}'
U "/create"        '{"name":"DU_SendBtn","parent":"DialogueUI"}'
U "/add-component" '{"gameObject":"DU_SendBtn","type":"Button"}'
U "/add-component" '{"gameObject":"DU_SendBtn","type":"Image"}'
U "/create"        '{"name":"DU_CloseBtn","parent":"DialogueUI"}'
U "/add-component" '{"gameObject":"DU_CloseBtn","type":"Button"}'
U "/add-component" '{"gameObject":"DU_CloseBtn","type":"Image"}'
U "/create"        '{"name":"DU_Thinking","parent":"DialogueUI"}'
U "/set-active"    '{"gameObject":"DU_Thinking","active":"false"}'
U "/set-active"    '{"gameObject":"DialogueUI","active":"false"}'

Write-Host "`n=== WIRING REFERENCES ===" -ForegroundColor Yellow

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

Write-Host "`n=== SAVING SCENE ===" -ForegroundColor Cyan
U "/save-scene" '{}'

Write-Host "`n=== DONE ===" -ForegroundColor Green
Write-Host "Scene built. Check Unity hierarchy - all GameObjects, components and references are wired."
