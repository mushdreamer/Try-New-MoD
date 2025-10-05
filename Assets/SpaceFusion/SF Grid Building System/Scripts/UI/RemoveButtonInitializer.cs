using System;
using SpaceFusion.SF_Grid_Building_System.Scripts.Core;
using SpaceFusion.SF_Grid_Building_System.Scripts.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.UI {
    public class RemoveButtonInitializer : MonoBehaviour {
        [SerializeField]
        private Button buttonPrefab;


        private void Start() {
            foreach (GridDataType gridType in Enum.GetValues(typeof(GridDataType))) {
                var removeButton = Instantiate(buttonPrefab, transform);
                removeButton.onClick.AddListener(() => PlacementSystem.Instance.StartRemoving(gridType));
                removeButton.GetComponentInChildren<TextMeshProUGUI>()?.SetText(gridType.ToString());
            }

            var removeAllButton = Instantiate(buttonPrefab, transform);
            removeAllButton.onClick.AddListener(() => PlacementSystem.Instance.StartRemovingAll());
            removeAllButton.GetComponentInChildren<TextMeshProUGUI>()?.SetText("All Grid Types");
        }
    }
}