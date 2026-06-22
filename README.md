# AetherThrone

A space strategy game with AI-powered character interactions using Unity and local LLM implementation.

## Features

- Dynamic galaxy exploration with star systems and trade routes
- AI-powered character interactions with relationship mechanics
- Local LLM implementation for offline gameplay
- Faction management and warfare systems
- Persistent memory and relationship tracking

## Prerequisites

- Unity 2022.3 LTS or Unity 6+
- Python 3.8+
- TextMeshPro package (will be installed automatically)

## Quick Setup with Local LLM

### 1. Install Required Packages

Run the package installer:

```bash
./install-packages.ps1
```

### 2. Set Up Local LLM

Run the local LLM setup script:

```bash
./setup-local-llm.ps1
```

### 3. Start Local Backend

Start the local backend server:

```bash
./run-local-backend.ps1
```

### 4. Build and Run

In Unity Editor:

```powershell
./build-scene-full.ps1
./layout-scene.ps1
./create-prefabs.ps1
./wire-all.ps1
```

Then press Play in Unity.

## Local vs Cloud Implementation

The project includes two implementations:

### Local LLM (Default)
- No API key required
- Works offline
- Uses simulated responses based on character traits
- Good for development and casual play

### Cloud Implementation (Optional)
- Requires Anthropic API key
- Higher quality responses
- Requires internet connection
- More sophisticated relationship modeling

## Configuration

### Local Backend
The local backend runs on `http://127.0.0.1:8000` by default.

### Environment Variables
If using cloud implementation, copy `Backend/.env.example` to `Backend/.env` and add your Anthropic API key.

## Project Structure

- `Assets/` - Unity assets and scripts
- `Backend/` - Python backend services
- `Assets/Scripts/AI/` - AI and LLM integration
- `Assets/Scripts/Core/` - Core game systems
- `Assets/Scripts/UI/` - User interface components
- `Assets/Scripts/Galaxy/` - Galaxy map and systems

## Customization

You can customize:

- Character personalities and backstories
- Game systems and mechanics
- UI layout and appearance
- Local LLM behavior and responses

## Contributing

See the contributing guidelines for more information on how to contribute to the project.

## License

This project is licensed under the MIT License - see the LICENSE file for details.