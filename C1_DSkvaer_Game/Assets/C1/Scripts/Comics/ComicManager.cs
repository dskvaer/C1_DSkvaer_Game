using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.InputSystem; // Добавляем новую систему ввода

public class ComicManager : MonoBehaviour {
    [SerializeField] private GameObject[] comicPanels; // Массив панелей (ComicPanel_1 - ComicPanel_5)
    [SerializeField] private Button skipButton; // Кнопка "Пропустить"/"Далее"
    [SerializeField] private TextMeshProUGUI skipButtonText; // Текст кнопки
    [SerializeField] private Color normalButtonColor = Color.gray; // Обычный цвет кнопки
    [SerializeField] private Color finalButtonColor = Color.green; // Зелёный цвет для последней панели
    [SerializeField] private float[] panelDurations; // Длительность анимации каждой панели (в секундах)

    private int currentPanelIndex = 0; // Текущая панель (0 - 4)
    private bool isAnimationComplete = false; // Завершена ли анимация текущей панели
    private Animator currentAnimator; // Animator текущей панели

    void Start()
    {
        // Проверяем, что все панели и длительности назначены
        if (comicPanels.Length != 5 || panelDurations.Length != 5)
        {
            Debug.LogError("Требуется ровно 5 панелей и 5 длительностей!");
            return;
        }

        // Проверяем, что кнопка и текст назначены
        if (skipButton == null || skipButtonText == null)
        {
            Debug.LogError("SkipButton или skipButtonText не назначены!");
            return;
        }

        // Инициализация первой панели
        InitializePanel();
        skipButton.onClick.AddListener(OnSkipButtonClicked);
    }

    void Update()
    {
        // Переход к следующей панели по пробелу или клику, если анимация завершена
        if (isAnimationComplete && (Keyboard.current.spaceKey.wasPressedThisFrame || Mouse.current.leftButton.wasPressedThisFrame))
        {
            NextPanel();
        }
    }

    void InitializePanel()
    {
        // Отключаем все панели
        foreach (GameObject panel in comicPanels)
        {
            panel.SetActive(false);
        }

        // Включаем текущую панель
        if (currentPanelIndex < comicPanels.Length)
        {
            comicPanels[currentPanelIndex].SetActive(true);
            currentAnimator = comicPanels[currentPanelIndex].GetComponent<Animator>();
            if (currentAnimator == null)
            {
                Debug.LogError($"Animator не найден на {comicPanels[currentPanelIndex].name}!");
                return;
            }

            // Сбрасываем триггер Skip и запускаем анимацию
            currentAnimator.ResetTrigger("Skip");
            isAnimationComplete = false;
            skipButtonText.text = "Пропустить";
            skipButton.GetComponent<Image>().color = normalButtonColor;

            // Для последней панели меняем цвет кнопки
            if (currentPanelIndex == comicPanels.Length - 1)
            {
                skipButton.GetComponent<Image>().color = finalButtonColor;
            }

            // Запускаем ожидание анимации
            StartCoroutine(WaitForAnimation());
        }
    }

    IEnumerator WaitForAnimation()
    {
        // Ждём длительность анимации текущей панели
        yield return new WaitForSeconds(panelDurations[currentPanelIndex]);
        isAnimationComplete = true;
        skipButtonText.text = "Далее";
    }

    void OnSkipButtonClicked()
    {
        if (!isAnimationComplete)
        {
            // Пропуск анимации текущей панели
            StopAllCoroutines();
            currentAnimator.SetTrigger("Skip");
            isAnimationComplete = true;
            skipButtonText.text = "Далее";
        }
        else
        {
            // Переход к следующей панели или к финальной сцене
            NextPanel();
        }
    }

    void NextPanel()
    {
        // Если это последняя панель, переходим к DEMO_SEA
        if (currentPanelIndex >= comicPanels.Length - 1)
        {
            SceneManager.LoadScene("DEMO_SEA");
            return;
        }

        // Переключаемся на следующую панель
        currentPanelIndex++;
        InitializePanel();
    }
}