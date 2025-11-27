using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CharacterCreationManager : MonoBehaviour {
    // UI References
    [SerializeField] private TextMeshProUGUI strengthValueText;
    [SerializeField] private TextMeshProUGUI dexterityValueText;
    [SerializeField] private TextMeshProUGUI vitalityValueText;
    [SerializeField] private TextMeshProUGUI staminaValueText;
    [SerializeField] private TextMeshProUGUI charmValueText;
    [SerializeField] private TextMeshProUGUI luckValueText;
    [SerializeField] private TextMeshProUGUI pointsCounterText;
    [SerializeField] private Button strengthPlusButton;
    [SerializeField] private Button strengthMinusButton;
    [SerializeField] private Button dexterityPlusButton;
    [SerializeField] private Button dexterityMinusButton;
    [SerializeField] private Button vitalityPlusButton;
    [SerializeField] private Button vitalityMinusButton;
    [SerializeField] private Button staminaPlusButton;
    [SerializeField] private Button staminaMinusButton;
    [SerializeField] private Button charmPlusButton;
    [SerializeField] private Button charmMinusButton;
    [SerializeField] private Button luckPlusButton;
    [SerializeField] private Button luckMinusButton;
    [SerializeField] private ToggleGroup specializationToggleGroup;
    [SerializeField] private Button randomButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button startButton;
    [SerializeField] private TextMeshProUGUI infoText;

    // Character Stats
    private int strength = 1;
    private int dexterity = 1;
    private int vitality = 1;
    private int stamina = 1;
    private int charm = 1;
    private int luck = 1;
    private const int totalStatPoints = 18; // 12 для распределения + 6 начальных
    private const int minStatValue = 1; // Минимальное значение
    private const int maxStatValue = 10; // Максимальное значение
    private int usedStatPoints = 6; // 6 характеристик по 1
    private string selectedSpecialization = "";
    private Dictionary<string, int> specializationBonuses = new Dictionary<string, int>();

    // Specialization Descriptions and Bonuses
    private Dictionary<string, (string description, Dictionary<string, int> bonuses)> specializations = new Dictionary<string, (string, Dictionary<string, int>)>
    {
        { "Adventurer", ("Бывалый путешественник морей и континентов, закаленный штормами и опасностями неизведанных земель. Твоя кожа покрыта шрамами от схваток с дикарями и морскими чудовищами, а глаза горят жаждой новых открытий. Когда другие дрожат перед неизвестностью, ты идешь вперед с ухмылкой.", new Dictionary<string, int> { { "Vitality", 1 }, { "Stamina", 1 } }) },
        { "Rogue", ("Стальные мускулы и кошачья грация – твои верные спутники в кровавых абордажах и ночных налетах. Ты знаешь каждый уголок корабельной палубы и можешь перерезать горло врагу, не издав ни звука. Твоя сабля жаждет крови, а ноги танцуют смертельный танец среди падающих тел.", new Dictionary<string, int> { { "Strength", 1 }, { "Dexterity", 1 } }) },
        { "Trader", ("Золотой язык и чарующая улыбка открывают тебе любые двери – от харчевен в захудалых портах до дворцов губернаторов. Ты превращаешь медяки в золото одними лишь словами, а враги становятся союзниками после бутылки рома и правильно рассказанной истории. Твое богатство – не только в сундуках, но и в людских сердцах.", new Dictionary<string, int> { { "Charm", 2 } }) },
        { "Sailor", ("Соленая кровь течет в твоих жилах, а море шепчет тебе свои секреты. Ты чувствуешь каждый порыв ветра и можешь управлять парусами даже в самый яростный шторм. Твои руки знают каждую веревку, а ноги крепко стоят на палубе, когда корабль пляшет на девятибалльных волнах.", new Dictionary<string, int> { { "Dexterity", 1 }, { "Stamina", 1 } }) },
        { "TreasureHunter", ("Древние карты и легенды о проклятых кладах зовут тебя в самые гиблые места. Там, где другие видят смерть, ты чуешь запах золота. Фортуна – твоя любовница, а опасность – лучший друг. Ты выживешь в любых джунглях и найдешь сокровище даже в пасти морского дьявола.", new Dictionary<string, int> { { "Luck", 1 }, { "Vitality", 1 } }) }
    };

    void Start()
    {
        // Гарантируем сброс всех настроек при старте сцены
        ResetCharacter();
        // Устанавливаем Allow Switch Off для ToggleGroup
        if (specializationToggleGroup != null)
        {
            specializationToggleGroup.allowSwitchOff = true;
        }
        SetupButtonListeners();
        Debug.Log("CharacterCreationManager: Start завершён, UI инициализирован");
    }

    void SetupButtonListeners()
    {
        // Stat Buttons
        strengthPlusButton.onClick.AddListener(() => AdjustStat(ref strength, 1, strengthPlusButton, strengthMinusButton));
        strengthMinusButton.onClick.AddListener(() => AdjustStat(ref strength, -1, strengthPlusButton, strengthMinusButton));
        dexterityPlusButton.onClick.AddListener(() => AdjustStat(ref dexterity, 1, dexterityPlusButton, dexterityMinusButton));
        dexterityMinusButton.onClick.AddListener(() => AdjustStat(ref dexterity, -1, dexterityPlusButton, dexterityMinusButton));
        vitalityPlusButton.onClick.AddListener(() => AdjustStat(ref vitality, 1, vitalityPlusButton, vitalityMinusButton));
        vitalityMinusButton.onClick.AddListener(() => AdjustStat(ref vitality, -1, vitalityPlusButton, vitalityMinusButton));
        staminaPlusButton.onClick.AddListener(() => AdjustStat(ref stamina, 1, staminaPlusButton, staminaMinusButton));
        staminaMinusButton.onClick.AddListener(() => AdjustStat(ref stamina, -1, staminaPlusButton, staminaMinusButton));
        charmPlusButton.onClick.AddListener(() => AdjustStat(ref charm, 1, charmPlusButton, charmMinusButton));
        charmMinusButton.onClick.AddListener(() => AdjustStat(ref charm, -1, charmPlusButton, charmMinusButton));
        luckPlusButton.onClick.AddListener(() => AdjustStat(ref luck, 1, luckPlusButton, luckMinusButton));
        luckMinusButton.onClick.AddListener(() => AdjustStat(ref luck, -1, luckPlusButton, luckMinusButton));

        // Random Button
        randomButton.onClick.AddListener(RandomizeCharacter);

        // Reset Button
        resetButton.onClick.AddListener(ResetCharacter);

        // Start Button
        startButton.onClick.AddListener(StartGame);

        // Specialization Toggles
        foreach (Toggle toggle in specializationToggleGroup.GetComponentsInChildren<Toggle>())
        {
            toggle.onValueChanged.AddListener((isOn) => OnToggleChanged(toggle, isOn));
        }
        Debug.Log("CharacterCreationManager: Слушатели кнопок и тогглов настроены");
    }

    void AdjustStat(ref int stat, int change, Button plusButton, Button minusButton)
    {
        int newStatValue = stat + change;
        int newUsedPoints = usedStatPoints + change;

        if (newStatValue >= minStatValue && newStatValue <= maxStatValue && newUsedPoints >= 0 && newUsedPoints <= totalStatPoints)
        {
            stat = newStatValue;
            usedStatPoints = newUsedPoints;
            UpdateStatUI();
            UpdateButtonStates();
            UpdateStartButtonState();
            Debug.Log($"CharacterCreationManager: Изменена характеристика, stat={stat}, usedPoints={usedStatPoints}");
        }
    }

    void UpdateStatUI()
    {
        // Reset colors to default (white)
        strengthValueText.color = Color.white;
        dexterityValueText.color = Color.white;
        vitalityValueText.color = Color.white;
        staminaValueText.color = Color.white;
        charmValueText.color = Color.white;
        luckValueText.color = Color.white;

        // Update text and color for stats with bonuses
        strengthValueText.text = GetStatWithBonus("Strength").ToString();
        if (specializationBonuses.ContainsKey("Strength")) strengthValueText.color = Color.green;

        dexterityValueText.text = GetStatWithBonus("Dexterity").ToString();
        if (specializationBonuses.ContainsKey("Dexterity")) dexterityValueText.color = Color.green;

        vitalityValueText.text = GetStatWithBonus("Vitality").ToString();
        if (specializationBonuses.ContainsKey("Vitality")) vitalityValueText.color = Color.green;

        staminaValueText.text = GetStatWithBonus("Stamina").ToString();
        if (specializationBonuses.ContainsKey("Stamina")) staminaValueText.color = Color.green;

        charmValueText.text = GetStatWithBonus("Charm").ToString();
        if (specializationBonuses.ContainsKey("Charm")) charmValueText.color = Color.green;

        luckValueText.text = GetStatWithBonus("Luck").ToString();
        if (specializationBonuses.ContainsKey("Luck")) luckValueText.color = Color.green;

        pointsCounterText.text = $"Осталось очков: {totalStatPoints - usedStatPoints}";
        Debug.Log("CharacterCreationManager: UI характеристик обновлён");
    }

    int GetStatWithBonus(string statName)
    {
        int baseValue = statName switch
        {
            "Strength" => strength,
            "Dexterity" => dexterity,
            "Vitality" => vitality,
            "Stamina" => stamina,
            "Charm" => charm,
            "Luck" => luck,
            _ => 0
        };

        if (specializationBonuses.ContainsKey(statName))
        {
            return Mathf.Min(baseValue + specializationBonuses[statName], maxStatValue);
        }
        return baseValue;
    }

    void UpdateButtonStates()
    {
        // Strength
        strengthPlusButton.interactable = strength < maxStatValue && usedStatPoints < totalStatPoints && GetStatWithBonus("Strength") < maxStatValue;
        strengthMinusButton.interactable = strength > minStatValue;
        // Dexterity
        dexterityPlusButton.interactable = dexterity < maxStatValue && usedStatPoints < totalStatPoints && GetStatWithBonus("Dexterity") < maxStatValue;
        dexterityMinusButton.interactable = dexterity > minStatValue;
        // Vitality
        vitalityPlusButton.interactable = vitality < maxStatValue && usedStatPoints < totalStatPoints && GetStatWithBonus("Vitality") < maxStatValue;
        vitalityMinusButton.interactable = vitality > minStatValue;
        // Stamina
        staminaPlusButton.interactable = stamina < maxStatValue && usedStatPoints < totalStatPoints && GetStatWithBonus("Stamina") < maxStatValue;
        staminaMinusButton.interactable = stamina > minStatValue;
        // Charm
        charmPlusButton.interactable = charm < maxStatValue && usedStatPoints < totalStatPoints && GetStatWithBonus("Charm") < maxStatValue;
        charmMinusButton.interactable = charm > minStatValue;
        // Luck
        luckPlusButton.interactable = luck < maxStatValue && usedStatPoints < totalStatPoints && GetStatWithBonus("Luck") < maxStatValue;
        luckMinusButton.interactable = luck > minStatValue;
        Debug.Log("CharacterCreationManager: Состояние кнопок обновлено");
    }

    void RandomizeCharacter()
    {
        // Reset stats to minimum
        strength = minStatValue;
        dexterity = minStatValue;
        vitality = minStatValue;
        stamina = minStatValue;
        charm = minStatValue;
        luck = minStatValue;
        usedStatPoints = 6;

        // Randomly distribute remaining points (18 - 6 = 12)
        int pointsToDistribute = totalStatPoints - usedStatPoints;
        while (pointsToDistribute > 0)
        {
            int statIndex = Random.Range(0, 6);
            switch (statIndex)
            {
                case 0: if (strength < maxStatValue && GetStatWithBonus("Strength") < maxStatValue) { strength++; usedStatPoints++; pointsToDistribute--; } break;
                case 1: if (dexterity < maxStatValue && GetStatWithBonus("Dexterity") < maxStatValue) { dexterity++; usedStatPoints++; pointsToDistribute--; } break;
                case 2: if (vitality < maxStatValue && GetStatWithBonus("Vitality") < maxStatValue) { vitality++; usedStatPoints++; pointsToDistribute--; } break;
                case 3: if (stamina < maxStatValue && GetStatWithBonus("Stamina") < maxStatValue) { stamina++; usedStatPoints++; pointsToDistribute--; } break;
                case 4: if (charm < maxStatValue && GetStatWithBonus("Charm") < maxStatValue) { charm++; usedStatPoints++; pointsToDistribute--; } break;
                case 5: if (luck < maxStatValue && GetStatWithBonus("Luck") < maxStatValue) { luck++; usedStatPoints++; pointsToDistribute--; } break;
            }
        }

        // Randomly select specialization
        Toggle[] toggles = specializationToggleGroup.GetComponentsInChildren<Toggle>();
        foreach (Toggle toggle in toggles)
        {
            toggle.isOn = false;
        }
        if (toggles.Length > 0)
        {
            int randomToggleIndex = Random.Range(0, toggles.Length);
            toggles[randomToggleIndex].isOn = true;
            string toggleName = toggles[randomToggleIndex].name;
            selectedSpecialization = toggleName.StartsWith("Toggle_") ? toggleName.Substring(7) : toggleName;
            if (specializations.ContainsKey(selectedSpecialization))
            {
                specializationBonuses = new Dictionary<string, int>(specializations[selectedSpecialization].bonuses);
            }
            else
            {
                selectedSpecialization = "";
                specializationBonuses.Clear();
            }
        }

        UpdateStatUI();
        UpdateButtonStates();
        UpdateStartButtonState();
        UpdateInfoText();
        Debug.Log("CharacterCreationManager: Персонаж рандомизирован");
    }

    void ResetCharacter()
    {
        // Reset all stats to minimum
        strength = minStatValue;
        dexterity = minStatValue;
        vitality = minStatValue;
        stamina = minStatValue;
        charm = minStatValue;
        luck = minStatValue;
        usedStatPoints = 6;

        // Reset specialization and toggles
        selectedSpecialization = "";
        specializationBonuses.Clear();
        if (specializationToggleGroup != null)
        {
            specializationToggleGroup.allowSwitchOff = true;
            foreach (Toggle toggle in specializationToggleGroup.GetComponentsInChildren<Toggle>())
            {
                toggle.isOn = false;
                toggle.interactable = true;
            }
            specializationToggleGroup.SetAllTogglesOff();
        }

        // Reset UI
        UpdateStatUI();
        UpdateButtonStates();
        UpdateStartButtonState();
        UpdateInfoText();
        Debug.Log("CharacterCreationManager: Персонаж сброшен");
    }

    void OnToggleChanged(Toggle toggle, bool isOn)
    {
        if (isOn)
        {
            string toggleName = toggle.name;
            selectedSpecialization = toggleName.StartsWith("Toggle_") ? toggleName.Substring(7) : toggleName;
            if (specializations.ContainsKey(selectedSpecialization))
            {
                specializationBonuses = new Dictionary<string, int>(specializations[selectedSpecialization].bonuses);
            }
            else
            {
                selectedSpecialization = "";
                specializationBonuses.Clear();
            }
            UpdateStatUI();
            UpdateButtonStates();
            UpdateStartButtonState();
            UpdateInfoText();
            Debug.Log($"CharacterCreationManager: Выбрана специализация {selectedSpecialization}");
        }
        else if (!specializationToggleGroup.AnyTogglesOn())
        {
            selectedSpecialization = "";
            specializationBonuses.Clear();
            UpdateStatUI();
            UpdateButtonStates();
            UpdateStartButtonState();
            UpdateInfoText();
            Debug.Log("CharacterCreationManager: Специализация сброшена");
        }
    }

    void UpdateInfoText()
    {
        if (!string.IsNullOrEmpty(selectedSpecialization) && specializations.ContainsKey(selectedSpecialization))
        {
            infoText.text = specializations[selectedSpecialization].description;
        }
        else
        {
            infoText.text = "Выберите специализацию для просмотра описания.";
        }
        Debug.Log("CharacterCreationManager: Текст информации обновлён");
    }

    void UpdateStartButtonState()
    {
        bool allStatsAssigned = usedStatPoints == totalStatPoints;
        bool specializationSelected = !string.IsNullOrEmpty(selectedSpecialization) && specializations.ContainsKey(selectedSpecialization);
        startButton.interactable = allStatsAssigned && specializationSelected;
        Debug.Log($"CharacterCreationManager: StartButton interactable={startButton.interactable}, allStatsAssigned={allStatsAssigned}, specializationSelected={specializationSelected}");
    }

    void StartGame()
    {
        // Save character data with bonuses
        PlayerPrefs.SetInt("Strength", GetStatWithBonus("Strength"));
        PlayerPrefs.SetInt("Dexterity", GetStatWithBonus("Dexterity"));
        PlayerPrefs.SetInt("Vitality", GetStatWithBonus("Vitality"));
        PlayerPrefs.SetInt("Stamina", GetStatWithBonus("Stamina"));
        PlayerPrefs.SetInt("Charm", GetStatWithBonus("Charm"));
        PlayerPrefs.SetInt("Luck", GetStatWithBonus("Luck"));
        PlayerPrefs.SetString("Specialization", selectedSpecialization);
        PlayerPrefs.Save();
        Debug.Log($"CharacterCreationManager: Сохранены характеристики: Strength={GetStatWithBonus("Strength")}, Dexterity={GetStatWithBonus("Dexterity")}, Vitality={GetStatWithBonus("Vitality")}, Stamina={GetStatWithBonus("Stamina")}, Charm={GetStatWithBonus("Charm")}, Luck={GetStatWithBonus("Luck")}, Specialization={selectedSpecialization}");

        // Load the IntroComicScene
        SceneManager.LoadScene("IntroComicsScene");
    }
}