using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClockObject : MonoBehaviour
{
    public GameObject canvas;
    public Text timeText;
    public bool isDigital;

    bool isFull; //화면에 크게 보이는지 여부 판단
    bool isProcessing; //연출 중 여부 판단
    Vector2 origin; //최초 위치 저장용
    Vector3 originRotate; //최초 각도 저장용
    Transform hourTr, minuteTr, secondTr; //시, 분, 초침 로테이션 접근용
    SpriteRenderer[] textSprite;
    Sprite[] numberImages;
    float hour, min, sec; //시, 분, 초 오일러 수치 변수

    void Start()
    {
        canvas.GetComponent<Canvas>().worldCamera = Camera.main;
        origin = transform.localPosition;
        originRotate = transform.localRotation.eulerAngles;
        if (!isDigital)
        {
            hourTr = transform.GetChild(0).transform;
            minuteTr = transform.GetChild(1).transform;
            secondTr = transform.GetChild(2).transform;
        }
        else
        {
            textSprite = new SpriteRenderer[4];
            for (int i = 0; i < textSprite.Length; i++)
            {
                textSprite[i] = transform.GetChild(i).GetComponent<SpriteRenderer>();
            }

            numberImages = new Sprite[10];
            for (int i = 0; i < numberImages.Length; i++)
            {
                numberImages[i] = Resources.Load<Sprite>("RoomSprite/" + i);
            }
        }
        isProcessing = false;
        isFull = false;
    }

    // Update is called once per frame
    void Update()
    {
        DateTime now = DateTime.Now;
        if (!isDigital)
        {
            //아날로그 시계 쪽 업데이트            
            hour = (now.Hour / 12f) * 360f + (now.Minute / 60f) * 30f;
            min = (now.Minute / 60f) * 360f;
            sec = (now.Second / 60f) * 360f;

            hourTr.localRotation = Quaternion.Euler(0f, 0f, -hour);
            minuteTr.localRotation = Quaternion.Euler(0f, 0f, -min);
            secondTr.localRotation = Quaternion.Euler(0f, 0f, -sec);            
        }
        else
        {
            string text = now.Hour.ToString("00") + now.Minute.ToString("00");

            for (int i = 0; i < textSprite.Length; i++)
            {
                int num = (int)char.GetNumericValue(text[i]);                
                textSprite[i].sprite = numberImages[num];
            }
        }

        if (timeText.gameObject.activeSelf)
        {
            string hour;
            if (now.Hour < 13) hour = now.Hour.ToString("00");
            else hour = (now.Hour - 12).ToString("00");

            string time = "";
            if (now.Hour >= 0 && now.Hour < 12) time += "오전 " + hour + " : " + now.Minute.ToString("00") + " : " + now.Second.ToString("00");
            else time += "오후 " + hour + " : " + now.Minute.ToString("00") + " : " + now.Second.ToString("00");
            timeText.text = time;
        }
    }

    private void OnMouseUp()
    {        
        if (isProcessing) return;

        isProcessing = true;
        if(!isFull)
        {
            isFull = true;
            DisplayDigitalClock();
            iTween.MoveTo(gameObject, iTween.Hash("x", 0f, "y", 2.8f, "time", 1f, "oncomplete", "EndProcess", "oncompletetarget", gameObject));
            iTween.RotateTo(gameObject, Vector3.one, 1f);
            iTween.ScaleTo(gameObject, new Vector3(4f, 4f, 1f), 1f);
        }
        else
        {
            isFull = false;
            DisplayDigitalClock();
            iTween.MoveTo(gameObject, iTween.Hash("x", origin.x, "y", origin.y, "time", 1f, "oncomplete", "EndProcess", "oncompletetarget", gameObject));
            iTween.RotateTo(gameObject, originRotate, 1f);
            iTween.ScaleTo(gameObject, Vector3.one, 1f);
        }        
    }

    void DisplayDigitalClock()
    {
        if (!timeText.gameObject.activeSelf)
            timeText.gameObject.SetActive(true);
        else
            timeText.gameObject.SetActive(false);
    }

    void EndProcess() => isProcessing = false;

}
