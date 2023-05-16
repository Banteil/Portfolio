using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Alarm : MonoBehaviour
{
    public Image alarmImage;
    public Text alarmNumText;
    public bool notificationArrival;
    int alarmNum = 0;
    public int AlarmNum
    {
        get { return alarmNum; }
        set
        {
            alarmNum = value;
            alarmNumText.text = "<color=#ff0000>" + alarmNum + "</color>";
        }
    }

    /// <summary>
    /// 내 알람 리스트에서 확인하지 않은게 있는지 체크, 있다면 개수 및 정보 표시
    /// </summary>
    void CheckAlarmList()
    {

    }

    public IEnumerator AlarmDirecting()
    {
        int direction = -1;
        while (notificationArrival)
        {
            alarmImage.transform.Rotate(Vector3.forward, (200f * direction) * Time.deltaTime);
            if (alarmImage.transform.rotation.z >= 0.18f || alarmImage.transform.rotation.z <= -0.18f)
            {
                direction *= -1;
            }
            yield return null;
        }
    }

    public void SelectAlarm()
    {
        notificationArrival = false;
        alarmImage.transform.rotation = Quaternion.identity;
        alarmNumText.text = "0";
    }
}
