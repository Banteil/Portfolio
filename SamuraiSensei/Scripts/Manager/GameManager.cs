using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;
    public static GameManager Instance
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
        switch (Application.platform)
        {
            case RuntimePlatform.WebGLPlayer:
                Application.targetFrameRate = -1;
                break;
            default:
                Application.targetFrameRate = 60;
                break;
        }

        if (null == instance)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            fps = GetComponent<FPSCheck>();
            checkTime = DateTime.Now;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    FPSCheck fps;

    ///////////////////////플레이어 정보

    [SerializeField]
    int playerLevel = 1;
    public int PlayerLevel { get { return playerLevel; } set { playerLevel = value; } }

    [SerializeField]
    float maxExp = 100f;
    public float MaxExp { get { return maxExp; } set { maxExp = value; } }

    [SerializeField]
    float exp = 0;
    public float Exp 
    { 
        get { return exp; }
        set
        {
            exp = value;
            if(exp >= maxExp)
            {
                playerLevel++;
                exp = 0;
            }
        }
    }

    const int staminaRecoveryTime = 1;
    DateTime checkTime;
    int minuteCal;
    public int MinuteCal { get { return minuteCal; } }
    int secondCal;
    public int SecondCal { get { return secondCal; } }

    [SerializeField]
    int maxStamina = 10;
    public int MaxStamina
    {
        get
        {
            int max = maxStamina + ((playerLevel - 1) * 5);
            return max;
        }
    }

    [SerializeField]
    int stamina = 9;
    public int Stamina { get { return stamina; } set { stamina = value; } }

    [SerializeField]
    int gold = 0;
    public int Gold { get { return gold; } set { gold = value; } }

    [SerializeField]
    int gem = 0;
    public int Gem { get { return gem; } set { gem = value; } }

    ///////////////////////

    //선택한 스테이지 정보 저장
    StageInfo selectStage;
    public StageInfo SelectStage { get { return selectStage; } set { selectStage = value; } }

    //선택한 캐릭터 정보 저장
    [SerializeField]
    int[] selectCharacter = new int[3];
    public int[] SelectCharacter { get { return selectCharacter; }}

    //로딩 여부 체크용 bool
    bool isLoading;
    public bool IsLoading { get { return isLoading; } set { isLoading = value; } }

    //이동하는 씬 index 저장용
    int movingSceneIndex;
    public int MovingSceneIndex { get { return movingSceneIndex; } set { movingSceneIndex = value; } }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.BackQuote))
        {
            if (fps.enabled) fps.enabled = false;
            else fps.enabled = true;
        }

        StaminaProcess();
    }

    void StaminaProcess()
    {
        if (stamina >= maxStamina)
        {
            checkTime = DateTime.Now;
            return;
        }

        TimeSpan timeCal = DateTime.Now - checkTime;
        minuteCal = timeCal.Minutes;
        secondCal = timeCal.Seconds;

        if (minuteCal >= staminaRecoveryTime)
        {
            stamina++;
            checkTime = DateTime.Now;
        }
    }

    public void CallLoadScene(int callScene)
    {
        if (callScene.Equals(SceneNumber.load)) return;

        movingSceneIndex = callScene;
        SceneManager.LoadScene(SceneNumber.load, LoadSceneMode.Additive);
    }

    public GameObject GetClickedObject()
    {
        //충돌이 감지된 영역
        RaycastHit hit;
        //찾은 오브젝트
        GameObject target = null;

        //마우스 포인트 근처 좌표를 만든다.
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //마우스 근처에 오브젝트가 있는지 확인
        if (true == (Physics.Raycast(ray.origin, ray.direction * 10, out hit)))
        {
            //있으면 오브젝트를 저장한다.
            target = hit.collider.gameObject;
        }

        return target;
    }

}
