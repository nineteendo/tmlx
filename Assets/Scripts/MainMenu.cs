using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void LoadOptionsMenu()
    {
        SceneManager.LoadScene("OptionsMenu");
    }

    public void Play()
    {
        SceneManager.LoadScene("Menu");
    }

    public void Quit() // Quit the application, when will you play it again?
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }


    void Update()
    {
        if (Input.GetButtonDown("Cancel")) // Player is tired of the application ;(
            Quit();
    }
}
