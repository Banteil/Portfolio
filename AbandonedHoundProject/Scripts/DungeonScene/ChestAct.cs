using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestAct : MonoBehaviour
{
    public bool open = false;
    
    void Update()
    {
        Vector3 camera = Camera.main.transform.position;
        camera.y = 0f;
        transform.LookAt(camera);
    }

    public void GetGem()
    {
        int randomNum = Random.Range(0, 100);

        if (randomNum < 60) DungeonManager.Instance.GetGem = DataPassing.stageNum;
        else if (randomNum < 90) DungeonManager.Instance.GetGem = DataPassing.stageNum + 1;
        else DungeonManager.Instance.GetGem += DataPassing.stageNum + 2;
    }

    public void GetMoney()
    {
        int randomNum = Random.Range(0, 100);

        if (randomNum < 70) DungeonManager.Instance.GetMoney = 500 * DataPassing.stageNum;
        else if (randomNum < 95) DungeonManager.Instance.GetMoney = 1000 * DataPassing.stageNum;
        else DungeonManager.Instance.GetMoney = 2000 * DataPassing.stageNum;
    }
}
