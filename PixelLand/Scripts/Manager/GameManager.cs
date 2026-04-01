using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;

    void Awake()
    {
        Application.targetFrameRate = 60;
        //Cursor.SetCursor(Resources.Load<Texture2D>("Sprites/Cursor/cursor1"), Vector2.zero, CursorMode.ForceSoftware);

        if (null == instance)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

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

    PlayerController player;
    public PlayerController Player { get { return player; } set { player = value; } }



    bool isDirecting = false;
    public bool IsDirecting
    {
        get { return isDirecting; }
        set { isDirecting = value; }
    }

    int movingSceneIndex;
    public int MovingSceneIndex { get { return movingSceneIndex; } }
    Vector2 transferPos;
    public Vector2 TransferPos { get { return transferPos; } }

    //로딩 관련 체크 bool
    bool portalMove; //포탈로 이동하는 것인가 체크
    public bool PortalMove { get { return portalMove;} set { portalMove = value; } }
    bool isLoading;
    public bool IsLoading { get { return isLoading; } set { isLoading = value; } }

    void RestartScene()
    {
        if (player != null)
            Destroy(player.gameObject);
        SceneManager.LoadScene(movingSceneIndex);        
    }

    public void CallLoadScene(int callScene)
    {
        if (callScene.Equals(SceneNumber.load)) return;

        movingSceneIndex = callScene;
        portalMove = false;
        SceneManager.LoadScene(SceneNumber.load, LoadSceneMode.Additive);
    }

    public void MoveOtherScene(int callScene, Vector2 transferPos)
    {
        if (callScene.Equals(SceneNumber.load)) return;

        MapManager.Instance.SaveMapData();
        player.InteractUI = null;
        movingSceneIndex = callScene;
        this.transferPos = transferPos;
        portalMove = true;
        SceneManager.LoadScene(SceneNumber.load, LoadSceneMode.Additive);
    }

    public void RestartGame()
    {
        Invoke("RestartScene", 3f);
    }
}
