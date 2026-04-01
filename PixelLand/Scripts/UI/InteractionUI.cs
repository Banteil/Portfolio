using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractionUI : CharacterUI
{
    [SerializeField]
    Text keyInfoText;
    [SerializeField]
    GameObject actInfo;
    [SerializeField]
    Text actInfoText;

    public void SetTextInfo(string key, string actInfo)
    {
        keyInfoText.text = key;
        actInfoText.text = actInfo;
        keyInfoText.enabled = true;
        this.actInfo.SetActive(true);
    }

    private void Update()
    {
        if (target == null) return;

        transform.position = target.transform.position + (Vector3.up * (target.SR.sprite.bounds.size.y + 0.3f));
    }
}
