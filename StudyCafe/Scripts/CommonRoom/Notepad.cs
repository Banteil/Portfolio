using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Notepad : MonoBehaviour
{
    InputField noteInput;
    void Awake()
    {
        noteInput = GetComponentInChildren<InputField>();
    }

    private void OnEnable()
    {
        noteInput.ActivateInputField();
        StartCoroutine(LoadNoteText());
    }

    public void EndEdit() => StartCoroutine(SaveNoteText());

    public IEnumerator LoadNoteText()
    {
        string saveNoteUrl = "https://stubugs.com/php/savenotetext.php";
        WWWForm form = new WWWForm();
        form.AddField("Purpose", (int)Purpose.GET);
        form.AddField("StudyGUID", DataManager.interactionData.studyGUID);
        form.AddField("CurriculumDate", DataManager.interactionData.curriculumDate);
        form.AddField("GUID", (string)PhotonNetwork.LocalPlayer.CustomProperties["GUID"]);
        form.AddField("Content", noteInput.text);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(saveNoteUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
            }
            else
            {
                string[] check = webRequest.downloadHandler.text.Split(';');
                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        Debug.Log("노트 정보 로드 실패");
                        break;
                    case CheckSuccessed.SUCCESS:
                        noteInput.text = check[1];
                        break;
                }
            }
        }
    }

    public IEnumerator SaveNoteText()
    {
        string saveNoteUrl = "https://stubugs.com/php/savenotetext.php";
        WWWForm form = new WWWForm();
        form.AddField("Purpose", (int)Purpose.SET);
        form.AddField("StudyGUID", DataManager.interactionData.studyGUID);
        form.AddField("CurriculumDate", DataManager.interactionData.curriculumDate);
        form.AddField("GUID", (string)PhotonNetwork.LocalPlayer.CustomProperties["GUID"]);
        form.AddField("Content", noteInput.text);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(saveNoteUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
            }
            else
            {
                string[] check = webRequest.downloadHandler.text.Split(';');

                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        Debug.Log("노트 정보 저장 실패");
                        break;
                    case CheckSuccessed.SUCCESS:
                        Debug.Log("노트 정보 저장 성공");
                        break;
                }
            }
        }
    }
}
