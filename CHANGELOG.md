# AetherThrone — Changelog

## [0.8.4] — 2026-06-20 Fix: backend API key missing (root cause of all LLM errors)

### Fixed

- **All LLM calls silently failing with HTTP 500** — `ANTHROPIC_API_KEY` was not set in the backend's process environment; `AsyncAnthropic()` had no key and every `/character/stream` and `/character/respond` call failed. Root cause of all "Unknown Error", curl 18, and 0-byte stream responses.
- **Backend now loads `.env`** — added `python-dotenv`; `main.py` calls `load_dotenv()` at startup so the key is read from `Backend/.env` automatically
- Added `Backend/.env.example` with placeholder; copy to `.env` and fill in real key

## [0.8.3] — 2026-06-20 Dialogue layout + backend startup

### Fixed

- **Backend exits immediately** — `main.py` had no `uvicorn.run()` entry point; added `if __name__ == "__main__": uvicorn.run(...)` so `python main.py` starts the server correctly
- **DialogueUI Awake self-deactivates** — `Awake()` called `gameObject.SetActive(false)`; when the panel starts inactive in the scene, the first `SetActive(true)` in `OpenForIntro` triggers `Awake`, which immediately set it inactive again → `StartCoroutine` in `AddBubble` crashed. Fixed: removed `SetActive(false)` from `Awake`; scene controls initial visibility
- **ScrollRect.m_Content not assigned** — wired in code inside `DialogueUI.Awake`: `_historyScroll.content = _historyContainer as RectTransform` so it can never be missing
- **Intro overlay covers DialogueUI** — `IntroOverlay` is the last Canvas child (renders on top); `DialogueUI` was invisible behind it. Fixed: overlay now fades out after the lore text, before Beat 2, so Vael's dialogue panel shows against the galaxy background
- **HistoryContainer needs VerticalLayoutGroup** — chat bubbles were stacking at origin, narrow and overlapping. Added `VerticalLayoutGroup` (child-control width + height, force-expand width, spacing 8, padding 12/8) and `ContentSizeFitter` (vertical = preferred size) to `DU_HistoryContainer`
- **Input field invisible** — added `Image` background (dark blue-grey) to `DU_InputField`; styled `DU_SendBtn` (blue), `DU_CloseBtn` (red), `DU_Portrait` (dark placeholder)

## [0.8.0] — 2026-06-20 UI layout — all panels positioned and styled

### Fixed

- **RectTransform missing on all pre-fix GameObjects** — 44 UI elements (GalaxyUI, HUD, EndTurnButton, SystemInfoPanel, RoutePanel, FactionStatusUI, FactionOfferUI, DialogueUI and their children) were created before the auto-upgrade was in place and had only `Transform`. Added `RectTransform` to all 44 via `/add-component`.
- **Image missing on panel containers** — 13 panels and buttons lacked `Image` components needed for visible backgrounds and hit-testing. Added via `/add-component`.
- **GalaxyUI Image opacity** — `GalaxyUI` is a full-screen transparent container; its auto-added Image was set to `alpha=0` so it does not occlude the 3D camera view.

### Added

- `fix-rects.ps1` — adds `RectTransform` to all 44 UI elements and `Image` to all 13 panels/buttons in one pass (58 API calls, 0 errors)
- `layout-scene.ps1` — sets anchors, anchored position, size delta, pivot, and `Image.m_Color` for every panel using dot-path `/set-field` calls; all 130+ calls succeed
- **EndTurnButton** — added `ETB_Label` child with `TextMeshProUGUI` "End Turn" text

### Scene state after v0.8.0

- `IntroOverlay` — full-screen black (anchors 0,0→1,1; Image alpha 1, color black)
- `LoreText` — centred inside overlay (0.1,0.2 → 0.9,0.8)
- `GalaxyUI` — full-screen container (transparent)
- `MapContainer` — 70% centred (0.15,0.1 → 0.85,0.9)
- `HUD` — top bar 50px anchored to top edge
- `SystemInfoPanel` — right drawer (0.78,0.1 → 1,0.85), dark blue-grey
- `RoutePanel` — centre-right popup (0.55,0.3 → 0.78,0.72), very dark
- `FactionStatusUI` — left overlay (0 0.3 → 0.22 0.92), dark
- `FactionOfferUI` — centre popup (0.2,0.2 → 0.8,0.8), nearly opaque
- `DialogueUI` — centre panel (0.1,0.05 → 0.9,0.95), near-opaque dark

