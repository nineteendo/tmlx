using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PreviewDropdown : MonoBehaviour
{
    public Button beforeButton;
    public Button blockerButton;
    public Button nextButton;
    public GameObject dropdownList;
    public Item[] items;
    public Text itemText;
    public UnityEvent<int> onValueChanged = new();
    public UnityEvent<int> onEndEdit = new();
    public string[] Options
    {
        get
        {
            return options;
        }

        set
        {
            options = value;
            Refresh();
        }
    }

    public int Value
    {
        get
        {
            return value;
        }

        set
        {
            this.value = value;
            Refresh();
        }
    }

    string[] options = { };

    int pageIndex;
    int value;

    public void Hide()
    {
        if (!dropdownList.activeSelf)
            return;

        onValueChanged.Invoke(value);
        blockerButton.gameObject.SetActive(false);
        dropdownList.SetActive(false);
        EventSystem.current.SetSelectedGameObject(gameObject);
    }

    public void LoadPage(int newPageIndex)
    {
        EventSystem.current.SetSelectedGameObject(items[0].gameObject);
        pageIndex = newPageIndex;
        beforeButton.gameObject.SetActive(pageIndex > 0);
        beforeButton.onClick.RemoveAllListeners();
        if (beforeButton.gameObject.activeSelf)
            beforeButton.onClick.AddListener(() => LoadPage(pageIndex - 1));

        nextButton.gameObject.SetActive(10 * pageIndex + 10 < options.Length);
        nextButton.onClick.RemoveAllListeners();
        if (nextButton.gameObject.activeSelf)
            nextButton.onClick.AddListener(() => LoadPage(pageIndex + 1));

        for (int itemIndex = 0; itemIndex < items.Length; itemIndex++)
        {
            int optionIndex = 10 * pageIndex + itemIndex;
            Item item = items[itemIndex];
            Toggle toggle = item.gameObject.GetComponent<Toggle>();
            item.onSelected.RemoveAllListeners();
            toggle.onValueChanged.RemoveAllListeners();
            toggle.isOn = optionIndex == value;
            if (toggle.isOn)
                EventSystem.current.SetSelectedGameObject(item.gameObject);

            item.gameObject.SetActive(optionIndex < options.Length);
            if (!item.gameObject.activeSelf)
                continue;

            item.onSelected.AddListener(() => onValueChanged.Invoke(optionIndex));
            toggle.onValueChanged.AddListener((_) => Submit(optionIndex));
            item.Setup(options[optionIndex]);
        }
    }

    public void Show()
    {
        if (dropdownList.activeSelf)
            return;

        LoadPage(value / 10);
        dropdownList.SetActive(true);
        blockerButton.gameObject.SetActive(true);
        blockerButton.onClick.RemoveListener(Hide);
        blockerButton.onClick.AddListener(Hide);
    }


    void Refresh()
    {
        if (options.Length == 0)
            return;

        itemText.text = options[value];
    }

    void Start()
    {
        Refresh();
    }

    void Submit(int optionIndex)
    {
        value = optionIndex;
        Refresh();
        Hide();
        onEndEdit.Invoke(value);
    }
}
