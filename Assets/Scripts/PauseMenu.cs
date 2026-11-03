using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private GameObject pausePanel;

    private bool isPaused = false;

    public bool IsPaused => isPaused;

    private void Start()
    {
        // Make sure it starts closed
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (isPaused)
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
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void Pause()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

