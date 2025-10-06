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
                case BuildingType.Institute:
                case BuildingType.PowerPlant:
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
                case BuildingType.Institute:
                case BuildingType.PowerPlant:
                    break;
            }
        }
    }
}