using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamageTextObject : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI damageText;
    public TextMeshProUGUI DamageText { get { return damageText; } set { damageText = value; } }

    void Start()
    {
        StartCoroutine(DestroySelf());
    }

    void Update()
    {
        transform.position += Vector3.up * 10f * Time.deltaTime;
    }

    IEnumerator DestroySelf()
    {
        yield return new WaitForSeconds(0.5f);
        BattleUIManager.Instance.DamageTexts.Enqueue(this);
        gameObject.SetActive(false);        
    }
}
