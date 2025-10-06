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

        // --- ����ȫ�ֱ��� ---
        private float _money;
        private int _currentPopulation;
        private int _populationCapacity;
        private int _basePopulation;
        private int _employedPopulation;
        private float _food;
        private float _foodProductionRate; // ÿ��������ʳ��
        private bool _bankExists = false;

        // --- ��Ϸƽ���Բ��� (�����ڱ༭���е���) ---
        [Header("Game Balance Settings")]
        public float startingMoney = 1000f;
        public int startingPopulationCapacity = 10;
        public float populationDecreaseRate = 0.2f;
        public float foodConsumptionPerPerson = 0.1f; // ÿ��ÿ�����ĵ�ʳ��
        public float populationGrowthRate = 0.5f; // ��ʳ�����ʱ��ÿ���˿������ĵ���
        public float moneyMultiplierFromFood = 0.5f;

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

            UpdateUI();
        }

        private void UpdateUI()
        {
            moneyText.text = $"Money: {_money:F0}";
            populationText.text = $"Population: {_currentPopulation} / {_populationCapacity}";
            foodText.text = $"Food: {_food:F0}";
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