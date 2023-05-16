using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShortAnswerPanel : MonoBehaviour
{
    public Text questionText;
    public Image background;
    public InputField answerInput;
    public Button inputButton;
    public ReviewQuiz reviewQuiz;
    Coroutine directing;
    bool isPlaying;

    private void OnEnable()
    {
        isPlaying = true;
        inputButton.interactable = true;
        answerInput.interactable = true;
        answerInput.ActivateInputField();
    }

    /// <summary>
    /// 주관식, 키워드 문제 정답 입력 함수
    /// </summary>
    public void InputAnswer()
    {
        if (!isPlaying) return;
        if (directing != null) StopCoroutine(directing);
        directing = StartCoroutine(DirectingAnswer(reviewQuiz.ConfirmCorrectAnswer(answerInput.text)));
    }

    IEnumerator DirectingAnswer(bool isCorrect)
    {
        if (isCorrect)
        {
            reviewQuiz.source.PlayOneShot(reviewQuiz.correctSound);
            isPlaying = false;
            background.color = Color.green;
            inputButton.interactable = false;
            answerInput.interactable = false;
        }
        else
        {
            answerInput.ActivateInputField();
            background.color = Color.red;
            yield return new WaitForSeconds(1f);
            background.color = Color.white;
        }
    }

    private void OnDisable()
    {        
        background.color = Color.white;
        questionText.text = "";
        answerInput.text = "";        
    }
}
