$BASE = "http://localhost:7777"
function U($path, $body) {
    try {
        $r = Invoke-RestMethod -Uri "$BASE$path" -Method POST -Body $body -ContentType "application/json" -ErrorAction Stop
        if ($r.ok -eq $false) { Write-Warning "  FAIL  >> $($r.error)" }
        else                   { Write-Host    "  ok    $($r.msg)" }
    } catch { Write-Warning "  ERR   >> $_" }
}

Write-Host "`n=== ADD STILL-MISSING COMPONENTS ===" -ForegroundColor Cyan
# These 3 had protocol violations in both previous passes
U "/add-component" '{"gameObject":"RP_CloseBtn","type":"Button"}'
U "/add-component" '{"gameObject":"DU_Portrait","type":"Image"}'
U "/add-component" '{"gameObject":"DU_SendBtn","type":"Button"}'

Write-Host "`n=== WIRE ALL PREVIOUSLY-FAILED REFERENCES ===" -ForegroundColor Yellow

U "/set-reference" '{"gameObject":"GalaxyUI","component":"GalaxyUI","field":"_mapContainer","target":"MapContainer","targetComponent":"RectTransform"}'
U "/set-reference" '{"gameObject":"GalaxyUI","component":"GalaxyUI","field":"_tensionBar","target":"TensionBar","targetComponent":"Slider"}'
U "/set-reference" '{"gameObject":"GalaxyUI","component":"GalaxyUI","field":"_endTurnButton","target":"EndTurnButton","targetComponent":"Button"}'

U "/set-reference" '{"gameObject":"RoutePanel","component":"RoutePanel","field":"_militaryValue","target":"RP_Military","targetComponent":"TextMeshProUGUI"}'
U "/set-reference" '{"gameObject":"RoutePanel","component":"RoutePanel","field":"_openButton","target":"RP_OpenBtn","targetComponent":"Button"}'
U "/set-reference" '{"gameObject":"RoutePanel","component":"RoutePanel","field":"_closeButton","target":"RP_CloseBtn","targetComponent":"Button"}'

U "/set-reference" '{"gameObject":"FactionStatusUI","component":"FactionStatusUI","field":"_toggleButton","target":"FS_ToggleBtn","targetComponent":"Button"}'
U "/set-reference" '{"gameObject":"FactionOfferUI","component":"FactionOfferUI","field":"_closeButton","target":"FO_CloseBtn","targetComponent":"Button"}'

U "/set-reference" '{"gameObject":"DialogueUI","component":"DialogueUI","field":"_portrait","target":"DU_Portrait","targetComponent":"Image"}'
U "/set-reference" '{"gameObject":"DialogueUI","component":"DialogueUI","field":"_bondBar","target":"DU_BondBar","targetComponent":"Slider"}'
U "/set-reference" '{"gameObject":"DialogueUI","component":"DialogueUI","field":"_sendButton","target":"DU_SendBtn","targetComponent":"Button"}'
U "/set-reference" '{"gameObject":"DialogueUI","component":"DialogueUI","field":"_closeButton","target":"DU_CloseBtn","targetComponent":"Button"}'

Write-Host "`n=== SAVE ===" -ForegroundColor Cyan
U "/save-scene" '{}'
Write-Host "`n=== DONE ===" -ForegroundColor Green
