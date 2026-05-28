# Project POO - Story-Driven Game Implementation

## Overview
This project is a Windows Forms-based narrative game engine that dynamically loads and executes story branches, manages player decisions, and tracks character progression through properties and ideas.

## Architecture

### Core Components

#### 1. **Story Definition System** (`JSONManager/`)
- **StoryJsonDefinition**: Root story container with title, starting block, global properties, days, and ideas
- **BlockJsonDefinition**: Story segments containing text and decision branching points
- **DecisionJsonDefinition**: Player choices with conditions, effects, and target blocks
- **EffectJsonDefinition**: Game state modifications (SET/ADD operations on properties)
- **ConditionAST**: Complex condition evaluation for dynamic content and locked decisions

#### 2. **Game State Management** (`GameState.cs`)
- Tracks current block position in the story
- Maintains all player properties/stats (Status objects)
- Applies decision effects (SET or ADD operations)
- Handles story progression and condition evaluation

#### 3. **Research & Ideas System** (`IdeeaJsonDefinition.cs`)
- **IdeaJsonDefinition**: Represents researched concepts with multiple levels
- **ResearchLevelJsonDefinition**: Tracks progression through research tiers
  - Each level has description, progress cost, and innovation rewards
  - Allows gradual research progression with meaningful rewards

#### 4. **UI Layer** (`Form1.cs`)
- Dynamic text animation for story narration
- Real-time property display (player stats)
- Interactive decision buttons with conditional visibility
- Tooltip system for tooltips on decisions

## Story Flow & Functionality

### How the Story Works

1. **Initialization**