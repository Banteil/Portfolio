using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Space : MonoBehaviour
{
    PersonalSpace pS;
    PolygonCollider2D range;

    private void Awake()
    {
        pS = transform.parent.GetComponent<PersonalSpace>();
        range = GetComponent<PolygonCollider2D>();
    }

    void CheckOwner(CharacterBasic cB)
    {
        if (pS.OwnerID.Equals("null"))
        {
            GameManager.Instance.Player.BuildMode = false;
            return;
        }

        GameManager.Instance.Player.BuildMode = pS.OwnerID.Equals(cB.ObjectID);
        if (GameManager.Instance.Player.BuildMode)
            DrawBuildLIne();
    }
    void DrawBuildLIne()
    {
        Debug.Log("공간 범위 sizeX : " + range.bounds.size.x);
        Debug.Log("공간 범위 sizeY : " + range.bounds.size.y);
        Debug.Log("공간 범위 max : " + range.bounds.max);
        Debug.Log("공간 범위 min : " + range.bounds.min);
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Character"))
        {
            CharacterHitBox cH = collision.GetComponent<CharacterHitBox>();
            if (cH.CB.gameObject.CompareTag("Player"))
            {
                CheckOwner(cH.CB);
                UIManager.Instance.GetUI("ResidentInformation").GetComponent<ResidentInformation>().SetSpaceInfoText(pS.GetSpaceName());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Character"))
        {
            CharacterHitBox cH = collision.GetComponent<CharacterHitBox>();
            if (cH.CB.gameObject.CompareTag("Player"))
            {
                GameManager.Instance.Player.BuildMode = false;
                UIManager.Instance.GetUI("ResidentInformation").GetComponent<ResidentInformation>().SetSpaceInfoText("");
            }
        }
    }
}
