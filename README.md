[Versão em Português](README.pt.md)
# Reinforcement Learning Level Generator

A powerful Unity-based procedural level generator using reinforcement learning to create dynamically difficulty-adjusted game levels. This system uses RL algorithms to train an agent that generates engaging levels tailored to player performance.

### Please, after using the asset, answer the [questionnaire](https://forms.office.com/e/cQVjxeDC6K).

**Author:** Afonso Costa
[20210584@iade.pt](mailto:20210584@iade.pt)

---

## Table of Contents

1. [Overview](#overview)
2. [Features](#features)
3. [Requirements](#requirements)
4. [Installation](#installation)
5. [Setup Guide](#setup-guide)
6. [Usage Guide](#usage-guide)
7. [Configuration](#configuration)
8. [API Reference](#api-reference)
9. [Best Practices](#best-practices)
10. [Troubleshooting](#troubleshooting)

---

## Overview

The Reinforcement Learning Level Generator is a sophisticated system designed to automatically generate game levels that adapt to player skill levels. By employing reinforcement learning principles, the generator trains an agent to create levels that maintain optimal challenge and engagement throughout gameplay.

### Key Components

- **RLGeneratorAgent.cs** - Core RL agent responsible for level generation decisions
- **RLLevelTrainer.cs** - Main controller for the training and generation pipeline
- **PlayerPerformanceTracker.cs** - Monitors player performance and adjusts difficulty
- **EnemyDifficultyAdjuster.cs** - Scales enemy parameters based on difficulty level
- **GeneratorState.cs** - Defines the state space for the RL agent
- **GeneratorAction.cs** - Defines the action space for the RL agent
- **TileType.cs** - Enumerates available tile/block types

---

## Features

✨ **Procedural Level Generation**
- Generates unique levels based on training
- Supports both Side-Scroller and Top-Down game styles

🎮 **Dynamic Difficulty Adjustment**
- Adapts level complexity based on player performance
- Tracks consecutive wins/losses and adjusts accordingly
- Configurable difficulty scaling

🎯 **Flexible Content Options**
- Optional enemies with difficulty scaling
- Platform generation with customizable heights
- Collectible items (coins, rings, etc.)
- Health pickups
- Hazards and obstacles

🤖 **Reinforcement Learning Integration**
- RL agent learns to generate engaging levels
- Configurable training episodes (1000+ recommended)
- Performance-based difficulty progression

🏗️ **Hierarchical Scene Organization**
- Automatic parent assignment for generated objects
- Clean Unity hierarchy management
- Composite collider support for optimized physics

---

## Requirements

### Software
- **Unity 2022.3 LTS** or newer
- C# 7.3 or higher
- 2D gameplay setup

### Hardware (Recommended)
- CPU: Multi-core processor for efficient training
- RAM: 4GB minimum (8GB+ recommended for larger levels)
- Storage: ~200MB for project files

---

## Installation

### Step 1: Import Files

Navigate to https://github.com/ItsYaBoiBirdMan/RL-2D-Level-Generator/releases/tag/asset_package and download RL.Level.Generator.unitypackage. This package must be imported in your Unity Project.

Ensure all required scripts are present in your project:

```
Assets/Scripts/
├── GeneratorAction.cs
├── GeneratorState.cs
├── RLGeneratorAgent.cs
├── RLLevelTrainer.cs
├── PlayerPerformanceTracker.cs
├── EnemyDifficultyAdjuster.cs
└── TileType.cs
```

### Step 2: Create Main Trainer Object

1. Create an empty GameObject in your scene (name is arbitrary)
2. Attach the `RLLevelTrainer.cs` script as a component
3. This will be your main level generation controller

---

## Setup Guide

### Phase 1: Basic Configuration

#### Step 1: Create Block Parent with Composite Collider

1. Create an empty GameObject (name is arbitrary)
2. Add the **Composite Collider 2D** component
3. Configure as shown below:
   - Material: Use your physics material (e.g., WallMaterial)
   - Geometry Type: Outlines
   - Generation Type: Synchronous
   - Vertex Distance: 0.0005
   - Offset Distance: 5e-05

#### Step 2: Set Up Parent Hierarchy

1. Create 5 empty GameObjects total:
   - 1 with Composite Collider 2D (for blocks)
   - 4 additional objects (for edges, enemies, coins, hazards)

2. Assign them in the RLLevelTrainer inspector under **Object Parents**:
   - **Block Parent** → Composite Collider object
   - **Edge Parent** → Empty GameObject
   - **Enemy Parent** → Empty GameObject
   - **Coin Parent** → Empty GameObject
   - **Hazard Parent** → Empty GameObject

#### Step 3: Configure Prefabs

1. In RLLevelTrainer, locate the **Prefabs** section
2. Assign your game prefabs:

| Field | Requirement | Notes |
|-------|-------------|-------|
| Ground Block | Box Collider 2D + "Used By Composite" ✓ | Recommended size: 1×1 |
| Platform Block | Box Collider 2D + "Used By Composite" ✓ | Same size as ground |
| Left Edge Trigger | Trigger Collider | Prevents enemies falling left |
| Right Edge Trigger | Trigger Collider | Prevents enemies falling right |
| Side Scrolling Enemy | Optional | Will scale with difficulty |
| Top Down Enemy | Optional | Will scale with difficulty |
| Coin Prefab | Optional | Collectible items |
| Pickup Prefab | Optional | Health/power-ups |
| Hazard Prefab | Optional | Obstacles/damage zones |
| Death Plane | Trigger Collider | Detects player falling |

**Important:** Ground and Platform blocks MUST have Box Collider 2D with "Used By Composite" enabled.

#### Step 4: Configure Basic Settings

In the RLLevelTrainer inspector, set **Basic Settings**:

```
Block Size: 1 (matching your prefab size)
Level Width: See recommended values below
Level Height: See recommended values below
Training Episodes: Minimum 1000
Difficulty: 1 (starting difficulty)
```

#### Step 5: Set Player Object

1. Place your Player GameObject in the scene
2. Assign it in RLLevelTrainer under **Player Object** field
3. Player position will be reset when level generation completes

#### Step 6: Configure Enemy Difficulty Scaling (Optional)

If your game includes enemies:

1. Add `EnemyDifficultyAdjuster.cs` to enemy prefabs
2. Public variables available:
   - `MaxHealth` - Enemy health scaled by difficulty
   - `MovementSpeed` - Enemy speed scaled by difficulty

#### Step 7: Set Up Performance Tracking

1. Create a new empty GameObject
2. Attach `PlayerPerformanceTracker.cs`
3. Assign it in RLLevelTrainer under **Player Performance Tracker**
4. Configure the Difficulty Increment value (default: 0.1)

---

### Phase 2: Generation Settings Configuration

#### Step 1: Choose Generation Mode

In RLLevelTrainer **Generation Settings**, select one:
- **Side Scroller** - Traditional horizontal scrolling levels
- **Top Down** - Overhead/bird's-eye view levels

#### Step 2: Apply Recommended Parameters

**For Side Scroller Levels:**
```
Level Width: 100-200
Level Height: 50-75
Training Episodes: ≥1000
```

**For Top Down Levels:**
```
Level Width: 20-40
Level Height: 20-40
Training Episodes: 1000-5000 (avoid exceeding 5000)
```

#### Step 3: Fine-Tune Generation Options

Configure the following flags in **Generation Settings**:

| Option | Purpose | Impact |
|--------|---------|--------|
| Have Enemies | Enable/disable enemy generation | Affects difficulty progression |
| Have Platforms | Enable/disable platforms | Changes level complexity |
| Have Pickups | Enable/disable health items | Affects survival difficulty |
| Have Hazards | Enable/disable obstacle placement | Increases challenge |
| Have Coins | Enable/disable collectibles | Affects exploration incentive |
| Have Enemy Scaling | Scale enemies with difficulty | Keeps challenge balanced |
| Platform Difficulty Relation | Platform density vs difficulty | "More on Higher" or vice versa |
| Min/Max Platform Height | Platform range from ground | Align with jump mechanics |

---

## Usage Guide

### Basic Level Generation

#### Step 1: Configure Generation Mode

```
Select Generation Mode: Side Scroller or Top Down
```

#### Step 2: Adjust Parameters

Modify **Basic Settings** based on your chosen mode (see Section 2.2 above).

#### Step 3: Customize Generation Features

Toggle the desired generation options in **Generation Settings**:

```csharp
// Example configuration for challenging platformer
Have Enemies: true
Have Platforms: true
Platform Difficulty Relation: "More On Higher Difficulty"
Min Platform Height: 4
Max Platform Height: 6
Have Coins: true
Have Pickups: false
Have Hazards: true
Have Enemy Scaling: true
```

### Adaptive Difficulty System

#### Integrating Performance Tracking

1. In your level completion/failure detection code, call:

```csharp
PlayerPerformanceTracker tracker = GetComponent<PlayerPerformanceTracker>();
tracker.UpdatePerformance(playerWon);
```

2. The system will:
   - Track consecutive wins/losses
   - Automatically adjust difficulty
   - Apply `AdjustDifficulty()` internally

#### Customizing Difficulty Scaling

Modify the `Difficulty Increment` field in PlayerPerformanceTracker:
- **Larger values** (0.2+) = More aggressive scaling
- **Smaller values** (0.05) = Gradual difficulty progression

### Querying Level Information

#### Get Enemy Count

```csharp
RLLevelTrainer trainer = GetComponent<RLLevelTrainer>();
int enemyCount = trainer.GetEnemyCount();
```

#### Get Collectible Count

```csharp
int coinCount = trainer.GetCoinCount();
```

### Regenerating Levels

#### Method 1: Scene Reload

```csharp
SceneManager.LoadScene(SceneManager.GetActiveScene().name);
```

#### Method 2: Flexible Generation

```csharp
RLLevelTrainer trainer = GetComponent<RLLevelTrainer>();
trainer.FlexibleGeneration();
```

---

## Configuration

### Parameter Reference

#### Basic Settings

| Parameter | Range | Default | Notes |
|-----------|-------|---------|-------|
| Level Width | 20-500 | 150 | Larger = longer generation time |
| Level Height | 20-200 | 50 | Must accommodate platforms |
| Training Episodes | 1000+ | 1000 | More episodes = better training |
| Block Size | 0.5-2 | 1 | Should match prefab size |
| Difficulty | 1-10 | 1 | Starting difficulty multiplier |

#### Generation Settings

| Parameter | Options | Impact |
|-----------|---------|--------|
| Generation Mode | Side Scroller, Top Down | Level layout style |
| Have Enemies | true/false | Adds combat challenge |
| Have Platforms | true/false | Adds navigation complexity |
| Have Pickups | true/false | Provides survival resources |
| Have Hazards | true/false | Adds environmental threats |
| Have Coins | true/false | Adds collectible incentive |
| Have Enemy Scaling | true/false | Scales difficulty with enemies |
| Platform Difficulty Relation | More/Less on Higher | Inverse difficulty mapping |
| Min Platform Height | 1-10 | Minimum platform placement |
| Max Platform Height | Min+1 to 20 | Maximum platform placement |

#### Enemy Difficulty Scaling

```csharp
// In EnemyDifficultyAdjuster.cs
public float MaxHealth = 100f;      // Scales with difficulty
public float MovementSpeed = 5f;    // Scales with difficulty
```

---

## API Reference

### RLLevelTrainer

#### Public Methods

```csharp
// Generate a new level using current settings
public void FlexibleGeneration()

// Get the number of enemies spawned in current level
public int GetEnemyCount()

// Get the number of coins/collectibles in current level
public int GetCoinCount()
```

#### Key Properties

```csharp
// Current difficulty value
public float Difficulty { get; set; }

// Generation mode (Side Scroller / Top Down)
public GenerationMode GenerationMode { get; set; }

// Trainer state
public bool IsTraining { get; private set; }
```

### PlayerPerformanceTracker

#### Public Methods

```csharp
// Update performance based on level outcome (true = win, false = loss)
public void UpdatePerformance(bool playerWon)

// Used to adjust difficulty when level is completed or failed
public void AdjustDifficulty()

```

#### Public Properties

```csharp
// Difficulty change per win/loss
public float DifficultyIncrement = 0.1f;

// Current difficulty multiplier
public float Difficulty { get; private set; }
```

### EnemyDifficultyAdjuster

#### Public Properties

```csharp
// Enemy maximum health (scales with difficulty)
public float MaxHealth = 100f;

// Enemy movement speed (scales with difficulty)
public float MovementSpeed = 5f;
```

## Best Practices

### Level Design

✅ **DO:**
- Start with conservative difficulty (1.0-2.0)
- Test with diverse player skill levels
- Ensure platforms align with player mechanics
- Provide enough space between hazards
- Balance difficulty scaling gradually

❌ **DON'T:**
- Use extremely large dimensions initially (causes long training times)
- Set training episodes too low (<1000)
- Mix incompatible generation modes
- Place impossible platform chains
- Ignore player feedback on difficulty

### Performance Optimization

1. **For faster generation:**
   - Reduce Level Width/Height
   - Lower Training Episodes (minimum 1000)
   - Disable unused features (enemies, hazards, etc.)

2. **For better quality:**
   - Increase Training Episodes (2000-5000)
   - Expand Level Dimensions
   - Enable all relevant features

3. **Memory Management:**
   - Monitor scene instantiation
   - Use object pooling for collectibles
   - Destroy previous levels before generating new ones

### Testing

1. **Sanity Checks:**
   - Verify all prefabs are assigned
   - Confirm parent objects exist
   - Test with single feature enabled first

2. **Difficulty Progression:**
   - Play through multiple levels
   - Verify difficulty scaling is felt
   - Adjust DifficultyIncrement if needed

3. **Content Verification:**
   - Use `GetEnemyCount()` and `GetCoinCount()`
   - Verify expected content appears
   - Check for generation errors

---

## Troubleshooting

### Common Issues

#### ❌ Levels take too long to generate

**Solution:**
- Reduce Level Width and Level Height
- Lower Training Episodes to 1000
- Switch from Top Down to Side Scroller
- Disable unnecessary features

#### ❌ Generated levels are unplayable

**Solution:**
- Adjust Min/Max Platform Heights
- Verify player mechanics align with level design
- Reduce difficulty setting
- Increase Training Episodes for better learning

#### ❌ Colliders not working properly

**Solution:**
- Verify Block Prefabs have Box Collider 2D
- Confirm "Used By Composite" is enabled
- Check that Block Parent has Composite Collider 2D
- Ensure colliders aren't set as triggers

#### ❌ Player spawning in wrong position

**Solution:**
- Ensure Player Object is assigned in inspector
- Check that player prefab is valid
- Verify player collider isn't overlapping with level geometry

#### ❌ Enemies not spawning

**Solution:**
- Confirm "Have Enemies" is enabled
- Verify enemy prefabs are assigned
- Check Enemy Parent exists
- Add EnemyDifficultyAdjuster to enemy prefabs

#### ❌ Difficulty not adjusting

**Solution:**
- Verify PlayerPerformanceTracker is assigned
- Confirm UpdatePerformance() is being called
- Check DifficultyIncrement value is > 0
- Ensure levels are long enough to track performance

### Debug Tips

```csharp
// Log generation information
Debug.Log($"Enemies: {trainer.GetEnemyCount()}, Coins: {trainer.GetCoinCount()}");

// Monitor difficulty progression
Debug.Log($"Current Difficulty: {tracker.CurrentDifficulty}");
Debug.Log($"Wins: {tracker.GetConsecutiveWins()}, Losses: {tracker.GetConsecutiveLosses()}");

// Track training progress
Debug.Log($"Training: {trainer.IsTraining}");
```

### Getting Support

If issues persist:
1. Review this guide's Setup Guide section carefully
2. Verify all required files are present
3. Check Unity console for specific error messages
4. Ensure Unity version compatibility (2020.3 LTS+)

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 05/2026 | Initial release |

---

## License

Copyright [2026] [Afonso Costa]

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

---

**Created by:** Afonso Costa
[20210584@iade.pt](mailto:20210584@iade.pt)

For questions or suggestions, please contact the project maintainer.
