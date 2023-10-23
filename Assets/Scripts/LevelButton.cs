using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LevelButton : MonoBehaviour
{
    public GameObject lockIcon;
    public GameObject[] stars;
    public Text levelText;

    public void Setup(int levelIndex, int starCount, bool unlocked)
    {
        Button button = GetComponent<Button>();
        button.interactable = unlocked;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => TmlxRuntime.LoadLevel(levelIndex));
        lockIcon.SetActive(!unlocked);
        levelText.gameObject.SetActive(unlocked);
#if UNITY_EDITOR
        // Add level 0 for debugging
        levelText.text = levelIndex.ToString();
#else
        levelText.text = (levelIndex + 1).ToString();
#endif
        for (int starIndex = 0; starIndex < stars.Length; starIndex++)
        {
            stars[starIndex].SetActive(starIndex < starCount);
        }
    }
}
