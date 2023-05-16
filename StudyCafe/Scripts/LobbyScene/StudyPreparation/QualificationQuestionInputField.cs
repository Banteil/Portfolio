using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QualificationQuestionInputField : MonoBehaviour
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
    public InputField contentText;
    public IntFunction deleteFunction;

    public void DeleteQuestionButton()
    {
        deleteFunction(index);
        Destroy(gameObject);
    }
}
