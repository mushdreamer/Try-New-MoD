using SpaceFusion.SF_Grid_Building_System.Scripts.Managers;
using UnityEngine;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.Core
{
    public enum BuildingType { House, Farm, Institute, PowerPlant, Bank }

    public class BuildingEffect : MonoBehaviour
    {
        public BuildingType type;

        [Header("House Settings")]
        public int populationCapacityIncrease = 5;
        public int initialPopulationGain = 2;

        [Header("Farm Settings")]
        public float foodProduction = 2f; // 每秒生产的食物
        public int workersRequired = 2;   // 需要的工人数量

        // 当建筑被成功放置时调用
        public void ApplyEffect()
        {
            switch (type)
            {
                case BuildingType.House:
                    ResourceManager.Instance.AddHouseEffect(populationCapacityIncrease, initialPopulationGain);
                    break;
                case BuildingType.Farm:
                    ResourceManager.Instance.AddFoodProduction(foodProduction, workersRequired);
                    break;
                case BuildingType.Bank:
                    ResourceManager.Instance.SetBankStatus(true);
                    break;
                case BuildingType.Institute:
                case BuildingType.PowerPlant:
                    // 未来迭代的功能
                    break;
            }
        }

        // 当建筑被移除时调用
        public void RemoveEffect()
        {
            switch (type)
            {
                case BuildingType.House:
                    ResourceManager.Instance.RemoveHouseEffect(populationCapacityIncrease, initialPopulationGain);
                    break;
                case BuildingType.Farm:
                    ResourceManager.Instance.RemoveFoodProduction(foodProduction, workersRequired);
                    break;
                case BuildingType.Bank:
                    // 注意：这里需要更复杂的逻辑来处理多个银行的情况
                    // 为简化，我们假设只有一个银行。如果最后一个银行被拆除，则停止加钱。
                    ResourceManager.Instance.SetBankStatus(false);
                    break;
                case BuildingType.Institute:
                case BuildingType.PowerPlant:
                    break;
            }
        }
    }
}