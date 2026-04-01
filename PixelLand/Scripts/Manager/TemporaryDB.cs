using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemporaryDB : MonoBehaviour
{
    private static TemporaryDB instance = null;

    void Awake()
    {
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

    public static TemporaryDB Instance
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

    ObjectDataList dataList = new ObjectDataList();
    public ObjectDataList DataList { get { return dataList; } set { dataList = value; } }
}
