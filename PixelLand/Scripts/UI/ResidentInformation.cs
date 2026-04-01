using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResidentInformation : MonoBehaviour
{
    [SerializeField]
    Text numberText;
    [SerializeField]
    Text spaceInfoText;

    public void SetResidentNumber(int num)
    {
        numberText.text = num.ToString("000");
    }

    public void SetSpaceInfoText(string info)
    {
        spaceInfoText.text = info;
    }
}
