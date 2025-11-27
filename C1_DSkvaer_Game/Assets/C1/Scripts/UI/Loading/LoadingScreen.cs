using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour {
    public Image fillImage;
    private float loadTime = 3f;
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        float progress = Mathf.Clamp01(timer / loadTime);
        fillImage.fillAmount = progress;

        if (timer >= loadTime)
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}