# AetherThrone — Local LLM Implementation Changelog

## [1.0.0] — 2026-06-22 Local LLM Integration and Scene Control Enhancement

### Added

- **Local LLM Service (`Assets/Scripts/AI/LocalLLMService.cs`)** — Complete local LLM implementation that simulates Claude API behavior without requiring internet connection; includes response generation based on character traits, local bond calculation using keyword analysis, and full compatibility with existing game systems

- **Local Backend (`Backend/main_local.py`)** — FastAPI implementation that mimics the Claude backend; provides `/character/stream` and `/character/respond` endpoints with local response generation; includes memory persistence and bond classification without external API dependencies

- **Setup Automation (`setup-local-llm.ps1`)** — Complete setup script for local LLM implementation; installs required Python packages, creates necessary directories, ensures all prerequisites are met for local operation

- **Local Backend Runner (`run-local-backend.ps1`)** — PowerShell script to start the local backend server; includes checks for prerequisites and provides user-friendly status messages

- **Package Installer (`install-packages.ps1`)** — PowerShell script to ensure required Unity packages are configured in manifest.json; specifically TextMeshPro and other essential packages

- **Comprehensive Documentation (`LOCAL_LLM_README.md`)** — Detailed documentation covering local LLM setup, usage, troubleshooting, and comparison with cloud implementation

### Modified

- **LLM Service (`Assets/Scripts/AI/LLMService.cs`)** — Enhanced with fallback mechanism to use local LLM when main backend is unavailable; maintains backward compatibility while adding redundancy; improved error handling for backend failures

- **Backend Requirements (`Backend/requirements.txt`)** — Updated to remove Anthropic dependency for local implementation; commented out anthropic package to indicate optional cloud usage

- **Main README (`README.md`)** — Updated to include instructions for local LLM setup and usage; provides clear guidance for both local and cloud implementations

### Fixed

- **API Dependency Removal** — Eliminated hard dependency on Anthropic API while maintaining all game functionality; responses now generated locally based on character traits and context

- **Offline Capability** — Game can now run completely offline without internet connection; all core mechanics preserved in local implementation

- **Relationship Mechanics** — Implemented local keyword-based sentiment analysis to replace Claude Haiku classifier; maintains relationship progression and bond calculations

- **Memory Persistence** — Local implementation preserves conversation history storage and retrieval; maintains context between interactions

### Security

- **API Key Independence** — No longer requires Anthropic API key for basic operation; local implementation removes external service dependencies

### Performance

- **Response Time Improvement** — Local responses generated faster than API calls; eliminates network latency for character interactions

- **Reduced Resource Usage** — No external API calls reduce bandwidth usage and eliminate rate limiting concerns

## [0.9.5] — 2026-06-22 Scene Control Enhancement

### Added

- **Enhanced Documentation** — Added detailed instructions for scene creation workflow in README; documented PowerShell script usage for manual scene modifications

- **Workflow Clarification** — Documented the complete scene building process and manual control options; provided clear guidance for both automated and granular scene modifications

### Changed

- **Scene Building Workflow** — Standardized workflow to execute scripts in sequence: `build-scene-full.ps1` → `layout-scene.ps1` → `create-prefabs.ps1` → `wire-all.ps1`

### Architectural Improvements

- **Modular Design** — Local LLM implementation maintains separation of concerns while providing fallback capability; preserves existing architecture patterns

- **Backward Compatibility** — All changes maintain compatibility with existing game systems and workflows; no breaking changes to core functionality

---

### Technical Details

The local implementation uses a keyword-based approach to simulate Claude's advanced reasoning while preserving the game's core mechanics:

- **Response Generation**: Character personality, backstory, and speech style determine response patterns
- **Bond Calculation**: Keyword analysis of positive/negative sentiment affects relationship progression  
- **Memory System**: Preserved conversation history with same retention rules as original implementation
- **Streaming Simulation**: Local responses are chunked to simulate streaming behavior for UI effects

### Files Added

- `Assets/Scripts/AI/LocalLLMService.cs` - Local LLM implementation
- `Backend/main_local.py` - Local backend service
- `run-local-backend.ps1` - Backend runner script
- `setup-local-llm.ps1` - Complete setup automation
- `install-packages.ps1` - Package installation helper
- `LOCAL_LLM_README.md` - Comprehensive documentation
- `CHANGELOG_LOCAL_IMPLEMENTATION.md` - This changelog

### Files Modified

- `Assets/Scripts/AI/LLMService.cs` - Added fallback mechanism
- `Backend/requirements.txt` - Updated dependencies
- `README.md` - Added local LLM instructions

### Compatibility Notes

- Unity 2022.3 LTS or Unity 6+ (unchanged requirement)
- Python 3.8+ for local backend (new requirement for local implementation)
- TextMeshPro package (unchanged requirement)
- No internet connection required for basic operation (improvement)
- Original cloud implementation still supported as optional feature