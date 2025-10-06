using UnityEngine;
using TMPro; // 用于UI显示

namespace SpaceFusion.SF_Grid_Building_System.Scripts.Managers
{
    public class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance;

        // --- UI Fields (在编辑器中拖拽对应的UI Text组件) ---
        public TextMeshProUGUI moneyText;
        public TextMeshProUGUI populationText;
        public TextMeshProUGUI foodText;

        // --- 核心全局变量 ---
        private float _money;
        private int _currentPopulation;
        private int _populationCapacity;
        private int _basePopulation;
        private int _employedPopulation;
        private float _food;
        private float _foodProductionRate; // 每秒生产的食物
        private bool _bankExists = false;

        // --- 游戏平衡性参数 (可以在编辑器中调整) ---
        [Header("Game Balance Settings")]
        public float startingMoney = 1000f;
        public int startingPopulationCapacity = 10;
        public float populationDecreaseRate = 0.2f;
        public float foodConsumptionPerPerson = 0.1f; // 每人每秒消耗的食物
        public float populationGrowthRate = 0.5f; // 当食物充足时，每秒人口增长的点数
        public float moneyMultiplierFromFood = 0.5f;

        private float _populationGrowthProgress = 0f;
        private float _populationDecreaseProgress = 0f;

        // --- 公开属性，用于其他脚本访问 ---
        public float Money => _money;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            _money = startingMoney;
            _populationCapacity = startingPopulationCapacity;
            UpdateUI();

            // 每秒调用一次核心逻辑更新
            InvokeRepeating(nameof(Tick), 1f, 1f);
        }

        // 每秒执行一次的核心游戏逻辑
        private void Tick()
        {
            // 1. 生产食物
            _food += _foodProductionRate;

            // 2. 人口消耗食物
            float foodConsumed = _currentPopulation * foodConsumptionPerPerson;

            // 3. 判断食物是否充足
            if (_food >= foodConsumed)
            {
                // 食物充足的逻辑
                _food -= foodConsumed;

                // --- 新的银行逻辑 ---
                // 只有在人口成功消耗食物后，银行才会产生收入
                if (_bankExists && foodConsumed > 0)
                {
                    AddMoney(foodConsumed * moneyMultiplierFromFood);
                }

                // 人口增长
                if (_currentPopulation < _populationCapacity)
                {
                    _populationGrowthProgress += populationGrowthRate;
                    if (_populationGrowthProgress >= 1f)
                    {
                        _currentPopulation++;
                        _populationGrowthProgress -= 1f;
                    }
                }
            }
            else
            {
                // 食物不足的逻辑 (饥饿)
                _food = 0; // 耗尽所有食物

                // --- 新的人口减少逻辑 ---
                // 只有在当前人口大于基础人口时，才会因饥饿减少
                if (_currentPopulation > _basePopulation)
                {
                    _populationDecreaseProgress += populationDecreaseRate;
                    if (_populationDecreaseProgress >= 1f)
                    {
                        _currentPopulation--;
                        _populationDecreaseProgress -= 1f;
                    }
                }
            }

            UpdateUI();
        }

        private void UpdateUI()
        {
            moneyText.text = $"Money: {_money:F0}";
            populationText.text = $"Population: {_currentPopulation} / {_populationCapacity}";
            foodText.text = $"Food: {_food:F0}";
        }

        // --- 公共方法，供其他脚本调用 ---
        public bool SpendMoney(float amount)
        {
            if (_money >= amount)
            {
                _money -= amount;
                UpdateUI();
                return true;
            }
            return false;
        }

        public void AddMoney(float amount)
        {
            _money += amount;
            UpdateUI();
        }

        public void AddHouseEffect(int capacityIncrease, int initialPopulation)
        {
            _populationCapacity += capacityIncrease;
            _currentPopulation += initialPopulation;
            _basePopulation += initialPopulation;

            // --- 关键逻辑：确保增加初始人口后，当前人口不会超过新的上限 ---
            _currentPopulation = Mathf.Min(_currentPopulation, _populationCapacity);

            UpdateUI();
        }

        public void RemoveHouseEffect(int capacityDecrease, int initialPopulationDecrease)
        {
            _populationCapacity -= capacityDecrease;
            _basePopulation -= initialPopulationDecrease;

            if (_basePopulation < 0) _basePopulation = 0;

            // --- 关键逻辑：移除房屋后，如果当前人口超过了新的上限，则将当前人口降低到与上限齐平 ---
            _currentPopulation = Mathf.Min(_currentPopulation, _populationCapacity);

            UpdateUI();
        }

        public void AddFoodProduction(float amount, int workersRequired)
        {
            // 检查是否有足够的人口来工作
            if (GetUnemployedPopulation() >= workersRequired)
            {
                _employedPopulation += workersRequired;
                _foodProductionRate += amount;
                UpdateUI();
            }
            else
            {
                Debug.LogWarning("没有足够的人口来农场工作!");
                // 这里可以提示玩家人口不足
            }
        }

        public void RemoveFoodProduction(float amount, int workersFreed)
        {
            _employedPopulation -= workersFreed;
            _foodProductionRate -= amount;
            if (_foodProductionRate < 0) _foodProductionRate = 0;
            UpdateUI();
        }

        public void SetBankStatus(bool exists)
        {
            _bankExists = exists;
        }

        public int GetUnemployedPopulation()
        {
            return _currentPopulation - _employedPopulation;
        }
    }
}