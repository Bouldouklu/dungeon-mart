# DungeonMart - Claude Development Guide

## Project Overview
DungeonMart is a Unity 6.2 game project. This file contains project-specific instructions and commands for Claude Code assistance.

## Development Environment
- **Unity Version**: 6.2
- **Platform**: Windows
- **Repository**: https://github.com/Bouldouklu/DungeonMart.git

## Unity Project Structure
```
Assets/
├── Scripts/          # Game logic and components
├── Scenes/           # Unity scenes
├── Prefabs/          # Reusable game objects
├── Materials/        # Rendering materials
├── Textures/         # Image assets
└── Audio/            # Sound effects and music
```

## Asset Store Plugins (Excluded from Git)
- **vFolders**: Project organization tool
- **vHierarchy**: Hierarchy enhancement tool

## Unity-Specific Development Guidelines

### Component Design
- Use component-based architecture with small, focused components
- Link references via Unity Inspector rather than using GetComponent()
- Prefer SerializeField over public fields for inspector variables
- Use Awake() for internal initialization, Start() for external references

### Modern Unity APIs (Unity 6+)
- Use `FindFirstObjectByType<T>()` instead of deprecated `FindObjectOfType<T>()`
- Use `FindAnyObjectByType<T>()` when any instance is acceptable

### Performance Best Practices
- Avoid Update() loops where possible - use event-driven architecture
- Implement object pooling for frequently instantiated objects
- Cache component references rather than repeated searches
- Use UnityActions for simple script communication

### Code Standards
- Follow PascalCase for classes, camelCase for methods/variables
- Use string constants instead of hardcoded strings
- Implement proper error handling with logging
- Prioritize readability and maintainability

## Development Commands

### Testing
```bash
# Add testing commands here when test framework is set up
```

### Build Commands
```bash
# Add build commands here when build pipeline is configured
```

### Linting/Code Quality
```bash
# Add linting commands here if code analysis tools are added
```

## Git Workflow
- Feature branches for new functionality
- Commit frequently with descriptive messages
- Test locally before pushing
- Use conventional commit format

## Notes for Claude
- Always test changes locally before committing
- Follow Unity best practices and component-based design
- Maintain clean, readable code with proper documentation
- Use the observer pattern for decoupled architecture

## Claude's Responsibilities

### What Claude Should Do:

- Write, create, and modify C# scripts only
- Provide clear instructions for what to do in the Unity Editor (e.g., "Create a GameObject
  named 'CustomerSpawner'", "Attach this script to the Main Camera", "Link the PlayerTransform
  field in the Inspector to the Player object")
- Use read-only tools (Read, Glob, Grep, MCP functions) to inspect scenes, prefabs, and
  existing code
- Plan and architect the code structure and component relationships

### What Claude Should NOT Do:

- Do NOT create or modify Unity scene files (.unity files)
- Do NOT create or modify prefabs (.prefab files)
- Do NOT create or modify Unity asset files (materials, sprites, etc.)
- Do NOT attempt to configure Unity Editor settings through file manipulation

### Workflow:

1. Claude writes the necessary C# scripts
2. Claude provides step-by-step Unity Editor instructions for you to follow
3. You perform the Unity Editor work (creating GameObjects, assigning references,
   configuring components, etc.)
4. Claude can read scene/prefab files to verify setup or understand the current state