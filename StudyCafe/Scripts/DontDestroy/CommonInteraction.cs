
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;


public delegate void BoolenFunction(bool check);
public delegate void VoidFunction();
public delegate void IntFunction(int value);
public delegate void StringFunction(string value);

public class CommonInteraction : Singleton<CommonInteraction>
{
    [Header("InfoPanel")]
    public GameObject infoPanel;
    public Text infoText;
    public Text buttonText;

    [Header("ConfirmPanel")]
    public GameObject confirmPanel;
    public Text confirmText;

    [Header("LoadingPanel")]
    public GameObject loadingPanel;
    public Text loadingText;
    [HideInInspector]
    public bool isLoading;

    [Header("FadeEffect")]
    public Image fadeImage;

    [Header("Calendar")]
    public GameObject calendarPanel;

    //채팅 고정 여부를 체크하는 bool 변수
    [HideInInspector]
    public bool isFreezeOption;
    //명함 출력 여부를 체크하는 bool 변수BoolenFuction
    [HideInInspector]
    public bool displayNamePlate;
    //UI 조작중인지 여부를 판단하는 bool 변수
    [HideInInspector]
    public bool isUIControl;
    [HideInInspector]
    public bool lobbyEntryComplete;

    //임시 델리게이트 Bool 함수(다른 객체, 클래스에 있는 bool 함수를 임시로 실행할 때 사용)
    public BoolenFunction confirmFunc;
    public VoidFunction eventFunc;

    #region 정보 출력, 행동 확인 패널
    /// <summary>
    /// 패널 닫기 버튼
    /// </summary>
    public void CloseInfoPanel()
    {
        eventFunc?.Invoke();
        infoPanel.SetActive(false);
        eventFunc = null;
        InputControl.Instance.popupEnter = null;
    }

    /// <summary>
    /// 패널 텍스트 업데이트 및 표시 함수
    /// </summary>
    public void InfoPanelUpdate(string str)
    {
        buttonText.text = "닫기";
        infoText.text = str;
        infoPanel.SetActive(true);
        InputControl.Instance.popupEnter = CloseInfoPanel;
    }

    public void EventPanelUpdate(string str)
    {
        buttonText.text = "확인";
        infoText.text = str;
        infoPanel.SetActive(true);
        InputControl.Instance.popupEnter = CloseInfoPanel;
    }

    public void StartLoding() => StartCoroutine(LoadingPresentation());

    IEnumerator LoadingPresentation()
    {
        isLoading = true;
        InputControl.Instance.isValid = false;
        loadingPanel.SetActive(true);
        int count = 0;
        while (isLoading)
        {
            loadingText.text = "Loading";
            for (int i = 0; i < count; i++)
            {
                loadingText.text += ".";
            }

            count++;
            if (count > 3) count = 0;

            yield return new WaitForSeconds(0.5f);
        }

        InputControl.Instance.isValid = true;
        loadingPanel.SetActive(false);
    }

    public void SelectConfirmPanelButton()
    {
        confirmPanel.SetActive(false);
        confirmFunc(true);
        InputControl.Instance.popupEnter = null;
        InputControl.Instance.popupCancel = null;
    }

    public void CancelConfirmPanelButton()
    {
        confirmPanel.SetActive(false);
        confirmFunc(false);
        InputControl.Instance.popupEnter = null;
        InputControl.Instance.popupCancel = null;
    }

    public void ConfirmPanelUpdate(string str)
    {
        confirmText.text = str;
        confirmPanel.SetActive(true);
        InputControl.Instance.popupEnter = SelectConfirmPanelButton;
        InputControl.Instance.popupCancel = CancelConfirmPanelButton;
    }

    public void LeaveRoomInitialization()
    {
        DataManager.isMaster = false;
        isFreezeOption = false;
        displayNamePlate = false;
        isUIControl = false;
    }
    #endregion

    #region 페이드 효과
    ///<summary>
    ///Image객체에 페이드 아웃 효과를 주는 함수
    ///</summary>
    public IEnumerator FadeOut(float fadeTime)
    {
        fadeImage.raycastTarget = true;
        Color fadecolor = fadeImage.color;
        float timer = 0f;

        while (fadecolor.a < 1f)
        {
            timer += Time.deltaTime / fadeTime;
            fadecolor.a = Mathf.Lerp(0, 1, timer);
            fadeImage.color = fadecolor;
            yield return null;
        }
    }

