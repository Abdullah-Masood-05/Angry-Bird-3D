using UnityEngine;
using UnityEngine.SceneManagement;

public class PuaseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public GameObject[] pauseMenuUI;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }
    public void Resume()
    {
        if (pauseMenuUI != null)
        {
            foreach (GameObject menu in pauseMenuUI)
            {
                if (menu != null)
                    menu.SetActive(false);
            }
        }
        Time.timeScale = 1f;
        GameIsPaused = false;
    }
    void Pause()
    {
        if (pauseMenuUI != null)
        {
            foreach (GameObject menu in pauseMenuUI)
            {
                if (menu != null)
                    menu.SetActive(true);
            }
        }
        Time.timeScale = 0f;
        GameIsPaused = true;
    }
    public void LoadMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
        Debug.Log("LoadingMenu...");
    }
    public void QuitGame()
    {
        Debug.Log("Quit!");
        Application.Quit();
    }
}
