using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReviewQuiz : MonoBehaviour
{
    [Header("QuizVariable")]
    List<string> multipleQuestionInfo = new List<string>();
    //Question_Examples(split(',')_Answer
    List<string> answerQuestionInfo = new List<string>();
    //Question_Answer
    List<string> keywordQuestionInfo = new List<string>();
    //Question_Keywords(split(',')_Answer

    //선택된 퀴즈의 타입 리스트
    List<QuestionType> typeList = new List<QuestionType>();
    //선택되어 제출될 퀴즈 리스트
    List<string> quizList = new List<string>();

    float limitTime;
    string currentAnswer;
    bool quizSolve;
    int maxCount;
    Coroutine timeRoutine, quizRoutine;

    [Header("QuizUI")]
    public Text typeText;
    public Text limitTimeText;
    public Text solvingInfoText;
    public MutiplePanel multiplePanel;
    public ShortAnswerPanel shortAnswerPanel;
    public KeywordPanel keywordPanel;

    [Header("QuizAudio")]
    [HideInInspector]
    public AudioSource source;
    public AudioClip correctSound;

    void Start()
    {
        StartCoroutine(InitializationProcess());
        source = GetComponent<AudioSource>();
    }

    IEnumerator InitializationProcess()
    {
        DataManager.interactionData.studyGUID = (string)PhotonNetwork.CurrentRoom.CustomProperties["StudyGUID"];
        yield return StartCoroutine(DataManager.Instance.GetReviewQuiz("M"));
        string multipleQuestions = DataManager.Instance.info;
        if(!multipleQuestions.Equals("")) SplitInfo(multipleQuestions, "M");
        ///////////////////////////////////////////////////////
        yield return StartCoroutine(DataManager.Instance.GetReviewQuiz("S"));
        string answerQuestions = DataManager.Instance.info;
        if (!answerQuestions.Equals("")) SplitInfo(answerQuestions, "S");
        ///////////////////////////////////////////////////////
        yield return StartCoroutine(DataManager.Instance.GetReviewQuiz("K"));
        string keywordQuestions = DataManager.Instance.info;
        if (!keywordQuestions.Equals("")) SplitInfo(keywordQuestions, "K");

        maxCount = multipleQuestionInfo.Count + answerQuestionInfo.Count + keywordQuestionInfo.Count;
        if (maxCount > 10) maxCount = 10;

        limitTime = 30 * maxCount;
        int intTime = (int)limitTime;
        limitTimeText.text = intTime.ToString();
        solvingInfoText.text = "문제 [1 / " + maxCount + "]";

        List<int> randList = new List<int>();
        if (multipleQuestionInfo.Count > 0) randList.Add(0);
        if (answerQuestionInfo.Count > 0) randList.Add(1);
        if (keywordQuestionInfo.Count > 0) randList.Add(2);

        for (int i = 0; i < maxCount; i++)
        {
            switch (randList[Random.Range(0, randList.Count)])
            {
                case 0:
                    {
                        typeList.Add(QuestionType.MULTIPLE);
                        int sel = Random.Range(0, multipleQuestionInfo.Count);
                        quizList.Add(multipleQuestionInfo[sel]);
                        multipleQuestionInfo.RemoveAt(sel);
                        if (multipleQuestionInfo.Count <= 0)
                        {
                            for (int j = 0; j < randList.Count; j++)
                            {
                                if (randList[j].Equals(0))
                                {
                                    randList.RemoveAt(j);
                                    break;
                                }
                            }
                        }
                    }
                    break;
                case 1:
                    {
                        typeList.Add(QuestionType.SHORTANSWER);
                        int sel = Random.Range(0, answerQuestionInfo.Count);
                        quizList.Add(answerQuestionInfo[sel]);
                        answerQuestionInfo.RemoveAt(sel);
                        if (answerQuestionInfo.Count <= 0)
                        {
                            for (int j = 0; j < randList.Count; j++)
                            {
                                if (randList[j].Equals(1))
                                {
                                    randList.RemoveAt(j);
                                    break;
                                }
                            }
                        }
                    }
                    break;
                case 2:
                    {
                        typeList.Add(QuestionType.KEYWORD);
                        int sel = Random.Range(0, keywordQuestionInfo.Count);
                        quizList.Add(keywordQuestionInfo[sel]);
                        keywordQuestionInfo.RemoveAt(sel);
                        if (keywordQuestionInfo.Count <= 0)
                        {
                            for (int j = 0; j < randList.Count; j++)
                            {
                                if (randList[j].Equals(2))
                                {
                                    randList.RemoveAt(j);
                                    break;
                                }
                            }
                        }
                    }
                    break;
            }
        }

        for (int i = 0; i < quizList.Count; i++)
        {
            Debug.Log(typeList[i]);
            Debug.Log(quizList[i]);
        }

        timeRoutine = StartCoroutine(LimitTimeProcess());
        quizRoutine = StartCoroutine(QuizSolvingProcess());
    }

    void SplitInfo(string question, string type)
    {
        string[] splitQuestions = question.Split('|');
        for (int i = 0; i < splitQuestions.Length - 1; i++)
        {
            switch (type)
            {
                case "M":
                    multipleQuestionInfo.Add(splitQuestions[i]);
                    break;
                case "S":
                    answerQuestionInfo.Add(splitQuestions[i]);
                    break;
                case "K":
                    keywordQuestionInfo.Add(splitQuestions[i]);
                    break;
            }

        }
    }

    public bool ConfirmCorrectAnswer(string text)
    {
        Debug.Log(text);
        if (text.Equals(currentAnswer))
        {
            quizSolve = true;
            return true;
        }
        return false;
    }

    IEnumerator LimitTimeProcess()
    {
        while (limitTime > 0f)
        {
            int intTime = (int)(limitTime -= Time.deltaTime);
            limitTimeText.text = intTime.ToString();
            yield return null;
        }

        StopCoroutine(quizRoutine);
        limitTimeText.text = "제한 시간 종료!";
        yield return new WaitForSeconds(2f);

        string guid = (string)PhotonNetwork.LocalPlayer.CustomProperties["GUID"];
        DataManager.interactionData.studyGUID = (string)PhotonNetwork.CurrentRoom.CustomProperties["StudyGUID"];
        DataManager.interactionData.curriculumDate = (string)PhotonNetwork.CurrentRoom.CustomProperties["CurriculumDate"];
        yield return StartCoroutine(DataManager.Instance.CheckAttendance(guid, "fail"));        
        gameObject.SetActive(false);
    }

    IEnumerator QuizSolvingProcess()
    {
        for (int i = 0; i < maxCount; i++)
        {
            solvingInfoText.text = "문제 [" + (i + 1) + " / " + maxCount + "]";
            string[] info = quizList[i].Split('_');
            switch (typeList[i])
            {
                case QuestionType.MULTIPLE:
                    typeText.text = "객관식";
                    Debug.Log(info[2]);
                    int intAnswer = int.Parse(info[2]) - 1;
                    multiplePanel.SetUIInfo(info[0], info[1]);
                    currentAnswer = multiplePanel.GetAnswer(intAnswer);
                    multiplePanel.gameObject.SetActive(true);
                    break;
                case QuestionType.SHORTANSWER:
                    typeText.text = "주관식";
                    currentAnswer = info[1];
                    shortAnswerPanel.questionText.text = info[0];
                    shortAnswerPanel.gameObject.SetActive(true);
                    break;
                case QuestionType.KEYWORD:
                    typeText.text = "키워드";
                    currentAnswer = info[2];
                    keywordPanel.questionText.text = info[0];
                    keywordPanel.SetKeywordList(info[1]);
                    keywordPanel.gameObject.SetActive(true);
                    break;
            }

            Debug.Log(currentAnswer);

            while (!quizSolve) { yield return null; }
            if (i.Equals(maxCount - 1))
            {
                StopCoroutine(timeRoutine);
                limitTimeText.text = "복습퀴즈 풀이 성공!";
            }
            yield return new WaitForSeconds(2f);
            multiplePanel.gameObject.SetActive(false);
            shortAnswerPanel.gameObject.SetActive(false);
            keywordPanel.gameObject.SetActive(false);
            quizSolve = false;
        }

        string guid = (string)PhotonNetwork.LocalPlayer.CustomProperties["GUID"];
        DataManager.interactionData.studyGUID = (string)PhotonNetwork.CurrentRoom.CustomProperties["StudyGUID"];
        DataManager.interactionData.curriculumDate = (string)PhotonNetwork.CurrentRoom.CustomProperties["CurriculumDate"];
        yield return StartCoroutine(DataManager.Instance.CheckAttendance(guid, "pass"));
        gameObject.SetActive(false);
    }
}
