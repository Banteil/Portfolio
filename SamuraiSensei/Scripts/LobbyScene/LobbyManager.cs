using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    private static LobbyManager instance = null;
    public static LobbyManager Instance
    {
        get
        {
            if (null == instance)
            {
                return null;
            }
            return instance;
        }
    }

    void Awake()
    {
        if (null == instance)
        {
            instance = this;
            SoundManager.Instance.BGMPlay(Resources.Load<AudioClip>("Sounds/LobbyBGM"));
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [SerializeField]
    BottomUI bottomUI;
    [SerializeField]
    RectTransform scrollContent;
    [SerializeField]
    GameObject ExitPopupUI;

    [SerializeField]
    LobbyMenu[] lobbyMenu;
    
    Coroutine scrollRoutine;

    private void Update()
    {
        if(Application.platform.Equals(RuntimePlatform.Android))
        {
            if(Input.GetKey(KeyCode.Escape))
            {
                ExitPopupUI.SetActive(true);
            }
        }
    }

    public void Quit() => Application.Quit();

    public void SelectMenu(int index)
    {        
        if (scrollRoutine != null) StopCoroutine(scrollRoutine);
        scrollRoutine = StartCoroutine(ScrollMove(index));
        AdjustMenuActivation(index);
    }

    IEnumerator ScrollMove(int index)
    {
        Vector2 targetPos = new Vector2(index * -1080f, scrollContent.anchoredPosition.y);
        while (true)
        {
            scrollContent.anchoredPosition = Vector2.Lerp(scrollContent.anchoredPosition, targetPos, 0.5f);
            if (Vector2.Distance(scrollContent.anchoredPosition, targetPos) <= 0.1f)
                break;

            yield return null;
        }
        scrollRoutine = null;
    }

    void AdjustMenuActivation(int index)
    {
        for (int i = 0; i < lobbyMenu.Length; i++)
        {
            if (i.Equals(index)) lobbyMenu[i].enabled = true;
            else
            {
                lobbyMenu[i].ExitMenu();
                lobbyMenu[i].enabled = false;
            }
        }
    }
}
