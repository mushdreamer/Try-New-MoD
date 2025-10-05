using SpaceFusion.SF_Grid_Building_System.Scripts.Interfaces;
using SpaceFusion.SF_Grid_Building_System.Scripts.Utils;
using UnityEngine;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.Core {
    [RequireComponent(typeof(BoxCollider))]
    public class PlacementGrid : MonoBehaviour, IPlacementGrid {
        [field: SerializeField]
        public Vector2Int Dimensions { get; private set; } = Vector2Int.one;

        [Tooltip("The size of one grid cell for this Grid")]
        [field: SerializeField]
        public float CellSize { get; private set; } = 1;

        [SerializeField]
        private Color debugGridColor = Color.cyan;

        private float _invertedGridSize;
        private GameObject _gridCellHolder;
        private GridVisualizer _gridVisualizer;
        private LayerMask _placementLayerMask;
        private GameObject _cellVisualizationPrefab;
        private GridVisualizer _gridVisualizationPrefab;

        private GameConfig _config;

        private void Awake() {
            ResizeBoxCollider();
            // Precalculate inverted grid size, to save a division every time we translate coords
            _invertedGridSize = 1 / CellSize;
        }

        public void Initialize() {
            _config = GameConfig.Instance;
            _placementLayerMask = _config.PlacementLayerMask;
            _cellVisualizationPrefab = _config.CellVisualizationPrefab;
            _gridVisualizationPrefab = _config.GridVisualizationPrefab;
            InstantiateGridVisualizations();
        }

        /// <summary>
        /// Converts a location in world space into local grid coordinates.
        /// </summary>
        public Vector3Int WorldToCell(Vector3 worldPosition) {
            var localLocation = transform.InverseTransformPoint(worldPosition);

            // Scale by inverse grid size
            localLocation *= _invertedGridSize;

            // Offset by half size
            var offset = new Vector3(CellSize * 0.5f, 0.0f, CellSize * 0.5f) * _invertedGridSize;
            localLocation -= offset;

            var xPos = SfMathUtils.RoundToInt(localLocation.x);
            var yPos = SfMathUtils.RoundToInt(localLocation.z);
            return new Vector3Int(xPos, 0, yPos);
        }

        /// <summary>
        /// cell position --> world position
        /// </summary>
        public Vector3 CellToWorld(Vector3Int gridPosition) {
            // Calculate scaled local position and transform to world space
            return transform.TransformPoint(new Vector3(gridPosition.x, 0, gridPosition.z) * CellSize);
        }

        /// <summary>
        /// adapts collider size on dimension change
        /// </summary>
        private void ResizeBoxCollider() {
            var myCollider = GetComponent<BoxCollider>();
            var size = new Vector3(Dimensions.x, 0, Dimensions.y) * CellSize;
            myCollider.size = size;
            // Collider origin = bottom-left corner
            myCollider.center = size * 0.5f;
        }

        /// <summary>
        /// validates if placeable object would be out of bounds of the current grid
        /// </summary>
        public bool IsWithinBounds(Vector3Int gridPosition, Vector2Int size) {
            // size out of bounds of the grid size
            if ((size.x > Dimensions.x) || (size.y > Dimensions.y)) {
                return false;
            }

            var furthestPoint = gridPosition + new Vector3Int(size.x, 0, size.y);
            // checks if all occupied object cells are inside the grid
            return gridPosition is { x: >= 0, z: >= 0 } && furthestPoint.x <= Dimensions.x && furthestPoint.z <= Dimensions.y;
        }


        public void SetVisualizationState(bool isActive) {
            if (_config.AlwaysShowGrid) {
                HandleGridVisualization(true);
                return;
            }

            if (_config.ShowGridOnPlacement) {
                HandleGridVisualization(isActive);
                return;
            }

            HandleGridVisualization(false);
        }

        private void HandleGridVisualization(bool isActive) {
            if (_gridCellHolder) {
                _gridCellHolder.SetActive(isActive);
            } else if (_gridVisualizer) {
                _gridVisualizer.gameObject.SetActive(isActive);
            }
        }

        /// <summary>
        /// Instantiates cell visualization prefabs and sets the proper layer mask for handling the object placements
        /// </summary>
        private void InstantiateGridVisualizations() {
            var placementLayer = Mathf.FloorToInt(Mathf.Log(_placementLayerMask.value, 2));

            if (_cellVisualizationPrefab == null && _gridVisualizationPrefab != null) {
                _gridVisualizer = Instantiate(_gridVisualizationPrefab, transform);
                _gridVisualizer.gameObject.layer = placementLayer;
                _gridVisualizer.Initialize(this, _config);

            }

            gameObject.layer = placementLayer;
            if (_cellVisualizationPrefab == null) {
                return;
            }

            _gridCellHolder = new GameObject("GridCellHolder") {
                transform = {
                    parent = transform,
                    localPosition = Vector3.zero,
                    localRotation = Quaternion.identity
                },
                layer = placementLayer
            };

            for (var x = 0; x < Dimensions.x; x++) {
                for (var y = 0; y < Dimensions.y; y++) {
                    var targetPos = CellToWorld(new Vector3Int(x, 0, y));
                    targetPos.y += 0.01f;
                    var cell = Instantiate(_cellVisualizationPrefab, _gridCellHolder.transform, true);
                    cell.transform.position = targetPos;
                    cell.transform.localRotation = Quaternion.identity;
                    cell.transform.localScale = new Vector3(CellSize, 1, CellSize);
                    cell.layer = placementLayer;
                    // TODO we could store the references of the cells into a list and make them removable again

                }
            }
            // state after initialisation should not show grid or cells unless GameConfig explicitly says so --> handled in SetVisualizationState
            SetVisualizationState(false);
        }

#if UNITY_EDITOR
        private void OnValidate() {
            // Validate grid size
            if (CellSize < 0) {
                Debug.LogWarning("Invalid cell size. Size should be greater than zero.");
            }

            // Validate dimensions
            if (Dimensions.x <= 0 || Dimensions.y <= 0) {
                Debug.LogError("Invalid grid dimensions. Dimensions should be greater than zero.");
                Dimensions = new Vector2Int(Mathf.Max(Dimensions.x, 1), Mathf.Max(Dimensions.y, 1));
            }

            ResizeBoxCollider();
            GetComponent<BoxCollider>().hideFlags = HideFlags.HideInInspector;
        }

        /// <summary>
        /// Debug grid in scene view
        /// </summary>
        private void OnDrawGizmos() {
            var prevCol = Gizmos.color;
            Gizmos.color = debugGridColor;

            var originalMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;

            // Draw local space flattened cubes
            for (var x = 0; x < Dimensions.x; x++) {
                for (var y = 0; y < Dimensions.y; y++) {
                    var position = new Vector3((x + 0.5f) * CellSize, 0, (y + 0.5f) * CellSize);
                    Gizmos.DrawWireCube(position, new Vector3(CellSize, 0, CellSize));
                }
            }

            Gizmos.matrix = originalMatrix;
            Gizmos.color = prevCol;
        }
#endif
    }
}