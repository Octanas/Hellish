using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void Start()
    {
        Cursor.visible = true;
    }

    ///<summary>
    /// Loads up the main scene in the selected difficulty
    ///</summary>
    public void PlayGame()
    {
        SceneManager.LoadScene("Scenes/LevelOne");
    }

    ///<summary>
    /// Quits the game
    ///</summary>
    public void QuitGame()
    {
        Application.Quit();
    }

    public void ButtonSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/Click");
    }
}