using System;
using SpaceFusion.SF_Grid_Building_System.Scripts.Enums;
using SpaceFusion.SF_Grid_Building_System.Scripts.Scriptables;
using UnityEngine;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.Utils {
    public static class PlaceableUtils {
        /// <summary>
        /// Calculates the total object offset based on the rotation and the object size
        /// calls internal methods for rotationSizeOffset and rotation pivot offset
        /// </summary>
        public static Vector3 GetTotalOffset(Vector3 offset, ObjectDirection direction) {
            var angle = GetRotationAngle(direction);
            var pivotOffset = GetRotatedPivotOffset(angle, offset);
            return pivotOffset;

        }

        /// <summary>
        /// Returns the next direction based on the currently selected direction
        /// </summary>
        public static ObjectDirection GetNextDir(ObjectDirection dir) {
            return dir switch {
                ObjectDirection.Down => ObjectDirection.Left,
                ObjectDirection.Left => ObjectDirection.Up,
                ObjectDirection.Up => ObjectDirection.Right,
                ObjectDirection.Right => ObjectDirection.Down,
                _ => ObjectDirection.Down
            };
        }

        /// <summary>
        /// returns the actual rotation angle for a specified direction
        /// </summary>
        public static int GetRotationAngle(ObjectDirection direction) {
            return direction switch {
                ObjectDirection.Down => 0,
                ObjectDirection.Left => 90,
                ObjectDirection.Up => 180,
                ObjectDirection.Right => 270,
                _ => 0
            };
        }

        /// <summary>
        /// calculates the corrected object size based on the rotated direction and the cellSize
        /// the object size will be rounded to the next cell size, so you actually get the length of the occupied cells:
        /// object size of 1.2 x 1.7 with a cell size of 1.5 would result into 1.5 x 2#
        /// and with a cell size of 1 i would result into 2 x 2
        ///
        /// For left and right rotation the cell size x and y values are switched
        /// </summary>
        public static Vector2 GetCorrectedObjectSize(Placeable placeable, ObjectDirection direction, float cellSize) {
            var correctedSize = HandleOptionalDynamicSize(placeable, cellSize);
            var cellBasedObjectSize = SfMathUtils.RoundToNextMultiple(correctedSize, cellSize);
            return direction switch {
                ObjectDirection.Up => cellBasedObjectSize,
                ObjectDirection.Down => cellBasedObjectSize,
                // based on rotation we need to invert the size
                ObjectDirection.Left => new Vector2(cellBasedObjectSize.y, cellBasedObjectSize.x),
                ObjectDirection.Right => new Vector2(cellBasedObjectSize.y, cellBasedObjectSize.x),
                _ => cellBasedObjectSize
            };
        }


        /// <summary>
        /// calculates the pivot offset at runtime
        /// </summary>
        public static Vector3 CalculateOffset(GameObject obj, float cellSize) {
            if (!obj) {
                return Vector3.zero;
            }

            var rend = obj.GetComponentInChildren<Renderer>();
            if (!rend) {
                Debug.LogError($"No renderer attached for object {obj.name}");
                return Vector3.zero;
            }

            var originalSize = rend.bounds.size;
            var roundedX = (int)Math.Ceiling(Math.Round(originalSize.x / cellSize, 3));
            var roundedZ = (int)Math.Ceiling(Math.Round(originalSize.z / cellSize, 3));
            var adjustedSizeX = roundedX * cellSize;
            var adjustedSizeZ = roundedZ * cellSize;

            var marginX = (adjustedSizeX - originalSize.x) / 2f;
            var marginZ = (adjustedSizeZ - originalSize.z) / 2f;

            var bottomLeft = rend.bounds.min;
            // Calculate pivot offset & round to just make the numbers a bit cleaner and to avoid numbers like 5.96e-8 which is nearly zero
            const int decimalsToRound = 6;
            var pivotOffset = SfMathUtils.RoundVector(new Vector3(marginX, 0, marginZ) + (obj.transform.position - bottomLeft), decimalsToRound);
            // Debug.Log($"Calculated for '{obj.name}':  Pivot Offset {pivotOffset}");
            return pivotOffset;
        }

        /// <summary>
        /// returns the corrected occupied cells based on rotation, objectSize and cellSize
        /// </summary>
        public static Vector2Int GetOccupiedCells(Placeable placeable, ObjectDirection direction, float cellSize) {
            var correctedSize = HandleOptionalDynamicSize(placeable, cellSize);
            var cellsX = Mathf.CeilToInt(correctedSize.x / cellSize);
            var cellsY = Mathf.CeilToInt(correctedSize.y / cellSize);
            var occupiedCells = new Vector2Int(cellsX, cellsY);
            return GetRotationBasedOccupiedCells(occupiedCells, direction);
        }


        #region Private Functions

        /// <summary>
        /// Adapts the obect size based on the dynamic size flag
        /// if dynamicSize is set, the object scale itself will be scaled to the cellSize, so we need to consider the new scale for calculating the occupied cells later
        /// </summary>
        private static Vector2 HandleOptionalDynamicSize(Placeable placeable, float cellSize) {
            if (placeable.DynamicSize) {

                return placeable.Size * cellSize;
            }

            return placeable.Size;
        }

        /// <summary>
        /// based on the rotation we need to change the occupied cell size vector, if it's rotated to left or right, then the x and y sizes need to be swapped
        /// </summary>
        private static Vector2Int GetRotationBasedOccupiedCells(Vector2Int size, ObjectDirection direction) {
            return direction switch {
                ObjectDirection.Up => size,
                ObjectDirection.Down => size,
                // based on rotation we need to invert the size
                ObjectDirection.Left => new Vector2Int(size.y, size.x),
                ObjectDirection.Right => new Vector2Int(size.y, size.x),
                _ => size
            };
        }

        /// <summary>
        /// recalculates the pivot offset based on the rotation angle
        /// </summary>
        private static Vector3 GetRotatedPivotOffset(float rot, Vector3 pivotOffset) {
            var adjustedOffset = pivotOffset;

            // Convert rotation to 0-360 range
            var rotation = (rot % 360 + 360) % 360;

            adjustedOffset = (int)rotation switch {
                90 => new Vector3(pivotOffset.z, pivotOffset.y, pivotOffset.x),
                180 => new Vector3(pivotOffset.x, pivotOffset.y, pivotOffset.z),
                270 => new Vector3(pivotOffset.z, pivotOffset.y, pivotOffset.x),
                _ => adjustedOffset
            };

            return adjustedOffset;
        }

        #endregion
    }
}