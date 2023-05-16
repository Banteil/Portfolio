using UnityEngine;
using UnityEngine.UI;

public class QuestionItem : MonoBehaviour
{
    int index;
    public int Index
    {
        set
        {
            index = value;
            indexText.text = (index + 1).ToString();
        }
    }
    public Text indexText;
    public Text questionText;
    public string question;
    public string answer;
    public string options;

    public void SelectItem()
    {
        MiniGameManager.Instance.SetIndex(index);
        MiniGameManager.Instance.startButton.interactable = true;
    }

    public void ButtonActive(bool isOn)
    {
        if (isOn)
            GetComponent<Button>().interactable = false;
        else
            GetComponent<Button>().interactable = true;
    }
}
