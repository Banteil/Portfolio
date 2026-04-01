using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapInfoUI : MonoBehaviour
{
    [SerializeField]
    Text _mapNameText;
    [SerializeField]
    Text _subInfoText;

    public void SetMapInfo(string mapName, string subInfo)
    {
        _mapNameText.text = mapName;
        _subInfoText.text = subInfo;
    }
}
