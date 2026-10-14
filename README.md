# DON'T MAKE THREE! — Reverse Tic-Tac-Toe 5×5

A complete, polished, and responsive 2D reverse Tic-Tac-Toe game built in **Unity 6 (6000.3.26f1)** with zero external asset dependencies.

---

## 1. Game Concept & Twist

**"DON'T MAKE THREE!"** is a reverse Tic-Tac-Toe game played on a 5×5 grid.

Normal Tic-Tac-Toe rewards making a line of symbols. In **DON'T MAKE THREE!**, the objective is inverted:

> **The player who creates three or more consecutive identical symbols in a horizontal row, vertical column, or diagonal LOSES the game immediately! The other player wins.**

Players must place their marks with precision, maneuvering to survive while forcing their opponent into an unavoidable trap where every available cell completes a losing line!

---

## 2. Game Rules

1. **Board Size**: 25 cells arranged in a 5×5 grid.
2. **Players**: Two players locally on the same device.
   - **Player 1**: Cyan Symbol **X**
   - **Player 2**: Coral Pink Symbol **O**
3. **Turn Order**: Players take turns selecting an empty cell to place their symbol.
4. **Losing Condition**:
   - A player immediately **loses** if their placed symbol completes **3 or more** consecutive identical symbols of their own in any direction:
     - **Horizontal** (left and right)
     - **Vertical** (up and down)
     - **Main Diagonal** (top-left to bottom-right)
     - **Anti-Diagonal** (top-right to bottom-left)
   - Lines of 4 or 5 consecutive identical symbols also trigger an immediate loss.
   - If a move creates multiple losing lines simultaneously (e.g. crossing horizontally and vertically), it still counts as one loss and highlights all affected cells.
5. **Draw Condition**:
   - If all 25 cells are filled without either player creating a losing line of 3+, the game declares a **Draw**.
6. **Move Validation**:
   - Occupied cells cannot be selected or overwritten.

---

## 3. Project Architecture & Scripts

