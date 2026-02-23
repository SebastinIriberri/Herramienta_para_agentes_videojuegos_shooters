using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("Escenas de UI (menús, victoria, derrota)")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string victorySceneName = "Victory";
    [SerializeField] private string gameOverSceneName = "GameOver";

    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;

        // Si es una escena de UI, aseguramos cursor visible y libre
        PrepareCursorForScene(sceneName);

        // Muy importante por si en gameplay pausaste el tiempo
        Time.timeScale = 1f;

        SceneManager.LoadScene(sceneName);
    }

    public void LoadScene(int buildIndex)
    {
        if (buildIndex < 0) return;

        // Por seguridad
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(buildIndex);
    }

    public void LoadMainMenu(string menuSceneName = "MainMenu")
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(menuSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void PrepareCursorForScene(string sceneName)
    {
        // Puedes ajustar esta lógica a tus nombres reales de escenas
        bool isUIScene =
            sceneName == mainMenuSceneName ||
            sceneName == victorySceneName ||
            sceneName == gameOverSceneName;

        if (isUIScene)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}