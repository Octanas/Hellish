using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the pause menu functions
/// </summary>
public class PauseMenu : MonoBehaviour
{
    public static bool gameIsPaused = false;
    private float unpausedTimeScale;
    public GameObject PauseMenuUI;
    private PlayerControls controls;

    private void Awake()
    {
        unpausedTimeScale = Time.timeScale;
        controls = new PlayerControls();
        controls.Menu.Pause.performed += PressedEscape;

    }

    // Update is called once per frame
    void PressedEscape(InputAction.CallbackContext context)
    {
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
    }

    ///<summary>
    /// Pauses the game
    ///</summary>
    void Pause()
    {
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
        SceneManager.LoadScene("LevelOne");
    }
}