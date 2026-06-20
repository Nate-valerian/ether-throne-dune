# AetherThrone — Scene Setup

## Unity Version

Unity 6 (or 2022 LTS+). Required packages:

- TextMeshPro (built-in)
- No other packages needed for core logic

## Canvas Settings

Add a **Canvas** to the scene root with:

- Render Mode: Screen Space — Overlay
- Add an **EventSystem** child (required for all Button / InputField clicks — Unity adds this automatically when you create a Canvas via the menu)

## Hierarchy Structure

```text
Main (Scene)
├── GameManager  [GameManager.cs]
│   ├── GalaxyMap       [GalaxyMap.cs]
│   ├── FactionManager  [FactionManager.cs]
│   ├── Navigator       [NavigatorController.cs]
│   ├── WarSystem       [WarSystem.cs]
│   └── Relationships   [RelationshipSystem.cs]
├── LLMService  [LLMService.cs]
├── Bootstrap   [GameBootstrap.cs]
└── Canvas  (Screen Space — Overlay)
    ├── EventSystem
    ├── GalaxyUI        [GalaxyUI.cs]
    │   ├── MapContainer   (RectTransform — system nodes spawn here)
    │   ├── HUD
    │   │   ├── TurnLabel         (TextMeshProUGUI)
    │   │   ├── InfluenceLabel    (TextMeshProUGUI)
    │   │   ├── SanityLabel       (TextMeshProUGUI)
    │   │   ├── TensionBar        (Slider)
    │   │   └── TensionLabel      (TextMeshProUGUI)
    │   └── EndTurnButton (Button)
    ├── SystemInfoPanel  [SystemInfoPanel.cs]  (active: false)
    ├── RoutePanel       [RoutePanel.cs]       (active: false)
    ├── FactionStatusUI  [FactionStatusUI.cs]  (active: false)
    │   └── ToggleButton  (Button — place anywhere on Canvas HUD; assign to _toggleButton)
    ├── FactionOfferUI   [FactionOfferUI.cs]   (active: false)
    │   ├── Title          (TextMeshProUGUI)
    │   ├── OfferContainer (VerticalLayoutGroup)
    │   └── CloseButton    (Button)
    └── DialogueUI       [DialogueUI.cs]       (active: false)
        ├── Portrait          (Image)
        ├── CharacterName     (TextMeshProUGUI)
        ├── StageBadge        (TextMeshProUGUI)
        ├── BondBar           (Slider)
        ├── BondLabel         (TextMeshProUGUI)
        ├── HistoryContainer  (VerticalLayoutGroup + ContentSizeFitter)
        ├── HistoryScroll     (ScrollRect — wraps HistoryContainer)
        ├── InputField        (TMP_InputField)
        ├── SendButton        (Button)
        ├── CloseButton       (Button)
        └── ThinkingIndicator (GameObject — spinner or "..." image, hidden by default)
```

## Prefabs to create

### SystemNodePrefab

- Image (circle, ~80px)
- Button component
- Child TextMeshProUGUI (system name, below circle)

### RouteLinePrefab

- Image (3px height, pivot centre, anchored centre)
- Button component (optional — enables clicking the route line)

### BubblePlayerPrefab / BubbleCharacterPrefab

- HorizontalLayoutGroup
- Image (background colour — different tint per prefab)
- Child TextMeshProUGUI (message text, word-wrap on)
- ContentSizeFitter (vertical: Preferred Size)

### CharacterButtonPrefab

- Button
- Child TextMeshProUGUI

### FactionRowPrefab

- HorizontalLayoutGroup
- 5× TextMeshProUGUI children in order: Name, Archetype, Trust, Military, Influence

### OfferRowPrefab

- HorizontalLayoutGroup
- 3× TextMeshProUGUI children in order: FactionLabel, DescLabel, RewardLabel
- 2× Button children in order: AcceptButton, DeclineButton

## Backend (LLM character engine)

```bash
cd Backend
pip install -r requirements.txt
set ANTHROPIC_API_KEY=your_key_here
uvicorn main:app --reload --port 8000
```

`LLMService` defaults to `http://localhost:8000`. Change the `_backendUrl` field in the Inspector if needed.

## Portraits

Place character portrait sprites at:

```text
Assets/Resources/Portraits/lyra.png
Assets/Resources/Portraits/vael.png
Assets/Resources/Portraits/kael.png
```

`DialogueUI` calls `Resources.Load<Sprite>("Portraits/{characterId}")` on open.
Filename must match character id exactly (lowercase). Missing portrait hides the Image automatically.

## Save / Load

- Auto-saves after every `AdvanceTurn()` call to `Application.persistentDataPath/save.json`
- Auto-loads on boot if the file exists
- Call `SaveSystem.DeleteSave()` from code (or delete the file manually) to reset to a fresh game

## Open issues

- `FactionStatusUI._toggleButton` — no fixed location mandated; place it wherever fits your HUD layout and drag it into the Inspector field

## First play test

1. Open the scene, press Play
2. Console should print:

   ```text
   [Bootstrap] AetherThrone initialised.
   [Galaxy] 5 systems, 5 routes.
   [Characters] 3 seeded: Lyra Voss, Ambassador Vael, Commander Kael
   ```

3. Click a system node → SystemInfoPanel shows
4. Click a route line → RoutePanel shows, open a route costs 10 influence
5. Click "Speak to Lyra Voss" → DialogueUI opens → type anything → LLM streams response live
