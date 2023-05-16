using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CalendarItem : MonoBehaviour
{
    public Calendar _parentCalendar;
    public Text _dayNumText;

    #region 아이템 상호작용
    public void SelectCalendarItem()
    {
        int dummy = int.Parse(_dayNumText.text);
        string dateInfo = _parentCalendar._yearNumText.text + "-" + _parentCalendar._monthNumText.text + "-" + dummy.ToString("00");
        _parentCalendar._selectedInputField.text = dateInfo;           
        _parentCalendar.gameObject.SetActive(false);
    }
    #endregion
}