## [0.7.0] — 2026-06-20 All 7 prefabs created and wired

### Added

- `Assets/Prefabs/SystemNodePrefab.prefab` — root [Image, Button] + child `Label` [TextMeshProUGUI]
- `Assets/Prefabs/RouteLinePrefab.prefab` — root [Image, Button]
- `Assets/Prefabs/BubblePlayerPrefab.prefab` — root [Image] + child [TextMeshProUGUI]
- `Assets/Prefabs/BubbleCharacterPrefab.prefab` — root [Image] + child [TextMeshProUGUI]
- `Assets/Prefabs/CharacterButtonPrefab.prefab` — root [Image, Button] + child [TextMeshProUGUI]
- `Assets/Prefabs/FactionRowPrefab.prefab` — root [HorizontalLayoutGroup] + 5 [TextMeshProUGUI] children
- `Assets/Prefabs/OfferRowPrefab.prefab` — root [HorizontalLayoutGroup] + 3 [TextMeshProUGUI] + 2 buttons (each with child label)
- `create-prefabs.ps1` — creates all 7 prefabs via ClaudeControlServer `/save-as-prefab` endpoint; wires them to Inspector fields
- `wire-all.ps1` — wires all 50 Inspector references across 8 MonoBehaviours (GameBootstrap, IntroSequence, GalaxyUI, SystemInfoPanel, RoutePanel, FactionStatusUI, FactionOfferUI, DialogueUI)

### Fixed

- **New server endpoints** — `/save-as-prefab`, `/wire-prefab`, `/remove-component`, `/reparent`, `/delete` added to `ClaudeControlServer.cs`
- **IntroSequence null guard** — Beat 2 skips safely when `vael` character is null (prevents coroutine crash on inactive DialogueUI)
- **Input System** — `ProjectSettings.asset` set to `activeInputHandler: 2` (Both); `StandaloneInputModule` restored on EventSystem

## [0.6.0] — 2026-06-20 Scene fully built and wired

### Added

- `build-scene-full.ps1` — creates all 50+ GameObjects, adds components, wires Inspector references in one pass
- `fix-scene.ps1` — retry script for first-pass failures
- `reparent-scene.ps1` — moves misplaced children to correct panel parents

### Fixed — ClaudeControlServer.cs

- **Double-respond bug** — `DrainQueue` was calling `Respond()` AND the listener thread was calling it too → protocol violations; fixed to let only the listener thread respond
- **Domain reload port leak** — hooked `AssemblyReloadEvents.beforeAssemblyReload` so `Stop()` is called before domain teardown; port 7777 is released properly every reload
- **ThreadAbortException log spam** — now caught silently in `Listen()` as expected during reloads
- **IPv6-only binding** — added `http://127.0.0.1:{PORT}/` prefix alongside `localhost` so IPv4 requests work on systems where `localhost` resolves to `::1`
- **Inactive parent lookup** — `CreateObject` now uses `FindGO()` instead of `GameObject.Find()` so inactive panel parents can be found when creating children
- **UI RectTransform** — `CreateObject` auto-adds `RectTransform` when the parent has one
- **New endpoints** — `/reparent` and `/delete`; `/reparent` moves a GameObject to a new parent via `Undo.SetTransformParent`; `/delete` destroys via `Undo.DestroyObjectImmediate`
- `claude-unity.ps1` — base URL changed from `http://localhost:7777` to `http://127.0.0.1:7777`

### Scene state (all wired — prefabs still needed)

Full hierarchy built: GameManager + 5 children, LLMService, Bootstrap (GameBootstrap + IntroSequence), Canvas with EventSystem + GalaxyUI (MapContainer, HUD, EndTurnButton) + SystemInfoPanel (8 children) + RoutePanel (8 children) + FactionStatusUI + FactionOfferUI (3 children) + DialogueUI (10 children) + IntroOverlay (LoreText). All Inspector references wired. Remaining: SystemNodePrefab, RouteLinePrefab, BubblePlayerPrefab, BubbleCharacterPrefab, CharacterButtonPrefab, FactionRowPrefab, OfferRowPrefab.

