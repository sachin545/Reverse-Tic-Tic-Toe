using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ReverseTicTacToe
{
    /// <summary>
    /// Centralized game manager that controls the 5x5 board state, turn management,
    /// move validation, losing-line detection, scoring, and round restarts.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public const int BOARD_SIZE = 5;
        public const int MAX_MOVES = BOARD_SIZE * BOARD_SIZE; // 25

        [Header("Colors")]
        [SerializeField] private Color playerXColor = new Color(0f, 0.9f, 1f, 1f); // Vibrant Cyan
        [SerializeField] private Color playerOColor = new Color(1f, 0.25f, 0.45f, 1f); // Vibrant Coral Pink
        [SerializeField] private Color losingLineColor = new Color(1f, 0.15f, 0.25f, 1f); // Alert Red

        [Header("References")]
        [SerializeField] private UIManager uiManager;

        // Board State
        private PlayerType[,] board = new PlayerType[BOARD_SIZE, BOARD_SIZE];
        private GridCell[,] cellViews = new GridCell[BOARD_SIZE, BOARD_SIZE];

        // Turn & Game Status
        private PlayerType currentPlayer = PlayerType.X;
        private int moveCount = 0;
        private bool isGameOver = false;
        private bool isInputLocked = false;

        // Session Scores
        private int playerXWins = 0;
        private int playerOWins = 0;
        private int sessionDraws = 0;

        // Starting player alternate tracker
        private PlayerType roundStartingPlayer = PlayerType.X;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            if (uiManager == null)
            {
                uiManager = FindFirstObjectByType<UIManager>();
            }

            if (uiManager != null)
            {
                uiManager.OnPlayClicked += StartNewGameSession;
                uiManager.OnRestartClicked += RestartCurrentRound;
                uiManager.OnMenuClicked += ReturnToMainMenu;
            }

            BuildGridCells();
            uiManager?.ShowMainMenu();
            uiManager?.UpdateScores(playerXWins, playerOWins, sessionDraws);
        }

        private void OnDestroy()
        {
            if (uiManager != null)
            {
                uiManager.OnPlayClicked -= StartNewGameSession;
                uiManager.OnRestartClicked -= RestartCurrentRound;
                uiManager.OnMenuClicked -= ReturnToMainMenu;
            }
        }

        /// <summary>
        /// Dynamically instantiates the 25 grid cells at runtime under the grid container.
        /// </summary>
        private void BuildGridCells()
        {
            if (uiManager == null || uiManager.GridContainer == null || uiManager.CellPrefab == null)
            {
                Debug.LogError("[GameManager] Missing UIManager, GridContainer, or CellPrefab!");
                return;
            }

            // Clear any preexisting children
            foreach (Transform child in uiManager.GridContainer)
            {
                Destroy(child.gameObject);
            }

            for (int r = 0; r < BOARD_SIZE; r++)
            {
                for (int c = 0; c < BOARD_SIZE; c++)
                {
                    GameObject cellObj = Instantiate(uiManager.CellPrefab, uiManager.GridContainer);
                    cellObj.name = $"Cell_{r}_{c}";
                    GridCell cell = cellObj.GetComponent<GridCell>();
                    if (cell == null)
                    {
                        cell = cellObj.AddComponent<GridCell>();
                    }

                    cell.Init(r, c, OnCellClicked);
                    cellViews[r, c] = cell;
                }
            }
        }

        public void StartNewGameSession()
        {
            uiManager.ShowGameplay();
            RestartCurrentRound();
        }

        public void ReturnToMainMenu()
        {
            uiManager.ShowMainMenu();
        }

        /// <summary>
        /// Restarts the board without reloading the Unity scene, preserving session scores.
        /// </summary>
        public void RestartCurrentRound()
        {
            StopAllCoroutines();
            isGameOver = false;
            isInputLocked = false;
            moveCount = 0;

            // Reset board data
            for (int r = 0; r < BOARD_SIZE; r++)
            {
                for (int c = 0; c < BOARD_SIZE; c++)
                {
                    board[r, c] = PlayerType.None;
                    if (cellViews[r, c] != null)
                    {
                        cellViews[r, c].ResetCell();
                    }
                }
            }

            uiManager?.HideGameOver();
            currentPlayer = roundStartingPlayer;
            UpdateTurnUI();
        }

        /// <summary>
        /// Handles click on cell (row, col).
        /// </summary>
        public void OnCellClicked(int row, int col)
        {
            if (isGameOver || isInputLocked) return;

            // Validation: cell must be inside bounds and empty
            if (row < 0 || row >= BOARD_SIZE || col < 0 || col >= BOARD_SIZE) return;
            if (board[row, col] != PlayerType.None) return;

            StartCoroutine(ProcessMoveSequence(row, col));
        }

        private IEnumerator ProcessMoveSequence(int row, int col)
        {
            isInputLocked = true;

            // 1. Place symbol
            board[row, col] = currentPlayer;
            moveCount++;

            Color playerColor = GetPlayerColor(currentPlayer);
            if (cellViews[row, col] != null)
            {
                cellViews[row, col].SetSymbol(currentPlayer, playerColor, true);
            }

            SoundManager.Instance?.PlayMove(currentPlayer);

            // Small delay for placement animation feel
            yield return new WaitForSeconds(0.18f);

            // 2. Check for losing condition from the latest move
            bool causedLoss = CheckLosingMove(row, col, currentPlayer, out List<Vector2Int> losingCells);

            if (causedLoss)
            {
                // Current player made 3 or more in a row! Current player loses; opponent wins!
                isGameOver = true;
                PlayerType opponent = GetOpponent(currentPlayer);

                // Highlight losing cells
                foreach (Vector2Int cellCoord in losingCells)
                {
                    if (cellViews[cellCoord.x, cellCoord.y] != null)
                    {
                        cellViews[cellCoord.x, cellCoord.y].HighlightLosingCell(losingLineColor);
                    }
                }

                // Update session scores
                if (opponent == PlayerType.X) playerXWins++;
                else playerOWins++;
                uiManager.UpdateScores(playerXWins, playerOWins, sessionDraws);

                // Alternate starting player for next round
                roundStartingPlayer = GetOpponent(roundStartingPlayer);

                SoundManager.Instance?.PlayLoss();

                yield return new WaitForSeconds(0.45f);

                string loserName = $"Player {currentPlayer}";
                string winnerName = $"Player {opponent}";
                string title = $"{winnerName.ToUpper()} WINS!";
                string subtitle = $"{loserName} made three! {winnerName} wins!";

                uiManager.ShowGameOver(title, subtitle, GetPlayerColor(opponent));
                isInputLocked = false;
                yield break;
            }

            // 3. Check for Draw (board completely full)
            if (moveCount >= MAX_MOVES)
            {
                isGameOver = true;
                sessionDraws++;
                uiManager.UpdateScores(playerXWins, playerOWins, sessionDraws);

                roundStartingPlayer = GetOpponent(roundStartingPlayer);

                SoundManager.Instance?.PlayDraw();

                yield return new WaitForSeconds(0.35f);

                uiManager.ShowGameOver("IT'S A DRAW!", "The 5x5 board is completely full!", Color.white);
                isInputLocked = false;
                yield break;
            }

            // 4. Game continues: switch turn to opponent
            currentPlayer = GetOpponent(currentPlayer);
            UpdateTurnUI();
            isInputLocked = false;
        }

        /// <summary>
        /// Optimized line detection checking all 4 axes around the latest move (row, col).
        /// Detects any consecutive run of 3 or more identical symbols of the given player.
        /// If multiple lines are created, collects all cells involved.
        /// </summary>
        public bool CheckLosingMove(int row, int col, PlayerType player, out List<Vector2Int> losingCells)
        {
            HashSet<Vector2Int> uniqueLosingCoords = new HashSet<Vector2Int>();

            // The 4 axes: (dRow, dCol)
            // 1. Horizontal: (0, 1)
            // 2. Vertical: (1, 0)
            // 3. Main Diagonal: (1, 1)
            // 4. Anti-Diagonal: (1, -1)
            Vector2Int[] directions = new Vector2Int[]
            {
                new Vector2Int(0, 1),
                new Vector2Int(1, 0),
                new Vector2Int(1, 1),
                new Vector2Int(1, -1)
            };

            foreach (var dir in directions)
            {
                List<Vector2Int> forwardCoords = new List<Vector2Int>();
                List<Vector2Int> backwardCoords = new List<Vector2Int>();

                // Count forward
                int r = row + dir.x;
                int c = col + dir.y;
                while (IsInBounds(r, c) && board[r, c] == player)
                {
                    forwardCoords.Add(new Vector2Int(r, c));
                    r += dir.x;
                    c += dir.y;
                }

                // Count backward
                r = row - dir.x;
                c = col - dir.y;
                while (IsInBounds(r, c) && board[r, c] == player)
                {
                    backwardCoords.Add(new Vector2Int(r, c));
                    r -= dir.x;
                    c -= dir.y;
                }

                // Total consecutive line length includes the latest move itself (+1)
                int totalLength = 1 + forwardCoords.Count + backwardCoords.Count;

                // Rule: 3 or more consecutive identical symbols triggers a loss
                if (totalLength >= 3)
                {
                    uniqueLosingCoords.Add(new Vector2Int(row, col));
                    foreach (var pt in forwardCoords) uniqueLosingCoords.Add(pt);
                    foreach (var pt in backwardCoords) uniqueLosingCoords.Add(pt);
                }
            }

            losingCells = new List<Vector2Int>(uniqueLosingCoords);
            return losingCells.Count > 0;
        }

        private bool IsInBounds(int r, int c)
        {
            return r >= 0 && r < BOARD_SIZE && c >= 0 && c < BOARD_SIZE;
        }

        public static PlayerType GetOpponent(PlayerType player)
        {
            return player == PlayerType.X ? PlayerType.O : PlayerType.X;
        }

        public Color GetPlayerColor(PlayerType player)
        {
            return player == PlayerType.X ? playerXColor : playerOColor;
        }

        private void UpdateTurnUI()
        {
            if (uiManager != null)
            {
                uiManager.UpdateTurn(currentPlayer, GetPlayerColor(currentPlayer));
            }
        }

        // Configuration helper for Editor setup script
        public void ConfigureReferences(UIManager ui, Color colX, Color colO, Color colLoss)
        {
            uiManager = ui;
            playerXColor = colX;
            playerOColor = colO;
            losingLineColor = colLoss;
        }

        // Test helper
        public PlayerType GetCellState(int r, int c) => board[r, c];
        public void SetCellStateForTesting(int r, int c, PlayerType p) => board[r, c] = p;
    }
}
