using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the pause menu functions
/// </summary>
public class PauseMenu : MonoBehaviour
{
    private bool gameIsPaused = false;
    private float unpausedTimeScale;
    public GameObject PauseMenuUI;
    private PlayerControls controls;

    private GameObject audioListener;
    private bool gameOver = false;

    private void Awake()
    {
        audioListener = GameObject.FindWithTag("MainCamera");
        unpausedTimeScale = Time.timeScale;
        controls = new PlayerControls();
        controls.Menu.Pause.performed += PressedEscape;
    }

    // Update is called once per frame
    void PressedEscape(InputAction.CallbackContext context)
    {
        if (gameOver) return;
        if (gameIsPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    ///<summary>
    /// Resumes the game
    ///</summary>
    public void Resume()
    {
        PauseMenuUI.SetActive(false);
        Time.timeScale = unpausedTimeScale;
        gameIsPaused = false;
        Cursor.visible = false;
        ButtonSound();
    }

    ///<summary>
    /// Pauses the game
    ///</summary>
    void Pause()
    {
        ButtonSound();
        Cursor.visible = true;
        PauseMenuUI.SetActive(true);
        unpausedTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        gameIsPaused = true;
    }

    ///<summary>
    /// Goes to the Main Menu
    ///</summary>
    public void LoadMenu()
    {
        Resume();
        SceneManager.LoadScene("MainMenu");
    }

    ///<summary>
    /// Quits the game
    ///</summary>
    public void QuitGame()
    {
        Application.Quit();
    }

    ///<summary>
    /// Restarts the scene
    ///</summary>
    public void Restart()
    {
        Resume();
        SceneManager.LoadScene("LevelOne");
    }

    private void ButtonSound()
    {
        FMODUnity.RuntimeManager.PlayOneShotAttached("event:/Click", audioListener);
    }

    private void OnEnable()
    {
        controls.Menu.Enable();
    }

    private void OnDisable()
    {
        controls.Menu.Disable();
    }

    public void GameOver()
    {
        Pause();
        PauseMenuUI.SetActive(false);
        gameOver = true;
    }
}