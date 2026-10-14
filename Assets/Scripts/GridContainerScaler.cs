using UnityEngine;
using UnityEngine.UI;

namespace ReverseTicTacToe
{
    /// <summary>
    /// Dynamically resizes the GridLayoutGroup cells to maintain equal square dimensions
    /// responsive to any screen resolution and aspect ratio.
    /// </summary>
    [RequireComponent(typeof(GridLayoutGroup))]
    [ExecuteAlways]
    public class GridContainerScaler : MonoBehaviour
    {
        [SerializeField] private GridLayoutGroup gridLayout;
        [SerializeField] private int gridDimension = 5;
        [SerializeField] private float cellSpacing = 10f;

        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if (gridLayout == null)
            {
                gridLayout = GetComponent<GridLayoutGroup>();
            }
        }

        private void Start()
        {
            UpdateCellSizes();
        }

        public void Configure(GridLayoutGroup layout, int dimension, float spacing)
        {
            gridLayout = layout;
            gridDimension = dimension;
            cellSpacing = spacing;
            rectTransform = GetComponent<RectTransform>();
            UpdateCellSizes();
        }

        private void OnRectTransformDimensionsChange()
        {
            UpdateCellSizes();
        }

        public void UpdateCellSizes()
        {
            if (gridLayout == null)
            {
                gridLayout = GetComponent<GridLayoutGroup>();
                if (gridLayout == null) return;
            }

            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
                if (rectTransform == null) return;
            }

            Rect rect = rectTransform.rect;
            if (rect.width <= 0 || rect.height <= 0) return;

            // Total available width and height excluding padding and spacing
            float padX = gridLayout.padding.horizontal;
            float padY = gridLayout.padding.vertical;

            float totalSpacingX = cellSpacing * (gridDimension - 1);
            float totalSpacingY = cellSpacing * (gridDimension - 1);

            float availableW = rect.width - padX - totalSpacingX;
            float availableH = rect.height - padY - totalSpacingY;

            float cellSide = Mathf.Min(availableW, availableH) / gridDimension;
            if (cellSide < 10f) cellSide = 10f;

            gridLayout.spacing = new Vector2(cellSpacing, cellSpacing);
            gridLayout.cellSize = new Vector2(cellSide, cellSide);
        }
    }
}