    ///<summary>
    ///Image객체에 페이드 인 효과를 주는 함수
    ///</summary>
    public IEnumerator FadeIn(float fadeTime)
    {
        Color fadecolor = fadeImage.color;
        float timer = 0f;

        while (fadecolor.a > 0f)
        {
            timer += Time.deltaTime / fadeTime;
            fadecolor.a = Mathf.Lerp(1, 0, timer);
            fadeImage.color = fadecolor;
            yield return null;
        }
        fadeImage.raycastTarget = false;
    }

    public void RemoveFade()
    {
        Color fadecolor = fadeImage.color;
        fadecolor.a = 0f;
        fadeImage.color = fadecolor;
        fadeImage.raycastTarget = false;
    }

    ///<summary>
    ///Text객체에 페이드 아웃 효과를 주는 함수
    ///</summary>
    public IEnumerator FadeOut(Text fadeText, float fadeTime)
    {
        Color fadecolor = fadeText.color;
        float timer = 0f;

        while (fadecolor.a < 1f)
        {
            timer += Time.deltaTime / fadeTime;
            fadecolor.a = Mathf.Lerp(0, 1, timer);
            fadeText.color = fadecolor;
            yield return null;
        }
    }

    ///<summary>
    ///Text객체에 페이드 인 효과를 주는 함수
    ///</summary>
    public IEnumerator FadeIn(Text fadeText, float fadeTime)
    {
        Color fadecolor = fadeText.color;
        float timer = 0f;

        while (fadecolor.a > 0f)
        {
            timer += Time.deltaTime / fadeTime;
            fadecolor.a = Mathf.Lerp(1, 0, timer);
            fadeText.color = fadecolor;
            yield return null;
        }
    }
    #endregion

    #region 유효성 검사
    ///<summary>
    ///Email 양식에 맞는지 유효성 검사
    ///</summary>
    public bool IsValidEmail(string email)
    {
        bool valid = Regex.IsMatch(email, @"[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?");
        return valid;
    }

    ///<summary>
    ///Email ID 글자 수 제한 유효성 검사
    ///</summary>
    public bool LimitEmailID(string email)
    {
        string[] splitEMail = email.Split('@');
        if (splitEMail[0].Length > 64) return false;
        else if (email.Length > 320) return false;
        return true;
    }

    ///<summary>
    ///패스워드 유효성 검사
    ///</summary>
    public bool IsValidPassword(string password)
    {
        //길이 검사
        if (password.Length < 8) return false;
        //공백 문자 검사
        if (password.Any(x => char.IsWhiteSpace(x).Equals(true))) return false;
        //숫자, 영어, 특문 포함 여부 검사
        bool intCheck = Regex.IsMatch(password, @"[0-9]");
        bool engCheck = Regex.IsMatch(password, @"[a-zA-Z]");
        bool specialCheck = Regex.IsMatch(password, @"[~`!@#$%^&*()_\-+={}[\]|\\;:'""<>,.?/]");
        if (intCheck && engCheck && specialCheck) return true;
        else return false;
    }

    ///<summary>
    ///패스워드 확인
    ///</summary>
    public bool PasswordReconfirmation(string password, string reconfirmationInfo)
    {
        if(password.Equals(reconfirmationInfo)) return true;
        else return false;
    }

    ///<summary>
    ///전화번호 유효성 검사
    ///</summary>
    public bool IsValidPhoneNumber(string phoneNum)
    {
        //길이 검사
        if (!phoneNum.Length.Equals(10) && !phoneNum.Length.Equals(11)) return false;
        //양식 검사
        return Regex.IsMatch(phoneNum, @"01{1}[016789]{1}[0-9]{7,8}");
    }

    ///<summary>
    ///닉네임 유효성 검사
    ///</summary>
    public bool IsValidNickName(string nickName)
    {
        //길이 검사
        if (nickName.Length <= 0 || nickName.Length > 8) return false;
        else return true;
    }
    #endregion
}
