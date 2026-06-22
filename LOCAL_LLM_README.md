# AetherThrone - Local LLM Implementation

This document explains how to run AetherThrone with a local LLM implementation instead of the Claude API.

## Overview

The project now includes a local LLM implementation that simulates the behavior of the Claude API without requiring an internet connection or API key. The local implementation includes:

- Local response generation based on character personality and context
- Keyword-based sentiment analysis for relationship mechanics
- Conversation memory persistence
- Streaming response simulation

## Prerequisites

- Python 3.8+
- Unity 2022.3 LTS or Unity 6+

## Setup Instructions

### 1. Install Python Dependencies

Run the setup script to install required Python packages:

```bash
./setup-local-llm.ps1
```

### 2. Start the Local Backend

Start the local backend server:

```bash
./run-local-backend.ps1
```

The server will be available at `http://127.0.0.1:8000`.

### 3. Run Unity Project

1. Open the project in Unity
2. Build the scene using the PowerShell scripts:
   ```powershell
   ./build-scene-full.ps1
   ./layout-scene.ps1
   ./create-prefabs.ps1
   ./wire-all.ps1
   ```
3. Press Play in Unity

## How It Works

### Local Response Generation

Instead of calling Claude, the local implementation:

1. Analyzes the character's personality, backstory, and speech style
2. Considers the current game context and relationship status
3. Generates a response that fits the character's personality
4. Applies keyword-based sentiment analysis for relationship changes

### Relationship Mechanics

The local implementation includes a keyword-based system that:

- Analyzes the sentiment of responses based on positive/negative keywords
- Adjusts for character relationship stage (Stranger, Acquaintance, Ally, etc.)
- Factors in the current bond level between character and player

### Memory Persistence

Conversation history is stored locally in `Backend/memory/` in JSON format, maintaining context between interactions.

## Files Added

- `Backend/main_local.py` - Local LLM backend implementation
- `Assets/Scripts/AI/LocalLLMService.cs` - Unity service for local LLM
- `run-local-backend.ps1` - Script to start the local backend
- `setup-local-llm.ps1` - Setup script for local implementation
- Updated `requirements.txt` - Without Anthropic dependency

## Fallback Mechanism

The main `LLMService.cs` now includes a fallback mechanism:

1. First attempts to connect to the main backend
2. If the backend is unavailable, falls back to the local LLM service
3. Maintains the same API contract for seamless integration

## Performance Notes

- Local responses are generated faster than API calls
- No internet connection required
- Lower quality responses compared to Claude (due to limitations of simulation)
- Perfect for development and offline play

## Troubleshooting

If you encounter issues:

1. Verify Python is installed and in your PATH
2. Check that the local backend server is running
3. Confirm Unity project packages are properly imported
4. Review the Unity console for any error messages