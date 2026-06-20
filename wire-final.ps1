$BASE = "http://localhost:7777"
function U($path, $body = "{}") {
    try {
        $r = Invoke-RestMethod -Uri "$BASE$path" -Method POST -Body $body -ContentType "application/json" -ErrorAction Stop
        if ($r.ok -eq $false) { Write-Warning "  FAIL  >> $($r.error)" }
        else                   { Write-Host    "  ok    $($r.msg)" }
    } catch { Write-Warning "  ERR   >> $_" }
}

Write-Host "`n=== FINAL WIRING PASS (Button/Slider/Image/RectTransform) ===" -ForegroundColor Yellow

# GalaxyUI: _mapContainer (RectTransform), _tensionBar (Slider), _endTurnButton (Button)
U "/set-reference" '{"gameObject":"GalaxyUI","component":"GalaxyUI","field":"_mapContainer","target":"MapContainer","targetComponent":"RectTransform"}'
U "/set-reference" '{"gameObject":"GalaxyUI","component":"GalaxyUI","field":"_tensionBar","target":"TensionBar","targetComponent":"Slider"}'
U "/set-reference" '{"gameObject":"GalaxyUI","component":"GalaxyUI","field":"_endTurnButton","target":"EndTurnButton","targetComponent":"Button"}'

# RoutePanel: _militaryValue, _openButton, _closeButton
U "/set-reference" '{"gameObject":"RoutePanel","component":"RoutePanel","field":"_militaryValue","target":"RP_Military","targetComponent":"TextMeshProUGUI"}'
U "/set-reference" '{"gameObject":"RoutePanel","component":"RoutePanel","field":"_openButton","target":"RP_OpenBtn","targetComponent":"Button"}'
U "/set-reference" '{"gameObject":"RoutePanel","component":"RoutePanel","field":"_closeButton","target":"RP_CloseBtn","targetComponent":"Button"}'

# FactionStatusUI: _toggleButton
U "/set-reference" '{"gameObject":"FactionStatusUI","component":"FactionStatusUI","field":"_toggleButton","target":"FS_ToggleBtn","targetComponent":"Button"}'

# FactionOfferUI: _closeButton
U "/set-reference" '{"gameObject":"FactionOfferUI","component":"FactionOfferUI","field":"_closeButton","target":"FO_CloseBtn","targetComponent":"Button"}'

# DialogueUI: _portrait, _bondBar, _sendButton, _closeButton
U "/set-reference" '{"gameObject":"DialogueUI","component":"DialogueUI","field":"_portrait","target":"DU_Portrait","targetComponent":"Image"}'
U "/set-reference" '{"gameObject":"DialogueUI","component":"DialogueUI","field":"_bondBar","target":"DU_BondBar","targetComponent":"Slider"}'
U "/set-reference" '{"gameObject":"DialogueUI","component":"DialogueUI","field":"_sendButton","target":"DU_SendBtn","targetComponent":"Button"}'
U "/set-reference" '{"gameObject":"DialogueUI","component":"DialogueUI","field":"_closeButton","target":"DU_CloseBtn","targetComponent":"Button"}'

Write-Host "`n=== SAVE ===" -ForegroundColor Cyan
U "/save-scene" '{}'
Write-Host "`n=== ALL DONE ===" -ForegroundColor Green
