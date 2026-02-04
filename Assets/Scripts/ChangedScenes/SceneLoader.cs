using UnityEngine;
using UnityEngine.SceneManagement;


public class SceneLoader : MonoBehaviour
{
   
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;
        SceneManager.LoadScene(sceneName);
    }

    public void LoadScene(int buildIndex)
    {
        if (buildIndex < 0) return;
        SceneManager.LoadScene(buildIndex);
    }

    public void LoadMainMenu(string menuSceneName = "MainMenu")
    {
        SceneManager.LoadScene(menuSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
