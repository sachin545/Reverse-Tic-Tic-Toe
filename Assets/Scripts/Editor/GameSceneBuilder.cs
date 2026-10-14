using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace ReverseTicTacToe.Editor
{
    [InitializeOnLoad]
    public static class GameSceneBuilder
    {
        private const string SCENE_PATH = "Assets/Scenes/SampleScene.unity";
        private const string PREFAB_DIR = "Assets/Prefabs";
        private const string PREFAB_PATH = "Assets/Prefabs/GridCellPrefab.prefab";
        private const string ART_DIR = "Assets/Art/Sprites";
        private const string FLAG_PATH = "Temp/ReverseTicTacToeBuilt.flag";
        private const string LOG_PATH = "Temp/ReverseTicTacToeBuildLog.txt";

        static GameSceneBuilder()
        {
            EditorApplication.delayCall += CheckAndAutoBuild;
        }

        private static void CheckAndAutoBuild()
        {
            if (File.Exists(PREFAB_PATH) || File.Exists(FLAG_PATH)) return;
            try
            {
                File.WriteAllText(LOG_PATH, "Starting BuildCompleteGame at " + System.DateTime.Now + "\n");
                BuildCompleteGame();
                File.WriteAllText(FLAG_PATH, "Success at " + System.DateTime.Now);
                File.AppendAllText(LOG_PATH, "BuildCompleteGame succeeded!\n");
            }
            catch (System.Exception ex)
            {
                File.AppendAllText(LOG_PATH, "BuildCompleteGame exception: " + ex + "\n");
                Debug.LogException(ex);
            }
        }

        [MenuItem("Tools/Build Reverse TicTacToe Scene")]
        public static void BuildCompleteGame()
        {
            Debug.Log("[GameSceneBuilder] Starting complete game scene setup...");

            EnsureDirectories();
            ImportTMPEssentialsIfMissing();

            Sprite roundBoxSprite = CreateOrGetRoundBoxSprite();
            Sprite circleSprite = CreateOrGetCircleSprite();
            Sprite borderSprite = CreateOrGetBorderSprite();

            GameObject cellPrefab = CreateOrUpdateGridCellPrefab(roundBoxSprite, borderSprite);

            BuildScene(roundBoxSprite, circleSprite, borderSprite, cellPrefab);

            Debug.Log("[GameSceneBuilder] Game scene successfully built and saved!");
        }

        private static void EnsureDirectories()
        {
            if (!Directory.Exists(PREFAB_DIR)) Directory.CreateDirectory(PREFAB_DIR);
            if (!Directory.Exists(ART_DIR)) Directory.CreateDirectory(ART_DIR);
            AssetDatabase.Refresh();
        }

        private static void ImportTMPEssentialsIfMissing()
        {
            // Check if TMP settings or default font asset exists
            TMP_Settings settings = Resources.Load<TMP_Settings>("TMP Settings");
            if (settings == null)
            {
                string packagePath = "Library/PackageCache/com.unity.ugui@0e6b3c20b946/Package Resources/TMP Essential Resources.unitypackage";
                if (!File.Exists(packagePath))
                {
                    // Search in PackageCache
                    string[] packages = Directory.GetFiles("Library/PackageCache", "TMP Essential Resources.unitypackage", SearchOption.AllDirectories);
                    if (packages.Length > 0) packagePath = packages[0].Replace('\\', '/');
                }

                if (File.Exists(packagePath))
                {
                    Debug.Log($"[GameSceneBuilder] Importing TMP Essential Resources from {packagePath}...");
                    AssetDatabase.ImportPackage(packagePath, false);
                    AssetDatabase.Refresh();
                }
            }
        }

        private static Sprite CreateOrGetRoundBoxSprite()
        {
            string path = $"{ART_DIR}/RoundBox.png";
            if (!File.Exists(path))
            {
                int size = 64;
                Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                Color fill = Color.white;
                Color clear = new Color(1, 1, 1, 0);
                float radius = 16f;

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float dx = Mathf.Max(0, Mathf.Abs(x - size * 0.5f) - (size * 0.5f - radius));
                        float dy = Mathf.Max(0, Mathf.Abs(y - size * 0.5f) - (size * 0.5f - radius));
                        float dist = Mathf.Sqrt(dx * dx + dy * dy);
                        float alpha = Mathf.Clamp01(radius + 0.5f - dist);
                        tex.SetPixel(x, y, new Color(fill.r, fill.g, fill.b, alpha));
                    }
                }
                tex.Apply();
                File.WriteAllBytes(path, tex.EncodeToPNG());
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteBorder = new Vector4(16, 16, 16, 16);
                    importer.alphaIsTransparency = true;
                    importer.SaveAndReimport();
                }
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Sprite CreateOrGetCircleSprite()
        {
            string path = $"{ART_DIR}/Circle.png";
            if (!File.Exists(path))
            {
                int size = 64;
                Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                float radius = size * 0.5f - 1f;
                Vector2 center = new Vector2(size * 0.5f, size * 0.5f);

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                        float alpha = Mathf.Clamp01(radius + 0.5f - dist);
                        tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
                    }
                }
                tex.Apply();
                File.WriteAllBytes(path, tex.EncodeToPNG());
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.alphaIsTransparency = true;
                    importer.SaveAndReimport();
                }
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Sprite CreateOrGetBorderSprite()
        {
            string path = $"{ART_DIR}/BorderBox.png";
            if (!File.Exists(path))
            {
                int size = 64;
                Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                float outerRadius = 16f;
                float borderWidth = 4f;
                float innerRadius = outerRadius - borderWidth;

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float odx = Mathf.Max(0, Mathf.Abs(x - size * 0.5f) - (size * 0.5f - outerRadius));
                        float ody = Mathf.Max(0, Mathf.Abs(y - size * 0.5f) - (size * 0.5f - outerRadius));
                        float oDist = Mathf.Sqrt(odx * odx + ody * ody);
                        float outerAlpha = Mathf.Clamp01(outerRadius + 0.5f - oDist);

                        float idx = Mathf.Max(0, Mathf.Abs(x - size * 0.5f) - (size * 0.5f - outerRadius));
                        float idy = Mathf.Max(0, Mathf.Abs(y - size * 0.5f) - (size * 0.5f - outerRadius));
                        float iDist = Mathf.Sqrt(idx * idx + idy * idy);
                        float innerAlpha = Mathf.Clamp01(innerRadius + 0.5f - iDist);

                        float borderAlpha = Mathf.Clamp01(outerAlpha - innerAlpha);
                        tex.SetPixel(x, y, new Color(1, 1, 1, borderAlpha));
                    }
                }
                tex.Apply();
                File.WriteAllBytes(path, tex.EncodeToPNG());
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteBorder = new Vector4(16, 16, 16, 16);
                    importer.alphaIsTransparency = true;
                    importer.SaveAndReimport();
                }
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static GameObject CreateOrUpdateGridCellPrefab(Sprite roundBox, Sprite borderSprite)
        {
            GameObject cellObj = new GameObject("GridCell", typeof(RectTransform));
            RectTransform rect = cellObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100, 100);

            // Background Image
            Image bg = cellObj.AddComponent<Image>();
            bg.sprite = roundBox;
            bg.type = Image.Type.Sliced;
            bg.color = new Color(0.12f, 0.16f, 0.23f, 1f); // #1E293B

            // Button
            Button btn = cellObj.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1.2f, 1.2f, 1.3f, 1f);
            cb.pressedColor = new Color(0.8f, 0.8f, 0.9f, 1f);
            cb.disabledColor = Color.white;
            btn.colors = cb;
            btn.targetGraphic = bg;

            // Highlight Overlay (for loss line pulsing)
            GameObject overlayObj = new GameObject("HighlightOverlay", typeof(RectTransform));
            overlayObj.transform.SetParent(cellObj.transform, false);
            RectTransform overlayRect = overlayObj.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            Image overlayImg = overlayObj.AddComponent<Image>();
            overlayImg.sprite = borderSprite != null ? borderSprite : roundBox;
            overlayImg.type = Image.Type.Sliced;
            overlayImg.color = new Color(1f, 0.15f, 0.25f, 0.9f);
            overlayObj.SetActive(false);

            // Symbol Text
            GameObject textObj = new GameObject("SymbolText", typeof(RectTransform));
            textObj.transform.SetParent(cellObj.transform, false);
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            TMP_FontAsset font = GetDefaultFont();
            if (font != null) tmp.font = font;
            tmp.text = "";
            tmp.fontSize = 58;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;

            // GridCell component
            GridCell gridCell = cellObj.AddComponent<GridCell>();
            gridCell.ConfigureComponents(btn, bg, tmp, overlayImg);

            // Save Prefab
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(cellObj, PREFAB_PATH);
            Object.DestroyImmediate(cellObj);
            AssetDatabase.Refresh();
            return prefab;
        }

        private static void BuildScene(Sprite roundBox, Sprite circleSprite, Sprite borderSprite, GameObject cellPrefab)
        {
            var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);

            // Clear old scene root objects
            GameObject[] rootObjects = scene.GetRootGameObjects();
            foreach (var go in rootObjects)
            {
                Object.DestroyImmediate(go);
            }

            // 1. Camera
            GameObject camObj = new GameObject("Main Camera");
            Camera cam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.08f, 0.12f, 1f); // #0F151F
            camObj.transform.position = new Vector3(0, 0, -10);

            // 2. Event System
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<InputSystemUIInputModule>();

            // 3. Canvas
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            // Background
            GameObject bgObj = CreateUIElement("Background", canvasObj.transform);
            SetStretchFull(bgObj.GetComponent<RectTransform>());
            Image bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0.06f, 0.08f, 0.12f, 1f); // Deep slate

            // 4. Controllers Object
            GameObject controllerObj = new GameObject("GameController");
            AudioSource audioSource = controllerObj.AddComponent<AudioSource>();
            SoundManager soundManager = controllerObj.AddComponent<SoundManager>();
            GameManager gameManager = controllerObj.AddComponent<GameManager>();
            UIManager uiManager = controllerObj.AddComponent<UIManager>();

            // 5. Build Main Menu Panel
            GameObject mainMenu = CreateUIElement("MainMenuPanel", canvasObj.transform);
            SetStretchFull(mainMenu.GetComponent<RectTransform>());

            // Title Container
            GameObject titleContainer = CreateUIElement("TitleContainer", mainMenu.transform);
            RectTransform titleRect = titleContainer.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.1f, 0.6f);
            titleRect.anchorMax = new Vector2(0.9f, 0.9f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            TextMeshProUGUI titleText = CreateTMPText("TitleText", titleContainer.transform, "DON'T MAKE THREE!", 68, FontStyles.Bold, new Color(0f, 0.9f, 1f, 1f), TextAlignmentOptions.Center);
            SetStretchFull(titleText.GetComponent<RectTransform>());
            titleText.GetComponent<RectTransform>().offsetMin = new Vector2(0, 100);

            TextMeshProUGUI subtitleText = CreateTMPText("SubtitleText", titleContainer.transform, "Three in a row means you lose.", 30, FontStyles.Italic, new Color(0.85f, 0.5f, 0.7f, 1f), TextAlignmentOptions.Center);
            RectTransform subRect = subtitleText.GetComponent<RectTransform>();
            subRect.anchorMin = new Vector2(0, 0.2f);
            subRect.anchorMax = new Vector2(1, 0.55f);
            subRect.offsetMin = Vector2.zero;
            subRect.offsetMax = Vector2.zero;

            TextMeshProUGUI tagText = CreateTMPText("TaglineText", titleContainer.transform, "5×5 Reverse Tic-Tac-Toe Strategy Game", 24, FontStyles.Normal, new Color(0.6f, 0.68f, 0.8f, 1f), TextAlignmentOptions.Center);
            RectTransform tagRect = tagText.GetComponent<RectTransform>();
            tagRect.anchorMin = new Vector2(0, 0f);
            tagRect.anchorMax = new Vector2(1, 0.25f);
            tagRect.offsetMin = Vector2.zero;
            tagRect.offsetMax = Vector2.zero;

            // Menu Buttons Container
            GameObject menuButtons = CreateUIElement("MenuButtons", mainMenu.transform);
            RectTransform mbRect = menuButtons.GetComponent<RectTransform>();
            mbRect.anchorMin = new Vector2(0.2f, 0.25f);
            mbRect.anchorMax = new Vector2(0.8f, 0.52f);
            mbRect.offsetMin = Vector2.zero;
            mbRect.offsetMax = Vector2.zero;
            VerticalLayoutGroup vlg = menuButtons.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 24;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            Button playBtn = CreateUIButton("BtnPlay", menuButtons.transform, roundBox, new Color(0.02f, 0.71f, 0.83f, 1f), "PLAY GAME", 36, Color.black, 85);
            Button howToBtn = CreateUIButton("BtnHowToPlay", menuButtons.transform, roundBox, new Color(0.18f, 0.24f, 0.35f, 1f), "HOW TO PLAY", 30, Color.white, 75);
            Button soundBtn = CreateUIButton("BtnSoundMenu", menuButtons.transform, roundBox, new Color(0.12f, 0.16f, 0.23f, 1f), "SOUND: ON", 26, new Color(0.7f, 0.8f, 0.9f, 1f), 65);
            TextMeshProUGUI soundMenuText = soundBtn.GetComponentInChildren<TextMeshProUGUI>();

            // 6. Build How To Play Panel (Modal)
            GameObject howToPlay = CreateUIElement("HowToPlayPanel", canvasObj.transform);
            SetStretchFull(howToPlay.GetComponent<RectTransform>());

            GameObject htpScrim = CreateUIElement("Scrim", howToPlay.transform);
            SetStretchFull(htpScrim.GetComponent<RectTransform>());
            Image scrimImg = htpScrim.AddComponent<Image>();
            scrimImg.color = new Color(0f, 0f, 0f, 0.85f);

            GameObject htpCard = CreateUIElement("Card", howToPlay.transform);
            RectTransform cardRect = htpCard.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.1f, 0.18f);
            cardRect.anchorMax = new Vector2(0.9f, 0.82f);
            cardRect.offsetMin = Vector2.zero;
            cardRect.offsetMax = Vector2.zero;
            Image cardImg = htpCard.AddComponent<Image>();
            cardImg.sprite = roundBox;
            cardImg.type = Image.Type.Sliced;
            cardImg.color = new Color(0.12f, 0.16f, 0.23f, 1f);

            TextMeshProUGUI htpTitle = CreateTMPText("Title", htpCard.transform, "HOW TO PLAY", 44, FontStyles.Bold, new Color(0f, 0.9f, 1f, 1f), TextAlignmentOptions.Center);
            RectTransform htpTitleRect = htpTitle.GetComponent<RectTransform>();
            htpTitleRect.anchorMin = new Vector2(0.05f, 0.85f);
            htpTitleRect.anchorMax = new Vector2(0.95f, 0.97f);
            htpTitleRect.offsetMin = Vector2.zero;
            htpTitleRect.offsetMax = Vector2.zero;

            string rulesContent = "<b>GRID & PLAYERS:</b>\n" +
                                  "• 5×5 Grid (25 empty cells)\n" +
                                  "• 2 Players locally on the same device\n" +
                                  "• Player 1 uses <b>X</b>, Player 2 uses <b>O</b>\n\n" +
                                  "<b>THE REVERSE RULE:</b>\n" +
                                  "• The player who creates <b>three or more consecutive identical symbols</b> in a line horizontally, vertically, or diagonally...\n" +
                                  "   <color=#FF3366><b>IMMEDIATELY LOSES THE GAME!</b></color>\n" +
                                  "• The other player wins!\n\n" +
                                  "<b>STRATEGY & DRAW:</b>\n" +
                                  "• Strategically place symbols without completing a 3-in-a-row.\n" +
                                  "• Trap your opponent so their only legal moves form a losing line!\n" +
                                  "• If all 25 cells are filled without any losing line, it's a <b>DRAW</b>.";

            TextMeshProUGUI htpBody = CreateTMPText("Body", htpCard.transform, rulesContent, 25, FontStyles.Normal, new Color(0.88f, 0.92f, 0.98f, 1f), TextAlignmentOptions.TopLeft);
            RectTransform htpBodyRect = htpBody.GetComponent<RectTransform>();
            htpBodyRect.anchorMin = new Vector2(0.07f, 0.18f);
            htpBodyRect.anchorMax = new Vector2(0.93f, 0.83f);
            htpBodyRect.offsetMin = Vector2.zero;
            htpBodyRect.offsetMax = Vector2.zero;

            Button htpCloseBtn = CreateUIButton("BtnCloseHowTo", htpCard.transform, roundBox, new Color(0.02f, 0.71f, 0.83f, 1f), "GOT IT!", 30, Color.black, 70);
            RectTransform htpCloseRect = htpCloseBtn.GetComponent<RectTransform>();
            htpCloseRect.anchorMin = new Vector2(0.25f, 0.04f);
            htpCloseRect.anchorMax = new Vector2(0.75f, 0.15f);
            htpCloseRect.offsetMin = Vector2.zero;
            htpCloseRect.offsetMax = Vector2.zero;

            howToPlay.SetActive(false);

            // 7. Build Gameplay Panel
            GameObject gameplay = CreateUIElement("GameplayPanel", canvasObj.transform);
            SetStretchFull(gameplay.GetComponent<RectTransform>());

            // Top Header Bar
            GameObject header = CreateUIElement("HeaderBar", gameplay.transform);
            RectTransform headerRect = header.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0.05f, 0.90f);
            headerRect.anchorMax = new Vector2(0.95f, 0.97f);
            headerRect.offsetMin = Vector2.zero;
            headerRect.offsetMax = Vector2.zero;

            Button menuBtn = CreateUIButton("BtnMenu", header.transform, roundBox, new Color(0.18f, 0.24f, 0.35f, 1f), "MENU", 22, Color.white, 0);
            RectTransform menuBtnRect = menuBtn.GetComponent<RectTransform>();
            menuBtnRect.anchorMin = new Vector2(0f, 0f);
            menuBtnRect.anchorMax = new Vector2(0.22f, 1f);
            menuBtnRect.offsetMin = Vector2.zero;
            menuBtnRect.offsetMax = Vector2.zero;

            // Turn Indicator (Center of Header)
            GameObject turnBadgeObj = CreateUIElement("TurnBadge", header.transform);
            RectTransform turnBadgeRect = turnBadgeObj.GetComponent<RectTransform>();
            turnBadgeRect.anchorMin = new Vector2(0.25f, 0.05f);
            turnBadgeRect.anchorMax = new Vector2(0.75f, 0.95f);
            turnBadgeRect.offsetMin = Vector2.zero;
            turnBadgeRect.offsetMax = Vector2.zero;
            Image turnBadgeImg = turnBadgeObj.AddComponent<Image>();
            turnBadgeImg.sprite = roundBox;
            turnBadgeImg.type = Image.Type.Sliced;
            turnBadgeImg.color = new Color(0.12f, 0.16f, 0.23f, 1f);

            GameObject dotObj = CreateUIElement("TurnDot", turnBadgeObj.transform);
            RectTransform dotRect = dotObj.GetComponent<RectTransform>();
            dotRect.anchorMin = new Vector2(0.06f, 0.25f);
            dotRect.anchorMax = new Vector2(0.18f, 0.75f);
            dotRect.offsetMin = Vector2.zero;
            dotRect.offsetMax = Vector2.zero;
            Image dotImg = dotObj.AddComponent<Image>();
            dotImg.sprite = circleSprite;
            dotImg.color = new Color(0f, 0.9f, 1f, 1f);

            TextMeshProUGUI turnText = CreateTMPText("TurnText", turnBadgeObj.transform, "PLAYER X'S TURN", 28, FontStyles.Bold, new Color(0f, 0.9f, 1f, 1f), TextAlignmentOptions.Center);
            RectTransform turnTextRect = turnText.GetComponent<RectTransform>();
            turnTextRect.anchorMin = new Vector2(0.2f, 0f);
            turnTextRect.anchorMax = new Vector2(0.95f, 1f);
            turnTextRect.offsetMin = Vector2.zero;
            turnTextRect.offsetMax = Vector2.zero;

            Button restartBtn = CreateUIButton("BtnRestart", header.transform, roundBox, new Color(0.18f, 0.24f, 0.35f, 1f), "RESET", 22, Color.white, 0);
            RectTransform restartBtnRect = restartBtn.GetComponent<RectTransform>();
            restartBtnRect.anchorMin = new Vector2(0.78f, 0f);
            restartBtnRect.anchorMax = new Vector2(1f, 1f);
            restartBtnRect.offsetMin = Vector2.zero;
            restartBtnRect.offsetMax = Vector2.zero;

            // Score Bar
            GameObject scoreBar = CreateUIElement("ScoreBar", gameplay.transform);
            RectTransform sbRect = scoreBar.GetComponent<RectTransform>();
            sbRect.anchorMin = new Vector2(0.06f, 0.81f);
            sbRect.anchorMax = new Vector2(0.94f, 0.88f);
            sbRect.offsetMin = Vector2.zero;
            sbRect.offsetMax = Vector2.zero;
            Image sbBg = scoreBar.AddComponent<Image>();
            sbBg.sprite = roundBox;
            sbBg.type = Image.Type.Sliced;
            sbBg.color = new Color(0.10f, 0.13f, 0.19f, 0.95f);

            // Three columns: Player X, Draws, Player O
            // Player X Column
            GameObject scoreXCol = CreateUIElement("ScoreX", scoreBar.transform);
            RectTransform xColRect = scoreXCol.GetComponent<RectTransform>();
            xColRect.anchorMin = new Vector2(0.03f, 0.05f);
            xColRect.anchorMax = new Vector2(0.32f, 0.95f);
            xColRect.offsetMin = Vector2.zero;
            xColRect.offsetMax = Vector2.zero;
            TextMeshProUGUI xLabel = CreateTMPText("Label", scoreXCol.transform, "PLAYER X", 20, FontStyles.Bold, new Color(0f, 0.9f, 1f, 1f), TextAlignmentOptions.Top);
            SetStretchFull(xLabel.GetComponent<RectTransform>());
            xLabel.GetComponent<RectTransform>().offsetMin = new Vector2(0, 40);
            TextMeshProUGUI scoreXText = CreateTMPText("Score", scoreXCol.transform, "0", 38, FontStyles.Bold, Color.white, TextAlignmentOptions.Bottom);
            SetStretchFull(scoreXText.GetComponent<RectTransform>());
            scoreXText.GetComponent<RectTransform>().offsetMax = new Vector2(0, -25);

            // Draws Column
            GameObject scoreDrawsCol = CreateUIElement("ScoreDraws", scoreBar.transform);
            RectTransform dColRect = scoreDrawsCol.GetComponent<RectTransform>();
            dColRect.anchorMin = new Vector2(0.36f, 0.05f);
            dColRect.anchorMax = new Vector2(0.64f, 0.95f);
            dColRect.offsetMin = Vector2.zero;
            dColRect.offsetMax = Vector2.zero;
            TextMeshProUGUI dLabel = CreateTMPText("Label", scoreDrawsCol.transform, "DRAWS", 20, FontStyles.Normal, new Color(0.6f, 0.68f, 0.8f, 1f), TextAlignmentOptions.Top);
            SetStretchFull(dLabel.GetComponent<RectTransform>());
            dLabel.GetComponent<RectTransform>().offsetMin = new Vector2(0, 40);
            TextMeshProUGUI scoreDrawsText = CreateTMPText("Score", scoreDrawsCol.transform, "0", 38, FontStyles.Bold, Color.white, TextAlignmentOptions.Bottom);
            SetStretchFull(scoreDrawsText.GetComponent<RectTransform>());
            scoreDrawsText.GetComponent<RectTransform>().offsetMax = new Vector2(0, -25);

            // Player O Column
            GameObject scoreOCol = CreateUIElement("ScoreO", scoreBar.transform);
            RectTransform oColRect = scoreOCol.GetComponent<RectTransform>();
            oColRect.anchorMin = new Vector2(0.68f, 0.05f);
            oColRect.anchorMax = new Vector2(0.97f, 0.95f);
            oColRect.offsetMin = Vector2.zero;
            oColRect.offsetMax = Vector2.zero;
            TextMeshProUGUI oLabel = CreateTMPText("Label", scoreOCol.transform, "PLAYER O", 20, FontStyles.Bold, new Color(1f, 0.25f, 0.45f, 1f), TextAlignmentOptions.Top);
            SetStretchFull(oLabel.GetComponent<RectTransform>());
            oLabel.GetComponent<RectTransform>().offsetMin = new Vector2(0, 40);
            TextMeshProUGUI scoreOText = CreateTMPText("Score", scoreOCol.transform, "0", 38, FontStyles.Bold, Color.white, TextAlignmentOptions.Bottom);
            SetStretchFull(scoreOText.GetComponent<RectTransform>());
            scoreOText.GetComponent<RectTransform>().offsetMax = new Vector2(0, -25);

            // 5x5 Grid Board Area (Centered, Responsive)
            GameObject boardArea = CreateUIElement("BoardArea", gameplay.transform);
            RectTransform boardAreaRect = boardArea.GetComponent<RectTransform>();
            boardAreaRect.anchorMin = new Vector2(0.04f, 0.16f);
            boardAreaRect.anchorMax = new Vector2(0.96f, 0.78f);
            boardAreaRect.offsetMin = Vector2.zero;
            boardAreaRect.offsetMax = Vector2.zero;
            AspectRatioFitter arf = boardArea.AddComponent<AspectRatioFitter>();
            arf.aspectRatio = 1.0f;
            arf.aspectMode = AspectRatioFitter.AspectMode.FitInParent;

            // Board Frame
            GameObject boardFrame = CreateUIElement("BoardFrame", boardArea.transform);
            SetStretchFull(boardFrame.GetComponent<RectTransform>());
            Image frameBg = boardFrame.AddComponent<Image>();
            frameBg.sprite = roundBox;
            frameBg.type = Image.Type.Sliced;
            frameBg.color = new Color(0.08f, 0.10f, 0.15f, 1f); // Dark container

            // Grid Container
            GameObject gridContainer = CreateUIElement("GridContainer", boardFrame.transform);
            RectTransform gcRect = gridContainer.GetComponent<RectTransform>();
            SetStretchFull(gcRect);
            gcRect.offsetMin = new Vector2(16, 16);
            gcRect.offsetMax = new Vector2(-16, -16);

            GridLayoutGroup glg = gridContainer.AddComponent<GridLayoutGroup>();
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 5;
            glg.spacing = new Vector2(10, 10);
            glg.cellSize = new Vector2(175, 175);
            glg.childAlignment = TextAnchor.MiddleCenter;

            // Responsive grid cell resizer script
            GridContainerScaler scalerComponent = gridContainer.AddComponent<GridContainerScaler>();
            scalerComponent.Configure(glg, 5, 10f);

            // Bottom Rules Reminder Tip
            GameObject bottomTip = CreateUIElement("BottomTip", gameplay.transform);
            RectTransform tipRect = bottomTip.GetComponent<RectTransform>();
            tipRect.anchorMin = new Vector2(0.05f, 0.05f);
            tipRect.anchorMax = new Vector2(0.95f, 0.12f);
            tipRect.offsetMin = Vector2.zero;
            tipRect.offsetMax = Vector2.zero;
            TextMeshProUGUI tipText = CreateTMPText("TipText", bottomTip.transform, "⚠ TRAP RULE: 3 or more in a row (horizontal, vertical, or diagonal) LOSES!", 22, FontStyles.Normal, new Color(0.85f, 0.5f, 0.6f, 0.9f), TextAlignmentOptions.Center);
            SetStretchFull(tipText.GetComponent<RectTransform>());

            gameplay.SetActive(false);

            // 8. Build Game Over Modal Panel
            GameObject gameOver = CreateUIElement("GameOverPanel", canvasObj.transform);
            SetStretchFull(gameOver.GetComponent<RectTransform>());

            GameObject goScrim = CreateUIElement("Scrim", gameOver.transform);
            SetStretchFull(goScrim.GetComponent<RectTransform>());
            Image goScrimImg = goScrim.AddComponent<Image>();
            goScrimImg.color = new Color(0f, 0f, 0f, 0.88f);

            GameObject goCard = CreateUIElement("Card", gameOver.transform);
            RectTransform goCardRect = goCard.GetComponent<RectTransform>();
            goCardRect.anchorMin = new Vector2(0.1f, 0.25f);
            goCardRect.anchorMax = new Vector2(0.9f, 0.75f);
            goCardRect.offsetMin = Vector2.zero;
            goCardRect.offsetMax = Vector2.zero;
            Image goCardBg = goCard.AddComponent<Image>();
            goCardBg.sprite = roundBox;
            goCardBg.type = Image.Type.Sliced;
            goCardBg.color = new Color(0.12f, 0.16f, 0.23f, 1f);

            // Result Badge
            GameObject resultBadge = CreateUIElement("ResultBadge", goCard.transform);
            RectTransform rbRect = resultBadge.GetComponent<RectTransform>();
            rbRect.anchorMin = new Vector2(0.35f, 0.78f);
            rbRect.anchorMax = new Vector2(0.65f, 0.94f);
            rbRect.offsetMin = Vector2.zero;
            rbRect.offsetMax = Vector2.zero;
            Image rbImg = resultBadge.AddComponent<Image>();
            rbImg.sprite = roundBox;
            rbImg.type = Image.Type.Sliced;
            rbImg.color = new Color(0f, 0.9f, 1f, 1f);

            TextMeshProUGUI gameOverTitle = CreateTMPText("GameOverTitle", goCard.transform, "PLAYER O WINS!", 46, FontStyles.Bold, new Color(1f, 0.25f, 0.45f, 1f), TextAlignmentOptions.Center);
            RectTransform gotRect = gameOverTitle.GetComponent<RectTransform>();
            gotRect.anchorMin = new Vector2(0.05f, 0.58f);
            gotRect.anchorMax = new Vector2(0.95f, 0.75f);
            gotRect.offsetMin = Vector2.zero;
            gotRect.offsetMax = Vector2.zero;

            TextMeshProUGUI gameOverSubtitle = CreateTMPText("GameOverSubtitle", goCard.transform, "Player X made three! Player O wins!", 26, FontStyles.Normal, new Color(0.85f, 0.90f, 0.98f, 1f), TextAlignmentOptions.Center);
            RectTransform gosRect = gameOverSubtitle.GetComponent<RectTransform>();
            gosRect.anchorMin = new Vector2(0.05f, 0.42f);
            gosRect.anchorMax = new Vector2(0.95f, 0.56f);
            gosRect.offsetMin = Vector2.zero;
            gosRect.offsetMax = Vector2.zero;

            Button playAgainBtn = CreateUIButton("BtnPlayAgain", goCard.transform, roundBox, new Color(0.02f, 0.71f, 0.83f, 1f), "PLAY AGAIN", 32, Color.black, 75);
            RectTransform paRect = playAgainBtn.GetComponent<RectTransform>();
            paRect.anchorMin = new Vector2(0.15f, 0.22f);
            paRect.anchorMax = new Vector2(0.85f, 0.36f);
            paRect.offsetMin = Vector2.zero;
            paRect.offsetMax = Vector2.zero;

            Button goMenuBtn = CreateUIButton("BtnGameOverMenu", goCard.transform, roundBox, new Color(0.18f, 0.24f, 0.35f, 1f), "MAIN MENU", 26, Color.white, 65);
            RectTransform gmRect = goMenuBtn.GetComponent<RectTransform>();
            gmRect.anchorMin = new Vector2(0.2f, 0.06f);
            gmRect.anchorMax = new Vector2(0.8f, 0.18f);
            gmRect.offsetMin = Vector2.zero;
            gmRect.offsetMax = Vector2.zero;

            gameOver.SetActive(false);

            // Hook up UI Manager References with Button parameters
            uiManager.ConfigureReferences(
                mainMenu, gameplay, howToPlay, gameOver,
                turnText, dotImg,
                scoreXText, scoreOText, scoreDrawsText,
                gameOverTitle, gameOverSubtitle, rbImg,
                gcRect, cellPrefab,
                soundMenuText,
                playBtn, howToBtn, htpCloseBtn,
                menuBtn, restartBtn,
                playAgainBtn, goMenuBtn, soundBtn
            );

            // Persistent Editor Listeners
            UnityEditor.Events.UnityEventTools.AddPersistentListener(playBtn.onClick, uiManager.OnBtnPlay);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(howToBtn.onClick, uiManager.OnBtnHowToPlayOpen);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(soundBtn.onClick, uiManager.OnBtnSoundToggle);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(htpCloseBtn.onClick, uiManager.OnBtnHowToPlayClose);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(menuBtn.onClick, uiManager.OnBtnMenu);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(restartBtn.onClick, uiManager.OnBtnRestart);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(playAgainBtn.onClick, uiManager.OnBtnRestart);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(goMenuBtn.onClick, uiManager.OnBtnMenu);

            // Configure GameManager References
            gameManager.ConfigureReferences(
                uiManager,
                new Color(0f, 0.9f, 1f, 1f),       // Player X: Cyan
                new Color(1f, 0.25f, 0.45f, 1f),   // Player O: Coral Pink
                new Color(1f, 0.15f, 0.25f, 1f)    // Loss alert: Red
            );

            // Save Scene
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static GameObject CreateUIElement(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void SetStretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static TMP_FontAsset s_defaultFont;
        private static TMP_FontAsset GetDefaultFont()
        {
            if (s_defaultFont == null)
            {
                s_defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
                if (s_defaultFont == null)
                {
                    s_defaultFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
                }
            }
            return s_defaultFont;
        }

        private static TextMeshProUGUI CreateTMPText(string name, Transform parent, string text, float fontSize, FontStyles style, Color color, TextAlignmentOptions alignment)
        {
            GameObject go = CreateUIElement(name, parent);
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            TMP_FontAsset font = GetDefaultFont();
            if (font != null) tmp.font = font;
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static Button CreateUIButton(string name, Transform parent, Sprite sprite, Color bgColor, string label, float fontSize, Color textColor, float preferredHeight)
        {
            GameObject btnObj = CreateUIElement(name, parent);
            RectTransform rect = btnObj.GetComponent<RectTransform>();
            if (preferredHeight > 0)
            {
                LayoutElement le = btnObj.AddComponent<LayoutElement>();
                le.preferredHeight = preferredHeight;
            }

            Image img = btnObj.AddComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            img.color = bgColor;

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1.15f, 1.15f, 1.25f, 1f);
            cb.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            btn.colors = cb;
            btn.targetGraphic = img;

            TextMeshProUGUI tmp = CreateTMPText("Label", btnObj.transform, label, fontSize, FontStyles.Bold, textColor, TextAlignmentOptions.Center);
            SetStretchFull(tmp.GetComponent<RectTransform>());

            return btn;
        }
    }
}