All scripts are located in [`Assets/Scripts`](file:///d:/MyUnity/TicTicToes/Assets/Scripts):

| Script | Path | Purpose |
| :--- | :--- | :--- |
| **`PlayerType.cs`** | [`Assets/Scripts/PlayerType.cs`](file:///d:/MyUnity/TicTicToes/Assets/Scripts/PlayerType.cs) | Enum representing `None`, `X`, and `O`. |
| **`GameManager.cs`** | [`Assets/Scripts/GameManager.cs`](file:///d:/MyUnity/TicTicToes/Assets/Scripts/GameManager.cs) | Central manager controlling the 5×5 board state, turns, move validation, optimized 4-axis line-detection around the latest placed move, session scores, and restarts. |
| **`GridCell.cs`** | [`Assets/Scripts/GridCell.cs`](file:///d:/MyUnity/TicTicToes/Assets/Scripts/GridCell.cs) | Controls individual cell button interactions, symbol display, pop-in bounce animation, and pulsating alert highlight on losing lines. |
| **`UIManager.cs`** | [`Assets/Scripts/UIManager.cs`](file:///d:/MyUnity/TicTicToes/Assets/Scripts/UIManager.cs) | Manages UI screens (Main Menu, Gameplay HUD, How To Play modal, Game Over popup), turn indicators with color accents, sound toggle, and live scores. |
| **`GridContainerScaler.cs`** | [`Assets/Scripts/GridContainerScaler.cs`](file:///d:/MyUnity/TicTicToes/Assets/Scripts/GridContainerScaler.cs) | Dynamically calculates and resizes 5×5 grid cells to keep them perfectly square and responsive across desktop and mobile screens. |
| **`SoundManager.cs`** | [`Assets/Scripts/SoundManager.cs`](file:///d:/MyUnity/TicTicToes/Assets/Scripts/SoundManager.cs) | Procedural audio synthesizer that generates clean chimes, pop sounds, victory fanfares, and defeat tones at runtime with zero external audio assets. |
| **`GameSceneBuilder.cs`** | [`Assets/Scripts/Editor/GameSceneBuilder.cs`](file:///d:/MyUnity/TicTicToes/Assets/Scripts/Editor/GameSceneBuilder.cs) | Editor tool (`Tools > Build Reverse TicTacToe Scene`) that automatically generates UI sprites, cell prefabs, Canvas hierarchy, and wires up all scene dependencies. |
| **`ReverseTicTacToeLogicTests.cs`**| [`Assets/Scripts/Tests/ReverseTicTacToeLogicTests.cs`](file:///d:/MyUnity/TicTicToes/Assets/Scripts/Tests/ReverseTicTacToeLogicTests.cs) | 13 comprehensive NUnit automated test cases validating all rules and edge cases. |

---

## 4. Line Detection Algorithm

The line detection in [`GameManager.CheckLosingMove`](file:///d:/MyUnity/TicTicToes/Assets/Scripts/GameManager.cs#L227-L290) is optimized to inspect only the latest placed symbol at coordinate `(row, col)` across 4 directional vectors:

- Horizontal: `(0, 1)`
- Vertical: `(1, 0)`
- Main Diagonal: `(1, 1)`
- Anti-Diagonal: `(1, -1)`

For each axis, it searches forward and backward for consecutive matching symbols:
$$\text{Total Length} = 1 + \text{Forward Count} + \text{Backward Count}$$

If $\text{Total Length} \ge 3$, all coordinates in the continuous run are flagged as losing cells and visually highlighted with a pulsating glow.

---

## 5. Automated Test Suite (13/13 Passed)

The project includes unit tests located at [`Assets/Scripts/Tests/ReverseTicTacToeLogicTests.cs`](file:///d:/MyUnity/TicTicToes/Assets/Scripts/Tests/ReverseTicTacToeLogicTests.cs). All 13 tests execute and pass via Unity Test Runner:

1. `Test_HorizontalLoss_ThreeInARow` — Verifies horizontal 3-in-a-row causes loss.
2. `Test_HorizontalLoss_PlacedInMiddle` — Verifies placing in the middle of 2 existing marks detects 3-in-a-row.
3. `Test_VerticalLoss_ThreeInAColumn` — Verifies vertical 3-in-a-column causes loss.
4. `Test_MainDiagonalLoss_TopLeftToBottomRight` — Verifies main diagonal `\` causes loss.
5. `Test_AntiDiagonalLoss_TopRightToBottomLeft` — Verifies anti-diagonal `/` causes loss.
6. `Test_FourInARowLoss` — Verifies 4 consecutive symbols triggers loss and collects all 4 cells.
7. `Test_FiveInARowLoss` — Verifies 5 consecutive symbols triggers loss and collects all 5 cells.
8. `Test_TwoConsecutive_DoesNotTriggerLoss` — Verifies 2 consecutive symbols does not cause loss.
9. `Test_Intersection_MultipleLosingLinesTriggered` — Verifies intersecting horizontal and vertical lines are both captured.
10. `Test_OpponentSymbolBlocksLine` — Verifies intervening opponent symbols prevent line completion.
11. `Test_InvalidCellSelection_DoesNotOverwrite` — Verifies occupied cells reject subsequent clicks.
12. `Test_RestartBehavior_ClearsAllCells` — Verifies board reset resets all 25 cells to `None` while preserving scores.
13. `Test_DrawDetection_Valid25MovesWithNoThreeInARow` — Verifies a full 25-cell board with no lines of 3 triggers a Draw.

---

## 6. How to Open and Play in Unity

1. Open **Unity Hub**.
2. Click **Add > Add project from disk** and select `D:\MyUnity\TicTicToes`.
3. Open the project using **Unity 6 (6000.3.26f1)** or any compatible Unity 6 LTS release.
4. In the Project window, navigate to [`Assets/Scenes/SampleScene.unity`](file:///d:/MyUnity/TicTicToes/Assets/Scenes/SampleScene.unity) and double-click to open it.
5. Press the **Play (▶)** button in the Unity toolbar.
6. Click **PLAY GAME** to start!
   - Player 1 (Cyan X) and Player 2 (Pink O) take turns clicking cells.
   - If a player creates 3 in a row, the losing line pulses red and the other player wins!
   - Click **PLAY AGAIN** to immediately start a new round with preserved session scores.
   - Click **HOW TO PLAY** anytime to view the rules.
   - Toggle procedural audio on/off via the **SOUND** button.
