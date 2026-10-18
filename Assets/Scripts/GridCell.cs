using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ReverseTicTacToe
{
    /// <summary>
    /// Represents an individual cell on the 5x5 game board.
    /// Manages interaction, symbol sprite display, and visual animations.
    /// </summary>
    public class GridCell : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button cellButton;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image symbolImage;
        [SerializeField] private TextMeshProUGUI symbolText;
        [SerializeField] private Image highlightOverlay;

        [Header("Symbol Sprites")]
        [SerializeField] private Sprite xSprite;
        [SerializeField] private Sprite oSprite;

        [Header("Colors")]
        [SerializeField] private Color defaultCellColor = new Color(0.12f, 0.16f, 0.23f, 1f); // Dark slate
        [SerializeField] private Color occupiedCellColor = new Color(0.08f, 0.11f, 0.16f, 1f);
        [SerializeField] private Color hoverColor = new Color(0.18f, 0.24f, 0.35f, 1f);

        public int Row { get; private set; }
        public int Col { get; private set; }
        public PlayerType CurrentSymbol { get; private set; } = PlayerType.None;
        public bool IsOccupied => CurrentSymbol != PlayerType.None;

        public Image SymbolImage => symbolImage;
        public Sprite XSprite => xSprite;
        public Sprite OSprite => oSprite;

        private System.Action<int, int> onCellClickedCallback;
        private Coroutine animCoroutine;
        private Coroutine pulseCoroutine;

        public void Init(int row, int col, System.Action<int, int> onClickCallback, Sprite xSp = null, Sprite oSp = null)
        {
            Row = row;
            Col = col;
            onCellClickedCallback = onClickCallback;

            if (xSp != null) xSprite = xSp;
            if (oSp != null) oSprite = oSp;

            EnsureSpritesLoaded();

            if (cellButton != null)
            {
                cellButton.onClick.RemoveAllListeners();
                cellButton.onClick.AddListener(HandleClick);
            }

            ResetCell();
        }

        public void SetSprites(Sprite x, Sprite o)
        {
            if (x != null) xSprite = x;
            if (o != null) oSprite = o;
        }

        private void EnsureSpritesLoaded()
        {
            if (xSprite == null)
            {
                xSprite = Resources.Load<Sprite>("Sprites/X_Sprite");
                if (xSprite == null) xSprite = Resources.Load<Sprite>("X_Sprite");
            }

            if (oSprite == null)
            {
                oSprite = Resources.Load<Sprite>("Sprites/O_Sprite");
                if (oSprite == null) oSprite = Resources.Load<Sprite>("O_Sprite");
            }
        }

        private void HandleClick()
        {
            if (IsOccupied) return;
            onCellClickedCallback?.Invoke(Row, Col);
        }

        /// <summary>
        /// Sets the symbol sprite for this cell with a pop-in animation.
        /// </summary>
        public void SetSymbol(PlayerType player, Color symbolColor, bool animate = true)
        {
            CurrentSymbol = player;
            EnsureSpritesLoaded();

            Sprite spriteToDisplay = player == PlayerType.X ? xSprite : (player == PlayerType.O ? oSprite : null);

            if (symbolImage != null)
            {
                symbolImage.sprite = spriteToDisplay;
                symbolImage.preserveAspect = true;
                symbolImage.color = Color.white;
                symbolImage.enabled = (spriteToDisplay != null);
            }

            // Clear text component so text does not overlay on the sprite
            if (symbolText != null)
            {
                symbolText.text = "";
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = occupiedCellColor;
            }

            if (cellButton != null)
            {
                cellButton.interactable = false;
            }

            if (animate && gameObject.activeInHierarchy)
            {
                if (animCoroutine != null) StopCoroutine(animCoroutine);
                animCoroutine = StartCoroutine(AnimateSymbolPop());
            }
            else
            {
                if (symbolImage != null) symbolImage.transform.localScale = Vector3.one;
                if (symbolText != null) symbolText.transform.localScale = Vector3.one;
            }
        }

        /// <summary>
        /// Highlights this cell as part of the losing line that triggered game over.
        /// </summary>
        public void HighlightLosingCell(Color alertColor)
        {
            if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
            pulseCoroutine = StartCoroutine(PulsateHighlight(alertColor));
        }

        /// <summary>
        /// Resets the cell back to its default clean state.
        /// </summary>
        public void ResetCell()
        {
            if (animCoroutine != null)
            {
                StopCoroutine(animCoroutine);
                animCoroutine = null;
            }

            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine);
                pulseCoroutine = null;
            }

            CurrentSymbol = PlayerType.None;

            if (symbolImage != null)
            {
                symbolImage.sprite = null;
                symbolImage.enabled = false;
                symbolImage.transform.localScale = Vector3.one;
            }

            if (symbolText != null)
            {
                symbolText.text = "";
                symbolText.transform.localScale = Vector3.one;
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = defaultCellColor;
            }

            if (highlightOverlay != null)
            {
                highlightOverlay.gameObject.SetActive(false);
            }

            if (cellButton != null)
            {
                cellButton.interactable = true;
            }
        }

        public void SetInteractable(bool interactable)
        {
            if (cellButton != null && !IsOccupied)
            {
                cellButton.interactable = interactable;
            }
        }

        private IEnumerator AnimateSymbolPop()
        {
            Transform t = symbolImage != null ? symbolImage.transform : (symbolText != null ? symbolText.transform : null);
            if (t == null) yield break;

            t.localScale = Vector3.zero;

            float duration = 0.22f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);

                // Overshoot bounce curve
                float scale = 1f + Mathf.Sin(progress * Mathf.PI) * 0.35f;
                if (progress >= 1f) scale = 1f;

                t.localScale = Vector3.one * (scale * progress);
                yield return null;
            }

            t.localScale = Vector3.one;
            animCoroutine = null;
        }

        private IEnumerator PulsateHighlight(Color alertColor)
        {
            if (highlightOverlay != null)
            {
                highlightOverlay.gameObject.SetActive(true);
                highlightOverlay.color = alertColor;
            }

            Color baseBg = occupiedCellColor;
            Color flashBg = alertColor * 0.75f;
            flashBg.a = 1f;

            float speed = 4f;

            while (true)
            {
                float t = (Mathf.Sin(Time.time * speed) + 1f) * 0.5f;
                if (backgroundImage != null)
                {
                    backgroundImage.color = Color.Lerp(baseBg, flashBg, t);
                }

                if (symbolImage != null)
                {
                    symbolImage.transform.localScale = Vector3.one * (1f + t * 0.15f);
                }
                else if (symbolText != null)
                {
                    symbolText.transform.localScale = Vector3.one * (1f + t * 0.15f);
                }

                yield return null;
            }
        }

        private void OnDisable()
        {
            if (animCoroutine != null) StopCoroutine(animCoroutine);
            if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
        }

        // Editor helper for setting references
        public void ConfigureComponents(Button btn, Image bg, Image symImg, TextMeshProUGUI text = null, Image overlay = null, Sprite xSp = null, Sprite oSp = null)
        {
            cellButton = btn;
            backgroundImage = bg;
            symbolImage = symImg;
            symbolText = text;
            highlightOverlay = overlay;
            if (xSp != null) xSprite = xSp;
            if (oSp != null) oSprite = oSp;
        }
    }
}
