using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public Button beforeButton;
    public Button nextButton;
    public LevelButton[] levelButtons;

    static int pageIndex;

    public void Back()
    {
        SceneManager.LoadScene("MainMenu");
    }


    void LoadPage(int newPageIndex)
    {
        Save save = SaveFunctions.LoadGame();
        pageIndex = newPageIndex;
        beforeButton.gameObject.SetActive(pageIndex > 0);
        beforeButton.onClick.RemoveAllListeners();
        if (beforeButton.gameObject.activeSelf)
            beforeButton.onClick.AddListener(() => LoadPage(pageIndex - 1));

        nextButton.gameObject.SetActive(15 * pageIndex + 15 < BtmlRuntime.LEVEL_COUNT && 15 * pageIndex + 15 < save.levels.Count);
        nextButton.onClick.RemoveAllListeners();
        if (nextButton.gameObject.activeSelf)
            nextButton.onClick.AddListener(() => LoadPage(pageIndex + 1));

        for (int levelButtonIndex = 0; levelButtonIndex < levelButtons.Length; levelButtonIndex++)
        {
            int levelIndex = 15 * pageIndex + levelButtonIndex;
            LevelButton levelButton = levelButtons[levelButtonIndex];
            levelButton.gameObject.SetActive(levelIndex < BtmlRuntime.LEVEL_COUNT);
            levelButton.Setup(levelIndex, levelIndex < save.levels.Count ? save.levels[levelIndex].starCount : 0, levelIndex < save.levels.Count);
        }

        PlayerPrefs.SetInt("pageIndex", pageIndex);
    }

    void Start()
    {
        LoadPage(PlayerPrefs.GetInt("pageIndex", 0));
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel")) // Player pressed ESCAPE or BACK
            Back();
    }

}