## [0.5.0] — 2026-06-20 Intro sequence

### Added

- `Assets/Scripts/UI/IntroSequence.cs` — 3-beat opening sequence:
  - Beat 1: full-screen black overlay + lore text crawl (fade in/hold/fade out)
  - Beat 2: forced DialogueUI on Vael with scripted opening monologue; player sends one message, LLM responds, panel locks
  - Beat 3: fires `GameStartedEvent`, fades out overlay to reveal galaxy map
- `GameBootstrap` — new `introSequence` field; routes through intro on first boot; skips if save file exists; `FinishBoot()` helper keeps logic clean
- `DialogueUI.OpenForIntro()` — opens panel with scripted first line, disables close button, fires callback after one LLM exchange; `Close()` resets all intro state
- `SCENE_SETUP.md` — added `IntroOverlay` to Canvas hierarchy, setup instructions, Inspector wiring table

## [0.4.0] — 2026-06-20 Lore pass: Dune-parallel narrative

### Changed — Star Systems

- `Sol Prime` → `Aethar Prime` (Capital, seat of House Vaethyr and the Ruling Conclave)
- `Keth IV` → `Kethara` (Industrial, home of the Fold Compact)
- `Dravos` → `Dravath` (Military fortress world, Iron Covenant stronghold)
- `Yeln` → `Edenos` (Agricultural, breadbasket world — site of the war, home of The Unbound)
- `The Void Station` → `The Null` (Frontier exile station — where the Navigator was found as a child)

### Changed — Factions

- `House Athar` → `House Vaethyr` — the imperial house that found the Navigator as a child and shaped them into a tool; they believe they own the Navigator
- `The Merchant Guild` → `The Fold Compact` — trade empire that depends on Fold-Routes to survive; most generous, most desperate
- `The Free Worlds` → `The Unbound` — people displaced by wars that moved through the Navigator's routes; resist all fold-travel

### Changed — Characters

- **Lyra Voss** — FactionId updated to `the-unbound`, system updated to `the-null`; backstory rewritten: settlement commander on Edenos destroyed when Vael arranged the Aethar Prime–Dravath route opening; now an exile at The Null; the only person who speaks to the Navigator without wanting something
- **Ambassador Vael** — FactionId updated to `house-vaethyr`, system updated to `aethar-prime`; backstory rewritten: the man who found the Navigator as a child and designed their entire existence; thirty years of guilt disguised as diplomacy
- **Commander Kael** — system updated to `dravath`; backstory rewritten: eighteen years at Dravath, never lost a battle in the field, only lost campaigns when Fold-Routes were closed; tells the Navigator exactly what he wants and exactly what he will do if he doesn't get it

### Added — Lore framework

- Fold-Routes are the new canonical term for hyperspace routes in dialogue context
- Void-Dust established as the substance that enables Navigator perception (referenced in backstory; mechanic planned for v0.5.0)

## [0.3.1] — 2026-06-19 Claude permissions: full project access

### Added

- `.claude/settings.json` — allows `Bash(*)`, `PowerShell(*)`, `Read(*)`, `Write(*)`, `Edit(*)`, `Glob(*)`, `Grep(*)` without prompts for this project; approved explicitly by Nate 2026-06-19

## [0.3.0] — 2026-06-19 Claude full control: Unity Editor HTTP server

### Added

- `Assets/Editor/ClaudeControlServer.cs` — `[InitializeOnLoad]` HTTP server on `localhost:7777`; runs on a background thread, dispatches to Unity main thread via queue; endpoints: `GET /ping`, `GET /hierarchy`, `GET /logs`, `POST /create`, `POST /add-component`, `POST /set-active`, `POST /set-field`, `POST /set-reference`, `POST /create-prefab-instance`, `POST /play`, `POST /stop`, `POST /save-scene`
- `claude-unity.ps1` — PowerShell helper script; maps short command names (`ping`, `ref`, `play`, etc.) to HTTP calls so Claude Code can drive Unity from the terminal without manual clicking

## [0.2.0] — 2026-06-19 Backend: streaming + memory persistence + bond classifier

### Added

