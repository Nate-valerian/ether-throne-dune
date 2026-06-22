$BASE = "http://127.0.0.1:7777"
$errors = 0

function U($path, $body) {
    Start-Sleep -Milliseconds 250
    try {
        $r = Invoke-RestMethod -Uri "$BASE$path" -Method POST -Body $body -ContentType "application/json" -TimeoutSec 20
        if ($r.ok -eq $false) { Write-Warning "FAIL: $($r.error)"; $script:errors++ }
        else { Write-Host "ok   $($r.msg)" }
    } catch { Write-Warning "ERR: $_"; $script:errors++ }
}

function MakePrefab($name, $path) {
    U "/save-as-prefab" "{`"gameObject`":`"$name`",`"path`":`"$path`"}"
    U "/delete" "{`"gameObject`":`"$name`"}"
}

function WireP($go, $comp, $field, $prefab) {
    U "/wire-prefab" "{`"gameObject`":`"$go`",`"component`":`"$comp`",`"field`":`"$field`",`"prefab`":`"$prefab`"}"
}

# ── 1. SystemNodePrefab ────────────────────────────────────────
Write-Host "`n=== SystemNodePrefab ===" -ForegroundColor Cyan
U "/create" '{"name":"_PF_SystemNode","parent":"Canvas"}'
U "/add-component" '{"gameObject":"_PF_SystemNode","type":"Image"}'
U "/add-component" '{"gameObject":"_PF_SystemNode","type":"Button"}'
U "/create" '{"name":"_PF_SN_Label","parent":"_PF_SystemNode"}'
U "/add-component" '{"gameObject":"_PF_SN_Label","type":"TextMeshProUGUI"}'
MakePrefab "_PF_SystemNode" "Assets/Prefabs/SystemNodePrefab.prefab"

# ── 2. RouteLinePrefab ────────────────────────────────────────
Write-Host "`n=== RouteLinePrefab ===" -ForegroundColor Cyan
U "/create" '{"name":"_PF_RouteLine","parent":"Canvas"}'
U "/add-component" '{"gameObject":"_PF_RouteLine","type":"Image"}'
U "/add-component" '{"gameObject":"_PF_RouteLine","type":"Button"}'
MakePrefab "_PF_RouteLine" "Assets/Prefabs/RouteLinePrefab.prefab"

# ── 3. BubblePlayerPrefab ─────────────────────────────────────
Write-Host "`n=== BubblePlayerPrefab ===" -ForegroundColor Cyan
U "/create" '{"name":"_PF_BubblePlayer","parent":"Canvas"}'
U "/add-component" '{"gameObject":"_PF_BubblePlayer","type":"Image"}'
U "/create" '{"name":"_PF_BP_Text","parent":"_PF_BubblePlayer"}'
U "/add-component" '{"gameObject":"_PF_BP_Text","type":"TextMeshProUGUI"}'
MakePrefab "_PF_BubblePlayer" "Assets/Prefabs/BubblePlayerPrefab.prefab"

# ── 4. BubbleCharacterPrefab ──────────────────────────────────
Write-Host "`n=== BubbleCharacterPrefab ===" -ForegroundColor Cyan
U "/create" '{"name":"_PF_BubbleChar","parent":"Canvas"}'
U "/add-component" '{"gameObject":"_PF_BubbleChar","type":"Image"}'
U "/create" '{"name":"_PF_BC_Text","parent":"_PF_BubbleChar"}'
U "/add-component" '{"gameObject":"_PF_BC_Text","type":"TextMeshProUGUI"}'
MakePrefab "_PF_BubbleChar" "Assets/Prefabs/BubbleCharacterPrefab.prefab"

# ── 5. CharacterButtonPrefab ──────────────────────────────────
Write-Host "`n=== CharacterButtonPrefab ===" -ForegroundColor Cyan
U "/create" '{"name":"_PF_CharBtn","parent":"Canvas"}'
U "/add-component" '{"gameObject":"_PF_CharBtn","type":"Image"}'
U "/add-component" '{"gameObject":"_PF_CharBtn","type":"Button"}'
U "/create" '{"name":"_PF_CB_Label","parent":"_PF_CharBtn"}'
U "/add-component" '{"gameObject":"_PF_CB_Label","type":"TextMeshProUGUI"}'
MakePrefab "_PF_CharBtn" "Assets/Prefabs/CharacterButtonPrefab.prefab"

