using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ReverseTicTacToe.Tests
{
    [TestFixture]
    public class ReverseTicTacToeLogicTests
    {
        private GameObject gameManagerObject;
        private GameManager gameManager;

        [SetUp]
        public void Setup()
        {
            gameManagerObject = new GameObject("TestGameManager");
            gameManager = gameManagerObject.AddComponent<GameManager>();
        }

        [TearDown]
        public void Teardown()
        {
            if (gameManagerObject != null)
            {
                Object.DestroyImmediate(gameManagerObject);
            }
        }

        [Test]
        public void Test_HorizontalLoss_ThreeInARow()
        {
            // Setup two X's at (2, 1) and (2, 2)
            gameManager.SetCellStateForTesting(2, 1, PlayerType.X);
            gameManager.SetCellStateForTesting(2, 2, PlayerType.X);

            // Place 3rd X at (2, 3)
            bool isLoss = gameManager.CheckLosingMove(2, 3, PlayerType.X, out List<Vector2Int> losingCells);

            Assert.IsTrue(isLoss, "Completing a horizontal row of 3 should cause a loss.");
            Assert.AreEqual(3, losingCells.Count, "Should detect exactly 3 cells in the losing line.");
            CollectionAssert.Contains(losingCells, new Vector2Int(2, 1));
            CollectionAssert.Contains(losingCells, new Vector2Int(2, 2));
            CollectionAssert.Contains(losingCells, new Vector2Int(2, 3));
        }

        [Test]
        public void Test_HorizontalLoss_PlacedInMiddle()
        {
            // Setup X at (1, 0) and (1, 2)
            gameManager.SetCellStateForTesting(1, 0, PlayerType.X);
            gameManager.SetCellStateForTesting(1, 2, PlayerType.X);

            // Place in the middle at (1, 1)
            bool isLoss = gameManager.CheckLosingMove(1, 1, PlayerType.X, out List<Vector2Int> losingCells);

            Assert.IsTrue(isLoss, "Placing in the middle of two identical symbols must detect 3 in a row.");
            Assert.AreEqual(3, losingCells.Count);
            CollectionAssert.Contains(losingCells, new Vector2Int(1, 0));
            CollectionAssert.Contains(losingCells, new Vector2Int(1, 1));
            CollectionAssert.Contains(losingCells, new Vector2Int(1, 2));
        }

        [Test]
        public void Test_VerticalLoss_ThreeInAColumn()
        {
            // Setup O at (0, 4) and (1, 4)
            gameManager.SetCellStateForTesting(0, 4, PlayerType.O);
            gameManager.SetCellStateForTesting(1, 4, PlayerType.O);

            // Place 3rd O at (2, 4)
            bool isLoss = gameManager.CheckLosingMove(2, 4, PlayerType.O, out List<Vector2Int> losingCells);

            Assert.IsTrue(isLoss, "Completing a vertical column of 3 should cause a loss.");
            Assert.AreEqual(3, losingCells.Count);
            CollectionAssert.Contains(losingCells, new Vector2Int(0, 4));
            CollectionAssert.Contains(losingCells, new Vector2Int(1, 4));
            CollectionAssert.Contains(losingCells, new Vector2Int(2, 4));
        }

        [Test]
        public void Test_MainDiagonalLoss_TopLeftToBottomRight()
        {
            // Setup X at (1, 1) and (2, 2)
            gameManager.SetCellStateForTesting(1, 1, PlayerType.X);
            gameManager.SetCellStateForTesting(2, 2, PlayerType.X);

            // Place 3rd X at (3, 3)
            bool isLoss = gameManager.CheckLosingMove(3, 3, PlayerType.X, out List<Vector2Int> losingCells);

            Assert.IsTrue(isLoss, "Completing a main diagonal of 3 should cause a loss.");
            Assert.AreEqual(3, losingCells.Count);
            CollectionAssert.Contains(losingCells, new Vector2Int(1, 1));
            CollectionAssert.Contains(losingCells, new Vector2Int(2, 2));
            CollectionAssert.Contains(losingCells, new Vector2Int(3, 3));
        }

        [Test]
        public void Test_AntiDiagonalLoss_TopRightToBottomLeft()
        {
            // Setup O at (1, 3) and (2, 2)
            gameManager.SetCellStateForTesting(1, 3, PlayerType.O);
            gameManager.SetCellStateForTesting(2, 2, PlayerType.O);

            // Place 3rd O at (3, 1)
            bool isLoss = gameManager.CheckLosingMove(3, 1, PlayerType.O, out List<Vector2Int> losingCells);

            Assert.IsTrue(isLoss, "Completing an anti-diagonal of 3 should cause a loss.");
            Assert.AreEqual(3, losingCells.Count);
            CollectionAssert.Contains(losingCells, new Vector2Int(1, 3));
            CollectionAssert.Contains(losingCells, new Vector2Int(2, 2));
            CollectionAssert.Contains(losingCells, new Vector2Int(3, 1));
        }

        [Test]
        public void Test_FourInARowLoss()
        {
            // Setup three X's at (0, 0), (0, 1), (0, 3)
            gameManager.SetCellStateForTesting(0, 0, PlayerType.X);
            gameManager.SetCellStateForTesting(0, 1, PlayerType.X);
            gameManager.SetCellStateForTesting(0, 3, PlayerType.X);

            // Place X at (0, 2) connecting all 4
            bool isLoss = gameManager.CheckLosingMove(0, 2, PlayerType.X, out List<Vector2Int> losingCells);

            Assert.IsTrue(isLoss, "Four in a row must trigger a loss.");
            Assert.AreEqual(4, losingCells.Count, "All 4 consecutive cells should be highlighted.");
            CollectionAssert.Contains(losingCells, new Vector2Int(0, 0));
            CollectionAssert.Contains(losingCells, new Vector2Int(0, 1));
            CollectionAssert.Contains(losingCells, new Vector2Int(0, 2));
            CollectionAssert.Contains(losingCells, new Vector2Int(0, 3));
        }

        [Test]
        public void Test_TwoConsecutive_DoesNotTriggerLoss()
        {
            gameManager.SetCellStateForTesting(4, 0, PlayerType.X);

            bool isLoss = gameManager.CheckLosingMove(4, 1, PlayerType.X, out List<Vector2Int> losingCells);

            Assert.IsFalse(isLoss, "Two consecutive symbols should NOT trigger a loss.");
            Assert.AreEqual(0, losingCells.Count);
        }

        [Test]
        public void Test_Intersection_MultipleLosingLinesTriggered()
        {
            // Horizontal row pieces
            gameManager.SetCellStateForTesting(2, 0, PlayerType.O);
            gameManager.SetCellStateForTesting(2, 1, PlayerType.O);

            // Vertical col pieces
            gameManager.SetCellStateForTesting(0, 2, PlayerType.O);
            gameManager.SetCellStateForTesting(1, 2, PlayerType.O);

            // Place O at (2, 2), completing BOTH horizontal (2,0)-(2,1)-(2,2) AND vertical (0,2)-(1,2)-(2,2)
            bool isLoss = gameManager.CheckLosingMove(2, 2, PlayerType.O, out List<Vector2Int> losingCells);

            Assert.IsTrue(isLoss, "Intersecting lines move must trigger a loss.");
            // Total unique cells: (2,0), (2,1), (0,2), (1,2), (2,2) = 5 cells
            Assert.AreEqual(5, losingCells.Count, "All cells from both losing lines should be included without duplicates.");
            CollectionAssert.Contains(losingCells, new Vector2Int(2, 0));
            CollectionAssert.Contains(losingCells, new Vector2Int(2, 1));
            CollectionAssert.Contains(losingCells, new Vector2Int(0, 2));
            CollectionAssert.Contains(losingCells, new Vector2Int(1, 2));
            CollectionAssert.Contains(losingCells, new Vector2Int(2, 2));
        }

        [Test]
        public void Test_OpponentSymbolBlocksLine()
        {
            // X O X
            gameManager.SetCellStateForTesting(3, 0, PlayerType.X);
            gameManager.SetCellStateForTesting(3, 1, PlayerType.O); // Intervening opponent

            bool isLoss = gameManager.CheckLosingMove(3, 2, PlayerType.X, out List<Vector2Int> losingCells);

            Assert.IsFalse(isLoss, "Opponent symbol between cells should block 3-in-a-row.");
            Assert.AreEqual(0, losingCells.Count);
        }

        [Test]
        public void Test_FiveInARowLoss()
        {
            // Set 4 X's in column 0
            gameManager.SetCellStateForTesting(0, 0, PlayerType.X);
            gameManager.SetCellStateForTesting(1, 0, PlayerType.X);
            gameManager.SetCellStateForTesting(2, 0, PlayerType.X);
            gameManager.SetCellStateForTesting(3, 0, PlayerType.X);

            // 5th X at (4, 0)
            bool isLoss = gameManager.CheckLosingMove(4, 0, PlayerType.X, out List<Vector2Int> losingCells);

            Assert.IsTrue(isLoss, "Five in a row must trigger a loss.");
            Assert.AreEqual(5, losingCells.Count, "All 5 cells in the line must be collected.");
        }

        [Test]
        public void Test_InvalidCellSelection_DoesNotOverwrite()
        {
            gameManager.SetCellStateForTesting(1, 1, PlayerType.X);

            // Attempting to click an already occupied cell
            gameManager.OnCellClicked(1, 1);

            // State should still be X, not modified
            Assert.AreEqual(PlayerType.X, gameManager.GetCellState(1, 1), "Occupied cell must remain unchanged.");
        }

        [Test]
        public void Test_RestartBehavior_ClearsAllCells()
        {
            // Place several symbols
            gameManager.SetCellStateForTesting(0, 0, PlayerType.X);
            gameManager.SetCellStateForTesting(2, 2, PlayerType.O);
            gameManager.SetCellStateForTesting(4, 4, PlayerType.X);

            // Trigger restart
            gameManager.RestartCurrentRound();

            // All cells must be empty
            for (int r = 0; r < GameManager.BOARD_SIZE; r++)
            {
                for (int c = 0; c < GameManager.BOARD_SIZE; c++)
                {
                    Assert.AreEqual(PlayerType.None, gameManager.GetCellState(r, c), $"Cell ({r}, {c}) must be None after restart.");
                }
            }
        }

        [Test]
        public void Test_DrawDetection_Valid25MovesWithNoThreeInARow()
        {
            // A mathematically valid 5x5 pattern with 13 X's and 12 O's where no player has 3 in a row in any row, column, or diagonal!
            // Pattern (balanced latin-like square layout):
            // Row 0: X  X  O  O  X
            // Row 1: O  O  X  X  O
            // Row 2: X  X  O  O  X
            // Row 3: O  O  X  X  O
            // Row 4: X  X  O  O  X
            // Let's verify this pattern:
            // Rows:
            // Row 0: XXOOX -> no XXX, no OOO.
            // Row 1: OOXXO -> no OOO, no XXX.
            // Cols:
            // Col 0: XOXOX -> no 3 in a row
            // Col 1: XOXOX -> no 3 in a row
            // Col 2: OXOXO -> no 3 in a row
            // Col 3: OXOXO -> no 3 in a row
            // Col 4: XOXOX -> no 3 in a row
            // Diagonals:
            // Main diag: (0,0)=X, (1,1)=O, (2,2)=O, (3,3)=X, (4,4)=X -> XOOXX (no 3 in a row)
            // Anti diag: (0,4)=X, (1,3)=X, (2,2)=O, (3,1)=O, (4,0)=X -> XXOOX (no 3 in a row)
            // All other sub-diagonals have max 4 or fewer and alternate!

            PlayerType[,] drawBoard = new PlayerType[5, 5]
            {
                { PlayerType.X, PlayerType.X, PlayerType.O, PlayerType.O, PlayerType.X },
                { PlayerType.O, PlayerType.O, PlayerType.X, PlayerType.X, PlayerType.O },
                { PlayerType.X, PlayerType.X, PlayerType.O, PlayerType.O, PlayerType.X },
                { PlayerType.O, PlayerType.O, PlayerType.X, PlayerType.X, PlayerType.O },
                { PlayerType.X, PlayerType.X, PlayerType.O, PlayerType.O, PlayerType.X }
            };

            for (int r = 0; r < 5; r++)
            {
                for (int c = 0; c < 5; c++)
                {
                    gameManager.SetCellStateForTesting(r, c, drawBoard[r, c]);
                }
            }

            // Verify no cell in this board causes a losing move
            for (int r = 0; r < 5; r++)
            {
                for (int c = 0; c < 5; c++)
                {
                    PlayerType p = drawBoard[r, c];
                    bool causedLoss = gameManager.CheckLosingMove(r, c, p, out List<Vector2Int> losingCells);
                    Assert.IsFalse(causedLoss, $"Cell ({r}, {c}) for player {p} should not trigger a loss in a draw board.");
                }
            }
        }

        [Test]
        public void Test_GridCell_SetsXSprite_ForPlayerX()
        {
            GameObject cellObj = new GameObject("TestCell");
            GridCell cell = cellObj.AddComponent<GridCell>();

            GameObject imgObj = new GameObject("TestSymbolImage");
            imgObj.transform.SetParent(cellObj.transform);
            UnityEngine.UI.Image img = imgObj.AddComponent<UnityEngine.UI.Image>();

            Sprite fakeX = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), Vector2.zero);
            Sprite fakeO = Sprite.Create(Texture2D.blackTexture, new Rect(0, 0, 4, 4), Vector2.zero);

            cell.ConfigureComponents(null, null, img, null, null, fakeX, fakeO);
            cell.SetSymbol(PlayerType.X, Color.white, animate: false);

            Assert.AreEqual(fakeX, cell.SymbolImage.sprite, "Player X must display the X sprite.");
            Assert.IsTrue(cell.SymbolImage.enabled, "SymbolImage must be enabled when a symbol is placed.");
            Assert.AreEqual(PlayerType.X, cell.CurrentSymbol);

            Object.DestroyImmediate(cellObj);
        }

        [Test]
        public void Test_GridCell_SetsOSprite_ForPlayerO()
        {
            GameObject cellObj = new GameObject("TestCell");
            GridCell cell = cellObj.AddComponent<GridCell>();

            GameObject imgObj = new GameObject("TestSymbolImage");
            imgObj.transform.SetParent(cellObj.transform);
            UnityEngine.UI.Image img = imgObj.AddComponent<UnityEngine.UI.Image>();

            Sprite fakeX = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), Vector2.zero);
            Sprite fakeO = Sprite.Create(Texture2D.blackTexture, new Rect(0, 0, 4, 4), Vector2.zero);

            cell.ConfigureComponents(null, null, img, null, null, fakeX, fakeO);
            cell.SetSymbol(PlayerType.O, Color.white, animate: false);

            Assert.AreEqual(fakeO, cell.SymbolImage.sprite, "Player O must display the O sprite.");
            Assert.IsTrue(cell.SymbolImage.enabled, "SymbolImage must be enabled when a symbol is placed.");
            Assert.AreEqual(PlayerType.O, cell.CurrentSymbol);

            Object.DestroyImmediate(cellObj);
        }

        [Test]
        public void Test_GridCell_Reset_ClearsSprite()
        {
            GameObject cellObj = new GameObject("TestCell");
            GridCell cell = cellObj.AddComponent<GridCell>();

            GameObject imgObj = new GameObject("TestSymbolImage");
            imgObj.transform.SetParent(cellObj.transform);
            UnityEngine.UI.Image img = imgObj.AddComponent<UnityEngine.UI.Image>();

            Sprite fakeX = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), Vector2.zero);
            Sprite fakeO = Sprite.Create(Texture2D.blackTexture, new Rect(0, 0, 4, 4), Vector2.zero);

            cell.ConfigureComponents(null, null, img, null, null, fakeX, fakeO);
            cell.SetSymbol(PlayerType.X, Color.white, animate: false);

            cell.ResetCell();

            Assert.IsNull(cell.SymbolImage.sprite, "Resetting cell must clear the sprite.");
            Assert.IsFalse(cell.SymbolImage.enabled, "SymbolImage must be disabled after reset.");
            Assert.AreEqual(PlayerType.None, cell.CurrentSymbol);

            Object.DestroyImmediate(cellObj);
        }
    }
}
