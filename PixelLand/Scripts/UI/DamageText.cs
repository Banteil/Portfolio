using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PositionOrder;

public class DamageText : MonoBehaviour
{
    float currentTime = 0f;

    PositionOrderer orderer = new PositionOrderer();
    List<Transform> transforms = new List<Transform>();

    public void SettingText(float value)
    {
        List<int> numList = new List<int>();
        int temp = (int)value;
        while (temp != 0)
        {
           numList.Add(temp % 10);
           temp /= 10;
        }

        for (int i = 0; i < numList.Count; i++)
        {
            CreateNumberObj(numList[i]);
        }

        if (transforms.Count > 1)
        {
            transforms.Reverse();
            SetPositions();
        }
    }

    void CreateNumberObj(int num)
    {
        GameObject obj = new GameObject("NumberObject");
        obj.transform.parent = transform;
        obj.transform.position = transform.position;
        SpriteRenderer sR = obj.AddComponent<SpriteRenderer>();
        sR.sortingLayerName = "UI";
        sR.sortingOrder = 1;
        sR.sprite = ResourceManager.Instance.NumberSprites[num];
        transforms.Add(obj.transform);
    }

    void SetPositions()
    {
        //리스트 초기화 및 트랜스폼 추가
        orderer.Transforms.Clear();
        orderer.Transforms.AddRange(transforms);

        //거리 설정
        orderer.Distance_X = 0.5f;
        orderer.Distance_Y = 0f;
        orderer.Distance_Z = 0f;

        orderer.ApplyLineOrder(LineAnchor.Start); //Line Type 정렬
    }

    void Update()
    {
        transform.Translate(Vector3.up * 0.01f);
        currentTime += Time.deltaTime;
        if (currentTime >= 0.5f)
            Destroy(gameObject);
    }
}