# ── 6. FactionRowPrefab (5 TMP labels in a row) ───────────────
Write-Host "`n=== FactionRowPrefab ===" -ForegroundColor Cyan
U "/create" '{"name":"_PF_FactionRow","parent":"Canvas"}'
U "/add-component" '{"gameObject":"_PF_FactionRow","type":"HorizontalLayoutGroup"}'
foreach ($label in @("_PF_FR_Name","_PF_FR_Arch","_PF_FR_Trust","_PF_FR_Mil","_PF_FR_Inf")) {
    U "/create" "{`"name`":`"$label`",`"parent`":`"_PF_FactionRow`"}"
    U "/add-component" "{`"gameObject`":`"$label`",`"type`":`"TextMeshProUGUI`"}"
}
MakePrefab "_PF_FactionRow" "Assets/Prefabs/FactionRowPrefab.prefab"

# ── 7. OfferRowPrefab (3 TMP + 2 Buttons) ────────────────────
Write-Host "`n=== OfferRowPrefab ===" -ForegroundColor Cyan
U "/create" '{"name":"_PF_OfferRow","parent":"Canvas"}'
U "/add-component" '{"gameObject":"_PF_OfferRow","type":"HorizontalLayoutGroup"}'
foreach ($label in @("_PF_OR_Faction","_PF_OR_Desc","_PF_OR_Reward")) {
    U "/create" "{`"name`":`"$label`",`"parent`":`"_PF_OfferRow`"}"
    U "/add-component" "{`"gameObject`":`"$label`",`"type`":`"TextMeshProUGUI`"}"
}
foreach ($btn in @("_PF_OR_Accept","_PF_OR_Decline")) {
    U "/create" "{`"name`":`"$btn`",`"parent`":`"_PF_OfferRow`"}"
    U "/add-component" "{`"gameObject`":`"$btn`",`"type`":`"Image`"}"
    U "/add-component" "{`"gameObject`":`"$btn`",`"type`":`"Button`"}"
    U "/create" "{`"name`":`"${btn}_Lbl`",`"parent`":`"$btn`"}"
    U "/add-component" "{`"gameObject`":`"${btn}_Lbl`",`"type`":`"TextMeshProUGUI`"}"
}
MakePrefab "_PF_OfferRow" "Assets/Prefabs/OfferRowPrefab.prefab"

# ── Wire all prefab refs ──────────────────────────────────────
Write-Host "`n=== Wire Prefab References ===" -ForegroundColor Yellow
WireP "GalaxyUI"      "GalaxyUI"      "_systemNodePrefab"      "Assets/Prefabs/SystemNodePrefab.prefab"
WireP "GalaxyUI"      "GalaxyUI"      "_routeLinePrefab"       "Assets/Prefabs/RouteLinePrefab.prefab"
WireP "DialogueUI"    "DialogueUI"    "_bubblePlayerPrefab"    "Assets/Prefabs/BubblePlayerPrefab.prefab"
WireP "DialogueUI"    "DialogueUI"    "_bubbleCharacterPrefab" "Assets/Prefabs/BubbleCharacterPrefab.prefab"
WireP "SystemInfoPanel" "SystemInfoPanel" "_characterButtonPrefab" "Assets/Prefabs/CharacterButtonPrefab.prefab"
WireP "FactionStatusUI" "FactionStatusUI" "_factionRowPrefab"  "Assets/Prefabs/FactionRowPrefab.prefab"
WireP "FactionOfferUI"  "FactionOfferUI"  "_offerRowPrefab"    "Assets/Prefabs/OfferRowPrefab.prefab"

# ── Save ──────────────────────────────────────────────────────
Write-Host "`n=== Save ===" -ForegroundColor Cyan
Start-Sleep -Milliseconds 500
U "/save-scene" '{}'

$color = if ($errors -eq 0) { "Green" } else { "Red" }
Write-Host "`n=== DONE: $errors error(s) ===" -ForegroundColor $color
