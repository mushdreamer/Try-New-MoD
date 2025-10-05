using System;
using SpaceFusion.SF_Grid_Building_System.Scripts.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.Managers {
    public class InputManager : MonoBehaviour {
        public static InputManager Instance;

        public event Action OnClicked;
        public event Action OnExit;
        public event Action OnRotate;
        public event Action<Vector2> OnLmbPress;
        public event Action<Vector2> OnLmbHold;
        public event Action OnLmbRelease;
        public event Action<Vector2> OnMmbDrag;
        public event Action<Vector2> OnRmbDrag;
        public event Action<float> OnScroll;
        public event Action<Vector2> OnMouseAtScreenCorner;

        private bool _isHolding;
        private float _holdTimer;
        private Camera _sceneCamera;
        private Vector2 _screenSize;
        private Vector2 _lastMousePositionMmb;
        private Vector2 _lastMousePositionRmb;
        private Vector3 _lastMousePosition;
        private Vector3 _lastGridPosition;

        private LayerMask _placementLayerMask;
        private float _holdThreshold;
        private float _edgeMarginForAutoMove;

        private void Awake() {
            if (Instance != null) {
                Destroy(gameObject);
            }

            Instance = this;
            _screenSize = new Vector2(Screen.width, Screen.height);

        }

        private void Start() {
            var config = GameConfig.Instance;
            _placementLayerMask = config.PlacementLayerMask;
            _holdThreshold = config.HoldThreshold;
            _edgeMarginForAutoMove = config.EdgeMarginForAutoMove;
            _sceneCamera = GameManager.Instance.SceneCamera;
        }

        private void Update() {
            if (Input.GetMouseButtonDown(0)) {
                _isHolding = true;
                _holdTimer = 0;
                OnLmbPress?.Invoke(Input.mousePosition);
                OnClicked?.Invoke();
            }

            if (_isHolding) {
                _holdTimer += Time.deltaTime;
                if (_holdTimer >= _holdThreshold) {
                    OnLmbHold?.Invoke(Input.mousePosition);
                    _isHolding = false; // Only invoke hold once
                }
            }

            if (Input.GetMouseButtonUp(0)) {
                _isHolding = false;
                OnLmbRelease?.Invoke();
            }

            if (Input.GetMouseButtonDown(2)) {
                _lastMousePositionMmb = Input.mousePosition;
            }

            if (Input.GetMouseButton(2)) {
                var delta = (Vector2)Input.mousePosition - _lastMousePositionMmb;
                _lastMousePositionMmb = Input.mousePosition;
                OnMmbDrag?.Invoke(delta);
            }

            if (Input.GetMouseButtonDown(1)) {
                _lastMousePositionRmb = Input.mousePosition;
            }

            if (Input.GetMouseButton(1)) {
                var delta = (Vector2)Input.mousePosition - _lastMousePositionRmb;
                _lastMousePositionRmb = Input.mousePosition;
                OnRmbDrag?.Invoke(delta);
            }

            var scrollInput = Input.GetAxis("Mouse ScrollWheel");
            if (scrollInput != 0) {
                OnScroll?.Invoke(scrollInput);
            }

            if (Input.GetKeyDown(KeyCode.Escape)) {
                OnExit?.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.R)) {
                OnRotate?.Invoke();
            }

            Vector2 mousePos = Input.mousePosition;

            // Check if the mouse is near the edges
            if (!(mousePos.x <= _edgeMarginForAutoMove) &&
                !(mousePos.x >= _screenSize.x - _edgeMarginForAutoMove) &&
                !(mousePos.y <= _edgeMarginForAutoMove) &&
                !(mousePos.y >= _screenSize.y - _edgeMarginForAutoMove)) {
                return;
            }

            // Send normalized direction from center
            var screenCenter = new Vector2(_screenSize.x / 2, _screenSize.y / 2);
            var direction = (mousePos - screenCenter).normalized;
            OnMouseAtScreenCorner?.Invoke(direction);
        }

        /// <summary>
        /// Checks if the pointer is over a UI object, since we do not want to place any object when we click on a UI object (action called in the PlacementSystem)
        /// </summary>
        public static bool IsPointerOverUIObject() {
            return EventSystem.current.IsPointerOverGameObject();
        }

        /// <summary>
        /// Selects the exact grid map position based on the mouse input
        /// The ground will have a layer defined, so we only track the positions on those layers and ignore all other objects
        /// </summary>
        public Vector3 GetSelectedMapPosition() {
            var mousePos = Input.mousePosition;
            mousePos.z = _sceneCamera.nearClipPlane;
            var ray = _sceneCamera.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, out var hit, 100, _placementLayerMask)) {
                _lastGridPosition = hit.point;
            }

            return _lastGridPosition;
        }
    }
}