- `Backend/main.py` — `/character/stream` SSE endpoint; yields `{"type":"chunk","text":"…"}` events then a final `{"type":"done","reply":"…","bondDelta":…}` event
- `Backend/main.py` — `load_memory` / `append_memory` — per-character conversation history persisted to `Backend/memory/{id}.json`; last 80 messages kept; history prepended to every Claude call so the model has real turn-by-turn context
- `Backend/main.py` — `classify_bond_delta` — async call to `claude-haiku-4-5-20251001` to rate the emotional valence of each reply (−10→+10 JSON); replaces the keyword heuristic
- `Backend/main.py` — `DELETE /character/{id}/memory` — clears persisted history for one character
- `Backend/requirements.txt` — upgraded `uvicorn` to `uvicorn[standard]` for WebSocket/SSE support
- `Assets/Scripts/AI/LLMService.cs` — rewritten to call `/character/stream`; parses SSE line-by-line; exposes `onChunk` callback for live text display; `onDone` fires after the `done` event with full reply + bondDelta
- `Assets/Scripts/Navigator/NavigatorController.SpeakTo` — added optional `onChunk` parameter forwarded to `LLMService`
- `Assets/Scripts/UI/DialogueUI.OnSend` — creates character bubble immediately; streams chunks into it live; final `onDone` sets authoritative reply text

### Changed

- Bond delta now comes from the Haiku classifier instead of keyword heuristics — more accurate, context-aware sentiment

## [0.1.3] — 2026-06-19 Feature: character portrait loading

### Added

- `DialogueUI.LoadPortrait` — calls `Resources.Load<Sprite>("Portraits/{characterId}")` on open; hides the portrait Image if no sprite is found
- `Assets/Resources/Portraits/` — drop `lyra.png`, `vael.png`, `kael.png` here (filename must match character id)
- `SCENE_SETUP.md` — documented portrait folder convention

## [0.1.2] — 2026-06-19 Feature: save system + faction offers

### Added

- `Assets/Scripts/Core/SaveSystem.cs` — static class; `Save()` serialises full game state (GameState, routes, characters, factions) to `Application.persistentDataPath/save.json` via `JsonUtility`; `Load()` restores it and fires `SaveLoadedEvent`; `DeleteSave()` for fresh-start reset
- `GameManager.AdvanceTurn` — auto-saves after every turn end
- `GameBootstrap.Start` — auto-loads save on boot if file exists
- `EventBus` — added `SaveLoadedEvent`
- `Assets/Scripts/UI/FactionOfferUI.cs` — panel shown after turn advance when factions have pending offers; Accept pays wealth+influence and opens the route; Decline docks faction trust and adds a grievance; deceptive offers shown in orange
- `FactionManager.GenerateOffers` — called each `UpdateRelationships()`; each faction has 30% chance per turn to propose a bribe for a closed route; Military factions have 40% chance of a deceptive offer
- `SCENE_SETUP.md` — added `FactionOfferUI` to Canvas hierarchy; documented OfferRowPrefab layout; added Save/Load section

## [0.1.1] — 2026-06-19 Bug fix: bondDelta wiring

### Fixed

- `LLMService.SendRequest` now calls `RelationshipSystem.UpdateBond(character.Id, response.bondDelta)` after every LLM reply — the backend's sentiment-derived delta is applied instead of being silently discarded
- `NavigatorController.SpeakTo` — removed hardcoded `+2f` bond increment; bond is now driven entirely by the backend heuristic (or future classifier)

## [0.1.0] — 2026-06-19 Initial scaffold

### Added — Core systems

- `Assets/Scripts/Core/EventBus.cs` — static decoupled event bus; events: `GameStartedEvent`, `TurnAdvancedEvent`, `RouteOpenedEvent`, `RouteClosedEvent`, `WarDeclaredEvent`, `BattleResolvedEvent`, `CharacterMetEvent`, `RelationshipChangedEvent`
- `Assets/Scripts/Core/GameState.cs` — serialisable state bag: turn, Navigator sanity/influence, galactic tension, active wars, destroyed systems
- `Assets/Scripts/Core/GameManager.cs` — singleton; resolves child systems via `GetComponentInChildren`; exposes `AdvanceTurn()`
- `Assets/Scripts/Core/GameBootstrap.cs` — `[DefaultExecutionOrder(-100)]` wiring; fires `GameStartedEvent`; prints system summary to console

### Added — Galaxy

