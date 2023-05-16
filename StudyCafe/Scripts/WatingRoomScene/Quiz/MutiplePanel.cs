using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MutiplePanel : MonoBehaviour
{
    public Text questionText;
    public Image background;
    public GameObject[] exampleButton;
    public ReviewQuiz reviewQuiz;
    Coroutine directing;

    public void SetUIInfo(string question, string examples)
    {
        questionText.text = question;
        string[] example = examples.Split(',');
        for (int i = 0; i < exampleButton.Length; i++)
        {
            Text exampleText = exampleButton[i].transform.GetChild(1).GetComponent<Text>();
            exampleText.text = example[i];
        }
    }

    public string GetAnswer(int index)
    {
        Text exampleText = exampleButton[index].transform.GetChild(1).GetComponent<Text>();
        return exampleText.text;
    }

    /// <summary>
    /// 주관식, 키워드 문제 정답 입력 함수
    /// </summary>
    public void InputAnswer(int number)
    {
        string answer = exampleButton[number].transform.GetChild(1).GetComponent<Text>().text;
        if (directing != null) StopCoroutine(directing);
        directing = StartCoroutine(DirectingAnswer(reviewQuiz.ConfirmCorrectAnswer(answer)));
    }

    IEnumerator DirectingAnswer(bool isCorrect)
    {
        if (isCorrect)
        {
            reviewQuiz.source.PlayOneShot(reviewQuiz.correctSound);
            background.color = Color.green;
            for (int i = 0; i < exampleButton.Length; i++)
            {
                exampleButton[i].GetComponent<Button>().interactable = false;
            }
        }
        else
        {
            background.color = Color.red;
            yield return new WaitForSeconds(1f);
            background.color = Color.white;
        }        
    }

    private void OnDisable()
    {
        background.color = Color.white;
        questionText.text = "";
        for (int i = 0; i < exampleButton.Length; i++)
        {
            exampleButton[i].transform.GetChild(1).GetComponent<Text>().text = "";
            exampleButton[i].GetComponent<Button>().interactable = true;
        }
    }
}
