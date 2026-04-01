using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextBox : MonoBehaviour
{
    [SerializeField]
    Text contentText;
    [SerializeField]
    Text nameText;

    public void SetConversation(string name, string content)
    {
        nameText.text = name;
        contentText.text = content;
    }
}
