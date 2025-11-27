using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {
    public Button newGameButton;
    public Button continueButton;

    void Start()
    {
        newGameButton.onClick.AddListener(StartNewGame);
        continueButton.onClick.AddListener(ContinueGame);
        continueButton.interactable = PlayerPrefs.HasKey("PlayerData"); // јктивна, если есть сохранение
    }

    void StartNewGame()
    {
        PlayerPrefs.DeleteAll(); // —брасываем данные дл€ новой игры
        SceneManager.LoadScene("CharacterCreation");
    }

    void ContinueGame()
    {
        SceneManager.LoadScene("Game");
    }
}