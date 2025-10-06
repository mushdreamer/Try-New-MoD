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
        public float foodProduction = 2f; // ÿ��������ʳ��
        public int workersRequired = 2;   // ��Ҫ�Ĺ�������

        [Header("PowerPlant Settings")]
        public float electricityProduction = 10f; // ÿ�뷢����
        public float co2Emission = 2f;            // ÿ�������̼�ŷ���

        // ���������ɹ�����ʱ����
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
                case BuildingType.PowerPlant: // <<< --- ����case ---
                    ResourceManager.Instance.AddPowerPlantEffect(electricityProduction, co2Emission);
                    break;
                case BuildingType.Institute:
                    // δ�������Ĺ���
                    break;
            }
        }

        // ���������Ƴ�ʱ����
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
                    // ע�⣺������Ҫ�����ӵ��߼������������е����
                    // Ϊ�򻯣����Ǽ���ֻ��һ�����С�������һ�����б��������ֹͣ��Ǯ��
                    ResourceManager.Instance.SetBankStatus(false);
                    break;
                case BuildingType.PowerPlant: // <<< --- ����case ---
                    ResourceManager.Instance.RemovePowerPlantEffect(electricityProduction, co2Emission);
                    break;
                case BuildingType.Institute:
                    break;
            }
        }
    }
}