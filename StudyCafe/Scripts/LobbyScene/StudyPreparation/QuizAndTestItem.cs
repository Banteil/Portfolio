using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuizAndTestItem : MonoBehaviour
{
    public Text typeAndIndexText;
    public Text contentText;
    int index;
    public int Index
    {
        set
        {
            index = value;
            typeAndIndexText.text = "Q." + (index + 1);
        }
    }

    QuizAndTestInfo info = new QuizAndTestInfo();
    public QuizAndTestInfo Info
    {
        get { return info; }
        set
        {
            info = value;
            string tempContent = "";
            if (info.question.Length > 80)
            {
                tempContent = info.question.Substring(0, 79);
                tempContent += "…";
            }
            else
                tempContent = info.question;
            contentText.text = tempContent;
        }
    }
    public string type;
    [HideInInspector]
    public string guid;

    public delegate void ItemInteractFunction(string type, int index);
    public ItemInteractFunction updateFunc;
    public ItemInteractFunction deleteFunc;

    public void UpdateButton()
    {
        updateFunc?.Invoke(type, index);
    }

    public void DeleteButton()
    {
        deleteFunc?.Invoke(type, index);
        Destroy(gameObject);
    }

}
