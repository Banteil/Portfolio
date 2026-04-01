using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillRange : MonoBehaviour
{
    [SerializeField]
    ControlType control;
    [SerializeField]
    int skillID;
    [SerializeField]
    int characterID;

    SpriteRenderer sR;

    private void Awake()
    {
        sR = GetComponent<SpriteRenderer>();        

        if (control.Equals(ControlType.LINE))
        {
            for (int i = 0; i < BattleManager.Instance.Playerbles.Length; i++)
            {
                if (BattleManager.Instance.Playerbles[i].Info.info.id.Equals(characterID))
                {
                    Vector3 charPos = BattleManager.Instance.Playerbles[i].transform.position;
                    transform.position = new Vector3(charPos.x, transform.position.y);
                    break;
                }
            }
        }
    }

    private void Update()
    {
        if (control.Equals(ControlType.FLEXIBLE))
            transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    public void RangeImageActive(bool active)
    {
        sR.enabled = active;
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).GetComponent<SpriteRenderer>() != null)
                transform.GetChild(i).GetComponent<SpriteRenderer>().enabled = active;
        }
    }
}
