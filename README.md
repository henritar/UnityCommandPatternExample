# Dungeon Command - Command Pattern Implementation

Dungeon Command is a 3D puzzle game where players navigate through tile-based dungeons, collect items, and solve puzzles by manipulating the board. The game implements the Command Pattern to handle user input, movement, and board manipulation.

## Table of Contents
- [Game Overview](#game-overview)
- [Command Pattern Implementation](#command-pattern-implementation)
  - [Command Interfaces](#command-interfaces)
  - [Concrete Commands](#concrete-commands)
  - [Command Invoker - PlayerController](#command-invoker---playercontroller)
  - [Command Receivers](#command-receivers)
- [User Controls](#user-controls)
- [Game Flow](#game-flow)
- [Technical Implementation Details](#technical-implementation-details)
  - [Command History and Undo](#command-history-and-undo)
  - [Board Rotation Logic](#board-rotation-logic)
- [Benefits of the Command Pattern](#benefits-of-the-command-pattern)

## Game Overview
- **Core Mechanic**: Navigate through a 3D puzzle board by moving the player character and rotating the board.
- **Goal**: Collect all items on the level while avoiding barriers and solving spatial puzzles.
- **Unique Feature**: The ability to rotate the entire game board, changing the layout and creating new paths.

## Command Pattern Implementation

### Command Interfaces

#### `ICommand` Interface
```csharp
public interface ICommand
{
    bool Execute();
    void Undo();
}
```
The basic interface all commands implement, providing:
- `Execute()`: Performs the command action and returns a boolean indicating success.
- `Undo()`: Reverses the effects of the command.

#### `IAsyncCommand` Interface
```csharp
public interface IAsyncCommand : ICommand
{
    bool IsCompleted { get; }
}
```
An extension of `ICommand` for operations that don't complete immediately:
- `IsCompleted`: Property that tracks when an asynchronous command has finished execution.

### Concrete Commands

#### `MoveCommand`
Handles player movement in four cardinal directions:
- **Purpose**: Moves the player character in a specified direction.
- **Key Components**:
  - Tracks previous position for undo functionality.
  - Validates moves against barriers and board boundaries.
  - Activates tiles when stepped on.
  - Collects items when the player moves to their position.
- **Usage**: Invoked when WASD keys are pressed.

#### `RotateBoardCommand`
Rotates the entire game board clockwise or counter-clockwise:
- **Purpose**: Changes the puzzle layout by rotating all tiles and barriers.
- **Key Components**:
  - Implements `IAsyncCommand` to track rotation completion.
  - Uses coroutines for smooth animation.
  - Prevents multiple rotations simultaneously.
  - Auto-undoes if rotation results in an invalid state.
  - Recalculates barrier positions after rotation.
- **Usage**: Invoked when Q (counter-clockwise) or E (clockwise) keys are pressed.

### Command Invoker - `PlayerController`

The `PlayerController` serves as the invoker in the Command Pattern:
- **Purpose**: Captures user input and executes appropriate commands.
- **Key Components**:
  - Maintains a stack of executed commands (`commandHistory`).
  - Handles user input in the `Update()` loop.
  - Provides undo functionality (Z key).
  - Special handling for both synchronous and asynchronous commands.
  - Manages player animation and visual feedback.
- **Implementation**:
  ```csharp
  public void ExecuteCommand(ICommand command)
  {
      if (command.Execute())
      {
          commandHistory.Push(command);
      }
      CollectableManager.Instance.CheckLevelComplete();
  }
  ```

### Command Receivers

#### `TileBehavior`
Represents individual tiles on the game board:
- **Purpose**: Manages tile state (active/inactive) and barriers.
- **Key Components**:
  - Tracks barriers in four directions (North, South, East, West).
  - Manages visual appearance based on active state.
  - Provides activation/deactivation methods.

#### `LevelManager`
Manages the game state and board layout:
- **Purpose**: Controls level generation, board rotation state, and game progression.
- **Key Components**:
  - Generates the tile grid and barriers.
  - Handles board state during rotation.
  - Manages collectables placement.
  - Tracks barrier positions and updates them when the board rotates.
  - Provides methods for level progression and restart.

## User Controls
- **Movement**: WASD keys
- **Board Rotation**: Q (counter-clockwise), E (clockwise)
- **Undo Last Action**: Z key
- **Reset Level**: R key

## Game Flow
1. Player navigates the tile-based board using WASD.
2. Tiles become active when the player steps on them.
3. Barriers block movement in specific directions.
4. Board rotation (Q/E) changes the layout, opening new paths.
5. Collect all items to complete the level.
6. Undo (Z) allows players to reverse mistakes.
7. Reset (R) starts the level from the beginning.

## Technical Implementation Details

### Command History and Undo
- Commands are stored in a `Stack<ICommand>` for LIFO undo operations.
- Asynchronous commands (like board rotation) are handled differently during undo:
  ```csharp
  private IEnumerator ProcessUndoMove()
  {
      isUndoing = true;
      ICommand lastCommand = commandHistory.Pop();
      if (lastCommand is IAsyncCommand asyncCommand)
      {
          asyncCommand.Undo();
          while (!asyncCommand.IsCompleted)
          {
              yield return null;
          }
      }
      else
      {
          lastCommand.Undo();
      }
      isUndoing = false;
  }
  ```

### Board Rotation Logic
When the board rotates:
1. Tile positions are recalculated based on rotation direction.
2. Barrier orientations are converted (horizontal to vertical and vice versa).
3. Player position is adjusted to the nearest tile center.
4. Barrier states are updated to reflect the new orientation.

## Benefits of the Command Pattern

1. **Separation of Concerns**
   - Input handling is decoupled from action execution.
   - Game state changes are encapsulated within commands.
   - UI and visual feedback can be managed independently.

2. **Flexible Undo/Redo System**
   - Command history is easily maintained.
   - Each command knows how to undo itself.
   - Complex operations can be reversed cleanly.

3. **Support for Asynchronous Operations**
   - The `IAsyncCommand` interface allows for animated commands.
   - Commands can take time to complete (like board rotation).
   - Proper state tracking during animations.

4. **Extensibility**
   - New commands can be added without modifying existing code.
   - Command sequences can be created (macro commands).
   - Special actions can be implemented as separate commands.

5. **Testability**
   - Commands can be unit tested in isolation.
   - Game logic is separated from Unity-specific implementation.
   - Input simulation is straightforward.

