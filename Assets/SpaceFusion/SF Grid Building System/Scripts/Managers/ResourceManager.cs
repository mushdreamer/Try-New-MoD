using UnityEngine;
using TMPro; // ����UI��ʾ

namespace SpaceFusion.SF_Grid_Building_System.Scripts.Managers
{
    public class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance;

        // --- UI Fields (�ڱ༭������ק��Ӧ��UI Text���) ---
        public TextMeshProUGUI moneyText;
        public TextMeshProUGUI populationText;
        public TextMeshProUGUI foodText;
        public TextMeshProUGUI electricityText;
        public TextMeshProUGUI happinessText;
        public TextMeshProUGUI airQualityText;

        // --- ����ȫ�ֱ��� ---
        private float _money;
        private int _currentPopulation;
        private int _populationCapacity;
        private int _basePopulation;
        private int _employedPopulation;
        private float _food;
        private float _foodProductionRate; // ÿ��������ʳ��
        private float _electricityProduction;
        private float _happiness;
        private float _carbonDioxideEmission;
        private float _airQuality;
        private bool _bankExists = false;

        // --- ��Ϸƽ���Բ��� (�����ڱ༭���е���) ---
        [Header("Game Balance Settings")]
        public float startingMoney = 1000f;
        public int startingPopulationCapacity = 10;
        public float populationDecreaseRate = 0.2f;
        public float foodConsumptionPerPerson = 0.1f; // ÿ��ÿ�����ĵ�ʳ��
        public float populationGrowthRate = 0.5f; // ��ʳ�����ʱ��ÿ���˿������ĵ���
        public float moneyMultiplierFromFood = 0.5f;

        [Header("Electricity & Environment")] // <<< --- ���������� ---
        public float electricityPerPerson = 0.2f; // ÿ��ÿ�����ĵĵ���
        public float happinessChangeRate = 1f;    // �Ҹ���ÿ��仯�ĵ���
        public float airQualityRecoveryRate = 0.1f; // ��������ÿ����Ȼ�ָ��ĵ���
        public float airQualityDeclineRate = 0.2f;  // ÿ��λ������̼�ŷŵ��¿��������½�������

        private float _populationGrowthProgress = 0f;
        private float _populationDecreaseProgress = 0f;

        // --- �������ԣ����������ű����� ---
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
            _happiness = 100f;
            _airQuality = 100f;
            UpdateUI();

            // ÿ�����һ�κ����߼�����
            InvokeRepeating(nameof(Tick), 1f, 1f);
        }

        // ÿ��ִ��һ�εĺ�����Ϸ�߼�
        private void Tick()
        {
            // 1. ����ʳ��
            _food += _foodProductionRate;

            // 2. �˿�����ʳ��
            float foodConsumed = _currentPopulation * foodConsumptionPerPerson;

            // 3. �ж�ʳ���Ƿ����
            if (_food >= foodConsumed)
            {
                // ʳ�������߼�
                _food -= foodConsumed;

                // --- �µ������߼� ---
                // ֻ�����˿ڳɹ�����ʳ������вŻ��������
                if (_bankExists && foodConsumed > 0)
                {
                    AddMoney(foodConsumed * moneyMultiplierFromFood);
                }

                // �˿�����
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
                // ʳ�ﲻ����߼� (����)
                _food = 0; // �ľ�����ʳ��

                // --- �µ��˿ڼ����߼� ---
                // ֻ���ڵ�ǰ�˿ڴ��ڻ����˿�ʱ���Ż��򼢶�����
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

            // 4. ��������
            float electricityConsumed = _currentPopulation * electricityPerPerson;

            // 5. �Ҹ��ȼ���
            if (_electricityProduction >= electricityConsumed)
            {
                // �������㣬�Ҹ��Ȼ�������
                _happiness += happinessChangeRate;
                if (_happiness > 100f) _happiness = 100f;
            }
            else
            {
                // �������㣬�Ҹ��Ȼ����½�
                _happiness -= happinessChangeRate;
                if (_happiness < 0f) _happiness = 0f;
            }

            // 6. ������������
            // a. ���ŷŶ��½�
            _airQuality -= _carbonDioxideEmission * airQualityDeclineRate;
            // b. ��Ȼ�ָ�
            _airQuality += airQualityRecoveryRate;

            // ȷ������������0-100֮��
            _airQuality = Mathf.Clamp(_airQuality, 0f, 100f);


            UpdateUI();
        }

        private void UpdateUI()
        {
            moneyText.text = $"Money: {_money:F0}";
            populationText.text = $"Population: {_currentPopulation} / {_populationCapacity}";
            foodText.text = $"Food: {_food:F0}";
            float electricityBalance = _electricityProduction - (_currentPopulation * electricityPerPerson);
            electricityText.text = $"Electricity: {electricityBalance:F1}";
            happinessText.text = $"Happiness: {_happiness:F0}%";
            airQualityText.text = $"AirQ: {_airQuality:F0}%";
        }

        // --- �����������������ű����� ---
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

            // --- �ؼ��߼���ȷ�����ӳ�ʼ�˿ں󣬵�ǰ�˿ڲ��ᳬ���µ����� ---
            _currentPopulation = Mathf.Min(_currentPopulation, _populationCapacity);

            UpdateUI();
        }

        public void RemoveHouseEffect(int capacityDecrease, int initialPopulationDecrease)
        {
            _populationCapacity -= capacityDecrease;
            _basePopulation -= initialPopulationDecrease;

            if (_basePopulation < 0) _basePopulation = 0;

            // --- �ؼ��߼����Ƴ����ݺ������ǰ�˿ڳ������µ����ޣ��򽫵�ǰ�˿ڽ��͵���������ƽ ---
            _currentPopulation = Mathf.Min(_currentPopulation, _populationCapacity);

            UpdateUI();
        }

        public void AddFoodProduction(float amount, int workersRequired)
        {
            // ����Ƿ����㹻���˿�������
            if (GetUnemployedPopulation() >= workersRequired)
            {
                _employedPopulation += workersRequired;
                _foodProductionRate += amount;
                UpdateUI();
            }
            else
            {
                Debug.LogWarning("û���㹻���˿���ũ������!");
                // ���������ʾ����˿ڲ���
            }
        }

        public void RemoveFoodProduction(float amount, int workersFreed)
        {
            _employedPopulation -= workersFreed;
            _foodProductionRate -= amount;
            if (_foodProductionRate < 0) _foodProductionRate = 0;
            UpdateUI();
        }

        // --- ������������ ---
        public void AddPowerPlantEffect(float electricity, float co2)
        {
            _electricityProduction += electricity;
            _carbonDioxideEmission += co2;
            UpdateUI();
        }

        public void RemovePowerPlantEffect(float electricity, float co2)
        {
            _electricityProduction -= electricity;
            _carbonDioxideEmission -= co2;
            if (_electricityProduction < 0) _electricityProduction = 0;
            if (_carbonDioxideEmission < 0) _carbonDioxideEmission = 0;
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