using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public Button beforeButton;
    public Button nextButton;
    public LevelButton[] levelButtons;

    private static int pageIndex;

    public void Back()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void LoadPage(int newPageIndex)
    {
        EventSystem.current.SetSelectedGameObject(levelButtons[0].gameObject);
        Save save = SaveFunctions.LoadGame();
        pageIndex = newPageIndex;
        beforeButton.gameObject.SetActive(pageIndex > 0);
        beforeButton.onClick.RemoveAllListeners();
        if (beforeButton.gameObject.activeSelf)
        {
            beforeButton.onClick.AddListener(() => LoadPage(pageIndex - 1));
        }

#if UNITY_EDITOR
        // Add level 0 for debugging
        nextButton.gameObject.SetActive((15 * pageIndex) + 15 <= BtmlRuntime.LEVEL_COUNT);
#else
        nextButton.gameObject.SetActive((15 * pageIndex) + 15 < BtmlRuntime.LEVEL_COUNT);
#endif
        nextButton.interactable = nextButton.gameObject.activeSelf && (15 * pageIndex) + 15 < save.levels.Count;
        nextButton.onClick.RemoveAllListeners();
        if (nextButton.gameObject.activeSelf)
        {
            nextButton.onClick.AddListener(() => LoadPage(pageIndex + 1));
        }

        for (int levelButtonIndex = 0; levelButtonIndex < levelButtons.Length; levelButtonIndex++)
        {
            int levelIndex = (15 * pageIndex) + levelButtonIndex;
            LevelButton levelButton = levelButtons[levelButtonIndex];
#if UNITY_EDITOR
            // Add level 0 for debugging
            levelButton.gameObject.SetActive(levelIndex <= BtmlRuntime.LEVEL_COUNT);
#else
            levelButton.gameObject.SetActive(levelIndex < BtmlRuntime.LEVEL_COUNT);
#endif
            levelButton.Setup(levelIndex, levelIndex < save.levels.Count ? save.levels[levelIndex].starCount : 0, levelIndex < save.levels.Count);
        }

        PlayerPrefs.SetInt("pageIndex", pageIndex);
    }

    private void Start()
    {
        LoadPage(PlayerPrefs.GetInt("pageIndex", 0));
    }

    private void Update()
    {
        if (Input.GetButtonDown("Cancel")) // Player pressed ESCAPE or BACK
        {
            Back();
        }
    }

}
