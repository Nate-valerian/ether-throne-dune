# AetherThrone — Changelog

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
