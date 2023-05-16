using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapCamAct : MonoBehaviour
{
    GameObject player;

    void Start()
    {
        player = GameObject.Find("Player");
    }

    void LateUpdate()
    {
        transform.position = player.transform.position + new Vector3(0,50f,0);
    }
}