- `Assets/Scripts/Galaxy/StarSystem.cs` — data class; `IsIsolated` computed from `ConnectedRoutes.Count == 0`; types: Capital, Industrial, Agricultural, Military, Frontier
- `Assets/Scripts/Galaxy/Route.cs` — data class; `IsOpen`, `InfluenceCost` (default 10), `TradeValue`, `MilitaryValue`, `PrimaryBeneficiaryFactionId`
- `Assets/Scripts/Galaxy/GalaxyMap.cs` — seeds 5 systems (Sol Prime, Keth IV, Dravos, Yeln, The Void Station) and 5 routes; `OpenRoute` / `CloseRoute` publish events

### Added — Factions

- `Assets/Scripts/Factions/Faction.cs` — data class; archetypes: Military, Merchant, Political, Religious, Rebel; `FactionOffer` with `IsDeceptive` / `SecretCondition`
- `Assets/Scripts/Factions/FactionManager.cs` — seeds 4 factions; reacts to route/battle events to adjust `NavigatorTrust` and `MilitaryPower`; `UpdateRelationships()` called each turn

### Added — Characters & Relationships

- `Assets/Scripts/Characters/Character.cs` — data class; bond range −100→100; `RelationshipStage` (Stranger→Devoted); memory system (top 20 by emotional weight); memory types: SharedMoment, Betrayal, Gift, Loss, Promise, Conflict, Intimacy
- `Assets/Scripts/Characters/RelationshipSystem.cs` — seeds 3 characters (Lyra Voss, Ambassador Vael, Commander Kael); `UpdateBond`, `DecayOverTime` (−1/turn if bond > 0; sanity penalty when no connections); reacts to battles and route openings

### Added — Navigator

- `Assets/Scripts/Navigator/NavigatorController.cs` — `OpenRoute` (influence check + cost), `CloseRoute`, `SpeakTo` (calls LLMService); restores +20 influence per turn

### Added — War

- `Assets/Scripts/War/WarSystem.cs` — `DeclareWar` queues `PendingBattle`, raises galactic tension +15; `ResolvePendingBattles` called on turn end; isolation halves defender strength

### Added — AI / LLM

- `Assets/Scripts/AI/LLMService.cs` — Unity MonoBehaviour; `UnityWebRequest` POST to FastAPI `/character/respond`; builds system prompt with personality, backstory, speech style, game context, relationship guidance per stage; stores exchange in character memory
- `Backend/main.py` — FastAPI app; `POST /character/respond` → calls `claude-sonnet-4-6`; heuristic `bondDelta` estimator (keyword match); `GET /health`
- `Backend/requirements.txt` — fastapi, uvicorn, anthropic, pydantic

### Added — UI

- `Assets/Scripts/UI/GalaxyUI.cs` — builds map nodes and route lines from `GalaxyMap` data; HUD (turn, influence, sanity, tension bar); end-turn button; redraws routes on open/close events; flashes tension bar red on war
- `Assets/Scripts/UI/SystemInfoPanel.cs` — shows system stats + character "Speak to" buttons; updates on system click
- `Assets/Scripts/UI/RoutePanel.cs` — shows route stats; open/close buttons with influence check and warning label
- `Assets/Scripts/UI/DialogueUI.cs` — chat bubble history (player/character prefabs); bond bar + stage badge with colour coding; thinking indicator; `ScrollRect` auto-scroll; subscribes to `RelationshipChangedEvent`
- `Assets/Scripts/UI/FactionStatusUI.cs` — toggle panel; faction rows (Name, Archetype, Trust, Military, Influence) with trust colour coding; rebuilds on turn advance / route open / battle

### Added — Documentation

- `SCENE_SETUP.md` — full Unity hierarchy, required prefabs (SystemNodePrefab, RouteLinePrefab, BubblePlayerPrefab, BubbleCharacterPrefab, CharacterButtonPrefab, FactionRowPrefab), backend startup instructions, first play-test checklist

### Known issues / TODO

- ~~`bondDelta` returned from backend never applied~~ — fixed in 0.1.1
- ~~No save/load system~~ — fixed in 0.1.2
- ~~No faction offer UI~~ — fixed in 0.1.2
- ~~Character portraits referenced in `DialogueUI` but no sprite loading logic~~ — fixed in 0.1.3
- `FactionStatusUI._toggleButton` has no assigned location in the scene hierarchy — open issue
