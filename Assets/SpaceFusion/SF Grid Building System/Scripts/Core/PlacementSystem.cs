using System;
using System.Collections.Generic;
using SpaceFusion.SF_Grid_Building_System.Scripts.Enums;
using SpaceFusion.SF_Grid_Building_System.Scripts.Interfaces;
using SpaceFusion.SF_Grid_Building_System.Scripts.Managers;
using SpaceFusion.SF_Grid_Building_System.Scripts.PlacementStates;
using SpaceFusion.SF_Grid_Building_System.Scripts.SaveSystem;
using SpaceFusion.SF_Grid_Building_System.Scripts.Scriptables;
using SpaceFusion.SF_Grid_Building_System.Scripts.Utils;
using UnityEngine;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.Core {
    public class PlacementSystem : MonoBehaviour {
        public static PlacementSystem Instance;

        [SerializeField]
        private PreviewSystem previewSystem;

        [SerializeField]
        private PlacementHandler placementHandler;


        // EVENTS
        public event Action OnPlacementStateStart;
        public event Action OnPlacementStateEnd;

        private readonly Dictionary<GridDataType, GridData> _gridDataMap = new();
        private Vector3Int _lastDetectedPosition = Vector3Int.zero;
        private IPlacementState _stateHandler;
        private InputManager _inputManager;
        private GameConfig _gameConfig;
        private PlaceableObjectDatabase _database;


        private PlacementGrid _grid;
        // !!!!some additional triggers to handle specific cases !!!
        // for moving state we want to stop immediately after action is executed
        // for remove and place we allow to have multi actions, so we don't need to select if after each action again
        private bool _stopStateAfterAction;

        /// <summary>
        /// Make singleton, we only need to have 1 placement system active at a time
        /// </summary>
        private void Awake() {
            if (Instance != null) {
                Destroy(this);
            }

            Instance = this;
        }


        public void Initialize(PlacementGrid grid) {
            _grid = grid;
            _gameConfig = GameConfig.Instance;
            _database = _gameConfig.PlaceableObjectDatabase;
            _inputManager = InputManager.Instance;
            foreach (GridDataType gridType in Enum.GetValues(typeof(GridDataType))) {
                // creates GridData for every possible gridType
                _gridDataMap[gridType] = new GridData();
            }

            StopState();
        }


        /// <summary>
        /// LoadedObjectPlacementState
        /// based on the loaded podata instantiates the according object and initializes it with all the loaded values
        /// </summary>
        public void InitializeLoadedObject(PlaceableObjectData podata) {
            _stateHandler = new LoadedObjectPlacementState(podata, _grid, _database, _gridDataMap, placementHandler);
            _stateHandler.OnAction(podata.gridPosition);
            _stateHandler = null;
        }

        /// <summary>
        /// initializes the PlacementState and adds all trigger functions
        /// </summary>
        public void StartPlacement(string assetIdentifier) {
            StopState();
            _grid.SetVisualizationState(true);
            _stateHandler = new PlacementState(assetIdentifier, _grid, previewSystem, _database, _gridDataMap, placementHandler);
            _inputManager.OnClicked += StateAction;
            _inputManager.OnExit += StopState;
            _inputManager.OnRotate += RotateStructure;
            OnPlacementStateStart?.Invoke();
        }

        /// <summary>
        /// initializes the RemoveState and adds all trigger functions
        /// </summary>
        public void StartRemoving(GridDataType gridType) {
            StopState();
            _grid.SetVisualizationState(true);
            _stateHandler = new RemoveState(_grid, previewSystem, _gridDataMap[gridType], placementHandler);
            _inputManager.OnClicked += StateAction;
            _inputManager.OnExit += StopState;
            _inputManager.OnExit += ObjectGrouper.Instance.DisplayAll;
            ObjectGrouper.Instance.DisplayOnlyObjectsOfSelectedGridType(gridType);
        }

        /// <summary>
        /// In the remove state if clicked on a grid cell, all objects across all gridData that have this position will be removed 
        /// </summary>
        public void StartRemovingAll() {
            StopState();
            _grid.SetVisualizationState(true);
            _stateHandler = new RemoveAllState(_grid, previewSystem, _gridDataMap, placementHandler);
            _inputManager.OnClicked += StateAction;
            _inputManager.OnExit += StopState;
            _inputManager.OnExit += ObjectGrouper.Instance.DisplayAll;
            ObjectGrouper.Instance.DisplayAll();
        }

        /// <summary>
        /// initializes the RemoveState, directly removes the object at the given gridPosition, and  sets the state to null again
        /// </summary>
        public void Remove(PlacedObject placedObject) {
            var gridType = placedObject.placeable.GridType;
            StopState();
            _stateHandler = new RemoveState(_grid, previewSystem, _gridDataMap[gridType], placementHandler);
            _stateHandler.OnAction(placedObject.data.gridPosition);
            _stateHandler.EndState();
            _stateHandler = null;

        }

        /// <summary>
        /// initializes the MoveState and adds all trigger functions
        /// </summary>
        public void StartMoving(PlacedObject target) {
            StopState();
            _stopStateAfterAction = true;
            _grid.SetVisualizationState(true);
            _stateHandler = new MovingState(target, _grid, previewSystem, _gridDataMap, placementHandler);
            _inputManager.OnClicked += StateAction;
            _inputManager.OnExit += StopState;
            _inputManager.OnRotate += RotateStructure;
            OnPlacementStateStart?.Invoke();
        }

        public void StopState() {
            //we should still disable the visualization even if there is no state available
            _grid.SetVisualizationState(false);
            if (_stateHandler == null) {
                return;
            }

            // reset stop trigger;
            _stopStateAfterAction = false;

            _stateHandler.EndState();
            _inputManager.OnClicked -= StateAction;
            _inputManager.OnExit -= StopState;
            _inputManager.OnExit -= ObjectGrouper.Instance.DisplayAll;
            _inputManager.OnRotate -= RotateStructure;
            _lastDetectedPosition = Vector3Int.zero;
            // very Important: reset the placement state when we stop the placement
            _stateHandler = null;
            ObjectGrouper.Instance.DisplayAll();
            OnPlacementStateEnd?.Invoke();
        }


        /// <summary>
        /// additional check if our pointer is over UI --> ignore action
        /// calculates the current gridPosition and triggers Action of the selected state,
        /// which will e.g. either place or remove the object (based on the state that we are currently in)
        /// </summary>
        private void StateAction() {
            if (InputManager.IsPointerOverUIObject()) {
                return;
            }

            var mousePosition = _inputManager.GetSelectedMapPosition();
            var gridPosition = _grid.WorldToCell(mousePosition);

            _stateHandler.OnAction(gridPosition);
            if (_stopStateAfterAction) {
                StopState();
            }
        }

        /// <summary>
        ///  triggers OnRotation function of the current state
        /// </summary>
        private void RotateStructure() {
            _stateHandler.OnRotation();
        }


        /// <summary>
        /// tracks the mousePosition and calculates the up to date gridPosition
        /// if the gridPosition would change, we update the stateHandler state and the last detected position
        /// </summary>
        private void Update() {
            if (_stateHandler == null) {
                return;
            }

            // actual raycasted position on the grid floor
            var mousePosition = _inputManager.GetSelectedMapPosition();
            var gridPosition = _grid.WorldToCell(mousePosition);
            // if nothing has changed for the grid position, we do not need to waste resources to calculate all the other stuff 
            if (_lastDetectedPosition == gridPosition) {
                return;
            }

            _stateHandler.UpdateState(gridPosition);
            _lastDetectedPosition = gridPosition;
        }
    }
}