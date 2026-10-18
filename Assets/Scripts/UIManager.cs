using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ReverseTicTacToe
{
    /// <summary>
    /// Handles all UI transitions, turn indicators, score panels, and game-over modals.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject gameplayPanel;
        [SerializeField] private GameObject howToPlayPanel;
        [SerializeField] private GameObject gameOverPanel;

        [Header("Turn Indicator")]
        [SerializeField] private TextMeshProUGUI turnText;
        [SerializeField] private Image turnIndicatorBadge;

        [Header("Score Panel")]
        [SerializeField] private TextMeshProUGUI scoreXText;
        [SerializeField] private TextMeshProUGUI scoreOText;
        [SerializeField] private TextMeshProUGUI scoreDrawsText;

        [Header("Game Over Display")]
        [SerializeField] private TextMeshProUGUI gameOverTitleText;
        [SerializeField] private TextMeshProUGUI gameOverSubtitleText;
        [SerializeField] private Image gameOverHeaderBadge;

        [Header("Grid Layout")]
        [SerializeField] private RectTransform gridContainer;
        [SerializeField] private GameObject cellPrefab;

        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button howToPlayOpenButton;
        [SerializeField] private Button howToPlayCloseButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button gameOverMenuButton;
        [SerializeField] private Button soundButton;
        [SerializeField] private TextMeshProUGUI soundToggleText;

        // Events
        public event Action OnPlayClicked;
        public event Action OnRestartClicked;
        public event Action OnMenuClicked;

        private Coroutine turnPulseCoroutine;

        private void Awake()
        {
            if (playButton != null) playButton.onClick.AddListener(OnBtnPlay);
            if (howToPlayOpenButton != null) howToPlayOpenButton.onClick.AddListener(OnBtnHowToPlayOpen);
            if (howToPlayCloseButton != null) howToPlayCloseButton.onClick.AddListener(OnBtnHowToPlayClose);
            if (menuButton != null) menuButton.onClick.AddListener(OnBtnMenu);
            if (restartButton != null) restartButton.onClick.AddListener(OnBtnRestart);
            if (playAgainButton != null) playAgainButton.onClick.AddListener(OnBtnRestart);
            if (gameOverMenuButton != null) gameOverMenuButton.onClick.AddListener(OnBtnMenu);
            if (soundButton != null) soundButton.onClick.AddListener(OnBtnSoundToggle);
        }

        private void Start()
        {
            UpdateSoundToggleUI();
        }

        public void ShowMainMenu()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
            if (gameplayPanel != null) gameplayPanel.SetActive(false);
            if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
        }

        public void ShowGameplay()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (gameplayPanel != null) gameplayPanel.SetActive(true);
            if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
        }

        public void ToggleHowToPlay(bool show)
        {
            if (howToPlayPanel != null)
            {
                howToPlayPanel.SetActive(show);
            }
        }

        public void UpdateTurn(PlayerType player, Color playerColor)
        {
            string playerSymbol = player == PlayerType.X ? "X" : "O";
            if (turnText != null)
            {
                turnText.text = $"PLAYER {playerSymbol}'S TURN";
                //turnText.color = playerColor;
            }

            if (turnIndicatorBadge != null)
            {
                turnIndicatorBadge.color = playerColor;
            }

            if (turnPulseCoroutine != null) StopCoroutine(turnPulseCoroutine);
            turnPulseCoroutine = StartCoroutine(PulseTurnBadge());
        }

        private IEnumerator PulseTurnBadge()
        {
            if (turnIndicatorBadge == null) yield break;
            Transform t = turnIndicatorBadge.transform;
            t.localScale = Vector3.one * 1.3f;
            float duration = 0.2f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                t.localScale = Vector3.Lerp(Vector3.one * 1.3f, Vector3.one, elapsed / duration);
                yield return null;
            }
            t.localScale = Vector3.one;
        }

        public void UpdateScores(int xWins, int oWins, int draws)
        {
            if (scoreXText != null) scoreXText.text = xWins.ToString();
            if (scoreOText != null) scoreOText.text = oWins.ToString();
            if (scoreDrawsText != null) scoreDrawsText.text = draws.ToString();
        }

        public void ShowGameOver(string title, string subtitle, Color bannerColor)
        {
            if (gameOverTitleText != null)
            {
                gameOverTitleText.text = title;
                gameOverTitleText.color = bannerColor;
            }

            if (gameOverSubtitleText != null)
            {
                gameOverSubtitleText.text = subtitle;
            }

            if (gameOverHeaderBadge != null)
            {
                gameOverHeaderBadge.color = bannerColor;
            }

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }
        }

        public void HideGameOver()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
        }

        public void ToggleSound()
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.ToggleMute();
                UpdateSoundToggleUI();
            }
        }

        private void UpdateSoundToggleUI()
        {
            if (soundToggleText != null && SoundManager.Instance != null)
            {
                soundToggleText.text = SoundManager.Instance.IsMuted ? "SOUND: OFF" : "SOUND: ON";
            }
        }

        public RectTransform GridContainer => gridContainer;
        public GameObject CellPrefab => cellPrefab;

        // Button Callbacks hooked from Inspector or via code
        public void OnBtnPlay()
        {
            SoundManager.Instance?.PlayClick();
            OnPlayClicked?.Invoke();
        }

        public void OnBtnHowToPlayOpen()
        {
            SoundManager.Instance?.PlayClick();
            ToggleHowToPlay(true);
        }

        public void OnBtnHowToPlayClose()
        {
            SoundManager.Instance?.PlayClick();
            ToggleHowToPlay(false);
        }

        public void OnBtnRestart()
        {
            SoundManager.Instance?.PlayClick();
            OnRestartClicked?.Invoke();
        }

        public void OnBtnMenu()
        {
            SoundManager.Instance?.PlayClick();
            OnMenuClicked?.Invoke();
        }

        public void OnBtnSoundToggle()
        {
            ToggleSound();
        }

        // Configuration helper for Editor setup script
        public void ConfigureReferences(
            GameObject mainPanel, GameObject gamePanel, GameObject howToPanel, GameObject overPanel,
            TextMeshProUGUI turnTxt, Image turnBadge,
            TextMeshProUGUI xTxt, TextMeshProUGUI oTxt, TextMeshProUGUI drawTxt,
            TextMeshProUGUI overTitle, TextMeshProUGUI overSub, Image overBadge,
            RectTransform gridCont, GameObject prefab,
            TextMeshProUGUI soundTxt,
            Button btnPlay = null, Button btnHtpOpen = null, Button btnHtpClose = null,
            Button btnMenuRef = null, Button btnRestartRef = null,
            Button btnPlayAgainRef = null, Button btnGoMenuRef = null, Button btnSoundRef = null)
        {
            mainMenuPanel = mainPanel;
            gameplayPanel = gamePanel;
            howToPlayPanel = howToPanel;
            gameOverPanel = overPanel;
            turnText = turnTxt;
            turnIndicatorBadge = turnBadge;
            scoreXText = xTxt;
            scoreOText = oTxt;
            scoreDrawsText = drawTxt;
            gameOverTitleText = overTitle;
            gameOverSubtitleText = overSub;
            gameOverHeaderBadge = overBadge;
            gridContainer = gridCont;
            cellPrefab = prefab;
            soundToggleText = soundTxt;

            playButton = btnPlay;
            howToPlayOpenButton = btnHtpOpen;
            howToPlayCloseButton = btnHtpClose;
            menuButton = btnMenuRef;
            restartButton = btnRestartRef;
            playAgainButton = btnPlayAgainRef;
            gameOverMenuButton = btnGoMenuRef;
            soundButton = btnSoundRef;
        }
    }
}
